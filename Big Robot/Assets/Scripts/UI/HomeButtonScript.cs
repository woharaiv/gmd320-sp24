using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeButtonScript : MonoBehaviour
{   void Start()
    {
        //Grabs the scene manager so that the button can find it after the game resets
        GetComponent<Button>().onClick.AddListener(LoadSceneManager.instance.LoadStart);
    }
}
