using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : Singleton<BoardManager> {
	Tile[,] tiles;

	public Tile tilePrefab;
	public int xSize, ySize;

	public bool IsBoardActive {get; set;}

	public Sprite reverseSprite;
	public Sprite[] normalSprites;

	void Start() {
		CreateTiles();
		IsBoardActive = true;
	}
	
	void CreateTiles() {
		tiles = new Tile[xSize, ySize];

		for (int x = 0; x < xSize; ++x) {
			for (int y = 0; y < ySize; ++y) {
				CreateTile(x, y);
			}
		}
	}


	public void CreateTile(int x, int y, bool reversed=false) {
		float tileX = transform.position.x + TileMetrics.tileSize * x;
		float tileY = transform.position.y + TileMetrics.tileSize * y;

		Tile tile = Instantiate<Tile>(
			tilePrefab, 
			new Vector3(tileX, tileY, 0f),
			tilePrefab.transform.rotation
		);
		tile.transform.parent = transform;

		List<Sprite> possibleSprites = GetPossibleSprites(x, y);

		TileType type;
		Sprite sprite;
		if (reversed) {
			sprite = reverseSprite;
			type = new ReverseTileType(tile);
		} else {
			sprite = possibleSprites[Random.Range(0, possibleSprites.Count)];
			type = new NormalTileType(tile);
		}

		tile.Init(type, sprite, x, y);

		tiles[x, y] = tile;
	}

	public List<Sprite> GetPossibleSprites(int x, int y) {
		Tile left = x > 0 ? tiles[x - 1, y] : null;
		Tile below = y > 0 ? tiles[x, y - 1] : null;

		List<Sprite> possibleSprites = new List<Sprite>(normalSprites);

		if (left != null) {
			possibleSprites.Remove(left.TileSprite);
		}
		if (below != null) {
			possibleSprites.Remove(below.TileSprite);
		}

		return possibleSprites;
	}

	public List<Tile> FindMatch(int x, int y, Sprite sprite, Vector2 direction) {
		Tile start = tiles[x, y];
		List<Tile> matches = new List<Tile>();

		Vector2 rayStart = new Vector2(
			start.transform.position.x + direction.x,
			start.transform.position.y + direction.y
		);
		RaycastHit2D hit = Physics2D.Raycast(rayStart, direction);
		while (hit.collider != null) {
			Tile tile =  hit.collider.gameObject.GetComponent<Tile>();

			if (tile != null && sprite == tile.TileSprite) {
				matches.Add(tile);

				rayStart = new Vector2(
					tile.transform.position.x + direction.x,
					tile.transform.position.y + direction.y
				);

				hit = Physics2D.Raycast(rayStart, direction);
			} else {
				return matches;
			}
		}
		return matches;
	}

	List<Tile> CheckMatchesInDirection(int x, int y, Sprite sprite, Vector2[] directions) {
		List<Tile> matches = new List<Tile>();
		foreach (Vector2 direction in directions) {
			matches.AddRange(FindMatch(x, y, sprite, direction));
		}
		return matches;
	}

	public bool CheckSwapHasMatch(int x, int y, Sprite sprite) {
		List<Tile> matches = CheckMatchesInDirection(x, y, sprite, new Vector2[]{Vector2.left, Vector2.right});
		if (matches.Count >= 2) {
			return true;
		}

		matches = CheckMatchesInDirection(x, y, sprite, new Vector2[]{Vector2.up, Vector2.down});
		if (matches.Count >= 2) {
			return true;
		}
		return false;
	}

	bool matchFound = false;

	void ClearMatchesInDirections(Tile start, Vector2[] directions) {
		List<Tile> matches = CheckMatchesInDirection(start.X, start.Y, start.TileSprite, directions);
		if (matches.Count >= 2) {
			foreach (Tile tile in matches) {
				tile.Type.Deactivate();
			}
			matchFound = true;
		}
		if (matches.Count > 2) {
			AddTileToReverse(matches[0]);
		}
	}

	public void ClearAllMatches(Tile start) {
		if (start.IsSpriteNull()) {
			return;
		}

		ClearMatchesInDirections(start, new Vector2[]{Vector2.left, Vector2.right});
		ClearMatchesInDirections(start, new Vector2[]{Vector2.up, Vector2.down});

		if (matchFound) {
			start.Type.Deactivate();
			matchFound = false;
			
			StopCoroutine(FindNullTiles());
			StartCoroutine(FindNullTiles());
		}
	}

	List<Tile> tilesToReverse = new List<Tile>();

	public void AddTileToReverse(Tile tile) {
		tilesToReverse.Add(tile);
	}

	public IEnumerator FindNullTiles() {
	    for (int x = 0; x < xSize; ++x) {

	    	if (gravityDown) {
	    		for (int y = 0; y < ySize; ++y) {
	    			if (tiles[x, y].IsSpriteNull()) {
		                yield return StartCoroutine(ShiftTiles(x, y));
		                break;
		            }
		       	}	
	    	} 
	    	else {
	    		for (int y = ySize - 1; y > -1; --y) {
	            	if (tiles[x, y].IsSpriteNull()) {
		                yield return StartCoroutine(ShiftTiles(x, y));
		                break;
		            }
		        }
	    	}
	    }

	    for (int x = 0; x < xSize; ++x) {
	    	if (gravityDown) {
	    		for (int y = 0; y < ySize; ++y) {
					ClearAllMatches(tiles[x, y]);
				}	
			}
			else {
		    	for (int y = ySize - 1; y > -1; --y) {
					ClearAllMatches(tiles[x, y]);
				}
			}
		}

		foreach (Tile tile in tilesToReverse) {
			tile.Convert();
		}
		tilesToReverse.Clear();
	}

	Sprite GenerateNewSprite(int x, int y) {
		List<Sprite> possibleSprites = new List<Sprite>(normalSprites);

		if (x > 0) {
			possibleSprites.Remove(tiles[x - 1, y].TileSprite);
		}
		if (x < xSize - 1) {
			possibleSprites.Remove(tiles[x + 1, y].TileSprite);
		}
		if (gravityDown && y > 0) {
			possibleSprites.Remove(tiles[x, y - 1].TileSprite);
		}
		if (!gravityDown && y < ySize - 1) {
			possibleSprites.Remove(tiles[x, y + 1].TileSprite);	
		}
		return possibleSprites[Random.Range(0, possibleSprites.Count)];
	}

	IEnumerator ShiftTiles(int x, int yStart, float delay=0.05f) {
		IsBoardActive = false;
		List<Tile> tilesToShift = new List<Tile>();
		int shift = 0;

		if (gravityDown) {
			for (int y = yStart; y < ySize; ++y) {
				if (tiles[x, y].IsSpriteNull()) {
					shift++;
				}
				tilesToShift.Add(tiles[x, y]);
			}
		} else {
			for (int y = yStart; y > -1; --y) {
				if (tiles[x, y].IsSpriteNull()) {
					shift++;
				}
				tilesToShift.Add(tiles[x, y]);
			}
		}

		for (int i = 0; i < shift; ++i) {
			yield return new WaitForSeconds(delay);
			for (int k = 0; k < tilesToShift.Count - 1; ++k) {
				tilesToShift[k].Type = tilesToShift[k + 1].Type.ChangeParent(tilesToShift[k]);
				tilesToShift[k].TileSprite = tilesToShift[k + 1].TileSprite;
			}

			int last = tilesToShift.Count - 1;
			if (tilesToShift[last].Y == (gravityDown ? ySize - 1 : 0)) {
				tilesToShift[last].Type = new NormalTileType(tilesToShift[last]);
				tilesToShift[last].TileSprite = GenerateNewSprite(x, gravityDown ? ySize - 1 : 0);	
			} 
			else {
				Tile tileAbove;
				if (gravityDown) {
					tileAbove = tiles[tilesToShift[last].X, tilesToShift[last].Y + 1];
				} else {
					tileAbove = tiles[tilesToShift[last].X, tilesToShift[last].Y - 1];
				}

				tilesToShift[last].Type = tileAbove.Type.ChangeParent(tilesToShift[last]);
				tilesToShift[last].TileSprite = tileAbove.TileSprite;				
			}
		}
		IsBoardActive = true;
	}

	bool gravityDown = true;

	public void SwitchGravity() {
		gravityDown = !gravityDown;
	}
}
