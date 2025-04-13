using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject cirlceBoom;

    public void SpawnParticleEffect(string effectName, Vector3 position, Quaternion rotation) // WORKS
    {
        GameObject particlePrefab;
        
        switch(effectName.ToLower())
        {
            case "circleboom":
                particlePrefab = cirlceBoom;
                break;
            case "rightdrumhit": 
                particlePrefab = cirlceBoom;
                break;
            default:
                Debug.LogWarning($"Unknown particle effect: {effectName}");
                return;
        }

        if (particlePrefab != null)
        {
            GameObject particleInstance = Instantiate(particlePrefab, position, rotation);
            Destroy(particleInstance, 3f);
        }
    }
}
