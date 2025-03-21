using UnityEngine;

public class pBullet : MonoBehaviour
{
    public int damage = 25;
    public float detectionRadius = 50f;
    public float bulletSpeed = 10f;

    private Transform enemyTransform;

    void Start()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            enemyTransform = nearestEnemy.transform;
            Debug.Log("Nearest Enemy found: " + nearestEnemy.name);
        }
        else
        {
            Debug.LogWarning("No enemy found within range!");
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.right * bulletSpeed;
        }
        else
        {
            Debug.LogError("Rigidbody2D not found on bullet!");
        }
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    void Update()
    {
        if (enemyTransform != null)
        {
            float distance = Vector3.Distance(transform.position, enemyTransform.position);

            if (distance <= detectionRadius)
            {
                Debug.Log("Hit detected on enemy! Dealing damage.");
                EHealth enemyHealth = enemyTransform.GetComponent<EHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log("Enemy took damage: " + damage);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogError("EHealth component not found on enemy object!");
                }
            }
        }
    }
}