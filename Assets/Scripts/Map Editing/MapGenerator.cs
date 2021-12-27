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

    public Pathfinding pathfinder;

    MapGrid grid;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

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

}
