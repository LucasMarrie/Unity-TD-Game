using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputs : MonoBehaviour
{
    [SerializeField] LayerMask raycastLayers = ~0;
    [SerializeField] float raycastDistance = 1000; 
    [SerializeField] Camera cam;
    RaycastHit hit;
    bool raycastSuccess;
    bool hoveringUI;

    public RaycastHit Hit {
        get => hit;
    }

    public bool HoveringUI {
        get => hoveringUI;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MouseRaycast();
    }

    void MouseRaycast(){
        if(EventSystem.current.IsPointerOverGameObject()){
            raycastSuccess = false;
            hoveringUI = true;
            return;
        }
        hoveringUI = false;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        raycastSuccess = Physics.Raycast(ray, out hit, raycastDistance, raycastLayers);
    }


    public bool RaycastHitLayer(LayerMask layerMask){
        return raycastSuccess && hit.transform != null && (layerMask & 1 << hit.transform.gameObject.layer) != 0;
    }
}
