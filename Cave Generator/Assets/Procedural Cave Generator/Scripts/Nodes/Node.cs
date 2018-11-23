using UnityEngine;
using System;
using System.Collections;

namespace CaveGenerator
{

	public enum NodeType
	{
		Invalid = -1,
		Wall,
		WallTopLeft,
		WallTopMiddle,
		WallTopRight,
		WallMiddleLeft,
		WallMiddle,
		WallMiddleRight,
		WallBottomLeft,
		WallBottomMiddle,
		WallBottomRight,
		Background,
		Entry,
		Exit,
		OutsideN,
		OutsideNE,
		OutsideE,
		OutsideSE,
		OutsideS,
		OutsideSW,
		OutsideW,
		OutsideNW,
		Max}
	;
	
	/// <summary>
	/// Logical representation of a block. Holds the node type (e.g. wall, floor etc),
	/// it's coordinates (a pointer to the node in a 2d array - not it's position on screen),
	/// it's position on screen and path finding variables.
	/// </summary>
	[System.Serializable]
	public class Node : IComparable
	{

		[SerializeField]
		private NodeType
			nodeState;

		public NodeType NodeState {
			get {
				return nodeState;
			}
			set {
				nodeState = value;
			}
		}

		[SerializeField]
		private Vector2
			coordinates;

		public Vector2 Coordinates {
			get {
				return coordinates;
			}
			set {
				coordinates = value;
			}
		}


		public Vector2? Position { get; set; }
		
		public GameObject Cell { get; set; }
				
		// The cost to move into this node.
		public float GScore { get; set; }
				
		// Estimated cost to mvoe from this node to end node.
		public float HScore { get; set; }
				
		// Used when traversing a path.
		public Node Parent { get; set; }

		public bool IsObstacle { get { return nodeState == NodeType.Wall; } }

		public Node () : this (Vector2.zero)
		{
		}
		
		public Node (Vector2 coordinates) : this (coordinates, NodeType.Invalid)
		{
		}


		public Node (Vector2 coordinates, NodeType state)
		{
			Coordinates = coordinates;
			NodeState = state;
			
			HScore = 0f;
			GScore = 1f;
			Parent = null;
			Position = null;
		}

		/// <summary>
		/// Total score. Returns GScore + HScore
		/// </summary>
		public float GetFScore ()
		{
			return GScore + HScore;
		}


		public int CompareTo (object obj)
		{

			Node other = (Node)obj;

			if (this.HScore < other.HScore)
				return -1;

			if (this.HScore > other.HScore)
				return 1;

			return 0;
	
		}

	}
}
