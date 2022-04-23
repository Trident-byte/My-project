using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire1Spell : BaseAttack
{
    public Fire1Spell()
    {
        attackName = "Fire 1";
        attackDescription = "A Basic Fireball";
        attackDamage = 20f;
        attackCost = 10f;
    }
}
