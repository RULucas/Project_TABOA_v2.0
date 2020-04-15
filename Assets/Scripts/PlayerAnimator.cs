using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimator : MonoBehaviour
{
    const float locomotionAnimationSmoothTime = .1f;
    public AnimationClip replaceableAttackAnim;
    public AnimationClip[] defaultAttackAnimSet;
    protected AnimationClip[] currentAttackAnimSet;

    Animator animator;
    NavMeshAgent agent;
    CharacterController controller;
    AnimatorOverrideController overrideController;

    float speedPercent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        currentAttackAnimSet = defaultAttackAnimSet;
    }

    // Update is called once per frame
    void Update()
    {
        /*speedPercent = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);*/
    }

    protected virtual void OnAttack()
    {
        animator.SetTrigger("attack");
        overrideController[replaceableAttackAnim.name] = currentAttackAnimSet[1];
    }
}
