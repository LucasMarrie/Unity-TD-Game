using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour, ITargetable
{
    MapGenerator map;
    MapGrid grid;

    public float movementSpeed = 1f;
    public float vertRotSpeed = 120f;
    public float horizRotSpeed = 180f;
    public float preTurnDistance = 0.2f;
    public float movementWeight = 2f;
    float fragmentedMoveWeight;

    public Transform targettingCenter;
    
    float dirErr = 0.001f; //amount applied to making WorldToGrid() consistant
    Stack<MovementNode> path = null;
    MovementNode currentNode;
    MovementNode nextNode;
    bool hasNode = false;

    public Transform targetCenter {get {return targettingCenter;}}

    void Start()
    {
        map = MapGenerator.map;
        grid = MapGenerator.map.GetGrid();
        SetPath(GetGridPos());
    }   

    void SetPath(Vector3Int position){
        path = map.pathfinder.FindPath(position);
        fragmentedMoveWeight = movementWeight / (path.Count/2 + 1);
        foreach(MovementNode node in path){
            ApplyMovementWeight(node, true);
        }
        if(path != null){
            GetNextNode();
        }
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

    void ReachedGoal(){
        Destroy(gameObject);
    }

    void FollowPath(){
        if(!hasNode){
            if(!GetNextNode()) return;   
        }

        transform.position = Vector3.MoveTowards(transform.position, nextNode.position, movementSpeed * Time.deltaTime);

        if(transform.position == nextNode.position){
            if(nextNode.gridPos != null && PathNode.goals.Contains((Vector3Int)nextNode.gridPos)){
                ReachedGoal();
                return;
            }
            if(!GetNextNode()) return;
        }

        //rotation
        Quaternion[] currentRotation = SplitRotation(transform.rotation);
        Quaternion[] targetRotation;

        //predictive turning
        if((transform.position - nextNode.position).sqrMagnitude <= preTurnDistance * preTurnDistance){
            targetRotation = SplitRotation(nextNode.rotation);
        }else{
            targetRotation = SplitRotation(currentNode.rotation);
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
            hasNode = false;
            path = null;
            return false;
        }
        hasNode = true;
        currentNode = nextNode;
        nextNode = path.Pop();
        ApplyMovementWeight(nextNode, false);
        return true;
    }

    void ApplyMovementWeight(MovementNode node, bool add){ //add (true) or substract (false)
        if(node.gridPos != null){
            PathNode.nodes[(Vector3Int)node.gridPos].AddMoveCost(add ? fragmentedMoveWeight: -fragmentedMoveWeight);
        }
    }

    public void ResolvePath(){
        foreach(MovementNode node in path){
            ApplyMovementWeight(node, false);
        }
        path = null;
    }

    //only accurate for when bullet travel time is approximately the same before and after prediction
    public Vector3 PredictMovement(float travelTime){
        float distance = travelTime * movementSpeed;
        Vector3 prevPos = transform.position;
        MovementNode nextNode = this.nextNode;
        float distanceBetween = (nextNode.position - prevPos).magnitude;
        if(distanceBetween < distance){
            distance -= distanceBetween;
            foreach(MovementNode node in path){
                prevPos = nextNode.position;
                nextNode = node;
                distanceBetween = (nextNode.position - prevPos).magnitude;
                if(distanceBetween < distance){
                    distance -= distanceBetween;
                }else{
                    break;
                }
            }
        }
        Vector3 targetCenterOffset = nextNode.rotation * (targettingCenter.position - transform.position);
        return Vector3.MoveTowards(prevPos, nextNode.position, distance) + targetCenterOffset;
    }

}
