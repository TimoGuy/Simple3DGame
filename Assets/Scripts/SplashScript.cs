using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

public class SplashScript : MonoBehaviour {

	public GameObject lightSystem;
	private float speed = 30;
	// Use this for initialization
	void Start () {
		Invoke ("EndSplash", 5.0F);
	}

	void EndSplash()
	{
		SceneManager.LoadScene ("MainMenu");
	}
	// Update is called once per frame
	void Update () {
		lightSystem.transform.Rotate (Vector3.up, speed * Time.deltaTime);
#if !MOBILE_INPUT
		if (Input.GetButton("Fire1")){
			SceneManager.LoadScene ("MainMenu");
		}
#else
		if (Input.touchCount > 0)
		{
		   SceneManager.LoadScene ("MainMenu");
		}
#endif

	}
}
