using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TilePathFinder : MonoBehaviour {
	public static List<Tile> FindPath(Tile originTile, Tile destinationTile){
		return FindPath(originTile, destinationTile, new Vector2[0],false);
	}
	public static List<Tile> FindPath(Tile originTile, Tile destinationTile, Vector2[] occupied, bool door){
		List<Tile> closed = new List<Tile> ();
		List<TilePath> open = new List<TilePath> ();
		TilePath originPath = new TilePath ();
		originPath.addTile (originTile);
		open.Add (originPath);
		while (open.Count > 0) {
			TilePath current = open[0];
			open.Remove (open [0]);
			if (closed.Contains (current.lastTile)) {
				continue;
			}
			if (current.lastTile == destinationTile) {
				current.listOfTiles.Distinct();
				current.listOfTiles.Remove (originTile);
				return current.listOfTiles;
			}
			closed.Add (current.lastTile);
			foreach (Tile t in current.lastTile.neighbors) {
				TilePath newTilePath = new TilePath (current);
				if (door) {
					newTilePath.addTile (t);
					open.Add (newTilePath);
				} else if (t.impassable || occupied.Contains (t.gridPosition))
					continue;
				else {
					newTilePath.addTile (t);
					open.Add (newTilePath);
				}
			}

		}
		return null;
	}
}
