using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Kill"))
            gameObject.GetComponent<ThirdPersonMovement>().Respawn(1);
    }
}
