using UnityEngine;

public class DrumNote : MonoBehaviour
{
    public bool isRightDrum; // true for right drum, false for left drum
    public float speed; // No longer static, calculated per note
    public Vector3 targetPosition;
    public Vector3 spawnRotation;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager.shouldGameContinue)
        {
            // Move note towards target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            // Get reference to NoteSpawner to check death zone
            NoteSpawner spawner = FindObjectOfType<NoteSpawner>();
            if (spawner != null && transform.position.z < spawner.deathZonePosition.z - 1f)
            {
                Destroy(gameObject);
                gameManager.UpdateMultiplier("Miss");
                gameManager.UpdateCombo("Miss");
                gameManager.LoseLife();
            }
        }
    }
} 