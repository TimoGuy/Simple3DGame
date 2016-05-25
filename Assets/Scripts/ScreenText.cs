using UnityEngine;
using System.Collections;

public class ScreenText : MonoBehaviour {

	private UnityEngine.UI.Text hudText;
	private UnityEngine.UI.Text healthText;
	private UnityEngine.UI.Text temporaryText;
	private int gameTimeSeconds = 0;
	private int gameTimeMinutes = 0;
	private int gameTimeHours = 0;
	private float highScore = 0;
	private float currentScore = 0;
	private int numDrones = 0;
	private int numDronesDestroyed = 0;
	private int health = 0;
	private int lives = 0;

	// Use this for initialization
	void Start () {
		hudText = GameObject.Find("HudText").GetComponent<UnityEngine.UI.Text>();
		healthText = GameObject.Find("HealthText").GetComponent<UnityEngine.UI.Text>();
		temporaryText = GameObject.Find("TemporaryText").GetComponent<UnityEngine.UI.Text>();
		InvokeRepeating ("GameTicker", Random.Range(0.5F, 1.0F), 1.0F);
	}

	private void GameTicker()
	{
	   gameTimeSeconds++;
	   numDrones = GameObject.FindGameObjectsWithTag ("AttackDrone").Length;
	   if (gameTimeSeconds == 60) {
			gameTimeSeconds = 0;
			gameTimeMinutes++;
			if (gameTimeMinutes == 60) {
				gameTimeMinutes = 0;
				gameTimeHours++;
			}
		}
		DisplayStatusText ();
	}

	public void UpdateHighScore(float inHighScore)
	{
		highScore = inHighScore;
	}

	public void UpdateScore(float inScore)
	{
		currentScore = inScore;
	}
		
	public void UpdateDronesDestroyed(int inDronesDestroyed)
	{
		numDronesDestroyed = inDronesDestroyed;
	}

	public void SetTempText(string input)
	{
		temporaryText.text = input;
		Invoke ("ClearTempText", 3.0F);
	}

	private void ClearTempText()
	{
		temporaryText.text = "";
	}

	public void UpdateHealth(int inputHealth)
	{
		health = inputHealth;
		healthText.text = "Lives: " + lives.ToString () + "\nHealth: " + health.ToString () + "%";
	}

	public void UpdateLives(int inputLives)
	{
		lives = inputLives;
		healthText.text = "Lives: " + lives.ToString () + "\nHealth: " + health.ToString () + "%";
	}

	private string PrintDoubleDigits(int input)
	{
		if (input < 10) {
			return ("0" + input.ToString ());
		}
		return input.ToString ();
	}
	private void DisplayStatusText()
	{
		hudText.text = "Ver: 1.1.30 (Alpha)";
		hudText.text += "\nGame Time: " + PrintDoubleDigits(gameTimeHours) + ":" + PrintDoubleDigits(gameTimeMinutes) + ":" + PrintDoubleDigits(gameTimeSeconds);
		hudText.text += "\nHigh Score: " + highScore.ToString ();
		hudText.text += "\nScore: " + currentScore.ToString ();
		hudText.text += "\nDrones In Game: " + numDrones.ToString ();
		hudText.text += "\nDrones Destroyed: " + numDronesDestroyed.ToString ();
	}
}
