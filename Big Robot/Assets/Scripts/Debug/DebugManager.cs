using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UImGui;
using ImGuiNET;
using System.Linq;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance = null;
    DebugControls debugControls = null;
    DebugControls.DebugActions debugActions;
    Cinemachine.CinemachineInputProvider inputProvider = null;

    static bool firstScene = true;
    private Camera cameraForUImGui = null;

    public bool debugEnabled = false;
    GameObject player = null;

    CursorLockMode prevLockMode = 0;
    bool prevCursorVisible = false;

    const int WARP_BUTTONS_PER_LINE = 5;

    private void Awake()
    {
        if (SingletonInitialize())
        {
            Debug.Log("Successfully intialized DebugManager");
            debugControls = new DebugControls();
            debugActions = debugControls.Debug;
            debugActions.ToggleDebug.performed += ctx => ToggleDebug();
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindWithTag("Player");
        //UImGui throws a warning if you try to set its camera to the camera it's already on, but it lacks a way to check which camera it's on, so that needs to be tracked in here.
        if (cameraForUImGui != GameObject.FindWithTag("MainCamera").GetComponent<Camera>() & !firstScene)
        {
            cameraForUImGui = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            GetComponent<UImGui.UImGui>().SetCamera(cameraForUImGui);
        }
        inputProvider = FindAnyObjectByType<Cinemachine.CinemachineInputProvider>();
        firstScene = false;
    }

    private void OnEnable()
    {
        debugControls.Enable();
        UImGuiUtility.Layout += OnLayout;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        //If the manager is destroyed for not being the singleton, then OnEnable never runs, so OnDisable shouldn't try to undo it.
        if (debugControls != null)
        {
            debugControls.Disable();
            UImGuiUtility.Layout -= OnLayout;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    private bool SingletonInitialize()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }
        else
        {
            Destroy(gameObject);
            return false;
        }
    }

    private void OnLayout(UImGui.UImGui obj)
    {
        if(debugEnabled)
        {
			ImGui.Begin("Debug Options");
            if (player != null)
            {
                if (ImGui.Button("Kill Robot"))
                {
                    FindAnyObjectByType<RobotBehavior>().health = 0;
                }
                if (ImGui.CollapsingHeader("Player Settings"))
                {
                    ImGui.Checkbox("Invincibility", ref player.GetComponent<PlayerHealth>().infiniteHealth);
                    if (ImGui.CollapsingHeader("Warps"))
                    {
                        int i = 0;
                        //Goes through all debug warp locations, sorted by the order number used in editor
                        foreach (DebugWarpLocation location in FindObjectsOfType<DebugWarpLocation>().OrderByDescending(loc => loc.orderInList))
                        {
                            //Keep locations in a grid with a predefined number of columns
                            if (i > 0 && i % WARP_BUTTONS_PER_LINE != 0)
                                ImGui.SameLine();
                            //PushID and PopID probably aren't needed here, but they allow the menu to function as expected if two warps somehow have the same name.
                            ImGui.PushID(i);
                            //When the button is clicked, teleport the player to the warp.
                            if (ImGui.Button(location.name))
                                player.transform.position = location.transform.position;
                            ImGui.PopID();
                            i++;
                        }
                    }
                }
            }
            else
                ImGui.Text("No player loaded");
        }
    }

    private void ToggleDebug()
    {
        Debug.Log("Toggling debug");
        if (!debugEnabled)
        {
            debugEnabled = true;
            
            //Temporarially unlock cursor and disable camera control to work with debug menu
            prevLockMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
            prevCursorVisible = Cursor.visible;
            Cursor.visible = true;
            if(inputProvider != null)
                inputProvider.enabled = false;
        }
        else
        {
            debugEnabled = false;
            Cursor.lockState = prevLockMode;
            Cursor.visible = prevCursorVisible;
            if (inputProvider != null)
                inputProvider.enabled = true;
        }
    }
}
