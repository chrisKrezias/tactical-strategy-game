using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NetTilePathFinder : MonoBehaviour {
	public static List<NetTile> FindPath(NetTile originTile, NetTile destinationTile){
		return FindPath(originTile, destinationTile, new Vector2[0]);
	}
	public static List<NetTile> FindPath(NetTile originTile, NetTile destinationTile, Vector2[] occupied){
		List<NetTile> closed = new List<NetTile> ();
		List<NetTilePath> open = new List<NetTilePath> ();
		NetTilePath originPath = new NetTilePath ();
		originPath.addTile (originTile);
		open.Add (originPath);
		while (open.Count > 0) {
			NetTilePath current = open[0];
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
			foreach (NetTile t in current.lastTile.neighbors) {
				if (t.impassable || occupied.Contains (t.gridPosition))
					continue;
				NetTilePath newTilePath = new NetTilePath(current);
				newTilePath.addTile (t);
				open.Add (newTilePath);
			}
		}
		return null;
	}
}
