using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMetrics {
	public const float tileSize = 1f;
	public static Color selectedColorDiff = new Color(0.2f, 0.2f, 0.2f, 1f);

	// TODO:
	public static Vector2[] adjacentDirections = new Vector2[]{ Vector2.up, Vector2.down, Vector2.left, Vector2.right };
}
