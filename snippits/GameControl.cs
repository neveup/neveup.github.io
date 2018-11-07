using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// The game controller that is shared across all scenes
/// </summary>
public class GameControl : MonoBehaviour
{
    /// <summary>
    /// Instance of the GameController
    /// </summary>
    public static GameControl Control;

    /// <summary>
    /// The model for the stored game data
    /// </summary>
    [HideInInspector]
    public GameData GameDataModel;

    /// <summary>
    /// The index of the current scene
    /// </summary>
    [HideInInspector]
    public int CurrentLevel;

    /// <summary>
    /// The next scene to load by index
    /// </summary>
    [HideInInspector]
    public int LevelIndexToLoad = 0;

    /// <summary>
    /// The next scene to load by index
    /// </summary>
    [HideInInspector]
    public string LevelNameToLoad = string.Empty;

    /// <summary>
    /// Boolean for setting if the the index or name shoul be used when loading next scene
    /// </summary>
    [HideInInspector]
    public bool UseLevelIndex = true;

    /// <summary>
    /// The index for the end game scene
    /// </summary>
    private int endgameIndex;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (Control == null)
        {
            DontDestroyOnLoad(gameObject);
            Control = this;
        }
        else if (Control != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        GameDataModel = new GameData();
        Load();
        if (GameDataModel.Levels == null)
        {
            GameDataModel.Levels = new List<LevelData>();
            Save();
        }

        endgameIndex = SceneManager.GetSceneByName("EndGame").buildIndex;
    }

	/// <summary>
    /// This function is called when saving the game data to a file
    /// </summary>
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gameData");

        bf.Serialize(file, GameDataModel);
        file.Close();
    }

	/// <summary>
    /// This function is called when loading the game data from a file
    /// </summary>
    public void Load()
    {
        BinaryFormatter bf = new BinaryFormatter();
        var filePath = Application.persistentDataPath + "/gameData";
        if (File.Exists(filePath))
        {
            FileStream file = File.Open(filePath, FileMode.Open);
            if (file != null)
            {
                GameDataModel = (GameData)bf.Deserialize(file);
                file.Close();
            }
        }
        else
        {
            Debug.Log("No file at " + filePath + " was found, no file loaded");
        }

    }

	/// <summary>
    /// This function calculates the final score
    /// </summary>
	/// <param name="timeLeft">Time remaining</param>
	/// <param name="timeScoreMultiplier">Time score multiplier</param>
	/// <param name="crashScore">The total crash score</param>
	/// <param name="crashCount">The number of total crashes</param>
	/// <param name="currentLevelData">The level for the currnet active scene</param>
    public void CalculateScore(float timeLeft, int timeScoreMultiplier, int crashScore, int crashCount, LevelData currentLevelData)
    {
        if (timeLeft >= 0)
        {
            var totalScore = (timeLeft * timeScoreMultiplier) - crashScore;

            if (crashCount > 0)
            {
                totalScore = totalScore / crashCount;
            }

            var displayScore = Mathf.Round(totalScore);
            currentLevelData.LastScore = displayScore;

            if (displayScore > currentLevelData.HighestScore)
            {
                currentLevelData.HighestScore = displayScore;
            }
        }
        else
        {
            var totalScore = 0;
            currentLevelData.LastScore = totalScore;

        }

        AddLevelDataToModel(currentLevelData);
        SceneManager.LoadScene("EndScene");
    }

	/// <summary>
    /// This function loads a new scene by name
    /// </summary>
	/// <param name="sceneName">Name of next scene</param>
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        LevelNameToLoad = sceneName;
        UseLevelIndex = false;
        SceneManager.LoadScene("Loading");
    }

	/// <summary>
    /// This function loads a new scene by index
    /// </summary>
	/// <param name="sceneId">Index of next scene</param>
    public void LoadScene(int sceneId)
    {
        Time.timeScale = 1f;
        LevelIndexToLoad = sceneId;
        UseLevelIndex = true;
        SceneManager.LoadScene("Loading");
    }

	/// <summary>
    /// This function adds an unplayed scene to the game data model
    /// </summary>
	/// <param name="currentLevelData">Current scenes level data</param>
    private void AddLevelDataToModel(LevelData currentLevelData)
    {
        bool _updated = false;
        var totalLevels = 0;
        if (GameControl.Control.GameDataModel.Levels != null)
        {
            totalLevels = GameControl.Control.GameDataModel.Levels.Count;

            GameControl.Control.GameDataModel.CurrentLevel = CurrentLevel;

            if (totalLevels > 0)
            {
                for (int i = 0; i < totalLevels; i++)
                {
                    if (GameControl.Control.GameDataModel.Levels[i].LevelId == currentLevelData.LevelId)
                    {
                        GameControl.Control.GameDataModel.Levels[i] = currentLevelData;
                        _updated = true;
                    }
                }
            }
        }

        if (!_updated)
        {
            GameControl.Control.GameDataModel.Levels.Add(currentLevelData);
        }
    }

	/// <summary>
    /// This function returns all the level data for the current scene
    /// </summary>
    public LevelData GetLevelData()
    {
        LevelData _levelData = null;
        if (GameControl.Control.GameDataModel.Levels != null)
        {
            foreach (var item in GameControl.Control.GameDataModel.Levels)
            {
                if (item.LevelId == GameControl.Control.CurrentLevel)
                {
                    _levelData = item;
                }
            }
        }

        if (_levelData == null)
        {
            _levelData = new LevelData();
            _levelData.LevelId = GameControl.Control.CurrentLevel;
            _levelData.HighestScore = 0;
            _levelData.LastScore = 0;
        }

        return _levelData;
    }


	/// <summary>
    /// This function validates there is a next scene to load
    /// </summary>
	/// <param name="nextLevel">The index of the next scene</param>
    public bool ValidGameScene(int nextLevel)
    {
        if (!SceneManager.GetSceneByBuildIndex(nextLevel).IsValid() && nextLevel == endgameIndex)
        {
            return false;
        }

        return true;
    }
}






