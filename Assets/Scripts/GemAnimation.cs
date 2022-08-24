using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using alu0100969535.Utils;

public class GemAnimation : MonoBehaviour {
    [Header("Controls")]
    [SerializeField] private KeyCode playAnimationKey;

    private Animator animator;


    void Awake() {
        animator = Utils.Get<Animator>(gameObject, "PlayerMovement needs an Animator");
    }

    void FixedUpdate() {
        if(CanPlayAnimation()){
            PlayAnimation();
        }
    }   

    private bool CanPlayAnimation() {
        return isAnimationKeyPressed();
    }

    private bool isAnimationKeyPressed() {
        return Input.GetKey(playAnimationKey);
    }

    private void PlayAnimation() {
        animator.Play("gem-idle");
    }

}
