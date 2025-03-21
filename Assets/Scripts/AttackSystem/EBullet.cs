using UnityEngine;

public class EBullet : MonoBehaviour
{
    public int damage = 20;  
    public float detectionRadius = 10f;  

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("Player found: " + player.name);
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= detectionRadius)
            {
                Debug.Log("Hit detected! Dealing damage.");
                PHealth playerHealth = playerTransform.GetComponent<PHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log("Player took damage: " + damage);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogError("PHealth component not found on player object!");
                }
            }
        }
    }
}