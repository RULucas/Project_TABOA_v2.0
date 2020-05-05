using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents.Policies;
using Panda;
using System.IO;
using UnityEngine.AI;

public class BossAgent : Agent
{
    [SerializeField] GameObject target;
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private CharacterStats targetStats;
    [SerializeField] private CharacterStats myStats;
    [SerializeField] private Boss boss;
    [SerializeField] NavMeshAgent myAgent;
    public GameObject fightArena;
    [SerializeField] private GameManager gameManager;
    public float currentThinkingTime;
    public float distance;
    public float lastDistance;
    Dictionary<int, BaseAttack> baseAttacks;
    Dictionary<int, BaseMovement> baseMovements;
    float maxRangeForAttacking;

    private float lastTime;
    private string rotateTo;

    float timer;
    float second;

    int stepsToReset;

    int randomSeed;

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
        timer += Time.deltaTime;
        second = timer % 60;
    }

    void FixedUpdate()
    {
        distance = Vector3.Distance(target.transform.position, transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.transform.position);           // 3
        sensor.AddObservation(transform.position);                  // 6
        sensor.AddObservation(myStats.currentHealth);               // 7
        /*sensor.AddObservation(myStats.damageDealt);                 // 8
        sensor.AddObservation(myStats.damageReceived);         */     // 9
        sensor.AddObservation(targetStats.currentHealth);           // 10
        sensor.AddObservation(distance);
        //sensor.AddObservation(lastDistance);                            // 1
        //sensor.AddObservation(maxRangeForAttacking);
        //sensor.AddObservation(currentThinkingTime);                 // 2
        //sensor.AddObservation(second);                              // 3
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
        /*transform.localPosition = new Vector3(Random.Range(-42, 42), transform.localPosition.y, Random.Range(-44, -2));
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        target.transform.localPosition = new Vector3(Random.Range(-42, 42), target.transform.localPosition.y, Random.Range(2, 44));
        target.transform.localRotation = Quaternion.Euler(0, -180, 0);*/
        transform.localPosition = new Vector3(2.7f, transform.localPosition.y, -27);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        target.transform.localPosition = new Vector3(2.7f, target.transform.localPosition.y, 27);
        target.transform.localRotation = Quaternion.Euler(0, -180, 0);
        /*myStats.*/
        ResetStats(myStats);
        /*targetStats.*/
        ResetStats(targetStats);
        lastDistance = 1000f;
    }

    private void ResetStats(CharacterStats stats)
    {
        stats.currentHealth = stats.maxHealth;
        stats.currentStamina = stats.maxStamina;
        stats.damageReceived = 0;
        stats.damageDealt = 0;
        stats.totalDamageDealt = 0;
        stats.totalDamageReceived = 0;
        stats.lastAttackMade = null;
        stats.lastMovementMade = null;
        stats.health.value = stats.maxHealth;
        stats.stamina.value = stats.maxStamina;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        #region /*****  REWARD SYSTEM FOR STAYING AT RANGE OF DEALING DAMAGE  *****/

        //var action = (int)vectorAction[0];
        //boss.DoIMove(baseMovements[movement]);
        /*switch (action)
        {
            case 0:
                Move(baseMovements[action]);
                break;
            case 1:
                Move(baseMovements[action]);
                break;
            case 2:
                DealDamage(baseAttacks[action]);
                break;
            case 3:
                DealDamage(baseAttacks[action]);
                break;
        }*/

        //Debug.Log((int)vectorAction[0]+" --- "+distance);
        if ((int)vectorAction[0] == 0)
            Move(baseMovements[0]);
        if ((int)vectorAction[0] == 1)
            Move(baseMovements[1]);
        if ((int)vectorAction[0] == 2)
            DealDamage(baseAttacks[2]);
        if ((int)vectorAction[0] == 3)
            DealDamage(baseAttacks[3]);

        /* if (lastDistance < distance && lastDistance>0.8)
         {
             AddReward(-1 / 5000f);
             //lastDistance = distance-0.5f;
         }
         if(lastDistance>distance)
         {
             AddReward(1 / 10f);
             lastDistance = distance;// -0.0004f;
         }*/
        if (distance <= 3.0f)
        {
            AddReward(0.1f);
        }
        /*if (myStats.damageDealt > 0)
        {
            AddReward(1 / 5000f);
            myStats.damageDealt = 0;
        }*/
        #endregion

        #region /*****  REWARD SYSTEM FOR ATTACKING ON INTERVALS  *****/

        //var attack = (int)vectorAction[1];

        /*if (second < currentThinkingTime && baseAttacks[attack].attackType != BaseAttack.AttackType.dud)
            AddReward(-1 / 100f);

        if ((second >= currentThinkingTime || second < currentThinkingTime + 2f) && baseAttacks[attack].attackType != BaseAttack.AttackType.dud)
        {
            boss.DoIAttack(baseAttacks[attack]);
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
        }*/

        #endregion
        //boss.DoIAttack(baseAttacks[attack]);
        //DealDamage(baseAttacks[attack]);
        //gameManager.SaveInfo(myStats, targetStats, baseAttacks[attack], second);

        #region /*****  REWARD SYSTEM FOR ENCOURAGING FIGHTING  *****/

        if (targetStats.currentHealth <= 0)
        {
            fightNumber = PlayerPrefs.GetInt("FightNumberTABFAv2" + arenaName + "", 0);
            path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo" + arenaName + "" + fightNumber.ToString() + ".txt";
            File.AppendAllText(path, targetStats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABFAv2" + arenaName + "", fightNumber);
            //AddReward(1 / 3000f);
            AddReward(0.9f);
            EndEpisode();
        }
        if (myStats.currentHealth <= 0)
        {
            fightNumber = PlayerPrefs.GetInt("FightNumberTABFAv2" + arenaName + "", 0);
            path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo" + arenaName + "" + fightNumber.ToString() + ".txt";
            File.AppendAllText(path, myStats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABFAv2" + arenaName + "", fightNumber);
            //AddReward(-1 / 1000f);
            AddReward(-1/2f);
            EndEpisode();
        }
        randomSeed++;
        /*if (StepCount > stepsToReset)
        {
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABFAv2" + arenaName + "", fightNumber);

            EndEpisode();
        }*/
        #endregion
    }

    public override void Heuristic(float[] actionsOut)
    {
        /*attackToDo = CalculateAttack(myStats, targetStats, distance);
        action[1] = attackToDo;*/
        /*moveToDo = CalculateMove(myStats, targetStats, distance);
        action[0] = moveToDo;*/
        if (randomSeed == 0)
            actionsOut[0] = 1;
        if (randomSeed == 1)
            actionsOut[0] = 2;
        if (randomSeed >= 2)
        {
            if (distance > 2.9f)
                actionsOut[0] = 0;
            else
                actionsOut[0] = 3;
        }
        //Debug.Log("ACTION:  " + (int)action[0] + " --- " + distance);
    }

    #region /*****  WAKING UP THE BRAIN AND SETTING THINGS  *****/

    void WakeUp()
    {
        currentThinkingTime = 0.1f;
        maxRangeForAttacking = 0;
        lastTime = Time.time;
        rotateTo = "right";
        stepsToReset = 1399;
        distance = 1000f;
        lastDistance = 1000f;
        randomSeed = 0;
        gameManager = fightArena.GetComponent<GameManager>();
        boss = GetComponent<Boss>();
        myStats = GetComponent<CharacterStats>();
        actionManager = gameManager.bossActionManager;
        myAgent = GetComponent<NavMeshAgent>();

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
            if (a.attackRange >= maxRangeForAttacking && a.attackType != BaseAttack.AttackType.dud)
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

    private void DealDamage(BaseAttack doAttack)
    {
        if (distance < doAttack.attackRange)
        {
            targetStats.currentHealth = targetStats.currentHealth - (targetStats.maxHealth * doAttack.attackDamage / 100);
            targetStats.health.value = targetStats.health.value - (targetStats.maxHealth * doAttack.attackDamage / 100);
            myStats.damageDealt = doAttack.attackDamage;
        }

        //Content of the file 
        //  BOSS HEALT , ATTACK ID (DONE) , ATTACK DAMAGE , DISTANCE TO TARGET , PLAYER HEALTH
        string content = myStats.currentHealth.ToString() + ";" + doAttack.attackID.ToString() + ";" + doAttack.attackDamage.ToString() + ";" + second.ToString() + ";" + targetStats.currentHealth.ToString() + "\n";
        fightNumber = PlayerPrefs.GetInt("FightNumberTABFAv2" + arenaName + "", 0);
        path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo" + arenaName + "" + fightNumber.ToString() + ".txt";
        File.AppendAllText(path, content);
    }

    private void Move(BaseMovement baseMovement)
    {
        switch (baseMovement.movementType)
        {
            case BaseMovement.MovementType.goToTarget:
                myAgent.stoppingDistance = 1f;
                myAgent.isStopped = myAgent.isStopped == true ? false : false;
                Vector3 lookDirection = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * .5f);
                myAgent.SetDestination(target.transform.position);
                break;
            case BaseMovement.MovementType.runAway:
                if (myAgent.velocity.magnitude == 0 && lastTime - Time.time < 0)
                {
                    switch (rotateTo)
                    {
                        case "left":
                            transform.position = new Vector3(transform.position.x + .3f, transform.position.y, transform.position.z + .3f);
                            break;
                        case "right":
                            transform.position = new Vector3(transform.position.x - .3f, transform.position.y, transform.position.z - .3f);
                            break;
                    }
                    if (lastTime - Time.time < 0)
                    {
                        rotateTo = rotateTo == "right" ? "left" : "right";
                        lastTime = Time.time + 1f;
                    }
                }
                else
                {
                    myAgent.stoppingDistance = 0;
                    myAgent.isStopped = myAgent.isStopped == true ? false : false;
                    /*** LOOK AT PLAYER ALL THE TIME ***/
                    lookDirection = -1 * (transform.position - target.transform.position).normalized;
                    lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
                    transform.rotation = Quaternion.Slerp(lookRotation, transform.rotation, Time.deltaTime * .5f);
                    /***********************************/
                    Vector3 direction = (transform.position - target.transform.position).normalized;
                    myAgent.SetDestination(transform.position + direction);
                }
                break;
                /* case BaseMovement.MovementType.standStill:
                     myAgent.isStopped = myAgent.isStopped == true ? false : false;
                     lookDirection = (target.transform.position - transform.position).normalized;
                     lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
                     transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * .5f);
                     myAgent.SetDestination(transform.position);
                     break;*/
        }
    }
}
