using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthMeter : MonoBehaviour
{
    [SerializeField] RobotBehavior healthSystem;
    void Update()
    {
        GetComponent<Image>().fillAmount = ((float)healthSystem.health / healthSystem.maxHealth);
    }
}
