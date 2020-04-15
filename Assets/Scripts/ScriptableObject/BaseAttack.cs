using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Attack")]
public class BaseAttack :  ScriptableObject
{
    public string attackName;
    public int attackID;

    public AttackType attackType;
    public int attackDamage;
    public float attackRange;
    public float attackStaminaCost;

    public float timeForNextAttack;

    public float attackCooldown;
    public float recoveryRate;
    public bool attackReady = true;

    //public AnimationClip attackAnimation;

    public float attackScore;

    public enum AttackType { dud,melee, range};

    public void Recover()
    {
        recoveryRate += Time.deltaTime * 0.3f;
        if (recoveryRate >= attackCooldown)
        {
            attackReady = true;
            recoveryRate = 0;
        }
    }
    public void Reset()
    {
        recoveryRate = 0;
        attackReady = true;
    }

}
