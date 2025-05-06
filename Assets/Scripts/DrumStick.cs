using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DrumStick : MonoBehaviour
{
    public bool isLeftDrumStick;
    private GameManager gameManager;

    // effects
    public ParticleManager particleManager;
    public AudioManager audioManager;

    public float collisionCheckRate = 0.01f; // check collisions every 0.01 seconds
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private bool isColliding = false;
    private GameObject currentCollisionObject = null;

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
        
        // store initial position and rotation
        previousPosition = transform.position;
        previousRotation = transform.rotation;
        
        // start continuous collision checking
        StartCoroutine(ContinuousCollisionCheck());
    }

    private IEnumerator ContinuousCollisionCheck()
    {
        while (true)
        {
            // check for collisions between previous and current position
            if (currentCollisionObject != null)
            {
                // if colliding check if still colliding
                if (!IsCollidingWith(currentCollisionObject))
                {
                    isColliding = false;
                    currentCollisionObject = null;
                }
            }
            
            // store current position and rotation for next check
            previousPosition = transform.position;
            previousRotation = transform.rotation;
            
            yield return new WaitForSeconds(collisionCheckRate);
        }
    }

    private bool IsCollidingWith(GameObject obj)
    {
        if (obj == null) return false;
        
        Collider stickCollider = GetComponent<Collider>();
        Collider noteCollider = obj.GetComponent<Collider>();
        
        if (stickCollider == null || noteCollider == null) return false;
        
        // check if colliders are intersecting
        return stickCollider.bounds.Intersects(noteCollider.bounds);
    }

    void OnTriggerEnter(Collider other)
    {
        // store collision object for continuous checking
        currentCollisionObject = other.gameObject;
        isColliding = true;
        
        HandleCollision(other);
    }
    
    void OnTriggerStay(Collider other)
    {
        // if already colliding handle again
        if (currentCollisionObject == other.gameObject)
        {
            HandleCollision(other);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // clear collision object when exit
        if (currentCollisionObject == other.gameObject)
        {
            currentCollisionObject = null;
            isColliding = false;
        }
    }
    
    private void HandleCollision(Collider other)
    {
        if(other.CompareTag("StartGame"))
        {
            gameManager.StartGame();
            Destroy(other.gameObject);
            return;
        }

        if (isLeftDrumStick)
        {
            if (other.CompareTag("LeftDrum")) // left hit
            {
                //audioManager.Play("DrumBoom");
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
            else if (other.CompareTag("RightDrum")) // left wrong hit
            {
                // vibrate left controller using xri
                UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                device.SendHapticImpulse(0, 0.5f, 0.1f);
                gameManager.LoseLife();
                gameManager.UpdateMultiplier("Miss");
                gameManager.UpdateCombo("Miss");
                Destroy(other.gameObject);
            }
        }
        else // right drumstick
        {
            if (other.CompareTag("RightDrum")) // right hit
            {
                //audioManager.Play("DrumBoom");
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
            else if (other.CompareTag("LeftDrum")) // right wrong hit
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
    }
}
