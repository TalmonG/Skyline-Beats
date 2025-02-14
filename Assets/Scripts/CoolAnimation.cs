using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolAnimation : MonoBehaviour
{
    public GameObject cubePrefab; // Single cube prefab
    public int numberOfCubes = 15; // Number of cubes to create
    public float spacing = 2f; // Space between each cube
    public Vector3 startPosition = Vector3.zero; // Starting position for first cube
    private GameObject[] cubes; // Array to store instantiated cubes
    public float delayBetweenCubes = 0.1f; // Delay before starting next cube
    public float rotationDuration = 1f; // How long each cube rotates

    void Start()
    {
        // Create and position cubes
        cubes = new GameObject[numberOfCubes];
        
        for (int i = 0; i < numberOfCubes; i++)
        {
            Vector3 position = startPosition + Vector3.forward * (spacing * i);
            cubes[i] = Instantiate(cubePrefab, position, Quaternion.identity);
        }
        
        StartCoroutine(AnimateCubes());
    }

    IEnumerator AnimateCubes()
    {
        // Forward rotation
        List<Coroutine> rotations = new List<Coroutine>();
        for (int i = 0; i < cubes.Length; i++)
        {
            rotations.Add(StartCoroutine(RotateCube(cubes[i], 90f)));
            yield return new WaitForSeconds(delayBetweenCubes);
        }

        // Wait for all rotations to complete
        foreach (var rotation in rotations)
        {
            yield return rotation;
        }

        // Reverse rotation
        rotations.Clear();
        for (int i = 0; i < cubes.Length; i++)
        {
            rotations.Add(StartCoroutine(RotateCube(cubes[i], -90f)));
            yield return new WaitForSeconds(delayBetweenCubes);
        }

        // Wait for all reverse rotations to complete
        foreach (var rotation in rotations)
        {
            yield return rotation;
        }
    }

    IEnumerator RotateCube(GameObject cube, float targetRotation)
    {
        float elapsedTime = 0f;
        float startRotation = cube.transform.eulerAngles.z;
        float endRotation = startRotation + targetRotation;
        
        while (elapsedTime < rotationDuration)
        {
            float currentRotation = Mathf.Lerp(startRotation, endRotation, elapsedTime / rotationDuration);
            cube.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exact
        cube.transform.rotation = Quaternion.Euler(0, 0, endRotation);
    }
}
