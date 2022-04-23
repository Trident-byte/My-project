using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseClass
{
    public string theName;

    public float baseHP;
    public float currentHP;

    public float baseMP;
    public float currentMP;

    public float baseATK;
    public float currentATK;
    
    public float baseDEF;
    public float currentDEF;

    public List<BaseAttack> Attacks = new List<BaseAttack> ();
    
}
