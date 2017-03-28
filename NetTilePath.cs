using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class NetTilePath{
	public List<NetTile> listOfTiles=new List<NetTile>();
	public int costOfPath=0;
	public NetTile lastTile;
	public NetTilePath(){}
	public NetTilePath(NetTilePath tp){
		listOfTiles = tp.listOfTiles.ToList();
		costOfPath = tp.costOfPath;
		lastTile = tp.lastTile;
	}
	public void addTile(NetTile t){
		costOfPath += t.movementCost;
		listOfTiles.Add (t);
		lastTile = t;
	}
	public void addStaticTile(NetTile t){
		costOfPath += 1;
		listOfTiles.Add (t);
		lastTile = t;
	}


}