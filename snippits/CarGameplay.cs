using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class CarGameplay : MonoBehaviour 
{
	public int CrashDamage = 500;
	public GameObject TimerUI;
	public TextMeshProUGUI TimerText;
	public TextMeshProUGUI CompleteText;	
	public float MaxLevelTime = 60f;
	public int TimeScoreMultiplier = 1000;

	
	private float timeLeft;
	private bool timerGo = false;
	private int crashCount = 0;
	private int crashScore = 0;
	private LevelData currentLevelData = null;


	/// <summary>
	/// This function is called when the object becomes enabled and active.
	/// </summary>
	void OnEnable()
	{
		FloatingDamageTextController.Initialize();
		GameControl.Control.CurrentLevel = SceneManager.GetActiveScene().buildIndex;
		currentLevelData =  GameControl.Control.GetLevelData();
		CompleteText.enabled = false;
	}

	/// <summary>
	/// Use start method for initialization
	/// </summary>
	void Start () {

		StartTimer();
		TimerText.text = MaxLevelTime.ToString();
		timeLeft = MaxLevelTime;

		crashCount = 0;
		crashScore = 0;
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		if(timerGo)
		{
			timeLeft -= Time.deltaTime;
         	if(timeLeft < 0)
         	{
				GameControl.Control.CalculateScore(timeLeft, TimeScoreMultiplier, crashScore, crashCount, currentLevelData);
         	}
		 	TimerText.text = timeLeft.ToString("#.00");
		}
	}
	
	/// <summary>
	/// OnCollisionEnter is called when this collider/rigidbody has begun
	/// touching another rigidbody/collider.
	/// </summary>
	/// <param name="other">The Collision data associated with this collision.</param>
	void OnCollisionEnter(Collision other)
	{
		if(other.transform.tag != "Touchable")
		{
			FindObjectOfType<AudioManager>().Play("Crash");
			FloatingDamageTextController.CreateDamageText("-" + CrashDamage.ToString());
			AddCrash(CrashDamage);
		}
		
	}

	public void StartTimer()
	{
		TimerUI.SetActive(true);
		timerGo = true;
	}

	/// <summary>
	/// Stop the timer and call the calculate score method
	/// </summary>
	public void StopTimer()
	{
		timerGo = false;
		TimerUI.SetActive(false);

		if(CompleteText != null)
			CompleteText.enabled = true;
			
		GameControl.Control.CalculateScore(timeLeft, TimeScoreMultiplier, crashScore, crashCount, currentLevelData);
	}

	/// <summary>
	/// When a crash is detected increment the crash count by 1 and add points to the crash score.false the crash score is taken away from the final score
	/// </summary>
	/// <param name="crashPoints">The set amount of points to be added to the crash srore</param>
	public void AddCrash(int crashPoints)
	{
		crashCount++;
		crashScore = crashScore + crashPoints;
	}

}
