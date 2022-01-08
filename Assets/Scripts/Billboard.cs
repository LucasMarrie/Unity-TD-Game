using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform playerView;

    void Update()
    {
        AlignWithView();
    }

    void AlignWithView(){
        transform.rotation = playerView.rotation;
    }
}
