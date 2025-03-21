using UnityEngine;

public class PAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 200f;
    public int bulletDamage = 10;
    public GameObject firstEnemy;
    public MoveObjectsByCommand moveObjectsByCommand;
    private bool isAttacking = false;

    void Start()
    {
        InvokeRepeating("ShootBullets", 1f, 1f);
    }

    public void ShootBullets()
    {
        if (isAttacking)
        {
            if (moveObjectsByCommand.targetEnemy != null) 
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity, moveObjectsByCommand.targetEnemy.transform); 
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    rb.linearVelocity = Vector2.right * bulletSpeed;

                    pBullet bulletController = bullet.AddComponent<pBullet>();
                    bulletController.damage = bulletDamage;
                    Destroy(bullet, 3f);
                }
            }
            else
            {
                Debug.LogWarning("No target enemy selected!");
            }
        }
    }


    public void StartAttacking()
    {
        isAttacking = true;
    }

    public void StopAttacking()
    {
        isAttacking = false;
    }
}


