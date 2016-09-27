using UnityEngine;
using System.Collections;

public class SpawnObject : MonoBehaviour {

	public GameObject spawnObject = null;
	public float spawnMaxTime = 60;
	public float spawnMinTime = 30;

	// Use this for initialization
	void Start () {
		Invoke("Spawn", Random.Range(spawnMinTime, spawnMaxTime));
	}

	/*****************************************************
    * SPAWN
    * DESCRIPTION: Spawns the game object a random times
    *****************************************************/
	private void Spawn()
	{
		if (spawnObject != null) {
			Instantiate (spawnObject, transform.position, transform.rotation);
			Invoke ("Spawn", Random.Range (spawnMinTime, spawnMaxTime));
		}
	}
}
