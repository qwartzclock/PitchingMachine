using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.XR;


//Go back to MM when finish
//Add players
//add random words
//add temp visuals + audios

public enum GameState
{
    MainMenu = 0,
    GameCountdown = 1,
    Pitching = 2,
    GameOver = 3
}

public class GameController : MonoBehaviour
{
    private GameState state;

    public CanvasGroup MainMenu;
    public CanvasGroup GameScreen;
    public List<string> topics;
    public List<string> themes;

    public Button GameTimeButton;
    public Button PitchTimeButton;
    public List<int> timings;
    private int gameTimingIndex = 0;
    private int pitchTimingIndex = 0;

    public void Start()
    {
        ReturnToMainMenu();
        AutoFillTopicsThemes();
        AutoSetTimers();
        state = GameState.MainMenu;
    }

    private void AutoSetTimers()
    {
        Text gameTimeTxt = GameTimeButton.transform.GetComponentInChildren<Text>();
        gameTimeTxt.text = TimeSpan.FromSeconds(timings[gameTimingIndex]).ToString("mm':'ss");
        Text pitchTimeTxt = PitchTimeButton.transform.GetComponentInChildren<Text>();
        pitchTimeTxt.text = TimeSpan.FromSeconds(timings[pitchTimingIndex]).ToString("mm':'ss");
	}

    private void AutoFillTopicsThemes()
	{
        if (topics.Count == 0)
        {
            topics.Add("Game");
            topics.Add("Musical");
            topics.Add("Story");
        }
        if (themes.Count == 0)
        {
            themes.Add("Theme1");
            themes.Add("Theme2");
            themes.Add("Theme3");
        }
	}

    public void ReturnToMainMenu()
    {
        
        MainMenu.gameObject.SetActive(true);
        GameScreen.gameObject.SetActive(false);

    }

    public void ChangeGameTimer()
	{
        gameTimingIndex++;
        if (gameTimingIndex == timings.Count)
            gameTimingIndex = 0;
        Text gameTimeTxt = GameTimeButton.transform.GetComponentInChildren<Text>();
        gameTimeTxt.text = TimeSpan.FromSeconds(timings[gameTimingIndex]).ToString("mm':'ss");
    }
    public void ChangePitchTimer()
	{
        pitchTimingIndex++;
        if (pitchTimingIndex == timings.Count)
            pitchTimingIndex = 0;
        Text pitchTimeTxt = PitchTimeButton.transform.GetComponentInChildren<Text>();
        pitchTimeTxt.text = TimeSpan.FromSeconds(timings[pitchTimingIndex]).ToString("mm':'ss");
    }

    public void StartGame()
    {
        state = GameState.GameCountdown;

        InitializeGameVars();

        GameScreen.gameObject.SetActive(true);
        MainMenu.gameObject.SetActive(false);
    }

    private void InitializeGameVars()
	{
        string topic;
        string theme;

        //Get text for topic and theme
        Transform inputTopic = MainMenu.gameObject.transform.Find("InputTopic");
        Transform topicTextTransf = inputTopic.GetChild(1).Find("Text");
        topic = topicTextTransf.gameObject.GetComponent<Text>().text;

        Transform inputTheme = MainMenu.gameObject.transform.Find("InputTheme");
        Transform themeTextTransf = inputTheme.GetChild(1).Find("Text");
        theme = themeTextTransf.gameObject.GetComponent<Text>().text;

        if (String.IsNullOrEmpty(theme))
            theme = themes[UnityEngine.Random.Range(0, themes.Count)];

        //Pass strings to GameScreen texts
        Text topicTitleTxt = GameScreen.transform.Find("Topic").gameObject.GetComponent<Text>();
        Text themeTitleTxt = GameScreen.transform.Find("Theme").gameObject.GetComponent<Text>();
        topicTitleTxt.text = "Topic: " + topic;
        themeTitleTxt.text = "Theme: " + theme;

        gameTimer = (float)timings[gameTimingIndex];
        pitchTimer = (float)timings[pitchTimingIndex];

        ChangeState(GameState.GameCountdown);
        UpdateTimerDisplay(0); UpdateTimerDisplay(1);

        ChangeActivePlayer(0);
	}

