using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OutOfBounds"))
            gameObject.GetComponent<ThirdPersonMovement>().Respawn(0);
    }
}
