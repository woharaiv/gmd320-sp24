using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmFollowPlayer : MonoBehaviour
{
    Transform playerTransform;
    [SerializeField] float rotSpeed = 20f;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Vector3 vecToPlayer = transform.position - playerTransform.position;
        vecToPlayer.y = 0;

        //Try to face robot
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vecToPlayer.normalized), rotSpeed * Time.deltaTime);
    }
}