    private float gameTimer;
    private float pitchTimer;
    public Text GameTimerDisplay;
    public Text PitchTimerDisplay;
    private int activePlayer = 0;
    public GameObject PlayerListUI;

    private void ChangeState(GameState g)
	{
        if (state == GameState.Pitching && g != GameState.Pitching)
            pitchTimer = (float)timings[pitchTimingIndex];
        state = g;
	}

	void Update()
	{
		switch (state)
		{
            case GameState.GameCountdown:
                
                gameTimer -= Time.deltaTime;
                int pitchRequesta = CheckPlayerWantsToPitch();
                if (pitchRequesta > 0)
                {
                    ChangeState(GameState.Pitching);
                    ChangeActivePlayer(pitchRequesta);
                }

                if (gameTimer <= 0)
                {
                    ChangeState(GameState.GameOver);
                }

                UpdateTimerDisplay(0);
                break;

            case GameState.Pitching:

                pitchTimer -= Time.deltaTime;
                int pitchRequest = CheckPlayerWantsToPitch();
                if (pitchRequest > 0) {
                    if (pitchRequest == activePlayer)
                    {
                        ChangeActivePlayer(0);
                        ChangeState(GameState.GameCountdown);
                    }
                    else
                    {
                        ChangeActivePlayer(pitchRequest);
                    }
                }
                if (pitchTimer <= 0)
                {
                    ChangeState(GameState.GameCountdown);
                }

                UpdateTimerDisplay(1);
                break;

            case GameState.GameOver:
                // Maybe a gameover screen?
                ReturnToMainMenu();
                break;
		}
	}

    private string GenerateScore()
	{
        return UnityEngine.Random.Range(1000.0f, 10000.0f).ToString();
	}

    private void AddScore(int player)
	{
        if (player > 0 && player < 5)
        {
            //Get the scorecard 
            Transform scorecard = GameScreen.transform.Find("PlayersInGame");
            Text playerScore = scorecard.GetChild(player - 1).GetChild(0).GetComponent<Text>();
            playerScore.text = "Player " + player + " \tScore " + GenerateScore();
        }
	}

    private void ChangeActivePlayer(int pitchRequest)
	{
        if (activePlayer > 0)
        {
            Transform currActivePlayerCard = PlayerListUI.transform.GetChild(activePlayer - 1);
            currActivePlayerCard.GetChild(1).gameObject.SetActive(false);
        }
        if (pitchRequest > 0)
        { 
            activePlayer = pitchRequest;
            Transform newActivePlayerCard = PlayerListUI.transform.GetChild(pitchRequest - 1);
            newActivePlayerCard.GetChild(1).gameObject.SetActive(true);
            AddScore(activePlayer);
        }
	}

    // timer: 0 == Game Timer, 1 == Pitch Timer
    private void UpdateTimerDisplay(int timer)
	{
        switch (timer)
		{
            case 0:
                GameTimerDisplay.text = TimeSpan.FromSeconds(gameTimer).ToString("mm':'ss");
                break;
            case 1:
                PitchTimerDisplay.text = TimeSpan.FromSeconds(pitchTimer).ToString("mm':'ss");
                break;
        }
        
	}

    private int CheckPlayerWantsToPitch()
	{
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 1;
        if (Input.GetButtonDown("Joy1")) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return 2;
        if (Input.GetButtonDown("Joy2")) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return 3;
        if (Input.GetButtonDown("Joy3")) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) return 4;
        if (Input.GetButtonDown("Joy4")) return 4;
        return 0;
	}

    public void StartPitch()
	{
        if (state == GameState.GameCountdown)
		{
            ChangeState(GameState.Pitching);
		}
	}
}
