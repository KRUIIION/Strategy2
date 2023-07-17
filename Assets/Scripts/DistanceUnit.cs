using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts
{
    public abstract class DistanceUnit : Unit
    {
        protected override void OnInit()
        {

        }

        protected override int OnAttack(Unit attackedUnit)
        {
            if (Math.Abs(transform.position.x - attackedUnit.transform.position.x) > littleHitDistance || Math.Abs(transform.position.z - attackedUnit.transform.position.z) > littleHitDistance)
                return damage;
            else
                return damage / 2;
        }
    }
}
