using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts
{
    public abstract class MeleeUnit : Unit
    {
        protected override void OnInit()
        {

        }

        protected override int OnAttack(Unit attackedUnit)
        {
            return damage;
        }
    }
}
