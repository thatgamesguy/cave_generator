using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGenerator
{
	/// <summary>
	/// Singleton. Identifies, manages and holds refence to the different node clusters. 
	/// Can connect un-connected clusters using path finding.
	/// </summary>
	[RequireComponent (typeof(PathManager))]
	public class NodeClusterManager : MonoBehaviour
	{

		private static string SCRIPT_NAME = typeof(NodeClusterManager).Name;


		public List<NodeCluster> Clusters { get; set; }

		private NodeCluster mainCluster;
		private int? mainClusterIndex;


		private PathManager pathManager;

		void Awake ()
		{
			pathManager = GetComponent<PathManager> ();
		}

		/// <summary>
		/// Gets the main cluster. The main cluster contains the largest number of nodes.
		/// </summary>
		/// <value>The main cluster.</value>
		public NodeCluster MainCluster {
			get {
				if (!mainClusterIndex.HasValue) {
					mainClusterIndex = CalculateMainCluster ();
				}
				return Clusters [mainClusterIndex.Value];

			}
		}

		/// <summary>
		/// Identifies the clusters. Uses a flood-fill algorithm to identifty neighbouring floor tiles.
		/// </summary>
		/// <param name="nodes">A list of all active nodes.</param>
		/// <param name="size">The size of the level.</param>
		public void IdentifyClusters (NodeList nodes, Vector2 size)
		{
			Clusters = new List<NodeCluster> ();
			
			Node[,] floodFillArray = new Node[(int)size.x, (int)size.y];
			
			// Create a copy of all nodes as the nodes are altered in the identification process.
			for (int x = 0; x < size.x; x++) {
				for (int y = 0; y < size.y; y++) {
					Vector2 coord = new Vector2 (x, y);

					if (GridManager.instance.Grid.IsValidCoordinate (coord)) {
						Node cellToCopy = nodes.GetNodeFromGridCoordinate (coord);
						floodFillArray [x, y] = new Node (cellToCopy.Coordinates, cellToCopy.NodeState);
					}
					
				}
			} 
			
			for (int x = 0; x < size.x; x++) {
				for (int y = 0; y < size.y; y++) {
					if (floodFillArray [x, y].NodeState == NodeType.Background) {
						Clusters.Add (new NodeCluster ());
						FloodFillCluster (floodFillArray, new Vector2 (x, y), size);
					}
				}
			}

			if (Utilities.instance.IsDebug)
				Debug.Log ("Number of clusters: " + Clusters.Count);
			
		}
				
		/// <summary>
		/// Converts the type of the disconnected nodes. Can be used to convert all disconnect nodes to
		/// walls.
		/// </summary>
		/// <param name="nodeType">The type of node to convert to.</param>
		public void ConvertDisconnectedClustersToNodeType (NodeType nodeType)
		{
			
			int mainClusterIndex = CalculateMainCluster ();
			
			int clustersCount = Clusters.Count;
			
			if (clustersCount > 0) {
				
				for (int i = 0; i < clustersCount; i++) {
					
					if (i != mainClusterIndex) {
						List<Node> cells = Clusters [i].Nodes;
						
						foreach (var node in cells) {
							node.NodeState = nodeType;
						}
						
					}
				}
				
			}
		}

		/// <summary>
		/// Uses A* algorithm to find a path from disconnected clusters to main cluster and 
		/// convert nodes on that path to floors.
		/// </summary>
		public void ConnectClusters ()
		{
			int mainClusterIndex = CalculateMainCluster ();
			
			NodeCluster mainCluster = Clusters [mainClusterIndex];
			
			for (int clusterIndex = 0; clusterIndex < Clusters.Count; clusterIndex++) {
				if (clusterIndex != mainClusterIndex) {
					
					NodeCluster origCluster = Clusters [clusterIndex];
					
					Node origCell = origCluster.Nodes [(int)((origCluster.Nodes.Count - 1) * Random.value)];
					
					Node destCell = mainCluster.Nodes [(int)((mainCluster.Nodes.Count - 1) * Random.value)];

					List<Node> path = pathManager.GetShortestPath (origCell, destCell, 1f, true);

				
					if (path == null || path.Count == 0) {
						if (Utilities.instance.IsDebug)
							Debug.Log (SCRIPT_NAME + ": no path found"); 
					} else {
						ConstructPath (path, NodeType.Background);
					}
				}
			}
		}

		/// <summary>
		/// Iterats through each cluster and returns the index of the cluster with the largest size.
		/// </summary>
		/// <returns>The main cluster index.</returns>
		public int CalculateMainCluster ()
		{
			int mainClusterIndex = -1;
			int maxClusterSize = 0;

			for (int i = 0; i < Clusters.Count; i++) {
				
				NodeCluster cluster = Clusters [i];
				
				int cellCount = cluster.Nodes.Count;
				
				if (cellCount > maxClusterSize) {
					maxClusterSize = cellCount;
					mainClusterIndex = i;
				}
			}
			
			return mainClusterIndex;
			
		}

		/// <summary>
		/// Recursive flood fill. Adds all connected floor nodes to a cluster.
		/// </summary>
		/// <param name="cells">2D array of nodes.</param>
		/// <param name="coordinate">Coordinate of current node.</param>
		/// <param name="gridSize">Grid size.</param>
		private void FloodFillCluster (Node[,] cells, Vector2 coordinate, Vector2 gridSize)
		{
			
			Node node = cells [(int)coordinate.x, (int)coordinate.y];
			
			// Only floor types should be considered.
			if (node.NodeState != NodeType.Background)
				return;
			
			// Alter node state so it is not added again.
			node.NodeState = NodeType.Max;
			
			Clusters [Clusters.Count - 1].Nodes.Add (node);
			
			
			if (coordinate.x > 0) {
				FloodFillCluster (cells, new Vector2 (coordinate.x - 1, coordinate.y), gridSize);
			}
			
			if (coordinate.x < gridSize.x - 1) {
				FloodFillCluster (cells, new Vector2 (coordinate.x + 1, coordinate.y), gridSize);	
			}
			
			if (coordinate.y > 0) {
				FloodFillCluster (cells, new Vector2 (coordinate.x, coordinate.y - 1), gridSize);
			}
			
			if (coordinate.y < gridSize.y - 1) {
				FloodFillCluster (cells, new Vector2 (coordinate.x, coordinate.y + 1), gridSize);
			}
			
		}

		/// <summary>
		/// Converts node type of nodes in list to a specific type.
		/// </summary>
		/// <param name="path">List of nodes to convert.</param>
		/// <param name="pathType">The type to convert nodes to.</param>
		private void ConstructPath (List<Node> path, NodeType pathType)
		{
			foreach (var node in path) {
				node.NodeState = pathType;
			}
	
		}

	

	}


}
