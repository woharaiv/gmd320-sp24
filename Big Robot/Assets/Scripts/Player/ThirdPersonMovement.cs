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
    [SerializeField] float rotSpeed = 7f;

    Transform robotTransform;
    Vector3 vecToRobot;

    [SerializeField] LayerMask groundMask;

    [SerializeField] float attackCooldown = 0.3125f;
    float attackTimer = 0;
    bool attackDir = false;
    
    [SerializeField] float blockingSpeedMod = 0f;
    [SerializeField] float timeBeforeParry = 0.25f;
    [SerializeField] float parryWindow = 0.25f;
    bool bufferingBlockEnd = false;
    
    [SerializeField] AudioClip[] attackSounds;
    [SerializeField] AudioClip blockSound;
    [SerializeField] AudioClip parryReadySound;
    [SerializeField] AudioClip hitSound;

    public bool canInput = true;

    public const float DRAG = 1f;

    bool isGrounded; 
    
    Vector3 moveVec;
    Vector2 movementInput;
    
    Rigidbody rb;
    Transform body;

    Vector3 respawnPoint;

    bool blocking = false;
    //Is the window to start a parry ready?
    bool canParry = false;
    //Should the code attempt to run the parry function?
    bool doParry = false;

    private void Awake()
    {
        respawnPoint = transform.position;
        rb = GetComponent<Rigidbody>();
        body = transform.Find("PlayerBody");
        rb.drag = DRAG;
        robotTransform = GameObject.FindGameObjectWithTag("Enemy").transform;
    }
    private void FixedUpdate()
    {
        if(attackTimer > 0)
            attackTimer -= Time.deltaTime;
        if (canInput)
            HandleMovement();

    }

    void HandleMovement()
    {
        //Reset movement vector and reposition forward/back movement to be relative to camera rather than world space
        moveVec = new Vector3(activeCamera.transform.forward.x * movementInput.y, 0, activeCamera.transform.forward.z * movementInput.y);
        //Reposition horizontal movement relative to camera
        moveVec += activeCamera.transform.right * movementInput.x;
        //Normalize movement vector to the movement speed
        moveVec = moveVec.normalized * moveSpeed;
        //If blocking, apply block move speed penalty (currently means no movement)
        if (blocking)
            moveVec *= blockingSpeedMod;
        //I can't really explain why I did it like this, but I like how it makes the movement feel.
        rb.AddForce(moveVec - new Vector3(rb.velocity.x, 0, rb.velocity.z), ForceMode.Force);

        CapSpeed();

        vecToRobot = robotTransform.position - transform.position;
        vecToRobot.y = 0;

        //Try to face robot
        body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(vecToRobot.normalized), rotSpeed * Time.deltaTime);
    }

    public void RecieveInput(Vector2 input)
    {
        if(canInput)
            movementInput = input;
    }

    public void OnAttackPressed()
    {
        if (attackTimer <= 0 && !blocking && canInput)
        {
            Debug.Log("Attack");
            AudioSource.PlayClipAtPoint(attackSounds[Random.Range(0, attackSounds.Length)], transform.position);
            attackDir = !attackDir;
            switch(attackDir)
            {
                case false:
                    GetComponent<Animator>().Play("SwordSwingL");
                    break;
                default:
                    GetComponent<Animator>().Play("SwordSwingR");
                    break;
            }
            attackTimer = attackCooldown;
        }
    }

    public void OnBlockStart()
    {
        if(canInput && attackTimer <= 0)
        {
            blocking = true;
            doParry = true;
            Debug.Log("Block start");
            GetComponent<Animator>().Play("BlockStart");
            Invoke(nameof(OnParryReady), timeBeforeParry);
            //AudioSource.PlayClipAtPoint(blockSound, transform.position, 0.25f);
        }
    }

    private void OnParryReady()
    {
        //Don't do this if doParry is false (probably means the block was canceled before the parry window)
        if (!doParry)
            return;
        canParry = true;
        Debug.Log("Parry");
        //AudioSource.PlayClipAtPoint(parryReadySound, transform.position, 1);
        Invoke(nameof(OnParryFinished), parryWindow);
    }

    private void OnParryFinished()
    {
        canParry = false;
        if(bufferingBlockEnd)
            OnBlockCancel();
    }

    public void OnBlockEnd()
    {
        Debug.Log(GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].ToString());
        //If still playing out the blocking animation, buffer the block end and don't play it until the block has finished.
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            bufferingBlockEnd = true;
        else //If the character is sitting at the end of the block animation, just end the block now.
            OnBlockCancel();
    }

    private void OnBlockCancel(bool skipAnim = false)
    {
        Debug.Log("Block end");
        canParry = false;
        doParry = false;
        bufferingBlockEnd = false;
        if(!skipAnim)
            GetComponent<Animator>().Play("BlockEnd");
        blocking = false;
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
        OnBlockEnd();
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
        OnBlockCancel(true);
        body.rotation = Quaternion.LookRotation(new Vector3(activeCamera.transform.position.z - transform.position.z, 0, activeCamera.transform.position.x - transform.position.x));
    }
    public void CutsceneEndResumePlayer() 
    {
        canInput = true;
    }
}
