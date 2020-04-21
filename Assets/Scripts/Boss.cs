using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;

public class Boss : MonoBehaviour
{
    #region /****** VARIABLES TO MANAGE THE ACTION SELECTION *******/

    public ActionManager actionManager;
    [SerializeField] private BaseMovement movementToDo;
    [SerializeField] private BaseAttack attackToDo;

    #endregion /*******************************************************/

    #region   /*    VARIABLES TO MANIPULATE AND CHANGE THE MAIN BEHAVIOR OF THE ENEMY   */

    public float thinkingTime;
    [HideInInspector] public int porcentageHealthTreshold;

    #endregion    /**************************************************************************/

    #region    /******* PLAYER VARIABLES *************************/

    public Transform target;
    public CharacterStats targetStats;

    #endregion    /***************************************************/

    #region    /*********  OWN (BOSS) VARIABLES *****************/

    public float currentThinkingTime;
    NavMeshAgent myAgent;
    public CharacterStats myStats;
    public GameManager gameManager;
    private float distance;
    [SerializeField]
    private string rotateTo;
    private float lastTime;
    public GameObject fightArena;

    #endregion    /**************************************************/

    #region    /********* PANDA VARIABLES *******/

    [Task] public bool goToTarget;
    [Task] public bool standStill;
    [Task] public bool runAway;
    [Task] public bool doMeleeAttack;
    [Task] public bool doRangeAttack;
    [Task] public bool doDudAttack;

    #endregion    /*********************************/

    void Awake()
    {
        currentThinkingTime = thinkingTime;
        rotateTo = "right";
    }

