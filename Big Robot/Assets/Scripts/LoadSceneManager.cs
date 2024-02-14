using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    float timer;
    public string timerString = string.Empty;
    TextMeshProUGUI timerText;
    float bestTime = 9999999;
    public string bestTimeString = string.Empty;
    TextMeshProUGUI bestTimeText;
    bool timing = false;
    public static LoadSceneManager instance;
    //Ensures this is the only instance ofthe singelton
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (timing)
        {
            timer += Time.deltaTime;
            timerString = string.Format("{0:N0}:{1:N2}", Mathf.Floor(timer / 60), timer % 60);
            timerText.text = ("Time:\n" + timerString);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //For some reason, I can't unlock the cursor from LoadStart, so I have to do this instead.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.Equals(scene.name, "GameScene"))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            timerText = GameObject.FindGameObjectWithTag("Timer").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            bestTimeText = GameObject.FindGameObjectWithTag("BestTime").GetComponent<TextMeshProUGUI>();
            if(bestTimeText != null)
            {
                bestTimeText.text = ("Best Time:\n" + bestTimeString);
            }
        }

    }

    public void LoadGame()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("GameScene");
        timer = 0;
        timing = true;
    }

    public void LoadEnd()
    {
        SceneManager.LoadScene("EndScene");
        timing = false;
        if (timer < bestTime) 
        { 
            bestTime = timer;
            bestTimeString = string.Format("{0:N0}:{1:N2}", Mathf.Floor(bestTime / 60), bestTime % 60);
        }
    }

    public void LoadStart()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
