using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Action Manager")]
public class ActionManager : ScriptableObject
{
    public BaseAttack[] availableAttacks;
    public BaseAttack selectedAttack;

    public BaseMovement[] availableMovementes;
    public BaseMovement selectedMovement;

    public BaseAttack CalculateAttack(CharacterStats myStats, CharacterStats targetStats, float distanceToTarget)
    {
        selectedAttack = null;
        foreach (BaseAttack a in availableAttacks)
        {
            if (selectedAttack == null && distanceToTarget <= a.attackRange && a.attackReady)
                selectedAttack = a;
            if (selectedAttack != null)
                if (distanceToTarget <= a.attackRange && selectedAttack.attackDamage < a.attackDamage && a.attackReady /*&& selectedAttack.attackScore < a.attackScore*/ )
                {
                    selectedAttack = a;
                }
        }
        return selectedAttack;
    }

    public BaseMovement CalculateMove(CharacterStats myStats, CharacterStats targetStats, float distanceToTarget)
    {
        if (myStats.damageDealt >= myStats.damageReceived)
        {
            foreach (BaseMovement m in availableMovementes)
            {
                if (m.movementType == BaseMovement.MovementType.standStill)
                    m.movementScore = 1f;
                else
                    m.movementScore = 0f;
            }
        }
        if (myStats.damageDealt == 0 || myStats.currentHealth > targetStats.currentHealth && myStats.damageReceived > myStats.damageDealt)
        {
            foreach (BaseMovement m in availableMovementes)
                if (m.movementType == BaseMovement.MovementType.goToTarget)
                    m.movementScore = 1f;
                else
                    m.movementScore = 0f;
        }
        if (myStats.totalDamageDealt < myStats.totalDamageReceived || myStats.currentHealth < targetStats.currentHealth)
        {
            foreach (BaseMovement m in availableMovementes)
                if (m.movementType == BaseMovement.MovementType.runAway)
                    m.movementScore = 1f;
                else
                    m.movementScore = 0f;
        }
        foreach (BaseMovement m in availableMovementes)
            if (m.movementScore > selectedMovement.movementScore)
                selectedMovement = m;
        return selectedMovement;
    }

    public BaseAttack GetPlayerAttack(CharacterStats stats)
    {
        selectedAttack = null;
        foreach (BaseAttack a in availableAttacks)
        {
            /*if (selectedAttack == null && stats.currentStamina >= a.attackStaminaCost)
                selectedAttack = a;
            if (selectedAttack != null)
                if (stats.currentStamina >= a.attackStaminaCost && selectedAttack.attackDamage < a.attackDamage)
                    selectedAttack = a;*/
            if (a.attackID == 0)
                selectedAttack = a;
        }
        return selectedAttack;
    }

    public void RecoverAttack()
    {
        foreach (BaseAttack a in availableAttacks)
            if (!a.attackReady)
                a.Recover();
    }

    public BaseAttack GetAttack(int attackIndex)
    {
        if (availableAttacks[attackIndex].attackReady)
            return availableAttacks[attackIndex];
        else
            return null;
    }

    public void ResetAttack()
    {
        foreach (BaseAttack a in availableAttacks)
            a.Reset();
    }
    public BaseAttack[] GetAvailableAttacks()
    {
        return availableAttacks;
    }
    public BaseMovement[] GetAvailableMovementes()
    {
        return availableMovementes;
    }
}
