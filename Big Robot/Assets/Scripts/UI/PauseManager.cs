using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    PlayerControls playerControls;
    PlayerControls.PauseMenuActions pauseMenuActions;

    public GameObject pauseMenu;

    public static bool paused = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
        pauseMenuActions = playerControls.PauseMenu;
        pauseMenuActions.TogglePause.performed += ctx => PauseUnpause();
    }
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void PauseUnpause()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        paused = pauseMenu.activeSelf;
        if (!paused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
