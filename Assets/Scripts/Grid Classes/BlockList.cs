using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "All blocks", menuName = "Script Objects")]
public class BlockList : ScriptableObject
{
    public BlockData[] blocks;

    public static BlockData[] blockDataList;
}


