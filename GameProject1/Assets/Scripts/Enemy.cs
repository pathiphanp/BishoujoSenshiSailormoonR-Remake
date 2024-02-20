using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, AddDebuff
{
    Rigidbody2D rb;
    Animator anim;
    Collider2D coll;
    [Header("Status")]
    [SerializeField] int hp;
    [Header("Move")]
    [SerializeField] float speedMove;
    float move;
    [Header("MoveBack")]
    [SerializeField] float speedMoveBack;
    [SerializeField] float moveBackRange;
    [SerializeField] float moveBackDelay;
    [SerializeField] bool canMoveback;
    bool moveBack;
    float huntRange;
    bool canMove = true;
    [Header("Attack")]
    [SerializeField] int damage;
    [SerializeField] float attackRange;
    [SerializeField] float delayAtk;
    bool canAtk = true;
    [Header("Knockback")]
    [SerializeField] float delayKnokback;
    bool canKnockback = true;
    public PlayerControl playerControl;
    void Awake()
    {
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    public void AddKnockback(Vector2 knockbackDiraction, float knockbackForce)
    {
        if (canKnockback)
        {
            canKnockback = false;
            // anim.Play("Hit");
            StartCoroutine(Knockback(knockbackDiraction, knockbackForce));
        }
    }
    IEnumerator Knockback(Vector2 knockbackDiraction, float knockbackForce)
    {
        canMove = false;
        rb.velocity = Vector3.zero;
        canAtk = false;
        rb.AddForce(knockbackDiraction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(delayKnokback);
        rb.velocity = Vector3.zero;
        move = speedMove;
        canMove = true;
        canKnockback = true;
        canAtk = true;
    }
    public void AddStun()
    {

    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            playerControl.enemyList = null;
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
