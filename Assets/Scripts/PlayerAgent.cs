using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents.Policies;
using Panda;
using System.IO;

public class PlayerAgent : Agent
{
    [SerializeField] GameObject target;
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private CharacterStats targetStats;
    [SerializeField] private CharacterStats myStats;
    [SerializeField] private Player player;
    public GameObject fightArena;
    [SerializeField] private GameManager gameManager;
    public float currentThinkingTime;
    public float distance;
    Dictionary<int, BaseAttack> baseAttacks;
    Dictionary<int, BaseMovement> baseMovements;
    float maxRangeForAttacking;

    float timer;
    float second;

    #region /*****  VARIABLES FOR CREATING THE FIGHT LOG  *****/

    private int fightNumber;
    private string path;
    private string arenaName;

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
        WakeUp();
    }
    void FixedUpdate()
    {
        distance = Vector3.Distance(target.transform.position, transform.position);
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        var attack = (int)vectorAction[0];

        DealDamage(baseAttacks[attack]);
    }
    public override void Heuristic(float[] actionsOut)
    {
        //attackToDo = CalculateAttack(myStats, targetStats, distance);
        actionsOut[0] = 0;// attackToDo;
    }

    #region /*****  WAKING UP THE BRAIN AND SETTING THINGS  *****/

    void WakeUp()
    {
        gameManager = fightArena.GetComponent<GameManager>();
        player = GetComponent<Player>();
        myStats = GetComponent<CharacterStats>();
        actionManager = gameManager.playerActionManager;

        baseAttacks = new Dictionary<int, BaseAttack>();
        PopulateAttackDictionary(actionManager.availableAttacks);
        availableAttacks = actionManager.GetAvailableAttacks();
        
        target = gameManager.boss;
        targetStats = target.GetComponent<CharacterStats>();
        
    }

    void PopulateMovesDictionary(BaseMovement[] availableMovementes)
    {
        foreach (BaseMovement m in availableMovementes)
            baseMovements.Add(m.moveID, m);
    }

    void PopulateAttackDictionary(BaseAttack[] availableAttacks)
    {
        foreach (BaseAttack a in availableAttacks)
            baseAttacks.Add(a.attackID, a);
    }

    void GetMaxRange()
    {
        foreach (BaseAttack a in availableAttacks)
            if (a.attackRange >= maxRangeForAttacking)
                maxRangeForAttacking = a.attackRange;
    }

    #endregion

    #region /***** ACTIONS FOR THE HEURISTICS *****/

    public int CalculateAttack(CharacterStats myStats, CharacterStats targetStats, float distanceToTarget)
    {
        BaseAttack selectedAttack = null;
        foreach (BaseAttack a in availableAttacks)
            if (a.attackID == 0)
                selectedAttack = a;
        return selectedAttack.attackID;
    }
    #endregion

    void DealDamage(BaseAttack doAttack)
    {
        if (distance < doAttack.attackRange)
        {
            targetStats.currentHealth = targetStats.currentHealth - (targetStats.maxHealth * doAttack.attackDamage / 100);
            targetStats.health.value = targetStats.health.value - (targetStats.maxHealth * doAttack.attackDamage / 100);
        }
    }
}
