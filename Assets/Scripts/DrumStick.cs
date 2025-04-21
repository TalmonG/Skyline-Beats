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

    // Collision detection settings
    public float collisionCheckRate = 0.01f; // Check collisions every 0.01 seconds
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
        
        // Store initial position and rotation
        previousPosition = transform.position;
        previousRotation = transform.rotation;
        
        // Start continuous collision checking
        StartCoroutine(ContinuousCollisionCheck());
    }

    private IEnumerator ContinuousCollisionCheck()
    {
        while (true)
        {
            // Check for collisions between previous and current position
            if (currentCollisionObject != null)
            {
                // If we were colliding, check if we're still colliding
                if (!IsCollidingWith(currentCollisionObject))
                {
                    // We've moved out of collision
                    isColliding = false;
                    currentCollisionObject = null;
                }
            }
            
            // Store current position and rotation for next check
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
        
        // Check if the colliders are intersecting
        return stickCollider.bounds.Intersects(noteCollider.bounds);
    }

    void OnTriggerEnter(Collider other)
    {
        // Store the collision object for continuous checking
        currentCollisionObject = other.gameObject;
        isColliding = true;
        
        HandleCollision(other);
    }
    
    void OnTriggerStay(Collider other)
    {
        // If we're already colliding with this object, handle it again
        if (currentCollisionObject == other.gameObject)
        {
            HandleCollision(other);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Clear the collision object when we exit
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
        else // Right drumstick
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
