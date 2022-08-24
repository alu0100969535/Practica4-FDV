using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using alu0100969535.Utils;

public class PlayerMovement : MonoBehaviour {

    [Header("Controls")]
    [SerializeField] private string axisHorizontal;
    [SerializeField] private string axisVertical;
    [SerializeField] private KeyCode keyUp;
    [SerializeField] private KeyCode keyLeft;
    [SerializeField] private KeyCode keyDown;
    [SerializeField] private KeyCode keyRight;

    [SerializeField] private KeyCode jumpKey;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float jumpForce = 2f;

    private Animator animator;
    private Rigidbody2D rigidbody;
    private Collider2D collider;
    private SpriteRenderer spriteRenderer;

    private Dictionary<KeyCode, Action<bool>> bindedKeys = new Dictionary<KeyCode, Action<bool>>();
    private Dictionary<string, Action<float>> bindedAxis = new Dictionary<string, Action<float>>();

    private bool isJumpingButtonPressed;

    private float speedH = 0.0f;
    private float speedV = -1f;
    private bool isJumping = false;
    private bool isCrouching = false;

    private bool isFacingRight = true;
    private bool canJump = true;

    Vector2 left;
    Vector2 right;
    Vector2 top;
    Vector2 bottom;

    void Awake() {
        animator = Utils.Get<Animator>(gameObject, "PlayerMovement needs an Animator");
        rigidbody = Utils.Get<Rigidbody2D>(gameObject, "PlayerMovement needs a Rigidbody2D");
        collider = Utils.Get<Collider2D>(gameObject, "PlayerMovement needs a Collider2D");
        spriteRenderer = Utils.Get<SpriteRenderer>(gameObject, "PlayerMovements needs a SpriteRenderer");

        BuildDictionaryOfBindings();
    }

    void FixedUpdate() {
        ProcessKeyBindings();
        UpdateAnimation();

        CalculateBoundingBox();

        MovePlayerHorizontally();
        MovePlayerVertically();
    }

    void ProcessKeyBindings() {
        const float defaultSpeed = 0.0f;

        speedH = defaultSpeed;
        isJumpingButtonPressed = false;
        isCrouching = false;

        foreach (var entry in bindedAxis) {
            float value = Input.GetAxis(entry.Key);
            entry.Value(value);
        }

        foreach (var entry in bindedKeys) {
            bool isPressed = Input.GetKey(entry.Key);
            entry.Value(isPressed);
        }
    }

    void UpdateAnimation() {
        FlipIfNeeded(speedH);
        animator.SetFloat("SpeedH", Mathf.Abs(speedH));
        animator.SetFloat("SpeedV", speedV);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsCrouching", isCrouching);
    }

    void CalculateBoundingBox() {
        Vector2 center = collider.bounds.center;
        Vector2 extents = collider.bounds.extents;

        left = center;
        left.x -= extents.x * 1.01f;

        right = center;
        right.x += extents.x * 1.01f;

        top = center;
        top.y += extents.y * 0.95f;

        bottom = center;
        bottom.y -= extents.y * 0.95f;
    }

    void MovePlayerHorizontally() {
        if(speedH == 0.0f || isCrouching) {
            return;
        } 

        if(speedH > 0 && !CanMoveRight() || speedH < 0 && !CanMoveLeft()){
            Debug.Log("Cannot move!");
            return;
        }
        
        var position = this.transform.position;
        position.x = position.x + speedH * movementSpeed * Time.fixedDeltaTime;
        this.transform.position = position;
    }

    bool CanMoveRight() {
        RaycastHit2D hit = Physics2D.Raycast(right, Vector2.right, 0.01f);

        if (hit.collider != null) {
            return false;
        }   

        return true;
    }

    bool CanMoveLeft() {
        RaycastHit2D hit = Physics2D.Raycast(left, Vector2.left, 0.01f);

        if (hit.collider != null) {
            return false;
        }   

        return true;
    }

    void MovePlayerVertically() {
        if(isJumpingButtonPressed && !isJumping && canJump){
            StartCoroutine(PerformJump());
        }

        if(speedV != 0.0f) {
            var position = this.transform.position;
            position.y = position.y + speedV * Time.fixedDeltaTime;
            this.transform.position = position;
        }
    }
    
    IEnumerator PerformJump() {
        isJumping = true;
        canJump = false;
        speedV = 10.0f;

        var timeJumping = 0.0f;
        var timeLimit = jumpForce;

        while((isJumpingButtonPressed && timeJumping < jumpForce * 3) || timeJumping < jumpForce) {
            yield return null;
            timeJumping += Time.fixedDeltaTime;
        }

        speedV = -10.0f;
    }

    void BuildDictionaryOfBindings() {


        bindedKeys.Add(keyUp, (isPressed) => {
            isJumpingButtonPressed |= isPressed;
        });

        bindedKeys.Add(keyRight, (isPressed) => {
            if (isPressed) {
                speedH = 1.0f;
            }
        });

        bindedKeys.Add(keyLeft, (isPressed) => {
            if (isPressed) {
                speedH = -1.0f;
            }
        });

        bindedKeys.Add(keyDown, (isPressed) => {
            isCrouching |= isPressed;
        });
        
        bindedKeys.Add(jumpKey, (isPressed) => {
            isJumpingButtonPressed |= isPressed;
        });

        // Axis

        /*bindedAxis.Add(axisHorizontal, (value) => {
            if(speedH != defaultSpeed) {
                speedH = value;
            }
        });*/

        /*bindedAxis.Add(axisVertical, (value) => {
            speedV = value;
            if (speedV == 0.0f) {
                isCrouching = false;
                isJumping = false;
            } else {
                isJumping = speedV > 0.0f;
                isCrouching = speedV < 0.0f;
            }
        });*/
    }

    private void FlipIfNeeded(float speedH) {

        if(speedH == 0.0f) {
            return;
        }
        isFacingRight = speedH < 0;
        UpdateFlip();
    }

    private void UpdateFlip() {
        spriteRenderer.flipX = isFacingRight;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        var contacts = collision.contacts;

        CalculateBoundingBox();

        foreach(var contact in contacts) {
            Vector3 contactPoint = contact.point;

            if(contactPoint.x <= left.x && contactPoint.x >= right.x){
                continue;
            }

            if(contactPoint.y <= bottom.y) {
                Debug.Log("Bottom collision");
                StopFalling();
                continue;
            }

            if(contactPoint.y >= top.y) {
                Debug.Log("Top collision");
                StopJumping();
                continue;
            }
        }

    }

    void StopFalling() {
        speedV = 0.0f;
        isJumping = false;
        canJump = true;
    }

    void StopJumping() {
        isJumping = false;
    }
}
