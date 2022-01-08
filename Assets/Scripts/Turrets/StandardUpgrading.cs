using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardUpgrading : MonoBehaviour, ITurretUpgradable
{
    public List<Vector3Int> occupiedCells {get; set;}

    public float windowHeight;
    public GameObject nextUpgrade;
    public int upgradeCost;
    public int sellAmount;


    public Vector3 windowPosition { 
        get{ return transform.position + windowHeight * Vector3.up;} 
    }

    public int sellPrice {
        get{return sellAmount;}
    }
    
    public int? upgradePrice { 
        get {
            if(nextUpgrade == null) return null;
            else return upgradeCost;
        } 
    }

    public GameObject Upgrade(){
        GameObject nextUpgrade = Instantiate(this.nextUpgrade, transform.position, transform.rotation);
        nextUpgrade.GetComponent<ITurretUpgradable>().occupiedCells = occupiedCells;
        Destroy(gameObject);
        return nextUpgrade;
    }

    public void Sell(){
        Destroy(gameObject);
    }

}
