using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class OptionalDestroyByTime : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (!SceneManager.GetActiveScene ().name.Equals ("MiniGame")) {
			Destroy (this, 300);
		}
	}
}
