using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RegionData : MonoBehaviour
{
    public string RegionName;
    public int maxAmountEnemies = 4;
    public string BattleScene;
    public List <GameObject> possibleEnemies = new List <GameObject>(); 
}
