using UnityEngine;
using System.Collections;

namespace CaveGenerator
{
	/// <summary>
	/// Singleton. Provides a centralised location for block tetures. 
	/// Provides a method to retrieve a texture based on a node type.
	/// </summary>
	public class TexturePack : MonoBehaviour
	{
		public Sprite WallTopLeft;
		public Sprite WallTopMiddle;
		public Sprite WallTopRight;
		public Sprite WallMiddleLeft;
		public Sprite WallMiddle;
		public Sprite WallMiddleRight;
		public Sprite WallBottomLeft;
		public Sprite WallBottomMiddle;
		public Sprite WallBottomRight;

		public Sprite Background;

		public Sprite[] Details;


		/// <summary>
		/// Returns a texture based on a node type.
		/// </summary>
		public Sprite GetSpriteFromCellType (NodeType cellType)
		{

			Sprite sprite = null;

			switch (cellType) {
			case NodeType.WallTopLeft:
				sprite = WallTopLeft;
				break;
			case NodeType.WallTopMiddle:
				sprite = WallTopMiddle;
				break;
			case NodeType.WallTopRight:
				sprite = WallTopRight;
				break;
			case NodeType.WallMiddleLeft:
				sprite = WallMiddleLeft;
				break;
			case NodeType.WallMiddle:
				sprite = WallMiddle;
				break;
			case NodeType.WallMiddleRight:
				sprite = WallMiddleRight;
				break;
			case NodeType.WallBottomLeft:
				sprite = WallBottomLeft;
				break;
			case NodeType.WallBottomMiddle:
				sprite = WallBottomMiddle;
				break;
			case NodeType.WallBottomRight:
				sprite = WallBottomRight; 
				break; 
			case NodeType.Background:
				sprite = Background;
				break;
			case NodeType.Entry:
				sprite = WallMiddle;
				break;
			case NodeType.OutsideN:
				sprite = WallTopMiddle;
				break;
			case NodeType.OutsideNE:
				sprite = WallTopRight;
				break;
			case NodeType.OutsideE:
				sprite = WallMiddleRight;
				break;
			case NodeType.OutsideSE:
				sprite = WallBottomRight;
				break;
			case NodeType.OutsideS:
				sprite = WallBottomMiddle;
				break;
			default:
				sprite = WallMiddle;
				break;
			}

			if (!sprite) {
				Debug.LogError (cellType + " not set");
			}

			return sprite;

		}


		public Vector2 GetSpriteSize (NodeType cellType, Vector2 localScale)
		{
			Sprite sprite = GetSpriteFromCellType (cellType);

			return new Vector2 (sprite.bounds.size.x * localScale.x, sprite.bounds.size.y * localScale.y);
		}

	



	
	
	}
}
