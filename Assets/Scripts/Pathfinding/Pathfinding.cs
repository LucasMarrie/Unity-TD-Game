using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public int pathHeight = 2;
    MapGenerator map;
    MapGrid grid;

    public void InnitPathfinder(){
        map = MapGenerator.map;
        grid = map.GetGrid();
        SetNodes();
    }

    void SetNodes(){
        PathNode.nodes = new Dictionary<Vector3Int, PathNode>();
        grid.LoopGrid(SetNode);
        foreach(PathNode node in PathNode.nodes.Values){
            node.SetNeighbours();
        }

    }

    void SetNode(int x, int y, int z){
        Vector3Int pos = new Vector3Int(x,y,z);
            GridInfo cell = grid.GetCell(pos);
            Vector3Int normal = Vector3Int.zero;
            if(ShapeData.shapeDict.ContainsKey(cell.shape)){
                normal =  Vector3Int.RoundToInt(cell.rotation * ShapeData.shapeDict[cell.shape].traversableNormal);
            }
            PathNode.TryAddNode(pos, normal, pathHeight);
    }

    //check Nodes within a radius of pathHeight of a recently updated Node
    void UpdateNode(Vector3Int changedNode){
        List<Vector3Int> updatedNodes = new List<Vector3Int>();
        for (int y = changedNode.y - pathHeight; y <= changedNode.y + pathHeight; y++){
            Vector3Int node = new Vector3Int(changedNode.x, y, changedNode.z);
            if(grid.InBounds(node)){
                if(PathNode.nodes.ContainsKey(node)){
                    PathNode.nodes[node].RevalidateNode(pathHeight);
                    if(PathNode.nodes.ContainsKey(node)) 
                        updatedNodes.Add(node);
                }else{
                    SetNode(node.x, node.y, node.z);
                    if(PathNode.nodes.ContainsKey(node)) 
                        updatedNodes.Add(node);
                }
            }
        }    
        foreach(Vector3Int node in updatedNodes){
            PathNode.nodes[node].SetNeighbours();
        } 
    }

    
}
