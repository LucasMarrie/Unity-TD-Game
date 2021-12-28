using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : IComparable<PathNode>
{
    public static MapGrid grid;
    public static Dictionary<Vector3Int, PathNode> nodes;
    //BlockContents that can be walked on
    static HashSet<BlockContent> pathContent = new HashSet<BlockContent>{
        BlockContent.path,
    };
    //BlockContents that can be walked through
    static HashSet<BlockContent> passableContent = new HashSet<BlockContent>{
        BlockContent.empty,
    };

    public Vector3Int gridPos;
    public Vector3 worldPos;
    Vector3Int normal;
    bool flat;
    Dictionary<Vector3Int, int> adjacents; //the integer represents change in elevation
    public HashSet<PathNode> neighbours;
    int moveCost = 10;
    public int gCost = 0;
    public int hCost = 0;

    public PathNode(Vector3Int _gridPos, Vector3Int _normal){
        gridPos = _gridPos;
        normal = _normal;
        neighbours = new HashSet<PathNode>();
        if(normal == Vector3Int.zero || normal.y < -1){
            flat = true;
        }else{
            flat = false;
        }
        SetAdjacents();
        SetWorldPos();
    }

    void SetWorldPos(){
        if(flat){
            worldPos = grid.GridToWorld(gridPos, side : Vector3.up);
        }else{
            worldPos = grid.GridToWorld(gridPos);
        }
    }

    public Vector3 IntersectionPos(PathNode neighbour){
        float midx = (worldPos.x + neighbour.worldPos.x) / 2;
        float midz = (worldPos.z + neighbour.worldPos.z) / 2;
        float midy;
        if(flat){
            midy = worldPos.y;
        }else{
            //doable because direction is only in one axis
            int direction = normal.z + normal.x;
            int nbrPos = neighbour.gridPos.z + neighbour.gridPos.x;
            if(gridPos.x + gridPos.z + direction == nbrPos){
                midy = worldPos.y - grid.cellSize/2;
            }else{
                midy = worldPos.y + grid.cellSize/2;
            }
        }
        return new Vector3(midx, midy, midz);
    } 

    // for movement in direction : current -> neighbour
    public Quaternion RotationTowards(PathNode neighbour, bool useNeighbourRot){
        Quaternion vertical = useNeighbourRot ? neighbour.VerticalRotation() : VerticalRotation();
        Vector3 direction = (neighbour.gridPos - gridPos);
        direction.y = 0;
        Quaternion horizontal = Quaternion.LookRotation(direction);
        return vertical * horizontal;
    }

    public Quaternion VerticalRotation(){
        if(flat) return Quaternion.identity;
        float angle = Vector3.Angle(Vector3.up, normal) * -(normal.x + normal.z);
        Quaternion vertical = Quaternion.AngleAxis(angle, Vector3.forward);
        return vertical;
    }
    
    void SetAdjacents(){
        adjacents = new Dictionary<Vector3Int, int>();
        if(flat){
            adjacents.Add(gridPos + Vector3Int.forward, 0);
            adjacents.Add(gridPos + Vector3Int.back, 0);
            adjacents.Add(gridPos + Vector3Int.right, 0);
            adjacents.Add(gridPos + Vector3Int.left, 0);
            adjacents.Add(gridPos + Vector3Int.forward + Vector3Int.up, 1);
            adjacents.Add(gridPos + Vector3Int.back + Vector3Int.up, 1);
            adjacents.Add(gridPos + Vector3Int.right + Vector3Int.up, 1);
            adjacents.Add(gridPos + Vector3Int.left + Vector3Int.up, 1);
        }else{
            Vector3Int direction = Vector3Int.forward * normal.z + Vector3Int.right * normal.x;
            adjacents.Add(gridPos + direction, -1);
            adjacents.Add(gridPos - direction, 0);
            adjacents.Add(gridPos + direction - Vector3Int.up, -1);
            adjacents.Add(gridPos - direction + Vector3Int.up, 1);
        }
    }

    public void SetNeighbours(){
        neighbours = new HashSet<PathNode>();
        foreach(Vector3Int adj in adjacents.Keys){
            if(grid.InBounds(adj)){
                if(nodes.ContainsKey(adj) && nodes[adj].adjacents.ContainsKey(gridPos)){
                    PathNode otherNode = nodes[adj];
                    if(neighbours.Contains(otherNode)) continue;
                    bool sameY = otherNode.gridPos.y == gridPos.y;
                    if((sameY && adjacents[adj] == otherNode.adjacents[gridPos])
                    || (!sameY && adjacents[adj] + otherNode.adjacents[gridPos] == 0)){
                        neighbours.Add(otherNode);
                        otherNode.neighbours.Add(this);
                    }
                }
            }
        }
    }

    public void RevalidateNode(int height){
        if(!ValidNode(gridPos, normal, height)){
            nodes.Remove(gridPos);
            foreach(PathNode neighbour in neighbours){
                neighbour.neighbours.Remove(this);
            }
        }
    }

    public static void TryAddNode(Vector3Int pos, Vector3Int normal, int height){
        if(ValidNode(pos, normal, height)){
            PathNode node = new PathNode(pos, normal);
            nodes.Add(pos, node);
        }
    }

    static bool ValidNode(Vector3Int pos, Vector3Int normal, int height){
        GridInfo cell = grid.GetCell(pos);
        if(!pathContent.Contains(cell.blockData.content)){
            return false;
        }
        if(normal != Vector3Int.zero && normal.y == 0){
            return false;
        }
        for(int i = 1; i <= height; i++){
            Vector3Int tempPos = pos + Vector3Int.up * i;
            GridInfo tempCell = grid.GetCell(tempPos);
            if(!grid.InBounds(tempPos)){
                return false;
            }else if(!passableContent.Contains(tempCell.blockData.content)){
                if(normal == Vector3Int.zero || ShapeData.shapeDict[tempCell.shape].traversableNormal != -normal){
                    return false;
                }
            }
        }
        return true;
    }

    public void SetHCost(Vector3Int goal){
        Vector3Int diff = goal - gridPos;
        hCost = Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z);
    }

    public bool UpdateGCost(int prevG){
        int newGCost = prevG + moveCost;
        if(newGCost < gCost){
            gCost = newGCost;
            return true;
        }
        return false;
    }

    public void SetGCost(int prevG){
        gCost = prevG + moveCost;
    }

    public int FCost(){
        return gCost + hCost;
    }

    public void ResetCost(){
        gCost = 0;
        hCost = 0;
    }

    public static void ResetCosts(){
        foreach(PathNode node in nodes.Values){
            node.ResetCost();
        }
    }

    public int CompareTo(PathNode node){
        int compare = FCost().CompareTo(node.FCost());
        if(compare == 0){
            compare = hCost.CompareTo(node.hCost);
        }
        return -compare;
    }

    
}
