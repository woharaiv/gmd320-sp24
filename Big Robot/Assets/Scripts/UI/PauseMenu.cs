using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    CursorLockMode prevLockMode = 0;
    bool prevCursorVisible = false;

    private void OnEnable()
    {
        Time.timeScale = 0;
        prevLockMode = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        prevCursorVisible = Cursor.visible;
        Cursor.visible = true;

    }
    private void OnDisable()
    {
        Cursor.lockState = prevLockMode;
        Cursor.visible = prevCursorVisible;
        Time.timeScale = 1;
    }
}
