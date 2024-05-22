using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class Enemy : MonoBehaviour, AddDebuff
{
    [SerializeField] public ActionType actionType;
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
    [Header("Moveback")]
    [SerializeField] int maxCountMoveback;
    int countMoveback;
    [SerializeField] bool movebackInMap;
    [SerializeField] float movebackDistanceMax;
    [SerializeField] float movebackDistanceMin;
    [SerializeField] float delayMoveback;
    bool selectTartgetMoveback;
    Vector3 targetMoveback;
    [Header("Attack")]
    [SerializeField] int damage;
    [SerializeField] float attackRange;
    float distanceAttack;
    [SerializeField] float moveBackSpeed;
    [SerializeField] float delayAttack;
    bool canAtk = false;
    [SerializeField] bool haveManyAttack;
    [SerializeField] int percentSpcialAttack;
    [HideInInspector] public bool specialAttack;
    [SerializeField] bool specialAttackAll;
    [SerializeField] bool countAttack;
    [SerializeField] int indexAttack;

    [Header("Knockback")]
    [SerializeField] float delayKnokback;
    bool canKnockback = true;
    [Header("Player")]
    [SerializeField] float diractionMagnitude;
    Vector2 diraction;
     public PlayerControl player;
    Vector3 targetPosition;
    [Header("Boss")]
    [SerializeField] bool bossMode;
    [SerializeField] bool halfHp;
    [Header("Coroutine")]
    Coroutine moveBack;
    Coroutine startAttack;
    Coroutine returnToMove;
    Coroutine checkNotAttack;
    void Awake()
    {
        spR = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        hp = enemyData.hp;
        damage = enemyData.damage;
        distanceAttack = attackRange;
    }
    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerControl>();
        }
        actionType = ActionType.MOVE;
        // actionType = ActionType.MOVEBACK;
    }
    void FixedUpdate()
    {
        CheckDistance();
        Move();
        Attack();
        MoveBack();
        CheckSide();
    }

    void CheckDistance()
    {
        //Calculate 
        //Distance
        diraction = player.gameObject.transform.position - transform.position;
        diractionMagnitude = diraction.magnitude;
        //Position Attack
        targetPosition = player.transform.position;
        // targetPosition.x += attackRange;
    }
    void CheckSide()
    {
        if (transform.position.x < player.gameObject.transform.position.x)
        {
            if (!spR.flipX)
            {
                attackRange = -attackRange;
                movebackDistanceMax = -movebackDistanceMax;
                movebackDistanceMin = -movebackDistanceMin;
            }
            spR.flipX = true;
        }
        else
        {
            if (spR.flipX)
            {
                attackRange = -attackRange;
                movebackDistanceMax = -movebackDistanceMax;
                movebackDistanceMin = -movebackDistanceMin;
            }
            spR.flipX = false;
        }
    }
    void Move()
    {
        if (actionType == ActionType.MOVE)
        {
            anim.Play("Walk");
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speedMove);
        }
    }
    public virtual void MoveBack()
    {
        if (actionType == ActionType.MOVEBACK)
        {
            anim.Play("WalkBack");
            if (!selectTartgetMoveback)
            {
                selectTartgetMoveback = true;
                countMoveback--;
                if (movebackInMap)
                {
                    targetMoveback = MoveBackRndInMap(ControlClampPlayer.Instance.virtualCameraClampFightPosition
                    .GetComponent<Collider2D>().bounds);
                }
                else
                {
                    targetMoveback = MoveBackDistance();
                }
            }
            transform.position = Vector3.MoveTowards(transform.position, targetMoveback, moveBackSpeed * Time.deltaTime);
            if (transform.position == targetMoveback)
            {
                if (countMoveback == 0)
                {
                    anim.Play("Idel");
                    actionType = ActionType.NORMAL;
                    returnToMove = StartCoroutine(ReturnToMove(delayMoveback));
                }
                else
                {
                    selectTartgetMoveback = false;
                }
            }
        }
    }
    void BossMoveBack()
    {
        actionType = ActionType.MOVEBACK;
        MoveBack();
    }
    Vector3 MoveBackRndInMap(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0
        );
    }

    Vector3 MoveBackDistance()
    {
        //ระยะห่างX 8 - 15 จากเพลเยอ || ค่าY = Max -2.5 ,Min -6.3 
        float rndX = Random.Range(movebackDistanceMin, movebackDistanceMax);
        float rndY = Random.Range(-2.5f, -6.3f);
        Vector3 rndInMap = new Vector3(rndX + player.transform.position.x, rndY, 0);
        return rndInMap;
    }
    #region Attack
    void Attack()
    {
        // Debug.Log(transform.position.x + " 1 " + transform.position.y);
        // Debug.Log(targetPosition.x + " 2 " + targetPosition.y);ไำ
        if (diractionMagnitude <= distanceAttack &&
            transform.position.y >= targetPosition.y - 0.2f &&
            transform.position.y <= targetPosition.y + 0.2f && !canAtk)
        {
            canAtk = true;
            if (actionType != ActionType.TAKEGRAB)
            {
                actionType = ActionType.NORMAL;
                actionType = ActionType.ATTACK;
                anim.Play("Idel");
                diractionMagnitude = attackRange;
                startAttack = StartCoroutine(StartAttack());
            }
        }
    }
    IEnumerator StartAttack()
    {
        if (checkNotAttack != null)
        {
            StopCoroutine(checkNotAttack);
        }
        if (startAttack != null)
        {
            StopCoroutine(startAttack);
        }
        yield return new WaitForSeconds(delayAttack);
        if (actionType == ActionType.ATTACK)
        {
            if (haveManyAttack)
            {
                int rndAtk = Random.Range(1, 101);
                if (rndAtk > percentSpcialAttack)
                {
                    anim.Play("Attack 2");
                    specialAttack = true;
                }
                else
                {
                    anim.Play("Attack 1");
                    specialAttack = false;
                }
            }
            else
            {
                if (countAttack)
                {
                    indexAttack++;
                    anim.SetInteger("IndexAttack", indexAttack);
                    if (indexAttack == 1)
                    {
                        anim.Play("Attack 1");
                    }
                }
                else
                {
                    anim.Play("Attack 1");
                }
                if (!specialAttackAll)
                {
                    specialAttack = false;
                }
                else
                {
                    specialAttack = true;
                }
            }
            if (diractionMagnitude <= distanceAttack &&
                transform.position.y >= targetPosition.y - 0.2f &&
                transform.position.y <= targetPosition.y + 0.2f)
            {
                // Debug.Log("Hit player");
                if (specialAttack)
                {
                    player.Takedamage(enemyData.damageSpecial, this);
                }
                else
                {
                    player.Takedamage(enemyData.damage, this);
                }
            }
            else
            {
                // Debug.Log("Miss player");
                anim.Play("Idel");
                if (checkNotAttack != null)
                {
                    StopCoroutine(checkNotAttack);
                }
                indexAttack = 0;
                anim.SetInteger("IndexAttack", indexAttack);
                yield return new WaitForSeconds(1f);
                countMoveback = maxCountMoveback;
                //moveback
                actionType = ActionType.MOVEBACK;
                canAtk = false;
            }
        }
    }
    void ReturnAtk()
    {
        checkNotAttack = StartCoroutine(CheckNotAttack());
        //Use In Animation
        startAttack = StartCoroutine(StartAttack());
    }
    IEnumerator CheckNotAttack()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(DelayToIdel());
    }
    #endregion
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
        indexAttack = 0;
        anim.SetInteger("IndexAttack", indexAttack);
        canKnockback = true;
        canAtk = false;
    }
    public void ReturnKnockbackToMove()
    {
        //ReturnToidel
        if (actionType == ActionType.DIE || hp <= 0)
        {
            anim.Play("Die");
        }
        else
        {
            anim.Play("Idel");
            StartCoroutine(DelayToIdel());
        }
    }
    IEnumerator DelayToIdel()
    {
        yield return new WaitForSeconds(0.5f);
        actionType = ActionType.MOVE;
    }
    #region Takedamage
    public void TakeDamage(int damage)
    {
        hp -= damage;
        StopAllMyCoroutines();
        if (hp <= 0)
        {
            if (bossMode)
            {
                Debug.Log("A");
                player.anim.Play("Win");
            }
            actionType = ActionType.DIE;
            hp = 0;
            if (actionType != ActionType.TAKEGRABTHORW)
            {
                Vector2 knockbackDirection = (transform.position - player.gameObject.transform.position).normalized;
                AddKnockback(-knockbackDirection, player.finishKnockbackForce, false);
            }
            else
            {
                player.CancelGrab();
            }
            ControlSpawnEnemy.Instance.CheckSpawnEnemy();
            actionType = ActionType.DIE;
            StartCoroutine(DieEffect());
            transform.parent = null;
            coll.enabled = false;
            player.enemyList.Remove(this);
        }
        else
        {
            if (bossMode && hp <= (hp * 0.5) && halfHp)
            {
                halfHp = false;
                anim.Play("SpawnBoss");
                //SpawnEnemy
                ControlSpawnEnemy.Instance.bossSpawn();
            }
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
    #endregion
    void GetHitReturn()
    {
        StartCoroutine(DelayGetHitReturn());
    }
    IEnumerator DelayGetHitReturn()
    {
        yield return new WaitForSeconds(0.5f);
        if (actionType != ActionType.TAKEGRAB)
        {
            canAtk = false;
        }
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

    IEnumerator ReturnToMove(float delay)
    {
        yield return new WaitForSeconds(delay);
        actionType = ActionType.MOVE;
        selectTartgetMoveback = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" && actionType == ActionType.TAKEGRABTHORW)
        {
            Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;
            Enemy enemyS = other.GetComponent<Enemy>();
            enemyS.AddKnockback(knockbackDirection, player.finishKnockbackForce, true);
            enemyS.TakeDamage(10);
        }
    }
}
