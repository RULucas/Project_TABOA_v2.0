using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;
using MLAgents.Policies;
using Panda;
using System.IO;

public class BossAgent : Agent
{
    [SerializeField] GameObject target;
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private CharacterStats targetStats;
    [SerializeField] private CharacterStats myStats;
    [SerializeField] private Boss boss;
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

    void Update()
    {
        distance = Vector3.Distance(target.transform.position, transform.position);
        timer += Time.deltaTime;
        second = timer % 60;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(target.transform.position);           // 3
        //sensor.AddObservation(transform.position);                  // 6
        //sensor.AddObservation(myStats.currentHealth);               // 7
        /*sensor.AddObservation(myStats.damageDealt);                 // 8
        sensor.AddObservation(myStats.damageReceived);         */     // 9
        //sensor.AddObservation(targetStats.currentHealth);           // 10
        sensor.AddObservation(distance);                            // 1
        sensor.AddObservation(currentThinkingTime);                 // 2
        sensor.AddObservation(second);                              // 3
        //sensor.AddObservation(baseAttacks[0].attackRange);          // 12
        //sensor.AddObservation(baseAttacks[0].timeForNextAttack);
        //sensor.AddObservation(baseAttacks[0].attackDamage);         // 13
        //sensor.AddObservation(baseAttacks[0].attackReady ? 1 : 0);  // 14
        //sensor.AddObservation(baseAttacks[1].attackRange);          // 15
        //sensor.AddObservation(baseAttacks[1].timeForNextAttack);
        //sensor.AddObservation(baseAttacks[1].attackDamage);         // 16
        //sensor.AddObservation(baseAttacks[1].attackReady ? 1 : 0);  // 17
        //sensor.AddObservation(baseAttacks[2].attackRange);          // 18
        //sensor.AddObservation(baseAttacks[2].attackDamage);         // 19
        //sensor.AddObservation(baseAttacks[2].attackReady ? 1 : 0);  // 20
        //sensor.AddObservation(baseAttacks[2].timeForNextAttack);
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-42, 42), transform.localPosition.y, Random.Range(-44, -2));
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        target.transform.localPosition = new Vector3(Random.Range(-42, 42), target.transform.localPosition.y, Random.Range(2, 44));
        target.transform.localRotation = Quaternion.Euler(0, -180, 0);
        myStats.ResetStats();
        targetStats.ResetStats();
        actionManager.ResetAttack();
    }

    public override void OnActionReceived(float[] vectorAction)
    {

        #region /*****  REWARD SYSTEM FOR STAYING AT RANGE OF DEALING DAMAGE  *****/

        var movement = (int)vectorAction[0];
        boss.DoIMove(baseMovements[movement]);

        if (distance > maxRangeForAttacking)
            AddReward(-1 / 7000);
        else
            AddReward(1 / 7000);

        #endregion

        #region /*****  REWARD SYSTEM FOR ATTACKING ON INTERVALS  *****/

        var attack = (int)vectorAction[1];

        if (/*(*/second < currentThinkingTime /*|| second >= 4)*/ && baseAttacks[attack].attackType != BaseAttack.AttackType.dud)
            AddReward(-1 / 100f);

        if ((second >= currentThinkingTime || second < currentThinkingTime + 2f) && baseAttacks[attack].attackType != BaseAttack.AttackType.dud)
        {
            boss.DoIAttack3(baseAttacks[attack]);
            gameManager.SaveInfo(myStats, targetStats, baseAttacks[attack], second);
            AddReward(1 / 2000f);
            currentThinkingTime = baseAttacks[attack].timeForNextAttack;
            timer = 0;
            second = 0;
        }
        boss.DoIAttack(baseAttacks[attack]);

        if (second >= currentThinkingTime + 2f)
        {
            AddReward(-1 / 100f);
            timer = 0;
            second = 0;
        }

        #endregion

        #region /*****  REWARD SYSTEM FOR ENCOURAGING FIGHTING  *****/

        if (targetStats.currentHealth <= 0)
        {
            fightNumber = PlayerPrefs.GetInt("FightNumberTABFAv2" + arenaName + "", 0);
            path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo" + arenaName + "" + fightNumber.ToString() + ".txt";
            File.AppendAllText(path, targetStats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABFAv2" + arenaName + "", fightNumber);
            AddReward(1 / 3000f);
            EndEpisode();
        }
        if (myStats.currentHealth <= 0)
        {
            fightNumber = PlayerPrefs.GetInt("FightNumberTABFAv2" + arenaName + "", 0);
            path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo" + arenaName + "" + fightNumber.ToString() + ".txt";
            File.AppendAllText(path, myStats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABFAv2" + arenaName + "", fightNumber);
            AddReward(-1 / 1000f);
            EndEpisode();
        }

        #endregion
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

    #region /*****  WAKING UP THE BRAIN AND SETTING THINGS  *****/

    void WakeUp()
    {
        currentThinkingTime = 0.1f;
        maxRangeForAttacking = 0;
        gameManager = fightArena.GetComponent<GameManager>();
        boss = GetComponent<Boss>();
        myStats = GetComponent<CharacterStats>();
        actionManager = gameManager.bossActionManager;

        baseAttacks = new Dictionary<int, BaseAttack>();
        PopulateAttackDictionary(actionManager.availableAttacks);
        availableAttacks = actionManager.GetAvailableAttacks();

        baseMovements = new Dictionary<int, BaseMovement>();
        PopulateMovesDictionary(actionManager.availableMovementes);
        availableMovementes = actionManager.GetAvailableMovementes();

        GetMaxRange();

        target = gameManager.player;
        targetStats = target.GetComponent<CharacterStats>();

        arenaName = boss.fightArena.name.Replace(" ", "");
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

    #endregion
}
