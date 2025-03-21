using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    public PAttack attackSkill;
    public ShieldSkill shieldSkill;

    public void UseAttackSkill()
    {
        attackSkill.ShootBullets();
    }

    public void UseShieldSkill()
    {
        shieldSkill.Activate();
    }
}