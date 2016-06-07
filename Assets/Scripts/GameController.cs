using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public GameObject heavyTurretObject;
	public GameObject turretObject;
	public GameObject battleCruiserObject;
	public GameObject battleCruiserSpawnPoint;
	public GameObject AttackDroneModel;
	public GameObject portalModel;
	public GameObject AlliedDroneModel;
	public GameObject TacticalDroneModel;
	public GameObject AlliedTacticalDroneModel;
	public GameObject GameOverOverlay;

	private int battlecruiser_speed = 25;
	private bool endGuyBuilt = false;
	private int num_cruisers_in_wave = 0;
	private float highScore = 0;
	private float scoreValue = 0;
	private int dronesCreated = 0;
	private int alliedDronesCreated = 0;
	private float droneLaunchTime = 20;
	private float cruiserLaunchTime = 300.0F;
	private UnityEngine.UI.Text hudText;
	private UnityEngine.UI.Text temporaryText;
	private UnityEngine.UI.Text weaponText;
	private UnityEngine.UI.Text targetText;
	private Image damageImage;
	private Color flashColor = new Color (1f, 0f, 0f, 0.1f);
	private Slider health_bar;
	private int gameTimeSeconds = 0;
	private int gameTimeMinutes = 0;
	private int gameTimeHours = 0;
	private int numDronesDestroyed = 0;
	private int health = 0;
	private int most_battlecruisers_passed = 0;
	private int cruisers_launched = 0;
	private float default_drone_launch_time = 30;
	private int tactical_drone_launch_bound = 10;
	private bool damaged = false;

	private static string version = "1.1.51 (Alpha)";

	// Use this for initialization
	void Start () {
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		if (difficulty == 0) {
			default_drone_launch_time = 45;
			droneLaunchTime = 30;
			cruiserLaunchTime = 200;
			tactical_drone_launch_bound = 20;
		} else if (difficulty == 1) {
			default_drone_launch_time = 30;
			droneLaunchTime = 20;
			cruiserLaunchTime = 180;
			tactical_drone_launch_bound = 10;
		} else {
			default_drone_launch_time = 20;
			droneLaunchTime = 15;
			cruiserLaunchTime = 120;
			tactical_drone_launch_bound = 5;
		}
		damageImage = GameObject.Find ("DamageImage").GetComponent<Image> ();
		hudText = GameObject.Find("HudText").GetComponent<UnityEngine.UI.Text>();
		temporaryText = GameObject.Find("TemporaryText").GetComponent<UnityEngine.UI.Text>();
		weaponText = GameObject.Find("WeaponText").GetComponent<UnityEngine.UI.Text>();
		targetText = GameObject.Find("TargetText").GetComponent<UnityEngine.UI.Text>();
		health_bar = GameObject.Find("HealthSlider").GetComponent<Slider>();
		gameTimeSeconds = SafeLoadPref("timeSec", 0);
		gameTimeMinutes = SafeLoadPref("timeMin", 0);
		gameTimeHours = SafeLoadPref("timeHour", 0);
		InvokeRepeating ("GameTicker", Random.Range(0.5F, 1.0F), 1.0F);
		if (SceneManager.GetActiveScene().name.Equals ("CaptureMode")) {
			Invoke("LaunchCruiser", Random.Range(10.0F, 20.0F)); //Only launch battle cruisers in survival mode
			default_drone_launch_time = 25;
		}
		if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
			highScore = PlayerPrefs.GetFloat ("HighScore");
			scoreValue = SafeLoadPref ("scoreValue", 0.0F);//PlayerPrefs.GetFloat ("scoreValue");
			dronesCreated = SafeLoadPref ("dronesCreated", 0);//PlayerPrefs.GetInt ("dronesCreated", dronesCreated);
			droneLaunchTime = SafeLoadPref ("DroneLaunchTime", default_drone_launch_time);//PlayerPrefs.GetFloat ("DroneLaunchTime");
			numDronesDestroyed = SafeLoadPref ("dronesKilled", 0);
			LoadGameObjects ();
		} else {
			most_battlecruisers_passed = SafeLoadPref ("mostCruisers", 0);
			Invoke("LaunchCruiser", Random.Range(20.0F, 60.0F)); //Only launch battle cruisers in survival mode
		}

		InvokeRepeating ("WinCheck", Random.Range (5.0F, 10.0F), 5.0F);
		Invoke ("LaunchDrone", Random.Range (5.00F, 10.0F));
		Invoke ("LaunchAlliedDrone", Random.Range (1.0F, 10.0F));
		Invoke ("DisplayStatusText", 0.3F);

	}

	public void SetLifeImages(int lives)
	{
		if (lives < 5)
		{
			GameObject.Find ("Life5").SetActive (false);
		}
		if (lives < 4)
		{
			GameObject.Find ("Life4").SetActive (false);
		}
		if (lives < 3)
		{
			GameObject.Find ("Life3").SetActive (false);
		}
		if (lives < 2)
		{
			GameObject.Find ("Life2").SetActive (false);
		}
		if (lives < 1)
		{
			GameObject.Find ("Life1").SetActive (false);
		}
	}

	/************************************************ 
    * SAFE LOAD INT PREF
    * DESCRIPTION: Loads the desired preference, if that 
    * preference is not available it sets it to the
    * default value.
    ************************************************/
	private int SafeLoadPref(string key, int default_value)
	{
		if (PlayerPrefs.HasKey (key + "_" + SceneManager.GetActiveScene ().name)) {
			return PlayerPrefs.GetInt (key + "_" + SceneManager.GetActiveScene ().name);
		} else {
			PlayerPrefs.SetInt (key + "_" + SceneManager.GetActiveScene ().name, default_value);
			return default_value;
		}
	}

	/************************************************ 
    * SAFE LOAD FLOAT PREF
    * DESCRIPTION: Loads the desired preference, if that 
    * preference is not available it sets it to the
    * default value.
    ************************************************/
	private float SafeLoadPref(string key, float default_value)
	{
		if (PlayerPrefs.HasKey (key + "_" + SceneManager.GetActiveScene ().name)) {
			return PlayerPrefs.GetFloat (key + "_" + SceneManager.GetActiveScene ().name);
		} else {
			PlayerPrefs.SetFloat (key + "_" + SceneManager.GetActiveScene ().name, default_value);
			return default_value;
		}
	}

	/************************************************ 
    * SAFE LOAD STRING PREF
    * DESCRIPTION: Loads the desired preference, if that 
    * preference is not available it sets it to the
    * default value.
    ************************************************/
	private string SafeLoadPref(string key, string default_value)
	{
		if (PlayerPrefs.HasKey (key + "_" + SceneManager.GetActiveScene ().name)) {
			return PlayerPrefs.GetString (key + "_" + SceneManager.GetActiveScene ().name);
		} else {
			PlayerPrefs.SetString (key + "_" + SceneManager.GetActiveScene ().name, default_value);
			return default_value;
		}
	}

	/*****************************************************
    * SET DEFAULTS
    * DESCRIPTION: Sets all the game defaults in the event
    * that the level is beaten or no defaults are present.
    *****************************************************/
	public void SetDefaults()
	{
		PlayerPrefs.DeleteKey("AttackDrone" + "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey("AlliedDrone" + "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey("BattleCruiser" + "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey("HeavyTurret" + "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey("Turret" + "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey("Portal" + "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("dronesKilled"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("dronesCreated"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("droneSpawnRate"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("DroneLaunchTime"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("scoreValue"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("timeSec"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("timeMin"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.DeleteKey ("timeHour"+ "_" + SceneManager.GetActiveScene ().name);
		PlayerPrefs.Save ();
	}

	/*****************************************************
    * SAVE GAME OBJECTS
    * DESCRIPTION: Saves all game objects with the input tag
    * to the player prefs file.
    *****************************************************/
	private void SaveGameObject(string tag)
	{
		GameObject[] objectsToSave = GameObject.FindGameObjectsWithTag (tag);
		string objectString = "";
		foreach (GameObject saveObject in objectsToSave) {
			Vector3 objectPosition = saveObject.transform.position;
			Vector3 objectRotation = saveObject.transform.eulerAngles;
			objectString += objectPosition.x.ToString () + "," + objectPosition.y.ToString () + "," + objectPosition.z.ToString () + ",";
			objectString += objectRotation.x.ToString () + "," + objectRotation.y.ToString () + "," + objectRotation.z.ToString () + ":";
		}
		//Debug.Log ("Saving String: " + tag + " length: " + objectString.Length);
		PlayerPrefs.SetString (tag + "_" + SceneManager.GetActiveScene ().name, objectString);
	}

	/********************************************************
    * SAVE GAME
    * DESCRIPTION: Saves the current game data.
    ********************************************************/
	public void SaveGame()
	{
		SaveGameObjects ();
		SaveDrones ();
	}

	/*****************************************************
    * LOAD GAME OBJECTS
    * DESCRIPTION: Loads game objects from the data string
    * in the preferences file.
    *****************************************************/
	private void LoadGameObject(string tag, GameObject loadModel)
	{
		string objectString = SafeLoadPref (tag, "");
		string[] objectStrings = objectString.Split (':');
		int count = 0;

		if (objectStrings.Length > 1) { //We have saved game objects stored, so we can proceed
			GameObject[] currentObjects = GameObject.FindGameObjectsWithTag (tag);
			//Remove all current game objects of this type
			foreach (GameObject currentObject in currentObjects) {
				Destroy (currentObject);
			}
			//Re-constitute the saved game objects
			foreach (string stringData in objectStrings) {
				string[] dataPoints = stringData.Split (',');
				//Each object must have 6 data points to be reconstituded
				if (dataPoints.Length == 6) {
					Vector3 objectPosition = new Vector3 (float.Parse (dataPoints [0]), float.Parse (dataPoints [1]), float.Parse (dataPoints [2]));
					GameObject newPorthole = Instantiate (loadModel, objectPosition, transform.rotation) as GameObject;
					newPorthole.transform.eulerAngles = new Vector3 (float.Parse (dataPoints [3]), float.Parse (dataPoints [4]), float.Parse (dataPoints [5]));
					count++;
				}
			}
		}
	}

	/*****************************************************
    * LOAD GAME OBJECTS
    * DESCRIPTION: Loads all the game objects from the
    * player pref file.
    *****************************************************/
	public void LoadGameObjects()
	{
		LoadGameObject("AttackDrone", AttackDroneModel);
		LoadGameObject("Portal", portalModel);
		LoadGameObject("AlliedDrone", AlliedDroneModel);
		//("BattleCruiser", battleCruiserObject);
		LoadGameObject("HeavyTurret", heavyTurretObject);
		LoadGameObject("Turret", turretObject);
		LoadGameObject("TacticalDrone", TacticalDroneModel);
		LoadGameObject("AlliedTacticalDrone", AlliedTacticalDroneModel);
	}

	/*****************************************************
    * SAVE GAME OBJECTS
    * DESCRIPTION: Saves all the game objects to the
    * player pref file.
    *****************************************************/
	public void SaveGameObjects()
	{
		SaveGameObject ("Portal");
		SaveGameObject ("AttackDrone");
		SaveGameObject ("AlliedDrone");
		//SaveGameObject ("BattleCruiser");
		SaveGameObject ("HeavyTurret");
		SaveGameObject ("Turret");
		SaveGameObject ("TacticalDrone");
		SaveGameObject ("AlliedTacticalDrone");
	}

	/*****************************************************
    * WIN CHECK
    * DESCRIPTION: Checks periodicially for victory conditions
    * and determines when it is time to send the end-guy.
    *****************************************************/
	private void WinCheck()
	{
		if (GameObject.FindGameObjectsWithTag ("Portal").Length <= 0 && endGuyBuilt == false) {
			SetTempText("Battle Cruiser inbound!");
			endGuyBuilt = true;
			Instantiate (battleCruiserObject, battleCruiserSpawnPoint.transform.position, battleCruiserSpawnPoint.transform.rotation);
		}
		else if (GameObject.FindGameObjectsWithTag ("BattleCruiser").Length <= 0 && endGuyBuilt == true)
		{
			SetTempText("Victory!");
			GameObject.FindGameObjectWithTag ("Player").SendMessage ("ClearFirstPersonController");
			Invoke ("EndGame", 3.0F);
		}
	}

	/*****************************************************
    * END GAME
    * DESCRIPTION: Either for victory or defeat; this function
    * is called when the game is over.
    *****************************************************/
	private void EndGame()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		SetDefaults ();
		SceneManager.LoadScene ("MainMenu");
	}

	/*****************************************************
    * GAME TICKER
    * DESCRIPTION: Ticks every second, showing the user
    * how long they have been playing.
    *****************************************************/
	private void GameTicker()
	{
		gameTimeSeconds++;
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

	/*****************************************************
    * PRINT DOUBLE DIGITS
    * DESCRIPTION: Prints the input int as a double digit string
    *****************************************************/
	private string PrintDoubleDigits(int input)
	{
		if (input < 10) {
			return ("0" + input.ToString ());
		}
		return input.ToString ();
	}

	/*****************************************************
    * DISPLAY STATUS TEXT
    * DESCRIPTION: Prints the current game status to the screen.
    *****************************************************/
	private void DisplayStatusText()
	{
		int numDrones = GameObject.FindGameObjectsWithTag ("AttackDrone").Length;
		int numPortholes = GameObject.FindGameObjectsWithTag ("Portal").Length;
		int numTacticalDrones = GameObject.FindGameObjectsWithTag ("TacticalDrone").Length;
		hudText.text = "Ver: " + version;
		hudText.text += "\nTime: " + PrintDoubleDigits(gameTimeHours) + ":" + PrintDoubleDigits(gameTimeMinutes) + ":" + PrintDoubleDigits(gameTimeSeconds);
		hudText.text += "\nScore: " + scoreValue.ToString () + " (High: " + highScore.ToString () + ")";
		if (SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
			hudText.text += "\nCruisers Launched: " + cruisers_launched.ToString () + " Most: " + most_battlecruisers_passed.ToString();
		}
		hudText.text += "\nDrones: " + numDrones.ToString ();
		hudText.text += "\nTactical: " + numTacticalDrones.ToString();
		hudText.text += "\nDestroyed: " + numDronesDestroyed.ToString ();
		hudText.text += "\nPortals: " + numPortholes.ToString ();
	}

	/*****************************************************
    * UPDATE HEALTH
    * DESCRIPTION: Updates the players on-screen health
    *****************************************************/
	public void UpdateHealth(int inputHealth)
	{
		if (inputHealth < health) {
			damaged = true;
		}
		health = inputHealth;
		health_bar.value = health;
		//healthText.text = "Lives: " + lives.ToString ();
	}

	/*****************************************************
    * SET WEAPON TEXT
    * DESCRIPTION: Sets the weapon text to the input.
    *****************************************************/
	public void SetWeaponText(string input)
	{
		if (weaponText != null) {
			weaponText.text = input;
		} else {
			weaponText = GameObject.Find ("WeaponText").GetComponent<UnityEngine.UI.Text> ();
			weaponText.text = input;
		}
	}

	/*****************************************************
    * SET TEMP TEXT
    * DESCRIPTION: Sets the temporary text on the screen.
    *****************************************************/
	public void SetTempText(string input)
	{
		temporaryText.text = input;
		Invoke ("ClearTempText", 3.0F);
	}

	public void SetTargetText(string input)
	{
		targetText.text = input;
	}

	/*****************************************************
    * CLEAR TEMP TEXT
    * DESCRIPTION: Clears the temporary text after a given time interval
    *****************************************************/
	private void ClearTempText()
	{
		temporaryText.text = "";
	}

	/*****************************************************
    * SAVE DRONES
    * DESCRIPTION: Saves the current drone statistics and objects
    *****************************************************/
	private void SaveDrones()
	{
		if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
			PlayerPrefs.SetInt ("dronesCreated" + "_" + SceneManager.GetActiveScene ().name, dronesCreated);
			PlayerPrefs.SetInt ("dronesKilled" + "_" + SceneManager.GetActiveScene ().name, numDronesDestroyed);
			PlayerPrefs.SetFloat ("DroneLaunchTime" + "_" + SceneManager.GetActiveScene ().name, droneLaunchTime);
			PlayerPrefs.SetInt ("timeMin" + "_" + SceneManager.GetActiveScene ().name, gameTimeMinutes);
			PlayerPrefs.SetInt ("timeSec" + "_" + SceneManager.GetActiveScene ().name, gameTimeSeconds);
			PlayerPrefs.SetInt ("timeHour" + "_" + SceneManager.GetActiveScene ().name, gameTimeHours);
			SaveGameObject ("AttackDrone");
		}
	}

	/***********************************************
    * DRONE DESTROYED
    * DESCRIPTION: Called whenever a drone is killed
    * to increment that counter.
    ***********************************************/
	public void DroneDestroyed()
	{
		numDronesDestroyed++;
	}

	public void GameOver()
	{
		GameOverOverlay.SetActive (true);
	}

	/*****************************************************
    * UPDATE LIVES
    * DESCRIPTION: This is called when the player loses a
    * life (or gains one)
    *****************************************************/
	public void UpdateLives(int inputLives)
	{
		//lives = inputLives;
		SetLifeImages(inputLives);
		//healthText.text = "Lives: " + lives.ToString ();
	}

	/*****************************************************
    * LAUNCH ALLIED DRONE
    * DESCRIPTION: Periodically launches an allied drone
    * into the arena.
    *****************************************************/
	private void LaunchAlliedDrone()
	{
		GameObject[] launchers = GameObject.FindGameObjectsWithTag ("AlliedDroneLauncher");
		int randLauncher = Random.Range (0, launchers.Length - 1);
		if (launchers.Length > 0){
			if ((alliedDronesCreated % tactical_drone_launch_bound == 0) && alliedDronesCreated != 0) {
				Instantiate (AlliedTacticalDroneModel, launchers [randLauncher].transform.position, launchers [randLauncher].transform.rotation);
			} else {
				Instantiate (AlliedDroneModel, launchers [randLauncher].transform.position, launchers [randLauncher].transform.rotation);
			}
			alliedDronesCreated++;
		}
		Invoke ("LaunchAlliedDrone", 45.0F);
	}

	/*****************************************************
    * LAUNCH DRONE
    * DESCRIPTION: Launches a drone and hanldes the increase
    * of drone objects and the drone production rate.
    *****************************************************/
	private void LaunchDrone()
	{
		GameObject[] launchers = GameObject.FindGameObjectsWithTag ("DroneLauncher");
		int randLauncher = Random.Range (0, launchers.Length - 1);
		if (launchers.Length > 0) {
			//Time to launch a tactical drone
			if ((dronesCreated % tactical_drone_launch_bound) == 0 && dronesCreated != 0) {
				Instantiate (TacticalDroneModel, launchers [randLauncher].transform.position, launchers [randLauncher].transform.rotation);
			} else {
				Instantiate (AttackDroneModel, launchers [randLauncher].transform.position, launchers [randLauncher].transform.rotation);
			}
			dronesCreated++;
		}

		if (dronesCreated % 15 == 0 && dronesCreated > 0 && droneLaunchTime > 8) {
#if MOBILE_INPUT
			droneLaunchTime--;
#else
			droneLaunchTime-= 2;
#endif
			//SaveDrones ();
		}
		Invoke ("LaunchDrone", droneLaunchTime);
	}

	/*****************************************************
    * PRINT DOUBLE DIGITS
    * DESCRIPTION: Launches a battle cruiser and handles the increase
    * of battlecruisers.
    *****************************************************/
	private void LaunchCruiser()
	{
		GameObject[] destinations = GameObject.FindGameObjectsWithTag ("SpawnPoint");
		Vector3 spawnPoint = new Vector3 (Random.Range(-100, 150), Random.Range(35, 60), Random.Range(800, 1000));
		if (destinations.Length > 0) {
			int destination = Random.Range (0, destinations.Length - 1); //Randomize the start/end points
			GameObject cruiser = Instantiate (battleCruiserObject, spawnPoint, destinations [destination].transform.rotation) as GameObject;
			cruiser.SendMessage ("SetSpeed", battlecruiser_speed);
			if (battlecruiser_speed > 7) {
				battlecruiser_speed--;
			}
			if ((cruisers_launched % 3) == 0 && cruisers_launched > 0) {
				 cruiser.SendMessage ("TargetPlayer");
			}
			if (cruisers_launched > most_battlecruisers_passed) {
				most_battlecruisers_passed = cruisers_launched;
				PlayerPrefs.SetInt ("mostCruisers" + "_" + SceneManager.GetActiveScene ().name, most_battlecruisers_passed);
				PlayerPrefs.Save ();
			}
			cruisers_launched++;
			//Start launching waves of two cruisers
			if ((cruisers_launched > 5 && num_cruisers_in_wave < 1) || (cruisers_launched > 12 && num_cruisers_in_wave < 3) ||
				(cruisers_launched > 18 && num_cruisers_in_wave < 6)) {
				Invoke ("LaunchCruiser", 2.0F);
				num_cruisers_in_wave++;
			} else {
				Invoke ("LaunchCruiser", cruiserLaunchTime);
				if (cruiserLaunchTime > 40) {
					cruiserLaunchTime -= 10;
					num_cruisers_in_wave = 0;
				}
			}
		}
	}

	/*****************************************************
    * ADD TO SCORE
    * DESCRIPTION: Adds points to the current score.
    *****************************************************/
	public void AddToScore(int value)
	{
		scoreValue += value;
		if (scoreValue > highScore) {
			highScore = scoreValue;
			PlayerPrefs.SetFloat ("HighScore", highScore);
		}
		PlayerPrefs.SetFloat ("scoreValue" + "_" + SceneManager.GetActiveScene ().name, scoreValue);
	}

	void Update()
	{
		if (damaged)
		{
			damageImage.color = flashColor;
		}
		else
		{
			damageImage.color = Color.Lerp(damageImage.color, Color.clear, 8f * Time.deltaTime);
		}
		damaged = false;
	}

}
