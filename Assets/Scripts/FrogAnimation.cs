using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using alu0100969535.Utils;

public class FrogAnimation : MonoBehaviour {

    [Header("FrogAnimation")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpDelay;

    [Header("Triggers")]
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToTriggerJump = 5.0f;

    private Animator animator;

    private bool isFalling;
    private bool isJumping;

    private float timeSinceLastJump;

    private void Awake() {
        animator = Utils.Get<Animator>(gameObject, "FrogAnimation needs an Animator");
    }

    void FixedUpdate() {
        UpdateState();
        UpdateAnimatorVariables();
    }

    private void UpdateState() {
        timeSinceLastJump += Time.fixedDeltaTime;

        if(CanJump()) {
            Jump();
        }
    }

    private bool CanJump(){

        bool isBusy = isJumping || isFalling;
        bool hasPassedEnoughTime = timeSinceLastJump > jumpDelay;
        bool isPlayerNearby = Vector2.Distance(gameObject.transform.position, player.transform.position) < distanceToTriggerJump; 

        return hasPassedEnoughTime && !isBusy && isPlayerNearby;
    }

    private void Jump() {
         StartCoroutine(PerformJump(() => {
            timeSinceLastJump = 0.0f;
         }));
    }

    IEnumerator PerformJump(Action onFinish) {
        isJumping = true;
        isFalling = false;

        var timeJumping = 0.0f;

        Debug.Log("Jumping");

        while(timeJumping < jumpForce) {
            yield return null;
            timeJumping += Time.fixedDeltaTime;
        }

        isJumping = false;
        isFalling = true;

        var timeFalling = 0.0f;
        Debug.Log("Falling");

        while(timeFalling < jumpForce) {
            yield return null;
            timeFalling += Time.fixedDeltaTime;
        }

        isJumping = false;
        isFalling = false;
        
        if(onFinish != null) {
            onFinish();
        }

        Debug.Log("Done");
    }

    private void UpdateAnimatorVariables() {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
    }
}
