using UnityEngine;

public class EAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 200f;
    public int bulletDamage = 10;
    public GameObject firstEnemy;
    void Start()
    {
        InvokeRepeating("ShootBullets", 1f, 1f); 
    }

    void ShootBullets()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity, firstEnemy.transform);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.left * bulletSpeed;
            EBullet bulletController = bullet.AddComponent<EBullet>();
           bulletController.damage = bulletDamage;
            Destroy(bullet, 3f);
        }
    }
}
