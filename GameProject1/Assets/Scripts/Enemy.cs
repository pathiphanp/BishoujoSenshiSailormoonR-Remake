using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public enum EnemyAction
{
    ATTACK, MOVE, STUN, KNOCKBACK
}
public class Enemy : MonoBehaviour, AddDebuff
{
    [SerializeField] ActionType actionType;
    [SerializeField] public DataCharacter enemyData;
    SpriteRenderer spR;
    Rigidbody2D rb;
    [SerializeField] Animator anim;
    [Header("Collider")]
    [SerializeField] Collider2D CheckAtkCollider;
    Collider2D coll;
    [Header("Status")]
    [SerializeField] public int hp;
    [Header("Move")]
    [SerializeField] float speedMove;
    bool moveToPlayer = true;
    bool canMoveback;
    [Header("Attack")]
    [SerializeField] int damage;
    [SerializeField] float readyRange;
    [SerializeField] float moveBackDuration;
    [SerializeField] float moveBackSpeed;
    [SerializeField] float delayAtk;
    [Header("Knockback")]
    [SerializeField] float delayKnokback;
    bool canKnockback = true;
    [Header("Player")]
    [HideInInspector] public PlayerControl playerControl;
    Vector2 diraction;
    [SerializeField] float diractionMagnitude;
    Coroutine moveBack;
    Coroutine startAttack;
    Coroutine returnToMove;
    float diX;
    void Awake()
    {
        spR = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        hp = enemyData.hp;
        damage = enemyData.damage;
    }
    void Start()
    {
        if (playerControl == null)
        {
            playerControl = FindObjectOfType<PlayerControl>();
        }
        actionType = ActionType.MOVE;
        diX = Random.Range(-0.5f, 0.5f);

    }
    void FixedUpdate()
    {
        CheckDistance();
        Move();
        MoveBack();
    }

    void CheckDistance()
    {
        diraction = playerControl.gameObject.transform.position - transform.position;
        diractionMagnitude = diraction.magnitude;

    }
    void CheckSide()
    {
        if (transform.position.x < playerControl.gameObject.transform.position.x)
        {
            spR.flipX = true;
        }
        else
        {
            spR.flipX = false;
        }
    }
    void Move()
    {
        if (actionType == ActionType.MOVE)
        {
            float _YClampP = transform.position.y;
            _YClampP = Mathf.Clamp(_YClampP, -3.4f, 0.3f);
            anim.Play("Walk");
            CheckSide();
            //ทิศทาง ซ้าย หรือ ขวา
            diraction.Normalize();
            //เคลื่อนที่หาผู้เล่น
            transform.Translate(diraction * speedMove * Time.deltaTime);
            diraction.x = diX;
            transform.Translate(diraction * speedMove * Time.deltaTime);
            // transform.position = new Vector3(transform.position.x, _YClampP, transform.position.z);
            if (Mathf.Abs(diractionMagnitude - readyRange) < 0.05)
            {
                anim.Play("Idel");
                diractionMagnitude = readyRange;
                actionType = ActionType.NORMAL;
                Attack();
            }
        }
    }

