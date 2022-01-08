using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurretEditor : MonoBehaviour
{
    MapGenerator map;
    MapGrid grid;
    Camera cam;
    public LayerMask turretMask = 0b_0001_0000_0000;
    public GameObject upgradeWindow;
    public Text upgradeText;
    public Text sellText;
    public Color outlineColor = Color.yellow;

    GameObject selectedTurret;
    ITurretUpgradable turretUpgrade;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        map = MapGenerator.map;
        grid = map.GetGrid();
    }

    // Update is called once per frame
    void Update()
    {
        SelectTurret();
    }

    void SelectTurret(){
        if(EventSystem.current.IsPointerOverGameObject()){
            return;
        }
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit) && (turretMask & 1 << hit.transform.gameObject.layer) != 0 && !TurretPlacer.placedThisFrame){
            if(Input.GetButtonDown("Fire1")){
                OpenEditor(hit.transform.root.gameObject);
            }
        }else if(Input.GetButtonDown("Fire1")){
            CloseEditor();
        }
    }   

    void OpenEditor(GameObject turret){
        //opens an editor with the information from IUpggradable
        CloseEditor();
        if(selectedTurret == turret){ 
            return;
        }
        selectedTurret = turret;
        turretUpgrade = turret.GetComponent<ITurretUpgradable>();
        SetOutline(true);
        UpdateWindowText();
        upgradeWindow.transform.position = turretUpgrade.windowPosition;
        upgradeWindow.SetActive(true);
    }

    void CloseEditor(){
        //closes the editor
        if(selectedTurret != null){
            SetOutline(false);
            selectedTurret = null;
        }
        upgradeWindow.SetActive(false);
    }

    void UpdateWindowText(){
        sellText.text = "SELL: " + turretUpgrade.sellPrice;
        if(turretUpgrade.upgradePrice == null){
            upgradeText.text = "MAXED OUT";
        }else{
            upgradeText.text = "UPGRADE: " + turretUpgrade.upgradePrice;
        }
    }

    void SetOutline(bool visible){
        if(visible){
            Outline turretOutline = selectedTurret.GetComponent<Outline>();
            if(turretOutline == null){
                turretOutline = selectedTurret.AddComponent<Outline>();
                turretOutline.OutlineColor = outlineColor;
                turretOutline.OutlineWidth = 5;
                turretOutline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            }
            turretOutline.enabled = true;
        }else{
            selectedTurret.GetComponent<Outline>().enabled = false;
        }
    }

    public void Sell(){
        int sellPrice = turretUpgrade.sellPrice;
        //to-do: give player money
        foreach(Vector3Int occupiedCell in turretUpgrade.occupiedCells){
            grid.SetCell(occupiedCell, GridInfo.empty);
        }
        turretUpgrade.Sell();
        CloseEditor();
    }

    public void Upgrade(){
        if(turretUpgrade.upgradePrice == null) return;
        int upgradePrice = (int)turretUpgrade.upgradePrice;
        OpenEditor(turretUpgrade.Upgrade());
        turretUpgrade = selectedTurret.GetComponent<ITurretUpgradable>();
        UpdateWindowText();
    }

}
