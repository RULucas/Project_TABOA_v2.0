using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth = 100;
    public int currentHealth; //{ get; private set; }
    public float maxStamina = 100;
    public float currentStamina;// { get; private set; }

    public int damageReceived;
    public int totalDamageReceived;
    public int damageDealt;
    public int totalDamageDealt;

    public BaseAttack lastAttackMade;
    public BaseMovement lastMovementMade;

    public Slider health;
    public Slider stamina;

    void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        health.value = maxHealth;
        stamina.value = maxStamina;
    }

    public void DecressStamina(float stamina)
    {
        currentStamina -= stamina;
    }

    public void RecoverStamina()
    {
        currentStamina += Time.deltaTime * 10f;
        stamina.value = currentStamina;
    }

    public void ResetStats()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        damageReceived = 0;
        damageDealt = 0;
        totalDamageDealt = 0;
        totalDamageReceived = 0;
        lastAttackMade = null;
        lastMovementMade = null;
        health.value = maxHealth;
        stamina.value = maxStamina;
    }
}
