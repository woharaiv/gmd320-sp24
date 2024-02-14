using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RobotBehavior : MonoBehaviour
{
    Transform playerTransform;
    [SerializeField] float rotSpeed = 20f;

    [SerializeField] Crusher crusherScript;
    [SerializeField] GameObject parryIndicator;

    [SerializeField] Vector2 attackTimingRange;
    float attackTimer;

    [SerializeField] Vector2 parryTimingRange;
    [SerializeField] float parryWindow = 0.75f;

    bool parryOpen = false;

    bool recoiling = false;

    public int maxHealth = 100;
    public int health;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        crusherScript.robotScript = this;
        attackTimer = UnityEngine.Random.Range(attackTimingRange.x, attackTimingRange.y);
        parryIndicator.SetActive(false);
        health = maxHealth;
    }

    void Update()
    {
        Vector3 vecToPlayer = transform.position - playerTransform.position;
        vecToPlayer.y = 0;

        //Constantly face player
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vecToPlayer.normalized), rotSpeed * Time.deltaTime);
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("RobotIdle"))
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0) 
            {
                GetComponent<Animator>().Play("SmashIn");
                attackTimer = UnityEngine.Random.Range(attackTimingRange.x, attackTimingRange.y);
            }
        }   
    }

    private void OnTriggerEnter(Collider other)
    {
        if(string.Compare(other.name, "Sword") == 0)
        {
            health--;
            if (health <= 0)
            {
                LoadSceneManager.instance.LoadEnd();
            }
        }
    }

    public void AttackParried(ThirdPersonMovement player)
    {
        player.BeginClash();
        GetComponent<Animator>().Play("SmashClash");
        Invoke(nameof(ParryOpening), UnityEngine.Random.Range(parryTimingRange.x, parryTimingRange.y));
    }

    public void AttackKnockedBack()
    {
        if(parryOpen) 
        {
            GetComponent<Animator>().Play("ParriedRecoil");
            health = Math.Clamp(health - 10, 1, maxHealth);
            recoiling = true;
            parryOpen = false;
            playerTransform.GetComponent<ThirdPersonMovement>().OnParrySuccess();
            parryIndicator.SetActive(false);
        }
    }

    void ParryOpening()
    {
        parryIndicator.SetActive(true);
        parryOpen = true;
        Invoke(nameof(ParryClose), parryWindow);
    }

    void ParryClose()
    {
        if (recoiling)
            recoiling = false;
        else
        {
            parryIndicator.SetActive(false);
            parryOpen = false;
            GetComponent<Animator>().Play("SmashUp", -1, 0.384f);
            playerTransform.GetComponent<ThirdPersonMovement>().OnBlockCancel();
        }
    }
}
