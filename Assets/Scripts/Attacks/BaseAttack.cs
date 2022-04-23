using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseAttack: MonoBehaviour 
{
    public string attackName; //
    public string attackDescription;
    public float attackDamage; //ex: Base Damage 15, melee lvl 10 stamina 35 = basedmg + lvl + stamina = 60
    public float attackCost; //Mana cost
}
