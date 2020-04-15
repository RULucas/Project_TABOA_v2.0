using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;

public class Player : MonoBehaviour
{
    public CharacterController controller;
    [SerializeField]
    private GameManager gameManager;

    public Transform meleeAttackRange;
    public float meleeAttackRadius;
    public Transform rangeAttackRange;
    Transform target;
    NavMeshAgent myAgent;


    [Task]
    public bool isAttacking = false;
    [Task]
    public bool inMeleeAttack = false;
    public float currentThinkingTime;

    public float speed = 12f;
    public float gravity = -9.81f;

    Vector3 velocity;

    public int maxHealth = 100;
    public int currentHealth { get; private set; }
    public float maxStamina = 100;
    public float currentStamina { get; private set; }

    public int damage;

    /**** ANIMATION CONTROLL ****/
    float speedPercent;
    Animator animator;
    const float locomotionAnimationSmoothTime = .1f;
    public AnimationClip[] defaultAttackAnimSet;
    AnimatorOverrideController overrideController;

    [Task] [SerializeField] private bool controlledByPlayer = false;

    [SerializeField] private CharacterStats myStats;
    [SerializeField] private CharacterStats targetStats;
    [SerializeField] private ActionManager actionManager;
    [Task] [SerializeField] private BaseAttack attack;
    [Task][SerializeField] private bool recoveringStamina;

    void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentThinkingTime = 3;
    }
    void Start()
    {
        gameManager = GameManager.instance;
        target = gameManager.boss.transform;
        myAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
        myStats = GetComponent<CharacterStats>();
        targetStats = gameManager.boss.GetComponent<CharacterStats>();
        actionManager = gameManager.playerActionManager;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(meleeAttackRange.position, meleeAttackRadius);
    }
    public void TakeDamage(int porcentageDamage)
    {
        porcentageDamage = Mathf.Clamp(porcentageDamage, 0, int.MaxValue);
        damage = (porcentageDamage * maxHealth) / 100;
        currentHealth -= damage;
        Debug.Log(transform.name + " takes " + damage + " damage.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        //Die in some way
        //This method is meant to be overwritten
        Debug.Log(transform.name + " Died.");
    }

    // Update is called once per frame
    void Update()
    {
        currentThinkingTime -= Time.deltaTime;
        if (controlledByPlayer)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            //Vector3 move = new Vector3(x, 0f, z);
            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.X))
           // isAttacking = true; //Attack();

        /*** TAKE CONTROLLED OVER CHARACTER ***/
        if (Input.GetKeyDown(KeyCode.T))
            controlledByPlayer = controlledByPlayer == false ? true : false;

        //RecoverStamina();
    }

    [Task]
    void Move()
    {
        if (myAgent.isStopped)
        {
            myAgent.isStopped = false;
            controller.enabled = false;
        }
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * .5f);
        myAgent.SetDestination(target.position);
        speedPercent = myAgent.velocity.magnitude / myAgent.speed;
        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
        /*if (distance <= rangeAttackRadius)
            inRangeAttack = true;
        else
            inRangeAttack = false;
        */
        if (distance <= 1.5f)
        {
            inMeleeAttack = true;
            animator.SetFloat("speedPercent", 0);
            myAgent.SetDestination(transform.position);
        }
        else
            inMeleeAttack = false;
        Task.current.Succeed();
        if (currentThinkingTime <= 0 && (inMeleeAttack /*|| inRangeAttack*/) && !isAttacking)
            isAttacking = true;
    }
    [Task]
    void Stop()
    {
        myAgent.SetDestination(transform.position);
        speedPercent = myAgent.velocity.magnitude / myAgent.speed;
        animator.SetFloat("speedPercent", 0);
        Task.current.Fail();
    }
    [Task]
    void StopTree()
    {
        controller.enabled = true;
        myAgent.SetDestination(transform.position);
        myAgent.isStopped = true;
        transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 0f));
        Task.current.Succeed();
    }
    [Task]
    void Combat()
    {
        attack=actionManager.GetPlayerAttack(myStats);
        if (attack != null && recoveringStamina!=true)
        {
            animator.SetTrigger("attack");
            overrideController["Armature|Punch"] = defaultAttackAnimSet[0];
            myStats.DecressStamina(attack.attackStaminaCost);
            gameManager.playerStamina.value -= attack.attackStaminaCost;
            isAttacking = false;
            currentThinkingTime = attack.attackCooldown;
            gameManager.TakeDamage(targetStats, attack.attackDamage);
            attack = null;
            Task.current.Succeed();
        }
        else
        {
            recoveringStamina = true;
            Task.current.Fail();
        }
    }

    [Task]
    void success()
    {
        Task.current.Succeed();
    }

    [Task]
    void RecoverStamina()
    {
        recoveringStamina = true;
        myStats.RecoverStamina();
        
        if (myStats.currentStamina >= 100)
            recoveringStamina = false;
        isAttacking = false;
        Task.current.Succeed();
        /* while (currentStamina < maxStamina)
         {*/
        /*currentStamina += Time.deltaTime*10f;
            gameManager.playerStamina.value = currentStamina;
        if (currentStamina >= 100)
            Task.current.Succeed();*/
        //}
        /*gameManager.playerStamina.value += Time.deltaTime * 30f;
        currentStamina = gameManager.playerStamina.value;*/
    }
}