    void MoveBack()
    {
        if (actionType == ActionType.MOVEBACK)
        {
            anim.Play("WalkBack");
            CheckSide();
            Vector2 moveBackS = (transform.position - playerControl.transform.position).normalized;
            moveBackS.y = 0;
            transform.Translate(moveBackS * speedMove * Time.deltaTime);
            if (!canMoveback)
            {
                canMoveback = true;
                moveBack = StartCoroutine(MoveBackDuration());
            }
        }
    }
    IEnumerator MoveBackDuration()
    {
        yield return new WaitForSeconds(1.5f);
        canMoveback = false;
        actionType = ActionType.MOVE;
    }
    void Attack()
    {
        startAttack = StartCoroutine(StartAttack());
    }
    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(1f);
        anim.Play("Attack 1");
        if (diraction.magnitude <= readyRange)
        {
            // Debug.Log("Hit player");
            playerControl.Takedamage(damage, this);
            Attack();
        }
        else
        {
            // Debug.Log("Miss player");
            yield return new WaitForSeconds(0.5f);
            //moveback
            actionType = ActionType.MOVEBACK;
        }
    }

    public void AddKnockback(Vector2 knockbackDiraction, float knockbackForce, bool onGrad)
    {
        if (canKnockback)
        {
            canKnockback = false;
            StartCoroutine(Knockback(knockbackDiraction, knockbackForce, onGrad));
        }
    }
    IEnumerator Knockback(Vector2 knockbackDiraction, float knockbackForce, bool onGrad)
    {
        rb.velocity = Vector3.zero;
        knockbackDiraction.y = 0;
        if (onGrad)
        {
            anim.Play("ThorwKnockback");
        }
        else
        {
            anim.Play("Knockback");
        }
        rb.AddForce(knockbackDiraction * knockbackForce, ForceMode2D.Impulse);
        rb.velocity = new Vector2(rb.velocity.x, 20f);
        yield return new WaitForSeconds(delayKnokback / 2);
        rb.velocity = new Vector2(rb.velocity.x, -20f);
        yield return new WaitForSeconds(delayKnokback / 2);
        rb.velocity = Vector3.zero;
        anim.Play("ReturnToIdel");
        canKnockback = true;
    }
    void ReturnKnockbackToMove()
    {
        if (actionType == ActionType.DIE || hp <= 0)
        {
            anim.Play("Die");
        }
        else
        {
            anim.Play("Idel");
            actionType = ActionType.MOVE;
        }
    }
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            StopAllMyCoroutines();
            if (actionType != ActionType.TAKEGRABTHORW)
            {
                Vector2 knockbackDirection = (transform.position - playerControl.gameObject.transform.position).normalized;
                AddKnockback(-knockbackDirection, playerControl.finishKnockbackForce, false);
            }
            else
            {
                playerControl.CancelGrab();
            }
            ControlSpawnEnemy.Instance.CheckSpawnEnemy();
            actionType = ActionType.DIE;
            StartCoroutine(DieEffect());
            coll.enabled = false;
            playerControl.enemyList.Remove(this);
        }
        else
        {
            StopAllMyCoroutines();
            if (actionType == ActionType.TAKEGRAB)
            {
                anim.Play("GrabGetHit");
            }
            else
            {
                anim.Play("GetHit");
            }
        }
        HpUIManager.Instance.CallShowHpEnemy(this);
    }
    IEnumerator DieEffect()
    {
        anim.Play("Die");
        float td = 0f;
        bool a = false;
        while (td <= 0.15f)
        {
            td += Time.deltaTime;
            a = !a;
            this.gameObject.GetComponent<SpriteRenderer>().enabled = a;
            yield return new WaitForSeconds(0.05f);
            yield return true;
        }
        Destroy(this.gameObject);
    }
    void Stun()
    {
        anim.Play("Stun");
    }
    void RemoveGameObject()
    {
        if (hp <= 0)
        {
            // Destroy(this.gameObject);
        }
    }

    public void SetOnGrab(GameObject _setgrab, ActionType _actionType, bool stopAllp)
    {
        if (stopAllp)
        {
            StopAllMyCoroutines();
        }
        rb.velocity = Vector2.zero;
        anim.Play("OnGrab");
        transform.position = _setgrab.transform.position;
        actionType = _actionType;
        transform.parent = _setgrab.transform;
    }

    void StopAllMyCoroutines()
    {
        actionType = ActionType.NORMAL;
        if (returnToMove != null)
        {
            StopCoroutine(returnToMove);
        }
        if (moveBack != null)
        {
            StopCoroutine(moveBack);
        }
        if (startAttack != null)
        {
            StopCoroutine(startAttack);
        }
    }

    IEnumerator ReturnToMove()
    {
        yield return new WaitForSeconds(0.5f);
        actionType = ActionType.MOVE;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" && actionType == ActionType.TAKEGRABTHORW)
        {
            Debug.Log(other);
            Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;
            Enemy enemyS = other.GetComponent<Enemy>();
            enemyS.AddKnockback(knockbackDirection, playerControl.finishKnockbackForce, false);
            enemyS.TakeDamage(10);
        }
    }
}
