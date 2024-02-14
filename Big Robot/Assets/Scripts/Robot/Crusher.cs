using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusher : MonoBehaviour
{
    public RobotBehavior robotScript;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Contact");
        ThirdPersonMovement player = other.GetComponentInParent<ThirdPersonMovement>();
        if (player != null && robotScript.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("SmashDown"))
        {
            Debug.Log("Touched Player");
            if (player.canParry)
            {
                robotScript.AttackParried(player);
            }
            else
            {
                player.HitPlayer(robotScript.gameObject, 10, 0, false, Vector3.zero);
                Debug.Log("Damaged Player");
            }
        }
        else
            Debug.Log(other.gameObject);
    }
}
