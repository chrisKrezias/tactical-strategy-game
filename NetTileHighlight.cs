using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class NetTileHighlight {



	public NetTileHighlight(){

	}

	public static List<NetTile> FindHighlight(NetTile originTile, int movementPoints){
		return FindHighlight (originTile, movementPoints, new Vector2[0],false);
	}

	public static List<NetTile> FindHighlight(NetTile originTile, int movementPoints, bool staticRange) {
		return FindHighlight(originTile, movementPoints, new Vector2[0], staticRange);
	}

	public static List<NetTile> FindHighlight(NetTile originTile, int movementPoints, Vector2[] occupied) {
		return FindHighlight(originTile, movementPoints, occupied, false);
	}

	public static List<NetTile> FindHighlight(NetTile originTile, int movementPoints, Vector2[] occupied, bool staticRange){
		List<NetTile> closed = new List<NetTile> ();
		List<NetTilePath> open = new List<NetTilePath> ();
		NetTilePath originPath = new NetTilePath ();
		if (staticRange)
			originPath.addStaticTile (originTile);
		else originPath.addTile (originTile);
		open.Add (originPath);
		while (open.Count > 0) {
			NetTilePath current = open[0];
			open.Remove (open [0]);
			if (closed.Contains (current.lastTile)) {
				continue;
			}
			if (current.costOfPath > movementPoints+1) {
				continue;
			}
			closed.Add (current.lastTile);
			foreach (NetTile t in current.lastTile.neighbors) {
				if (t.impassable || occupied.Contains (t.gridPosition))
					continue;
				NetTilePath newTilePath = new NetTilePath(current);
				if (staticRange)
					newTilePath.addStaticTile (t);
				else newTilePath.addTile (t);
				open.Add (newTilePath);
			}
		}
		closed.Remove (originTile);
		closed.Distinct();
		return closed;
	}
}