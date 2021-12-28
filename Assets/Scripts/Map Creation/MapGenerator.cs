using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class MapGenerator : MonoBehaviour
{
    //map loading data
    public static string mapName = "";
    public static bool customMap = false;

    public BlockList blockList;
    public Vector3Int gridSize = new Vector3Int(30, 10, 30);
    public float cellSize = 1f;
    public Transform floor;

    //singleton
    public static MapGenerator map;

    MapGrid grid;
    Pathfinding pathfinder;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    [Header("Pathfinding test")]
    public Vector3Int start;
    public Vector3Int end;
    public GameObject waypoint;

    void Awake()
    {
        map = this; //set singleton

        BlockList.blockDataList = blockList.blocks;
        GridData.InitBlockData();

        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        if(File.Exists((customMap ? GridData.customMapsPath : GridData.mapdataPath) + "/" + mapName)){
            grid = GridData.LoadGrid(mapName, false);
            transform.position = grid.worldPos;
            gridSize = grid.gridSize;
            cellSize = grid.cellSize;
        }else{
            grid = new MapGrid(transform.position, gridSize, cellSize);
        }
        if(floor != null){
            floor.position = transform.position + Vector3.down * (gridSize.y + 1)/2 * cellSize;
            floor.localScale = new Vector3(gridSize.x, 1, gridSize.z) * cellSize;
        }
        UpdateMesh();

        pathfinder = GetComponent<Pathfinding>();
        pathfinder.InnitPathfinder();
    }   

    void UpdateMesh(){
        Material[] materials;
        mesh = MeshGenerator.CreateMesh(grid, out materials);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.materials = materials;
    }

    public void AddBlock(Vector3Int cell, BlockData blockData, Quaternion rotation, Shape shape){
        GridInfo gridInfo = new GridInfo(blockData, rotation, shape);
        grid.SetCell(cell, gridInfo);
        UpdateMesh();
    }

    public void DeleteBlock(Vector3Int cell){
        grid.cells[cell.x, cell.y, cell.z] = GridInfo.Empty;
        UpdateMesh();
    }

    public MapGrid GetGrid(){
        return grid;
    }

    public void SetGrid(MapGrid _grid){
        grid = _grid;
        UpdateMesh();
    }

    public void TestPathfinder(){
        pathfinder.InnitPathfinder();
        var path = pathfinder.FindPath(start, end);
        if(path == null){
            Debug.Log("no path");
        }else{
            while(path.Count > 0){
                var coord = path.Pop();
                Instantiate(waypoint, coord.Item1, coord.Item2);
                Debug.Log(coord.Item1 + "  |  " + coord.Item2);
            }
        }
    }

    void OnDrawGizmos(){
        if(grid != null){
            Gizmos.DrawCube(grid.GridToWorld(start), Vector3.one * cellSize);
            Gizmos.DrawCube(grid.GridToWorld(end), Vector3.one * cellSize);
        }
    }
}
