using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

	public GameObject menu;
	public GameObject aboutMenu;
	public GameObject optionsMenu;

	public RectTransform startButton;
	public GameObject xSlider;
	public UnityEngine.UI.Text xSliderPerc;

	public GameObject ySlider;
	public UnityEngine.UI.Text ySliderPerc;

	public GameObject volumeSlider;
	public UnityEngine.UI.Text volumePerc;

	public GameObject mediumCheck;
	public GameObject easyCheck;
	public GameObject hardCheck;
	public GameObject mobileText;

	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey ("defaultsSet")) {
			PlayerPrefs.DeleteAll (); //Cleanup old player preferences
		}
		if (!PlayerPrefs.HasKey ("lookYSensitivity")) {
			PlayerPrefs.SetFloat ("lookYSensitivity", 0.40F);
		}
		if (!PlayerPrefs.HasKey ("lookXSensitivity")) {
			PlayerPrefs.SetFloat ("lookXSensitivity", 0.40F);
		}
		if (!PlayerPrefs.HasKey ("Volume")) {
			PlayerPrefs.SetFloat ("Volume", 0.50F);
		}

		if (!PlayerPrefs.HasKey ("AILevel")) {
			PlayerPrefs.SetInt ("AILevel", 0);
		}
		Slider tempSlider = xSlider.GetComponent<Slider>();
		if (tempSlider != null)
		{
			tempSlider.normalizedValue = PlayerPrefs.GetFloat("lookXSensitivity");
		}
		tempSlider = ySlider.GetComponent<Slider>();
		if (tempSlider != null)
		{
			tempSlider.normalizedValue = PlayerPrefs.GetFloat("lookYSensitivity");
		}
		tempSlider = volumeSlider.GetComponent<Slider> ();
		if (tempSlider != null)
		{
			tempSlider.normalizedValue = PlayerPrefs.GetFloat ("Volume");
		}
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		if (difficulty == 0) {
			easyCheck.GetComponent<Toggle> ().isOn = true;
		}
		else if (difficulty == 1) {
			 mediumCheck.GetComponent<Toggle> ().isOn = true;
		}
		else {
			hardCheck.GetComponent<Toggle> ().isOn = true;
		}
		PlayerPrefs.Save();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

	}

	public void ResetSettingsHandler()
	{
		float highScore = PlayerPrefs.GetFloat ("HighScore"); //Protect the high score
		PlayerPrefs.DeleteAll ();
		PlayerPrefs.SetFloat ("lookXSensitivity", 0.40F);
		PlayerPrefs.SetFloat ("lookYSensitivity", 0.40F);
		PlayerPrefs.SetFloat ("Volume", 0.50F);
		PlayerPrefs.SetInt ("AILevel", 0);
		PlayerPrefs.SetInt ("UseGyro", 0);
		Slider tempSlider = xSlider.GetComponent<Slider>();
		if (tempSlider != null)
		{
			tempSlider.normalizedValue = PlayerPrefs.GetFloat("lookXSensitivity");
		}
		tempSlider = ySlider.GetComponent<Slider>();
		if (tempSlider != null)
		{
			tempSlider.normalizedValue = PlayerPrefs.GetFloat("lookYSensitivity");
		}
		tempSlider = volumeSlider.GetComponent<Slider> ();
		if (tempSlider != null)
		{
			tempSlider.normalizedValue = PlayerPrefs.GetFloat ("Volume");
		}
		PlayerPrefs.SetFloat ("HighScore", highScore);
		PlayerPrefs.Save ();
	}

	public void StartGameHandler()
	{
		SceneManager.LoadScene ("MiniGame");
	}

	public void SurvivalModeHandler()
	{
		SceneManager.LoadScene ("SurvivalMode");
	}

	public void CaptureModeHandler()
	{
		SceneManager.LoadScene ("CaptureMode");
	}

	bool otherChange = false;
	public void EasyCheckboxHandler()
	{
		if (otherChange) {
			return;
		}
		otherChange = true;
		easyCheck.GetComponent<Toggle> ().isOn = true;
		mediumCheck.GetComponent<Toggle> ().isOn = false;
		hardCheck.GetComponent<Toggle> ().isOn = false;
		otherChange = false;
	}

	public void NormalCheckBoxHandler()
	{
		if (otherChange) {
			return;
		}
		otherChange = true;
		easyCheck.GetComponent<Toggle> ().isOn = false;
		mediumCheck.GetComponent<Toggle> ().isOn = true;
		hardCheck.GetComponent<Toggle> ().isOn = false;
		otherChange = false;
	}

	public void HardCheckBoxHandler()
	{
		if (otherChange) {
			return;
		}
		otherChange = true;
		easyCheck.GetComponent<Toggle> ().isOn = false;
		mediumCheck.GetComponent<Toggle> ().isOn = false;
		hardCheck.GetComponent<Toggle> ().isOn = true;
		otherChange = false;
	}

	public void AboutHandler()
	{
		menu.SetActive (false);
		aboutMenu.SetActive (true);
	}

	public void ReturnToMenuHandler()
	{
		aboutMenu.SetActive (false);
		optionsMenu.SetActive (false);
		menu.SetActive (true);
		if (easyCheck.GetComponent<Toggle>().isOn)
		{
			PlayerPrefs.SetInt ("AILevel", 0);
		}
		else if (mediumCheck.GetComponent<Toggle>().isOn)
		{
			PlayerPrefs.SetInt ("AILevel", 1);
		}
		else if (hardCheck.GetComponent<Toggle>().isOn)
		{
			PlayerPrefs.SetInt ("AILevel", 2);
		}
		PlayerPrefs.Save ();
	}

	public void OptionsHandler()
	{
		menu.SetActive (false);
		optionsMenu.SetActive (true);
	}

	public void QuitHandler()
	{
		Application.Quit ();

	}

	public void OnVolumeSlide()
	{
		Slider volumeSlide = volumeSlider.GetComponent<Slider>();
		if (volumeSlide != null)
		{
			PlayerPrefs.SetFloat("Volume", volumeSlide.normalizedValue);
			volumePerc.text = (Mathf.Round (volumeSlide.normalizedValue * 100)).ToString () + "%";
			AudioListener.volume = PlayerPrefs.GetFloat ("Volume");
		}
	}

	public void OnXSlide()
	{
		Slider xSliderSlide = xSlider.GetComponent<Slider>();
		if (xSliderSlide != null)
		{
			PlayerPrefs.SetFloat("lookXSensitivity", xSliderSlide.normalizedValue);
			xSliderPerc.text = (Mathf.Round (xSliderSlide.normalizedValue * 100)).ToString () + "%";
		}
	}
	public void OnYSlide()
	{
		Slider ySliderSlide = ySlider.GetComponent<Slider>();
		if (ySliderSlide != null)
		{
			PlayerPrefs.SetFloat("lookYSensitivity", ySliderSlide.normalizedValue);
			ySliderPerc.text = (Mathf.Round (ySliderSlide.normalizedValue * 100)).ToString () + "%";
		}
	}
}
