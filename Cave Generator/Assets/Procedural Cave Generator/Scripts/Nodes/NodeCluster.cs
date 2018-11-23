using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGenerator
{

	/// <summary>
	/// Represents a group of neighbouring floor nodes i.e. a cavern in the environment.
	/// </summary>
	public class NodeCluster
	{
				
		private List<Node> nodes;
		public List<Node> Nodes {
			get {
				return nodes;
			}
			set {
				nodes = value;
			}
		}

		public NodeCluster ()
		{
			nodes = new List<Node> ();
						
		}
	}
}
