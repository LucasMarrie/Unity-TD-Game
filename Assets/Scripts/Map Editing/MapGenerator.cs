using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class MapGenerator : MonoBehaviour
{
    //map loading data
    public static string mapName; //= "Mabamba.json";
    public static bool customMap = false;
    public string selectedMap;

    public BlockList blockList;
    public Vector3Int gridSize = new Vector3Int(30, 10, 30);
    public float cellSize = 1f;
    public Transform floor;

    public Pathfinding pathfinder;

    //singleton
    public static MapGenerator map;

    MapGrid grid;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    [Header("Pathfinding test")]
    public GameObject pathVisualizer;

    void Awake()
    {
        if(mapName == null){
            mapName = selectedMap;
        }
        map = this; //set singleton

        BlockList.blockList = blockList.blocks;
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

        pathfinder.InnitPathfinder();
    }   

    void UpdateMesh(){
        Material[] materials;
        mesh = MeshGenerator.CreateMesh(grid, out materials);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.materials = materials;
    }

    public void AddBlocks(List<Vector3Int> cells, BlockData blockData, Quaternion rotation, Shape shape){
        GridInfo gridInfo = new GridInfo(blockData, rotation, shape);
        for (int i = 0; i < cells.Count; i++)
        {
            grid.SetCell(cells[i], gridInfo);
        }
        UpdateMesh();
    }

    public void DeleteBlocks(List<Vector3Int> cells){
        for (int i = 0; i < cells.Count; i++)
        {
            grid.SetCell(cells[i], GridInfo.empty);
        }
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
        Stopwatch timer = new Stopwatch();
        pathfinder.InnitPathfinder();
        timer.Start();
        foreach(Vector3Int start in grid.startCells){
            Stack<MovementNode> path = pathfinder.FindPath(start);
            if(path == null){
                UnityEngine.Debug.Log($"no path: {start} to any Goal");
            }else{
                VisualizePath(path);
            }
        }
        timer.Stop();
        UnityEngine.Debug.Log("Elapsed Mills: " + timer.ElapsedMilliseconds);
    }

    public void VisualizePath(Stack<MovementNode> path){
        GameObject visualizer = Instantiate(pathVisualizer);
        LineRenderer line = visualizer.GetComponent<LineRenderer>();
        line.positionCount = path.Count;
        while(path.Count > 0){
            var coord = path.Pop();
            line.SetPosition(path.Count, coord.position);
        }
    }

}
