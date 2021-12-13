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

    void Awake()
    {
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

    void UpdateMesh(){
        mesh = MeshGenerator.CreateMesh(grid);
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void AddBlock(Vector3Int cell, GridContent blockType){
        grid.cells[cell.x, cell.y, cell.z] = blockType;
        UpdateMesh();
    }

    public void DeleteBlock(Vector3Int cell){
        grid.cells[cell.x, cell.y, cell.z] = GridContent.empty;
        UpdateMesh();
    }

    public Grid GetGrid(){
        return grid;
    }

}
