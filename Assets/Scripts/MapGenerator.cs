using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class MapGenerator : MonoBehaviour
{
    public Vector3Int gridSize = new Vector3Int(30, 10, 30);
    public float cellSize = 1f;
    public Transform floor;

    [System.NonSerialized]
    Grid grid;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        floor.position = transform.position + Vector3.down * (gridSize.y + 1)/2 * cellSize;
        floor.localScale = new Vector3(gridSize.x, 1, gridSize.z) * cellSize;

        grid = new Grid(transform.position, gridSize, cellSize);
        UpdateMesh();
    }


    void Start(){

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.grey;
        Gizmos.DrawCube(transform.position + Vector3.down * (gridSize.y + 1)/2 * cellSize, new Vector3(gridSize.x, 1, gridSize.z) * cellSize);
    }

    void UpdateMesh(){
        Material[] materials;
        mesh = MeshGenerator.CreateMesh(grid, out materials);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.materials = materials;

    }

    public void AddBlock(Vector3Int cell, GridContent blockType, Quaternion rotation, Material material){
        GridInfo gridInfo = new GridInfo(blockType, rotation, material);
        grid.cells[cell.x, cell.y, cell.z] = gridInfo;
        UpdateMesh();
    }

    public void DeleteBlock(Vector3Int cell){
        grid.cells[cell.x, cell.y, cell.z] = GridInfo.Empty;
        UpdateMesh();
    }

    public Grid GetGrid(){
        return grid;
    }

}
