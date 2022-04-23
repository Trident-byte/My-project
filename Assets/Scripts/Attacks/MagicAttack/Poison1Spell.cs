using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poison1Spell : BaseAttack
{
    public Poison1Spell()
    {
        attackName = "Poison 1";
        attackDescription = "A Poison attack sucking the life out of your enemy.";
        attackDamage = 5f;
        attackCost = 5f;
    } 
}
