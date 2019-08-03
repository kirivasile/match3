using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTileType : TileType {
	public NormalTileType(Tile tileHolder) : base(tileHolder) {}

	public override void Deactivate() {
		tileHolder.TileSprite = null;
	}

	public override void Convert() {
		tileHolder.TileSprite = BoardManager.Instance.reverseSprite;
		tileHolder.Type = new ReverseTileType(tileHolder);
	}
}
