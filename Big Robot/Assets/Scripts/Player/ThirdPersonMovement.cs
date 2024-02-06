using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;
using DG.Tweening;

public class ThirdPersonMovement : MonoBehaviour
{
    [SerializeField] public Camera activeCamera;
    
    [SerializeField] float moveSpeed;
    [SerializeField] float airSpeed = 0.7f;
    [SerializeField] float rotTime = 7f;

    [SerializeField] float jumpStrength = 5f;
    [SerializeField] float jumpCooldown = 1f;

    [SerializeField] float checkHeight = 0.25f;
    [SerializeField] float checkRadius = 0.5f;
    [SerializeField] LayerMask groundMask;

    [SerializeField] float crouchSquash = 0.5f;
    [SerializeField] float crouchTime = 0.5f;
    [SerializeField] float crouchSpeedMult = 0.6f;
    [SerializeField] float crouchJumpStrength = 1.5f;

    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip crouchSound;
    [SerializeField] AudioClip bigJumpSound;
    [SerializeField] AudioClip hitSound;

    [SerializeField] GameObject crouchJumpPoof;

    public bool canInput = true;

    public const float DRAG = 1f;

    bool isGrounded; 
    bool initJump = false;
    bool canJump = true;

    Vector3 moveVec;
    Vector2 movementInput;
    
    Rigidbody rb;
    Transform body;

    Vector3 respawnPoint;

    bool crouching = false;
    bool canCrouchJump = false;
    Tween crouchTween = null;
    private void Awake()
    {
        respawnPoint = transform.position;
        rb = GetComponent<Rigidbody>();
        body = transform.Find("PlayerBody");
    }
    private void FixedUpdate()
    {
        //Check ground using sphere slightly protuding from the bottom of the body
        //TODO: current calculation for sphere check causes sphere check to sink deep into ground. may want to fix this if it ends up causing issues.
        isGrounded = Physics.CheckSphere(transform.position - (Vector3.up * (body.GetComponent<CapsuleCollider>().height * checkHeight)), checkRadius, groundMask, RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Editor);

        //Apply drag only if grounded
        if (isGrounded)
            rb.drag = 0;
        else rb.drag = DRAG;

        if (canInput)
            HandleMovement();
    }

    void HandleMovement()
    {
        if (initJump)
            Jump();

        //Reset movement vector and reposition forward/back movement to be relative to camera rather than world space
        moveVec = new Vector3(activeCamera.transform.forward.x * movementInput.y, 0, activeCamera.transform.forward.z * movementInput.y);
        //Reposition side to size movement relative to camera
        moveVec += activeCamera.transform.right * movementInput.x;
        //Normalize movement vector to the movement speed
        moveVec = moveVec.normalized * moveSpeed;
        //If airborne, apply air speed penalty and uncrouch.
        if (!isGrounded)
        {
            moveVec *= airSpeed;
            OnCrouchEnd();
        }
        //If crouching, apply crouch move speed penalty
        if (crouching)
            moveVec *= crouchSpeedMult;
        //I can't really explain why I did it like this, but I like how it makes the movement feel.
        rb.AddForce(moveVec - new Vector3(rb.velocity.x, 0, rb.velocity.z), ForceMode.Force);

        //TODO: Change how capping speed works to allow effects that make the character move faster than running speed
        CapSpeed();

        //Rotate body separately from movement to create smoother turning animation
        if (moveVec.magnitude > 0)
        {
            body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(new Vector3(-moveVec.z, 0, moveVec.x)), rotTime * Time.deltaTime);
        }
    }

    public void RecieveInput(Vector2 input)
    {
        if(canInput)
            movementInput = input;
    }

    public void OnJumpPressed()
    {
        if (isGrounded && canJump && canInput)
        {
            initJump = true;
        }
    }

    public void ResetJump()
    {
        canJump = true;
    }

    public void OnCrouchStart()
    {
        if(isGrounded && canInput)
        {
            crouching = true;
            crouchTween = transform.DOScaleY(crouchSquash, crouchTime).OnComplete(OnCrouchReady);
            AudioSource.PlayClipAtPoint(crouchSound, transform.position, 0.25f);
        }
    }

    private void OnCrouchReady()
    {
        canCrouchJump = true;
    }

    public void OnCrouchEnd(bool skipAnim = false)
    {
        //This extra layer allows some game events to cancel crouching on their own when they know they can't double-call OnCrouchCancel
        if (crouching)
            OnCrouchCancel(skipAnim);
    }

    private void OnCrouchCancel(bool skipAnim = false)
    {
        crouching = false;
        canCrouchJump = false;
        if (crouchTween != null)
            crouchTween.Kill();
        crouchTween = transform.DOScaleY(1f, (skipAnim ? 0 : (crouchTime / 2)));
    }

    public void Jump()
    {
        initJump = false;
        canJump = false;
        //Stop any gravity or other downward forces that could mess with the jump
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (canCrouchJump)
        {
            rb.AddForce(transform.up * jumpStrength * crouchJumpStrength, ForceMode.Impulse);
            AudioSource.PlayClipAtPoint(bigJumpSound, transform.position, 0.25f);
            GameObject.Instantiate(crouchJumpPoof, transform.position, transform.rotation);
        }
        else
        {
            rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
        }
        OnCrouchEnd();
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void CapSpeed()
    {
        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (currentVelocity.magnitude > moveSpeed)
        {
            Vector3 maxVelocity = currentVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(maxVelocity.x, rb.velocity.y, maxVelocity.z);
        }
    }

    public void Respawn(int damage = 0)
    {
        OnCrouchEnd(true);
        GetComponent<PlayerHealth>().DealDamage(damage);
        transform.position = respawnPoint;
    }

    public void HitPlayer(GameObject source, float strength = 10, int damage = 1, bool ignoreIFrameIntangibility = false, Vector3? knockbackOverride = null)
    {
        //Don't do damage things if player is still flashing from invincibility
        if (!ignoreIFrameIntangibility && GetComponent<PlayerHealth>().invincibilityTimer > 0)
        {
            return;
        }
        Vector3 knockbackVec = Vector3.zero;
        if (knockbackOverride != null)
            knockbackVec = (Vector3)knockbackOverride;
        else
        {
            knockbackVec = transform.position - source.transform.position;
            knockbackVec.y *= 0;
        }
           
        knockbackVec.Normalize();
        knockbackVec += Vector3.up / 4;
        knockbackVec *= strength;
        rb.velocity = knockbackVec;
        GetComponent<PlayerHealth>().DealDamage(damage);
        AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.25f);
    }

    public void CutsceneBeginStopPlayer()
    {
        canInput = false;
        rb.velocity = Vector3.zero;
        OnCrouchCancel(true);
        body.rotation = Quaternion.LookRotation(new Vector3(activeCamera.transform.position.z - transform.position.z, 0, activeCamera.transform.position.x - transform.position.x));
    }
    public void CutsceneEndResumePlayer() 
    {
        canInput = true;
    }
}
