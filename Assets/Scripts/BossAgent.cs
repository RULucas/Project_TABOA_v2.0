using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;
using MLAgents.Policies;
using System;
using Panda;
using System.IO;

public class BossAgent : Agent
{
    //Transform target;
    GameObject target;
    public GameManager gameManager;
    public ActionManager actionManager;
    [SerializeField] private CharacterStats targetStats;
    [SerializeField] private CharacterStats myStats;
    public Boss boss;
    public float currentThinkingTime;
    public float distance;
    Dictionary<int, BaseAttack> baseAttacks;
    Dictionary<int, BaseMovement> baseMovements;

    #region /*****  VARIABLES FOR CREATING THE FIGHT LOG  *****/

    private int fightNumber;
    private string path;

    #endregion

    #region   /*****    VARIABLES TO CHOOSE ATTACK AND MOVEMENT WHEN USING HEURISTICS  *****/

    public BaseAttack[] availableAttacks;
    public BaseMovement[] availableMovementes;
    public BaseMovement selectedMovement;
    int attackToDo;
    int moveToDo;

    #endregion
    void Start()
    {
        currentThinkingTime = 0.1f;
        boss = GetComponent<Boss>();
        gameManager = GameManager.instance;
        baseAttacks = new Dictionary<int, BaseAttack>();
        PopulateAttackDictionary(actionManager.availableAttacks);
        baseMovements = new Dictionary<int, BaseMovement>();
        PopulateMovesDictionary(actionManager.availableMovementes);
        target = gameManager.player;
        myStats = GetComponent<CharacterStats>();
        targetStats = target.GetComponent<CharacterStats>();

        availableAttacks = actionManager.GetAvailableAttacks();
        availableMovementes = actionManager.GetAvailableMovementes();
    }

    void Update()
    {
        distance = Vector3.Distance(target.transform.position, transform.position);
        currentThinkingTime -= Time.deltaTime;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.transform.position);           // 3
        sensor.AddObservation(transform.position);                  // 6
        sensor.AddObservation(myStats.currentHealth);               // 7
        sensor.AddObservation(myStats.damageDealt);                 // 8
        sensor.AddObservation(myStats.damageReceived);              // 9
        sensor.AddObservation(targetStats.currentHealth);           // 10
        sensor.AddObservation(distance);                            // 11
        sensor.AddObservation(baseAttacks[0].attackRange);          // 12
        sensor.AddObservation(baseAttacks[0].attackDamage);         // 13
        sensor.AddObservation(baseAttacks[0].attackReady ? 1 : 0);  // 14
        sensor.AddObservation(baseAttacks[1].attackRange);          // 15
        sensor.AddObservation(baseAttacks[1].attackDamage);         // 16
        sensor.AddObservation(baseAttacks[1].attackReady ? 1 : 0);  // 17
        sensor.AddObservation(baseAttacks[2].attackRange);          // 18
        sensor.AddObservation(baseAttacks[2].attackDamage);         // 19
        sensor.AddObservation(baseAttacks[2].attackReady ? 1 : 0);  // 20
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(2.7f, transform.localPosition.y, -27f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        target.transform.localPosition = new Vector3(2.7f, target.transform.localPosition.y, 27f);
        target.transform.localRotation = Quaternion.Euler(0, -180, 0);
        myStats.ResetStats();
        targetStats.ResetStats();
        actionManager.ResetAttack();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var movement = (int)vectorAction[0];
        boss.DoIMove(baseMovements[movement]);
        var attack = (int)vectorAction[1];
        if (currentThinkingTime <= 0)
        {
            /* if (currentThinkingTime <= 0 && distance <= baseAttacks[attack].attackRange && baseAttacks[attack].attackReady)
                 boss.DoIAttack(baseAttacks[attack]);
             currentThinkingTime = baseAttacks[attack].timeForNextAttack;*/
            /* if (!baseAttacks[attack].attackReady)
                 AddReward(-1 / 2000f);
             if (baseAttacks[attack].attackRange <= distance)
                 AddReward(-1 / 3000f);*/
            if (distance <= baseAttacks[attack].attackRange && baseAttacks[attack].attackReady)
            {
                boss.DoIAttack(baseAttacks[attack]);
                /* if (baseAttacks[attack].attackType != BaseAttack.AttackType.dud)
                     AddReward(1 / 100f);*/
            }
            currentThinkingTime = baseAttacks[attack].timeForNextAttack;
        }

        /*if (myStats.damageDealt > myStats.damageReceived)
            AddReward(1f / 2000f);*/
        if (targetStats.currentHealth <= 0)
        {
            fightNumber = PlayerPrefs.GetInt("FightNumberTABOAv2", 0);
            path = "D:/Documentos/Unity/Fight Logs TABOA v2/FighInfo" + fightNumber.ToString() + ".txt";
            File.AppendAllText(path, targetStats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABOAv2", fightNumber);
            SetReward(1f);
            EndEpisode();
        }
        if (myStats.currentHealth <= 0)
        {
            fightNumber = PlayerPrefs.GetInt("FightNumberTABOAv2", 0);
            path = "D:/Documentos/Unity/Fight Logs TABOA v2/FighInfo" + fightNumber.ToString() + ".txt";
            File.AppendAllText(path, myStats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABOAv2", fightNumber);
            SetReward(-1f);
            EndEpisode();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        attackToDo = CalculateAttack(myStats, targetStats, distance);
        action[1] = attackToDo;
        moveToDo = CalculateMove(myStats, targetStats, distance);
        action[0] = moveToDo;
        return action;
    }

    private void PopulateMovesDictionary(BaseMovement[] availableMovementes)
    {
        foreach (BaseMovement m in availableMovementes)
            baseMovements.Add(m.moveID, m);
    }

    void PopulateAttackDictionary(BaseAttack[] availableAttacks)
    {
        foreach (BaseAttack a in availableAttacks)
            baseAttacks.Add(a.attackID, a);
    }

    public int CalculateAttack(CharacterStats myStats, CharacterStats targetStats, float distanceToTarget)
    {
        BaseAttack selectedAttack = null;
        foreach (BaseAttack a in availableAttacks)
        {
            if (selectedAttack == null && distanceToTarget <= a.attackRange && a.attackReady)
                selectedAttack = a;
            if (selectedAttack != null)
                if (distanceToTarget <= a.attackRange && selectedAttack.attackDamage < a.attackDamage && a.attackReady)
                    selectedAttack = a;
        }
        return selectedAttack.attackID;
    }
    public int CalculateMove(CharacterStats myStats, CharacterStats targetStats, float distanceToTarget)
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
        if (myStats.damageDealt == 0 || myStats.currentHealth >= targetStats.currentHealth && myStats.damageReceived > myStats.damageDealt)
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
            if (selectedMovement == null)
                selectedMovement = m;
            else
            if (m.movementScore > selectedMovement.movementScore)
                selectedMovement = m;
        return selectedMovement.moveID;
    }

}
