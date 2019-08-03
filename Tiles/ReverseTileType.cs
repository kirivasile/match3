using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseTileType : TileType {
	public ReverseTileType(Tile tileHolder) : base(tileHolder) {}

	public override void Deactivate() {
		tileHolder.TileSprite = null;
		BoardManager.Instance.SwitchGravity();
	}

	public override void Convert() {
		List<Sprite> possibleSprites = BoardManager.Instance.GetPossibleSprites(tileHolder.X, tileHolder.Y);
		tileHolder.TileSprite = possibleSprites[Random.Range(1, possibleSprites.Count)];
		tileHolder.Type = new NormalTileType(tileHolder);
	}
}
