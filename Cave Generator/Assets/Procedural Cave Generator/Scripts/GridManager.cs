using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace CaveGenerator
{
	/// <summary>
	/// Singleton. Manages the grid of cells that represent the level. 
	/// </summary>
	[RequireComponent (typeof(TexturePack))]
	[RequireComponent (typeof(NodeClusterManager))]
	[RequireComponent (typeof(Utilities))]
	[ExecuteInEditMode, System.Serializable]
	public class GridManager : MonoBehaviour
	{
		public int Seed = 1;
		
		[Header ("Grid Size and Scale")]
		public Vector2
			GridSize;
		public Vector3 GridScale = Vector3.one;
		
		[Header ("Wall and Background")]
		public int
			NumberOfTransistionSteps = 0;
		public bool IsConnectedEnvironment = false;
		
		[Range (0, 1)]
		public float
			ChanceToBecomeWall = 0.40f;

		[Range (0, 8)]
		public int
			BackgroundToWallConversion = 4;

		[Range (0, 8)]
		public int
			WallToBackgroundConversion = 3;

		[Header ("Layer Masks")]
		public LayerMask
			WallLayer;
		public LayerMask BackgroundLayer;
		
		[Header ("Sorting Order")]
		public int
			WallSpriteSortingOrder = 5;
		public int BackgroundSpriteSortingOrder = 0;

		[Header ("Game Objects")]
		public GameObject
			CellPrefab;
		public GameObject ParentContainer;

		public NodeList Grid { get; set; }
		public Node StartNode { get; set; }
		public Node EndNode { get; set; }

		private static readonly string SCRIPT_NAME = typeof(GridManager).Name;

		private float minDistanceBetweenStartAndEnd;

		private NodeClusterManager nodeClusterManager;

		private List<GameObject> nodes = new List<GameObject> ();

		[SerializeField, HideInInspector]
		private List<Node>
			emptyFloorNodes = new List<Node> ();
		public List<Node> FloorNodes { get { return emptyFloorNodes; } }

		[SerializeField, HideInInspector]
		private TexturePack[]
			texturePacks;
			
		[SerializeField, HideInInspector]
		private int
			currentTexturePack = 0;
		
		private int _backgroundLayer;
		private int _wallLayer;


		public TexturePack TexturePack {
			get {
				return texturePacks [currentTexturePack];
			}
		}

		private static GridManager _instance;
		public static GridManager instance {
			get {

				if (!_instance) {
					_instance = GameObject.FindObjectOfType<GridManager> ();
				}

				return _instance;
			}
		}

		void Start ()
		{
			if (!CellPrefab) {
				Debug.LogError ("Cell prefab not set");
				enabled = false;
			}
		}

		public void ReGenerate ()
		{
			if (Utilities.instance.IsDebug)
				Debug.Log ("Destroying old environment if present");
			DestroyEnvironment ();
			Generate ();
		}
		
		

		public void Generate ()
		{
			if (EnvironmentGenerated ()) {
				return;
			}

			if (!nodeClusterManager) {
				nodeClusterManager = GetComponent<NodeClusterManager> ();
			}

			ParentContainer.transform.parent.localScale = Vector3.one;

			texturePacks = GetTexturePacks ();
			
			string backgroundLayerName = BackgroundLayer.MaskToNames ().Length > 0 ? BackgroundLayer.MaskToNames ()[0] : "Nothing";
			_backgroundLayer = LayerMask.NameToLayer (backgroundLayerName);
			
			string wallLayerName = WallLayer.MaskToNames ().Length > 0 ? WallLayer.MaskToNames ()[0] : "Nothing";
			_wallLayer = LayerMask.NameToLayer (wallLayerName);


			Random.seed = Seed;

			minDistanceBetweenStartAndEnd = (GridSize.x + GridSize.y) / 2 - 25f;

			float beginGeneratingTime = Time.realtimeSinceStartup;


			if (Utilities.instance.IsDebug)
				Debug.Log ("Generating environment");
			InitialiseEnvironment ();

			for (int step = 0; step < NumberOfTransistionSteps; step++) {
				if (Utilities.instance.IsDebug)
					Debug.Log ("Performing transition step: " + (step + 1));
				PerformTransistionStep ();
			}

			// Identify clusters so they can be connected using Path manager 
			nodeClusterManager.IdentifyClusters (Grid, GridSize);

			if (IsConnectedEnvironment) {
				nodeClusterManager.ConnectClusters ();

				// Need to re-identify main cavern to place enter and exit
				nodeClusterManager.IdentifyClusters (Grid, GridSize); 
			} 

			if (Utilities.instance.IsDebug)
				Debug.Log ("Placing Entrance and Exit");
		
			RemoveExtraneous ();
			CacheFloorCells ();
			
		
			PlaceEntranceAndExit ();

	
			GenerateEnvironment ();

			nodeClusterManager.CalculateMainCluster ();
			
			ParentContainer.transform.parent.localScale = GridScale;
				
			if (Utilities.instance.IsDebug)
				Debug.Log ("Generated environment in " + (Time.realtimeSinceStartup - beginGeneratingTime) + " seconds"); 
			
		}
		
		public void RandomSeed ()
		{
			Seed = Random.Range (0, 1000000);
		}

		public void LoadNextTexturePack ()
		{
			currentTexturePack = (currentTexturePack + 1) % texturePacks.Length;
		}
	

		public bool DestroyEnvironment ()
		{
			bool destroyed = EnvironmentGenerated ();
			
			ParentContainer.transform.ClearImmediate ();
			emptyFloorNodes.Clear ();
			nodes.Clear ();
			
			return destroyed;
		}
		

		private bool EnvironmentGenerated ()
		{
			return ParentContainer.transform.childCount > 0;
		}

		private void InitialiseEnvironment ()
		{
			Grid = new NodeList (GridSize);

			for (int x = 0; x < GridSize.x; x++) {
				for (int y = 0; y < GridSize.y; y++) {

	
					Vector2 coord = new Vector2 (x, y);

					NodeType cellType;

					if (IsEdge (coord)) {
						cellType = NodeType.Wall;
					} else {
						cellType = Random.value < ChanceToBecomeWall ? NodeType.Wall : NodeType.Background;
					}

					Grid.Add (new Node (coord, cellType));
				}
			}
		}

		private void PerformTransistionStep ()
		{
			
			NodeList newGrid = new NodeList (GridSize);
			
			for (int x = 0; x < GridSize.x; x++) {
				for (int y = 0; y < GridSize.y; y++) {

					Vector2 coord = new Vector2 (x, y);

					int neighbourCount = CountTileWallNeighbours (coord);
					
					Node oldCell = Grid.GetNodeFromGridCoordinate (coord);
					Node newCell = new Node (coord);
					
					if (oldCell.NodeState == NodeType.Wall) {
						newCell.NodeState = (neighbourCount < WallToBackgroundConversion) ? NodeType.Background : NodeType.Wall;
					} else {
						newCell.NodeState = (neighbourCount > BackgroundToWallConversion) ? NodeType.Wall : NodeType.Background;
					}

					newGrid.Add (newCell);
				}
			}
			
			Grid = newGrid;
			
			
		}



		private void RemoveExtraneous ()
		{
			for (int i = 0; i < 2; i++) {
				for (int x = 0; x < GridSize.x; x++) {
					for (int y = 0; y < GridSize.y; y++) {

						var coord = new Vector2 (x, y);

						var node = Grid.GetNodeFromGridCoordinate (coord);

						if (node.NodeState != NodeType.Wall)
							continue;
					
						if (IsExtraneousCell (node.Coordinates)) {
							node.NodeState = NodeType.Background;
						}
					}
				}
			}

			RemoveLoneCells ();
		}

		private void RemoveLoneCells ()
		{
			for (int x = 0; x < GridSize.x; x++) {
				for (int y = 0; y < GridSize.y; y++) {
					
					var coord = new Vector2 (x, y);

					var node = Grid.GetNodeFromGridCoordinate (coord);
					
					if (node.NodeState != NodeType.Wall)
						continue;

					if (IsLoneCell (node.Coordinates)) {
						node.NodeState = NodeType.Background;
					}
				}
			}
		}

		private void CacheFloorCells ()
		{
			for (int x = 0; x < GridSize.x; x++) {
				for (int y = 0; y < GridSize.y; y++) {
					
					var coord = new Vector2 (x, y);
					var node = Grid.GetNodeFromGridCoordinate (coord);
					
					if (node.NodeState != NodeType.Background)
						continue;

					if (IsFloorCell (node.Coordinates)) {
						emptyFloorNodes.Add (node);
					}
				}
			}
		}

		private bool IsFloorCell (Vector2 coord)
		{
			var cellBelow = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y - 1), NodeType.Wall));
			var floorLeft = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x - 1, coord.y), NodeType.Background));
			var floorRight = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x + 1, coord.y), NodeType.Background));
			var floorAbove = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y + 1), NodeType.Background));

			return cellBelow && floorLeft && floorRight && floorAbove;

		}

		private bool IsLoneCell (Vector2 coord)
		{
			var cellBelow = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y - 1), NodeType.Wall));
			var cellLeft = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x - 1, coord.y), NodeType.Wall));
			var cellRight = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x + 1, coord.y), NodeType.Wall));
			var cellAbove = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y + 1), NodeType.Wall));

			if (!cellLeft && !cellRight && !cellBelow && !cellAbove)
				return true;

			return false;
		}
		
		private bool IsExtraneousCell (Vector2 coord)
		{
			var cellBelow = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y - 1), NodeType.Wall));
			var cellLeft = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x - 1, coord.y), NodeType.Wall));
			var cellRight = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x + 1, coord.y), NodeType.Wall));
			var cellAbove = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y + 1), NodeType.Wall));

			var floorBelow = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y - 1), NodeType.Background));
			var floorLeft = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x - 1, coord.y), NodeType.Background));
			var floorRight = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x + 1, coord.y), NodeType.Background));
			var floorAbove = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y + 1), NodeType.Background));
	

			if (!cellLeft && !cellRight && !cellBelow && !cellAbove)
				return true;

			if (!cellLeft && !cellAbove && !cellRight)
				return true;

			if (!cellLeft && !cellAbove && !cellBelow)
				return true;

			if (!cellBelow && !cellAbove && !cellRight)
				return true;

			if (!cellBelow && !cellLeft && !cellRight)
				return true;

			if (floorLeft && floorRight && cellAbove && cellBelow)
				return true;

			if (floorAbove && floorBelow && cellRight && cellLeft)
				return true;

			
			return false;
		}

		private void GenerateEnvironment ()
		{
			
			for (int x = 0; x < GridSize.x; x++) {
				for (int y = 0; y < GridSize.y; y++) {

					var coord = new Vector2 (x, y);
					var node = Grid.GetNodeFromGridCoordinate (coord);
					var cell = ObjectManager.instance.GetObject (CellPrefab, Utilities.instance.GetNodePosition (node)); 

					var collider = GetCellCollider (cell);

					int sortingOrder = BackgroundSpriteSortingOrder;

					if (node.NodeState == NodeType.Background || node.NodeState == NodeType.Entry || node.NodeState == NodeType.Exit) {
						collider.enabled = true;
						collider.isTrigger = true;

						if (_backgroundLayer >= 0 && _backgroundLayer <= 31) {
							cell.layer = _backgroundLayer;
						}
					} else {

						DefineWallType (node);

						collider.enabled = true;
						collider.isTrigger = false;
						sortingOrder = WallSpriteSortingOrder;

						if (_wallLayer >= 0 && _wallLayer <= 31) {
							cell.layer = _wallLayer;
						}
					} 

					Utilities.instance.UpdateNodeSortingOrder (SCRIPT_NAME, cell, sortingOrder);
												
					UpdateNodeSprite (cell, node.NodeState);
					node.Position = cell.transform.position;
					node.Cell = cell;
					cell.transform.SetParent (ParentContainer.transform);

					cell.SetActive (true);

					nodes.Add (cell);

				
				}
			}

		}

		private Collider2D GetCellCollider (GameObject cell)
		{
			var collider2D = cell.GetComponent<Collider2D> ();

			if (!collider2D) {
				collider2D = cell.AddComponent<Collider2D> ();
			}

			return collider2D;
		}



		private void DefineWallType (Node node)
		{
			var coord = node.Coordinates;
			var floorBelow = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y - 1), NodeType.Background));
			var floorLeft = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x - 1, coord.y), NodeType.Background));
			var floorRight = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x + 1, coord.y), NodeType.Background));
			var floorAbove = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y + 1), NodeType.Background));

			if (!floorBelow && !floorLeft && floorAbove && floorRight) {
				node.NodeState = NodeType.WallTopRight;
			} else if (!floorBelow && !floorLeft && floorAbove && !floorRight) {
				node.NodeState = NodeType.WallTopMiddle;
			} else if (!floorBelow && floorLeft && floorAbove && !floorRight) {
				node.NodeState = NodeType.WallTopLeft;
			} else if (floorBelow && !floorLeft && !floorAbove && floorRight) {
				node.NodeState = NodeType.WallBottomRight;
			} else if (floorBelow && !floorLeft && !floorAbove && !floorRight) {
				node.NodeState = NodeType.WallBottomMiddle;
			} else if (floorBelow && floorLeft && !floorAbove && !floorRight) {
				node.NodeState = NodeType.WallBottomLeft;
			} else if (!floorBelow && !floorAbove && floorLeft && !floorRight) {
				node.NodeState = NodeType.WallMiddleLeft;
			} else if (!floorBelow && !floorAbove && !floorLeft && floorRight) {
				node.NodeState = NodeType.WallMiddleRight;
			} else {
				node.NodeState = NodeType.WallMiddle;
			}

		}

		private void DefineOutsideWallType (Node node)
		{
			var coord = node.Coordinates;
			var caveBelow = Grid.IsValidCoordinate (new Vector2 (coord.x, coord.y - 1));
			var caveAbove = Grid.IsValidCoordinate (new Vector2 (coord.x, coord.y + 1));
			var caveLeft = Grid.IsValidCoordinate (new Vector2 (coord.x - 1, coord.y));
			var caveRight = Grid.IsValidCoordinate (new Vector2 (coord.x + 1, coord.y));


			if (caveBelow && caveLeft && caveRight && !caveAbove) { // North 
				node.NodeState = NodeType.OutsideN;
			} else if (caveBelow && caveLeft && !caveAbove && !caveRight) { //North East
				node.NodeState = NodeType.WallTopRight;
			} else if (caveBelow && caveLeft && caveAbove && !caveRight) { // East
				node.NodeState = NodeType.WallMiddleRight;
			} else if (caveRight && caveLeft && caveAbove && !caveBelow) { // South
				node.NodeState = NodeType.OutsideS;
			} else if (caveLeft && caveAbove && !caveRight && !caveBelow) { //South East
				node.NodeState = NodeType.WallBottomRight;
			}
	
		}

		private bool IsOutsideWall (Node wallNode)
		{
			if (wallNode.Coordinates.x == 0) {
				return true;
			}

			if (wallNode.Coordinates.y == 0) {
				return true;
			}

			if (wallNode.Coordinates.x == GridSize.x - 1) {
				return true;
			}

			if (wallNode.Coordinates.y == GridSize.y - 1) {
				return true;
			}
			
			return false;
		} 


		private void UpdateNodeSprite (GameObject node, NodeType nodeType)
		{
			SpriteRenderer spriteRenderer = Utilities.instance.GetChildComponent<SpriteRenderer> (SCRIPT_NAME, node.transform); 

			if (spriteRenderer) {
				spriteRenderer.sprite = texturePacks [currentTexturePack].GetSpriteFromCellType (nodeType);  
			}
				
		}

		private bool IsEdge (Vector2 coordinate)
		{
			return ((int)coordinate.x == 0 ||
				(int)coordinate.x == (int)GridSize.x - 1 ||
				(int)coordinate.y == 0 ||
				(int)coordinate.y == (int)GridSize.y - 1);
		}


		public int CountTileWallNeighbours (Vector2 coord)
		{

			int wallCount = 0;

			int x = (int)coord.x;
			int y = (int)coord.y;

			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {

					if (i == 0 && j == 0) {
						continue;
					}
					
					Vector2 neighborCoordinate = new Vector2 (x + i, y + j);

					if ((!Grid.IsValidCoordinate (neighborCoordinate) || (Grid.GetNodeFromGridCoordinate (neighborCoordinate).NodeState != NodeType.Background))) {
						wallCount += 1;
					}					
				}
			}
			return wallCount;
		}

		private void PlaceEntranceAndExit ()
		{

			var entranceCell = GetRandomFloorNode ();

			if (entranceCell == null) {
				return;
			}
		
			Grid.GetNodeFromGridCoordinate (entranceCell.Coordinates).NodeState = NodeType.Entry;

			StartNode = entranceCell;

			Node exitCell = GetFloorNodeMinDistanceFromStartNode (minDistanceBetweenStartAndEnd, entranceCell);

			Grid.GetNodeFromGridCoordinate (exitCell.Coordinates).NodeState = NodeType.Exit;

			EndNode = exitCell;
		}

		private bool IsValidEntrance (Node node)
		{
			var coord = node.Coordinates;
			var floorAbove = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y + 1), NodeType.Background));
			var cellBelow = (Grid.ContainsNodeTypeAtPosition (new Vector2 (coord.x, coord.y - 1), NodeType.Wall));

			return floorAbove && cellBelow;
		}

		private Node GetMinDistanceNodeFromCluster (List<Node> cluster, float minDistance, Node startNode)
		{
			int mainClusterCount = cluster.Count - 1;

			float currentDistance = 0f;

			Node node = null;

			int count = 0;
			do {
				// Do not want an infinite loop, where you cannot find a node far enough away so we decrement the distance until a node is found.
				count++;
				if (count == 10) { 
					if (minDistance > 0.05f) {
						minDistance -= (minDistance * 0.1f);
					}

					count = 0;
				}

				node = cluster [(int)(Random.value * mainClusterCount)];		

				int a = (int)(node.Coordinates.x - startNode.Coordinates.x);
				int b = (int)(node.Coordinates.y - startNode.Coordinates.y);
				currentDistance = Mathf.Sqrt (a * a + b * b);

			} while (currentDistance < minDistance); 

			return node;
		}



		private Node GetMaxDistanceNodeFromCluster (List<Node> cluster, float maxDistance, Node startNode)
		{
			int mainClusterCount = cluster.Count - 1;
			
			float currentDistance = 0f;
			
			Node node = null;
			
			int count = 0;
			do {
				// Do not want an infinite loop, where you cannot find a node far enough away so we decrement the distance until a node is found.
				count++;
				if (count == 10) { 
					if (maxDistance < 40f) {
						maxDistance += (maxDistance * 0.1f);
					}
					
					count = 0;
				}

				node = cluster [(int)(Random.value * mainClusterCount)];		
				
				int a = (int)(node.Coordinates.x - startNode.Coordinates.x);
				int b = (int)(node.Coordinates.y - startNode.Coordinates.y);
				currentDistance = Mathf.Sqrt (a * a + b * b);
				
			} while (currentDistance > maxDistance); 
			
			return node;
		}

		public Node GetFloorNodeMaxDistanceFromStartNode (float maxDistance, Node startNode)
		{
			return GetMaxDistanceNodeFromCluster (emptyFloorNodes, maxDistance, startNode);
		}

		public Node GetFloorNodeMinDistanceFromStartNode (float minDistance, Node startNode)
		{
			return GetMinDistanceNodeFromCluster (emptyFloorNodes, minDistance, startNode);		
		}

		public Node GetRandomBackgroundNode ()
		{
			List<Node> cluster = nodeClusterManager.MainCluster.Nodes;
					
			return cluster [(int)(Random.value * cluster.Count - 1)];
			
		}

		public List<Node> GetBackgroundNodes ()
		{
			return nodeClusterManager.MainCluster.Nodes;
		}

		public Node GetRandomFloorNode ()
		{
			if (emptyFloorNodes.Count == 0)
				return null;

			return emptyFloorNodes [Random.Range (0, emptyFloorNodes.Count)];
		}

		private TexturePack[] GetTexturePacks ()
		{
			var texturePacks = GetComponents<TexturePack> ();
			
			
			if (texturePacks.Length == 0) {
				Debug.LogError ("No texture packs found");
				enabled = false;
			}
			
			return texturePacks;
			
		}


	}
}
