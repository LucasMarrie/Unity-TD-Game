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
    public static string mapName = "Mabamba.json";
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
    public GameObject pathVisualizer;

    void Awake()
    {
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
        grid.SetCell(cell, GridInfo.Empty);
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
            foreach(Vector3Int end in grid.endCells){
                Stack<Tuple<Vector3, Quaternion>> path = pathfinder.FindPath(start, end);
                if(path == null){
                    UnityEngine.Debug.Log($"no path: {start} to {end}");
                }else{
                    VisualizePath(path);
                }
            }
        }
        timer.Stop();
        UnityEngine.Debug.Log("Elapsed Mills: " + timer.ElapsedMilliseconds + " | Elapsed Ticks: " + timer.ElapsedTicks)    ;
    }

    public void VisualizePath(Stack<Tuple<Vector3, Quaternion>> path){
        GameObject visualizer = Instantiate(pathVisualizer);
        LineRenderer line = visualizer.GetComponent<LineRenderer>();
        line.positionCount = path.Count;
        while(path.Count > 0){
            var coord = path.Pop();
            line.SetPosition(path.Count, coord.Item1);
        }
    }

}
