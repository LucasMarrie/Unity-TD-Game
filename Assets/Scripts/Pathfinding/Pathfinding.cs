using System;
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
        PathNode.grid = grid;
        SetNodes();
        PathNode.BakeGoalHCosts();
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
            if(ShapeData.shapeDict.ContainsKey(cell.shape) && ShapeData.shapeDict[cell.shape].traversableNormals != null){

                normal = Vector3Int.RoundToInt(cell.rotation * ShapeData.shapeDict[cell.shape].traversableNormals[0]);
            }
            PathNode.TryAddNode(pos, normal, pathHeight);
    }

    //check Nodes within a radius of pathHeight of a recently updated Node
    //probably not going to be used
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

    //if no path was found it returns empty Stack
    public Stack<MovementNode> FindPath(Vector3Int start, bool findGoalNode = true, Vector3Int goal = default){
        Stack<MovementNode> path = null;
        if(!PathNode.nodes.ContainsKey(start) || (findGoalNode && PathNode.goals.Count == 0) 
        || (!findGoalNode && !PathNode.nodes.ContainsKey(goal))){
            return path;
        }
        Dictionary<PathNode, PathNode> parents = new Dictionary<PathNode, PathNode>();
        Heap<PathNode> openSet = new Heap<PathNode>(PathNode.nodes.Count);
        openSet.Add(PathNode.nodes[start]);
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        while(openSet.Count > 0){
            PathNode node = openSet.Pop();
            closedSet.Add(node);

            if((!findGoalNode && node.gridPos == goal) || (findGoalNode && PathNode.goals.Contains(node.gridPos))){
                path = RetraceParents(start, node, parents);
                break;
            }

            foreach(PathNode neighbour in node.neighbours){
                if(closedSet.Contains(neighbour)) continue;
                if(openSet.Contains(neighbour)){
                    if(neighbour.UpdateGCost(node.gCost)){
                        openSet.UpdateItem(neighbour);
                        parents[neighbour] = node;
                    }
                }else{
                    neighbour.SetGCost(node.gCost);
                    if(findGoalNode){
                        neighbour.SetGoalHCost();
                    }else{
                        neighbour.SetHCost(goal);
                    }
                    openSet.Add(neighbour);
                    parents.Add(neighbour, node);
                }
            }
        }
        PathNode.ResetCosts();
        return path;
    }

    //returns in world movement waypoints and rotation to face once it reaches the node including the points in between each nodes
    Stack<MovementNode> RetraceParents(Vector3Int start, PathNode endNode, Dictionary<PathNode, PathNode> parents){
        Stack<MovementNode> path = new Stack<MovementNode>();
        PathNode current = endNode;
        path.Push(new MovementNode(current.worldPos, Quaternion.identity, current.gridPos));
        while (current.gridPos != start)
        {
            PathNode next = parents[current];
            path.Push(new MovementNode(next.IntersectionPos(current), next.RotationTowards(current, true), null));
            path.Push(new MovementNode(next.worldPos, next.RotationTowards(current, false), next.gridPos));
            current = next;    
        }
        return path;
    }

    Tuple<Vector3, Quaternion> PosRotPair(Vector3 pos, Quaternion rot){
        return new Tuple<Vector3, Quaternion>(pos, rot);
    }

}

public struct MovementNode{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3Int? gridPos;

    public MovementNode(Vector3 position, Quaternion rotation, Vector3Int? gridPos){
        this.position = position;
        this.rotation = rotation;
        this.gridPos = gridPos;
    }
}
