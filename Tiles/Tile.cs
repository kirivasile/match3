using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	static Tile previousSelected = null;

	bool isSelected = false;

	public int X { get; private set; }
	public int Y { get; private set; }

	public TileType Type { get; set; }
	SpriteRenderer tileRenderer;

	public Sprite TileSprite {
		get {
			return tileRenderer.sprite;
		}
		set {
			if (tileRenderer.sprite == value) {
				return;
			}
			tileRenderer.sprite = value;
		}
	}

	void Awake () {
		tileRenderer = GetComponent<SpriteRenderer>();
	}

	public void Init(TileType type, Sprite sprite, int x, int y) {
		Type = type;
		TileSprite = sprite;
		X = x;
		Y = y;
	}

	public bool IsSpriteNull() {
		return TileSprite == null;
	}

	void OnMouseDown () {
		if (!BoardManager.Instance.IsBoardActive || IsSpriteNull()) {
			return;
		}
		if (isSelected) {
			DeSelect();
		} else {
			if (previousSelected == null) {
				Select();
			} else {
				List<Tile> adjacent = GetAllAdjacentTiles();
				if (adjacent.Contains(previousSelected)) {
					if (BoardManager.Instance.CheckSwapHasMatch(X, Y, previousSelected)) {
						SwapTile(previousSelected);
						BoardManager.Instance.ClearAllMatches(previousSelected);
						previousSelected.DeSelect();
						BoardManager.Instance.ClearAllMatches(this);
					}
				} else {
					previousSelected.DeSelect();
					Select();	
				}
			}
		}
	}

	void Select() {
		isSelected = true;
		tileRenderer.color = TileMetrics.selectedColorDiff;
		previousSelected = this;
	}

	void DeSelect() {
		isSelected = false;
		tileRenderer.color = Color.white;
		previousSelected = null;
	}

	public void SwapTile(Tile other) {
		Sprite otherSprite = other.TileSprite;
		other.TileSprite = TileSprite;
		this.TileSprite = otherSprite;

		TileType otherType = other.Type;
		other.Type = Type.ChangeParent(other);
		this.Type = otherType.ChangeParent(this);
	}

	Tile GetAdjacentTile(Vector2 direction) {
		Vector2 rayStart = new Vector2(
			transform.position.x + direction.x,
			transform.position.y + direction.y
		);

		RaycastHit2D hit = Physics2D.Raycast(rayStart, direction);
		if (hit.collider != null) {
			return hit.collider.gameObject.GetComponent<Tile>();
		}
		return null;
	}

	List<Tile> GetAllAdjacentTiles() {
		List<Tile> adjacentTiles = new List<Tile>();
		foreach (Vector2 direction in TileMetrics.adjacentDirections) {
			adjacentTiles.Add(GetAdjacentTile(direction));
		}
		return adjacentTiles;
	}

	public void Convert() {
		Type.Convert();
	}

	public bool Equals(Tile other) {
		return X == other.X && Y == other.Y;
	}
}
