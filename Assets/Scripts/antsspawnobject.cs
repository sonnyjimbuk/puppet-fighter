using UnityEngine;
using System.Collections; // Required for Coroutines

public class antsspawnobject : MonoBehaviour
{
    public GameObject[] objectsToSpawn; // Array of prefabs to spawn
    public Transform[] spawnPoints;     // Array of Transforms representing spawn locations
    public float spawnInterval = 3f;    // Time between spawns

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval; // Set the initial spawn time
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnObject();
            nextSpawnTime = Time.time + spawnInterval; // Reset the timer for the next spawn
        }
    }

    void SpawnObject()
    {
        if (objectsToSpawn.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Objects to spawn or spawn points array is empty!");
            return;
        }

        // Choose a random object from the array
        int randomObjectIndex = Random.Range(0, objectsToSpawn.Length);
        GameObject objectToInstantiate = objectsToSpawn[randomObjectIndex];

        // Choose a random spawn point from the array
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomSpawnPointIndex];

        // Instantiate the chosen object at the chosen spawn point's position and rotation
        Instantiate(objectToInstantiate, chosenSpawnPoint.position, chosenSpawnPoint.rotation);
    }
}