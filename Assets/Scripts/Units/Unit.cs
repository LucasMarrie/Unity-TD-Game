using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    MapGenerator map;

    public float movementSpeed = 1f;
    public float vertRotSpeed = 120f;
    public float horizRotSpeed = 180f;
    public float preTurnDistance = 0.2f;
    public float movementWeight = 10f;
    
    float dirErr = 0.001f; //amount applied to making WorldToGrid() consistant
    Stack<Tuple<Vector3, Quaternion>> path = null;
    Tuple<Vector3, Quaternion> currentNode = null;
    Tuple<Vector3, Quaternion> nextNode = null;

    MapGrid grid;

    void Start()
    {
        map = MapGenerator.map;
        grid = MapGenerator.map.GetGrid();
        SetPath(GetGridPos());
    }   

    void SetPath(Vector3Int position){
        path = map.pathfinder.FindPath(position);
    }

    Vector3Int GetGridPos(){
        return grid.WorldToGrid(transform.position - Vector3.up * dirErr - transform.rotation * Vector3.forward * dirErr);
    }

    // Update is called once per frame
    void Update()
    {
        if(path != null){
            FollowPath();
        }
    }

    bool ReachedGoal(){
        return PathNode.goals.Contains(GetGridPos());
    }

    void FollowPath(){
        while(currentNode == null){
            if(!GetNextNode()) return;   
        }

        transform.position = Vector3.MoveTowards(transform.position, nextNode.Item1, movementSpeed * Time.deltaTime);

        if(transform.position == nextNode.Item1){
           if(!GetNextNode()) return;
        }

        //rotation
        Quaternion[] currentRotation = SplitRotation(transform.rotation);
        Quaternion[] targetRotation;

        //predictive turning
        if((transform.position - nextNode.Item1).sqrMagnitude <= preTurnDistance * preTurnDistance){
            targetRotation = SplitRotation(nextNode.Item2);
        }else{
            targetRotation = SplitRotation(currentNode.Item2);
        }

        if(currentRotation[0] != targetRotation[0]){
            currentRotation[0] = Quaternion.RotateTowards(currentRotation[0], targetRotation[0], horizRotSpeed * Time.deltaTime);
        }
        if(currentRotation[1] != targetRotation[1]){
            currentRotation[1] = Quaternion.RotateTowards(currentRotation[1], targetRotation[1], vertRotSpeed * Time.deltaTime);
        }
        transform.rotation = currentRotation[0] * currentRotation[1];
        
    }

    Quaternion[] SplitRotation(Quaternion rotation){
        Vector3 rot = rotation.eulerAngles;
        Quaternion horizRot = Quaternion.Euler(rot.y * Vector3.up);
        rot.y = 0;
        Quaternion vertRot = Quaternion.Euler(rot);
        return new Quaternion[]{horizRot, vertRot};
    }

    bool GetNextNode(){
        if(path.Count == 0){
            nextNode = null;
            currentNode = null;
            path = null;
            return false;
        }
        currentNode = nextNode;
        nextNode = path.Pop();
        return true;
    }
}
