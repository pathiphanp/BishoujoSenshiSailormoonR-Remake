using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAround : MonoBehaviour
{
    [SerializeField] PlayerControl player;
    [SerializeField] int damageUseAroundAtk;
    int damage;
    float finishKnockbackForce;
    [SerializeField] Collider2D aroundColl;
    [SerializeField] Collider2D specialColl;
    bool special;
    public void CastAroundAttack(int _damage, float _finishKnockbackForce, bool _special)
    {
        damage = _damage;
        finishKnockbackForce = _finishKnockbackForce;
        special = _special;
        CheckSpell(true);
    }
    public void CancelCastSkill()
    {
        CheckSpell(false);
    }
    void CheckSpell(bool order)
    {
        if (!special)
        {
            aroundColl.enabled = order;
        }
        else
        {
            specialColl.enabled = order;
            if (order)
            {
                player.specialAttackNum--;
            }
        }
    }
    void AroundAttackDamageToPlayer()
    {
        player.Takedamage(damageUseAroundAtk, null);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Enemy _enemy = other.GetComponent<Enemy>();
            _enemy.player = player;
            _enemy.TakeDamage(damage);
            Vector2 knockbackDirection = (transform.position - _enemy.gameObject.transform.position).normalized;
            _enemy.AddKnockback(knockbackDirection, finishKnockbackForce, false);
            if (!special)
            {
                AroundAttackDamageToPlayer();
            }
        }
    }
}
