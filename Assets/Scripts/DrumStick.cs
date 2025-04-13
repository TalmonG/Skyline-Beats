using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DrumStick : MonoBehaviour
{
    public bool isLeftDrumStick;
    private GameManager gameManager;

    // Effects
    public ParticleManager particleManager;
    public AudioManager audioManager;


    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        gameManager = FindObjectOfType<GameManager>();
        particleManager = FindObjectOfType<ParticleManager>();

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {

        if(other.CompareTag("StartGame"))
        {
            gameManager.StartGame();
            Destroy(other.gameObject);
        }


        if (isLeftDrumStick)
        {
            if (other.CompareTag("LeftDrum")) // left hit
            {
                Vector3 effectPosition = other.gameObject.transform.position;
                effectPosition.y += 0.1f;
                effectPosition.z -= 0.25f;
                Quaternion effectRotation = other.gameObject.transform.rotation * Quaternion.Euler(90, 0, 0);
                particleManager.SpawnParticleEffect("CircleBoom", effectPosition, effectRotation);
                Destroy(other.gameObject);
                gameManager.AddScore();
                gameManager.AddLife();
                gameManager.UpdateMultiplier("Hit");
                gameManager.UpdateCombo("Hit");
            }
        }
        if (isLeftDrumStick == false)
        {
            if (other.CompareTag("RightDrum")) // right hit
            {
                Vector3 effectPosition = other.gameObject.transform.position ;
                effectPosition.y += 0.1f;
                effectPosition.z -= 0.25f;
                Quaternion effectRotation = other.gameObject.transform.rotation * Quaternion.Euler(90, 0, 0);
                particleManager.SpawnParticleEffect("CircleBoom", effectPosition, effectRotation);
                Destroy(other.gameObject);
                gameManager.AddScore();
                gameManager.AddLife();
                gameManager.UpdateMultiplier("Hit");
                gameManager.UpdateCombo("Hit");
            }
        }

        if (isLeftDrumStick == false)
        {
            if (other.CompareTag("LeftDrum")) // right wrong hit
            {
                // vibrate right controller using xri
                UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                device.SendHapticImpulse(0, 0.5f, 0.1f);
                gameManager.LoseLife();
                gameManager.UpdateMultiplier("Miss");
                gameManager.UpdateCombo("Miss");
                Destroy(other.gameObject);
            }
        }

        if (isLeftDrumStick == true)
        {
            if (other.CompareTag("RightDrum")) // left wrong hit
            {
                // vibrate right controller using xri
                UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                device.SendHapticImpulse(0, 0.5f, 0.1f);
                gameManager.LoseLife();
                gameManager.UpdateMultiplier("Miss");
                gameManager.UpdateCombo("Miss");
                Destroy(other.gameObject);
            }
        }
    }
}
