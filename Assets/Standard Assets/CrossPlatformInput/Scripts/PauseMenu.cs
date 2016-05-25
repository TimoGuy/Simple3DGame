using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	public bool isPaused = false;

	public GameObject pauseMenuCanvas;
	private UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller;
	// Use this for initialization
	void Start () {
		pauseMenuCanvas.SetActive (false);
	}

	// Update is called once per frame
	void Update () {

		if (isPaused) {
			pauseMenuCanvas.SetActive (true);
			Time.timeScale = 0;
		} else {
			pauseMenuCanvas.SetActive (false);
			Time.timeScale = 1;
		}
#if !MOBILE_INPUT
		if (Input.GetKeyDown (KeyCode.Escape)) 
#else
		if (CrossPlatformInputManager.GetButtonDown("Pause"))
#endif
		{
			isPaused = !isPaused;
			if (isPaused) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}
	
	public void Resume()
	{
		isPaused = false;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void ProgramExit()
	{
		Application.Quit ();
	}

	public void GameExit()
	{
		SceneManager.LoadScene ("MainMenu");
	}
}
