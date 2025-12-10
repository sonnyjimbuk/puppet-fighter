using UnityEngine;
using System.Collections; // Required for Coroutines

public class antsobjectmover : MonoBehaviour
{
    public GameObject[] objectPrefabs; // Array to hold your different prefabs
    public float spawnInterval = 2f; // Time between spawns
    public float spawnXPosition = -10f; // Starting X position (left edge)
    public float minY = -4f; // Minimum Y spawn position
    public float maxY = 4f; // Maximum Y spawn position

    void Start()
    {
        // Start the spawning routine
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true) // Infinite loop to keep spawning
        {
            yield return new WaitForSeconds(spawnInterval); // Wait for the interval

            // 1. Pick a random prefab from the array
            int randomIndex = Random.Range(0, objectPrefabs.Length);
            GameObject objectToSpawn = objectPrefabs[randomIndex];

            // 2. Generate a random Y position
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(spawnXPosition, randomY, 0f);

            // 3. Instantiate the selected prefab at the random position
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}
