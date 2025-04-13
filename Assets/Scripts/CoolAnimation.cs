using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolAnimation : MonoBehaviour
{
    // references
    public GameObject cubePrefab; 
    public int numberOfCubes = 15; 
    public float spacing = 2f; 
    public Vector3 startPosition = Vector3.zero; 
    private GameObject[] cubes; 
    public float delayBetweenCubes = 0.1f; 
    public float rotationDuration = 1f; 

    void Start()
    {
        // create and position cubes
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
        // forward rotation
        List<Coroutine> rotations = new List<Coroutine>();
        for (int i = 0; i < cubes.Length; i++)
        {
            rotations.Add(StartCoroutine(RotateCube(cubes[i], 90f)));
            yield return new WaitForSeconds(delayBetweenCubes);
        }

        // wait rotations to complete
        foreach (var rotation in rotations)
        {
            yield return rotation;
        }

        // reverse rotation
        rotations.Clear();
        for (int i = 0; i < cubes.Length; i++)
        {
            rotations.Add(StartCoroutine(RotateCube(cubes[i], -90f)));
            yield return new WaitForSeconds(delayBetweenCubes);
        }

        // wait reverse rotations to complete
        foreach (var rotation in rotations)
        {
            yield return rotation;
        }
        //Debug.Log("anim done");
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

        cube.transform.rotation = Quaternion.Euler(0, 0, endRotation);
        //Debug.Log("helo");
    }
}