    // Start is called before the first frame update
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        gameManager = fightArena.GetComponent<GameManager>();
        target = gameManager.player.transform;
        actionManager = gameManager.bossActionManager;
        myStats = GetComponent<CharacterStats>();
        targetStats = target.GetComponent<CharacterStats>();
        lastTime = Time.time;
    }

    #region    /******************************* PANDA FUCTIONS *****************************************/

    #region    /******************* DECISION FUNCTIONS *************************/

    public void DoIMove(BaseMovement movementToDo)
    {
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        if (movementToDo != null)
            switch (movementToDo.movementType)
            {
                case BaseMovement.MovementType.goToTarget:
                    goToTarget = true;
                    runAway = false;
                    standStill = false;
                    break;
                case BaseMovement.MovementType.runAway:
                    goToTarget = false;
                    runAway = true;
                    standStill = false;
                    break;
                case BaseMovement.MovementType.standStill:
                    goToTarget = false;
                    runAway = false;
                    standStill = true;
                    break;
            }
    }

    /*[Task]
    public void DoIMove2()
    {
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        movementToDo = actionManager.CalculateMove(myStats, targetStats, distanceToTarget);
        if (movementToDo != null)
            switch (movementToDo.movementType)
            {
                case BaseMovement.MovementType.goToTarget:
                    goToTarget = true;
                    runAway = false;
                    standStill = false;
                    break;
                case BaseMovement.MovementType.runAway:
                    goToTarget = false;
                    runAway = true;
                    standStill = false;
                    break;
                case BaseMovement.MovementType.standStill:
                    goToTarget = false;
                    runAway = false;
                    standStill = true;
                    break;
            }
    }*/

    public void DoIAttack(BaseAttack attackGiven)
    {
        attackToDo = attackGiven;
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        //if (attackToDo != null)
        if (attackGiven.attackReady && distanceToTarget <= attackGiven.attackRange)
        {
            switch (attackGiven.attackType)
            {
                case BaseAttack.AttackType.dud:
                    doDudAttack = true;
                    break;
                case BaseAttack.AttackType.melee:
                    lastTime = Time.time + 1f;
                    goToTarget = false;
                    runAway = false;
                    doMeleeAttack = true;
                    distance = distanceToTarget;
                    break;
                case BaseAttack.AttackType.range:
                    lastTime = Time.time + 1f;
                    goToTarget = false;
                    runAway = false;
                    doRangeAttack = true;
                    distance = distanceToTarget;
                    break;
            }
        }
    }

    public void DoIAttack3(BaseAttack attackGiven)
    {
        attackToDo= attackGiven;
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        //if (attackToDo != null)
        if (/*attackGiven.attackReady &&*/ distanceToTarget <= attackGiven.attackRange)
        {
            switch (attackGiven.attackType)
            {
                case BaseAttack.AttackType.dud:
                    myStats.damageDealt = attackGiven.attackDamage;
                    doDudAttack = false;
                    break;
                case BaseAttack.AttackType.melee:
                    lastTime = Time.time + 1f;
                    goToTarget = false;
                    runAway = false;
                    doMeleeAttack = true;
                    myAgent.SetDestination(transform.position);
                    myAgent.isStopped = myAgent.isStopped == true ? true : true;
                    //gameManager.SaveInfo(myStats, targetStats, attackGiven, distanceToTarget);
                    gameManager.TakeDamage(targetStats, attackGiven.attackDamage);
                    NewResetVariables();
                    break;
                case BaseAttack.AttackType.range:
                    lastTime = Time.time + 1f;
                    goToTarget = false;
                    runAway = false;
                    doRangeAttack = true;
                    myAgent.SetDestination(transform.position);
                    myAgent.isStopped = myAgent.isStopped == true ? true : true;
                    //gameManager.SaveInfo(myStats, targetStats, attackGiven, distanceToTarget);
                    gameManager.TakeDamage(targetStats, attackGiven.attackDamage);
                    NewResetVariables();
                    break;
            }
        }
    }

    /* [Task]
     public void DoIAttack2()
     {
         currentThinkingTime -= Time.deltaTime;
         float distanceToTarget = Vector3.Distance(target.position, transform.position);
         if (currentThinkingTime <= 0)
             attackToDo = actionManager.CalculateAttack(myStats, targetStats, distanceToTarget);
         if (attackToDo != null)
         {
             switch (attackToDo.attackType)
             {
                 case BaseAttack.AttackType.dud:
                     break;
                 case BaseAttack.AttackType.melee:
                     lastTime = Time.time + 1f;
                     goToTarget = false;
                     runAway = false;
                     doMeleeAttack = true;
                     distance = distanceToTarget;
                     break;
                 case BaseAttack.AttackType.range:
                     lastTime = Time.time + 1f;
                     goToTarget = false;
                     runAway = false;
                     doRangeAttack = true;
                     distance = distanceToTarget;
                     break;
             }
         }
     }*/

    [Task]
    void RecoverAttack()
    {
        actionManager.RecoverAttack();
    }

    #endregion    /************************************************************/

    #region    /*********** MOVEMENT FUNCTIONS ************************/
    [Task]
    void MoveTowards()
    {
        myAgent.stoppingDistance = 1f;
        myAgent.isStopped = myAgent.isStopped == true ? false : false;
        Vector3 lookDirection = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * .5f);
        myAgent.SetDestination(target.position);
        //Task.current.Fail();
    }
    [Task]
    void RunAway()
    {
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
            Vector3 lookDirection = -1 * (transform.position - target.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
            transform.rotation = Quaternion.Slerp(lookRotation, transform.rotation, Time.deltaTime * .5f);
            /***********************************/
            Vector3 direction = (transform.position - target.position).normalized;
            myAgent.SetDestination(transform.position + direction);
        }
        // Task.current.Fail();
    }
    [Task]
    void StandStill()
    {
        myAgent.isStopped = myAgent.isStopped == true ? false : false;
        Vector3 lookDirection = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * .5f);
        myAgent.SetDestination(transform.position);
        // Task.current.Fail();
    }

    #endregion    /*****************************************************/


    #region    /**************** ATTACK FUNCTIONS ***********************/

    [Task]
    void DudAttack()
    {
        myStats.damageDealt = attackToDo.attackDamage;
        doDudAttack = false;
        //NewResetVariables();
    }

    [Task]
    void MeleeAttack()
    {
        myAgent.SetDestination(transform.position);
        myAgent.isStopped = myAgent.isStopped == true ? true : true;
        gameManager.SaveInfo(myStats, targetStats, attackToDo, distance);
        gameManager.TakeDamage(targetStats, attackToDo.attackDamage);
        NewResetVariables();
    }

    [Task]
    void RangeAttack()
    {
        myAgent.SetDestination(transform.position);
        myAgent.isStopped = myAgent.isStopped == true ? true : true;
        gameManager.SaveInfo(myStats, targetStats, attackToDo, distance);
        gameManager.TakeDamage(targetStats, attackToDo.attackDamage);
        NewResetVariables();
    }

    #endregion    /******************************************************/

    [Task]
    public void NewResetVariables()
    {
        doDudAttack = false;
        doRangeAttack = false;
        doMeleeAttack = false;
        myStats.damageDealt = attackToDo.attackDamage;
        currentThinkingTime = attackToDo.timeForNextAttack;
        myStats.lastAttackMade = attackToDo;
        attackToDo.attackReady = false;
        attackToDo = null;
        //Task.current.Fail();
    }

    #endregion    /**************************************************************************************************************/

}
