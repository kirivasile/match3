using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileType {
	protected Tile tileHolder;

	public TileType(Tile tileHolder) {
		this.tileHolder = tileHolder;
	}

	public TileType ChangeParent(Tile tileHolder) {
		this.tileHolder = tileHolder;
		return this;
	}

	public abstract void Deactivate();

	public abstract void Convert();

}
