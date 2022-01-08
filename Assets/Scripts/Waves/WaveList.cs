using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Waves", menuName = "Script Assets/Waves/Wave List")]
public class WaveList : ScriptableObject 
{
    public List<Wave> waves;
}
