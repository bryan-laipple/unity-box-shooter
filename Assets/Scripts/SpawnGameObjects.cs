﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnGameObjects : MonoBehaviour
{
	[System.Serializable]
	public class SpawnSeed {
		public GameObject objectToSpawn;
		[Tooltip("relative probability this object will be spawned")]
		public int howLikely = 1;
	}

	// public variables
	public float secondsBetweenSpawning = 0.1f;
	public float xMinRange = -25.0f;
	public float xMaxRange = 25.0f;
	public float yMinRange = 8.0f;
	public float yMaxRange = 25.0f;
	public float zMinRange = -25.0f;
	public float zMaxRange = 25.0f;
	[Space(5)]
	public SpawnSeed[] spawnConfiguration;

	private List<GameObject> spawnObjects;

	private float nextSpawnTime;

	// Use this for initialization
	void Start ()
	{
		// determine when to spawn the next object
		nextSpawnTime = Time.time+secondsBetweenSpawning;
		spawnObjects = new List<GameObject> ();
		foreach (SpawnSeed seed in spawnConfiguration) {
			if (seed.howLikely > 0) {
				int i = seed.howLikely;
				while (i-- > 0) {
					spawnObjects.Add (seed.objectToSpawn);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		// exit if there is a game manager and the game is over
		if (GameManager.IsPresent() && GameManager.IsLevelOver()) {
				return;
		}

		// if time to spawn a new game object
		if (Time.time  >= nextSpawnTime) {
			// Spawn the game object through function below
			MakeThingToSpawn ();

			// determine the next time to spawn the object
			nextSpawnTime = Time.time+secondsBetweenSpawning;
		}	
	}

	void MakeThingToSpawn ()
	{
		Vector3 spawnPosition;

		// get a random position between the specified ranges
		spawnPosition.x = Random.Range (xMinRange, xMaxRange);
		spawnPosition.y = Random.Range (yMinRange, yMaxRange);
		spawnPosition.z = Random.Range (zMinRange, zMaxRange);

		// determine which object to spawn
		int indexOfObjectToSpawn = Random.Range (0, spawnObjects.Count);
		GameObject objectToSpawn = spawnObjects [indexOfObjectToSpawn];

		// actually spawn the game object
		GameObject spawnedObject = Instantiate (objectToSpawn, spawnPosition, transform.rotation) as GameObject;

		// make the parent the spawner so hierarchy doesn't get super messy
		spawnedObject.transform.parent = gameObject.transform;
	}
}
