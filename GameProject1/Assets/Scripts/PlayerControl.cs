using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using static PlayerActionController;

public class PlayerControl : MonoBehaviour, IGamePlayControlActions
{
    [Header("Animator")]
    Animator anim;
    [Header("Rigidbody")]
    Rigidbody2D rbGround;
    Rigidbody2D rb;
    [Header("PlayerController")]
    PlayerActionController playerControl;
    [Header("Body Player")]
    [SerializeField] GameObject body;
    [Header("CheckEnemy")]
    [SerializeField] GameObject checkEnemy;
    [Header("CheckGround")]
    [SerializeField] Transform jumpDetector;
    [SerializeField] float detectionDistance;
    [SerializeField] LayerMask groundLayer;
    [Header("Ground")]
    [SerializeField] GameObject ground;
    bool canFollowPlayer = true;
    [Header("Control")]
    [Header("Idel")]
    [SerializeField] float delayToIdel;
    List<Coroutine> checkIdel = new List<Coroutine>();
    bool onIdel = true;
    Coroutine countIdel;
    [Header("Move")]
    [SerializeField] float speedMove;
    bool canMove = true;
    [SerializeField] float clampYMax;
    [SerializeField] float clampYMin;
    Vector2 inputVector;
    [Header("Jump")]
    [SerializeField] float jump;
    [SerializeField] float moveJump;
    [SerializeField] float jumpDown;
    bool onJump;
    [Header("Attack")]
    [SerializeField] int damage;
    [SerializeField] float knockbackForce;
    [SerializeField] float delayAttack;
    int attackIndex;
    [SerializeField] bool canAtk = true;
    Coroutine _CountAtk;
    [Header("Enemy")]
    [SerializeField] public List<Enemy> enemyList = new List<Enemy>();

    void OnEnable()
    {
        playerControl.GamePlayControl.Enable();
    }
    void OnDisable()
    {
        playerControl.GamePlayControl.Disable();
    }
    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rbGround = ground.GetComponent<Rigidbody2D>();
        playerControl = new PlayerActionController();
        playerControl.GamePlayControl.SetCallbacks(this);

    }
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        GroundFollowPlayer();
    }
    void FixedUpdate()
    {
        MoveControl();
        DetectBase();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            enemyList.Add(other.gameObject.GetComponent<Enemy>());
            foreach (Enemy e in enemyList)
            {
                e.playerControl = this;
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            RemoveEnemy(other.gameObject.GetComponent<Enemy>());
        }
    }
    public void RemoveEnemy(Enemy enemy)
    {
        enemyList.Remove(enemy);
    }
    #region Scripts For Animation

    #endregion
    public void OnMoveMent(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            onIdel = false;
            CallStopCheckIdel();
        }
        inputVector = context.ReadValue<Vector2>();
        if (inputVector.x < 0)
        {
            body.GetComponent<SpriteRenderer>().flipX = true;
            checkEnemy.transform.localRotation = Quaternion.Euler(0, -180f, 0);
        }
        else if (inputVector.x > 0)
        {
            body.GetComponent<SpriteRenderer>().flipX = false;
            checkEnemy.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (context.canceled)
        {
            if (!onJump)
            {
                onIdel = true;
                countIdel = StartCoroutine(CountIdel());
                rb.velocity = Vector2.zero;
            }
        }
    }
    IEnumerator CountIdel()
    {
        yield return new WaitForSeconds(0.1f);
        checkIdel.Add(StartCoroutine(CheckIdel()));
    }
    void MoveControl()
    {
        //Set Clamp Y Position Player
        float _YClampP = transform.position.y;
        _YClampP = Mathf.Clamp(_YClampP, clampYMin, clampYMax);
        //Set Clamp Y Position Ground
        float clampYMaxG = -2.8f;
        float clampYMinG = -6.45f;
        float _YClampG = ground.transform.position.y;
        _YClampG = Mathf.Clamp(_YClampG, clampYMinG, clampYMaxG);
        //Move
        if (canMove)
        {
            //Move Character
            rb.velocity = new Vector2(inputVector.x * speedMove, inputVector.y * speedMove);
            //Move Up Down Ground
            rbGround.velocity = new Vector2(rbGround.velocity.x, inputVector.y * speedMove);
            //Clamp Y Position
            transform.position = new Vector3(transform.position.x, _YClampP, transform.position.z);
            ground.transform.position = new Vector3(ground.transform.position.x, _YClampG, ground.transform.position.z);
            if (inputVector != Vector2.zero)
            {
                anim.Play("Walk");
            }
        }
    }
    void GroundFollowPlayer()
    {
        if (canFollowPlayer)
        {
            ground.transform.position = new Vector3(transform.position.x,
                    ground.transform.position.y, ground.transform.position.z);
        }
    }
    void DetectBase()
    {
        RaycastHit2D hit = Physics2D.Raycast(jumpDetector.position, -Vector2.up, detectionDistance, groundLayer);
        if (hit && onJump && rb.velocity.y <= 0)
        {
            onJump = false;
            rb.velocity = Vector2.zero;
            checkIdel.Add(StartCoroutine(CheckIdel()));
            rb.gravityScale = 0f;
            canFollowPlayer = true;
            canMove = true;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!onJump)
        {
            onJump = true;
            CallStopCheckIdel();
            SetStopMove();
            if (inputVector.x == 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jump);
                rb.gravityScale = jumpDown;
                anim.Play("Jump");
            }
            else
            {
                rb.velocity = new Vector2(inputVector.x * moveJump, jump);
                rb.gravityScale = jumpDown;
                anim.Play("MoveJump");
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(jumpDetector.transform.position, -Vector2.up * detectionDistance);
    }
    IEnumerator CheckIdel()
    {
        anim.Play("Normal");
        yield return new WaitForSeconds(delayToIdel);
        anim.Play("Idel");
    }
    void CallStopCheckIdel()
    {
        if (countIdel != null)
        {
            StopCoroutine(countIdel);
        }
        if (checkIdel.Count > 0)
        {
            foreach (Coroutine si in checkIdel)
            {
                StopCoroutine(si);
            }
        }
    }
    public void OnNormalAttack(InputAction.CallbackContext context)
    {
        CallStopCheckIdel();
        if (onJump)
        {
            anim.Play("KickJump");
        }
        else if (canAtk)
        {
            canAtk = false;
            SetStopMove();
            if (_CountAtk != null)
            {
                StopCoroutine(_CountAtk);
            }
            attackIndex++;
            anim.SetInteger("Attack", attackIndex);
            if (enemyList.Count > 0)
            {
                Debug.Log("Have Enemy");
                foreach (Enemy e in enemyList)
                {
                    Vector2 knockbackDirection = (transform.position - e.transform.position).normalized;
                    e.AddKnockback(knockbackDirection, knockbackForce);
                    e.TakeDamage(damage);
                }
            }
            else
            {
                Debug.Log("Not Enemy");
                attackIndex = 0;
            }
        }
    }

    IEnumerator CountAttckDelay()
    {
        yield return new WaitForSeconds(delayAttack);
        Debug.Log("Can not do combo");
        attackIndex = 0;
        anim.SetInteger("Attack", attackIndex);
        checkIdel.Add(StartCoroutine(CheckIdel()));
    }
    void ReturnAtk()
    {
        canAtk = true;
        canMove = true;
        _CountAtk = StartCoroutine(CountAttckDelay());
    }
    void EndComboAttack()
    {
        attackIndex = 0;
        anim.SetInteger("Attack", attackIndex);
        checkIdel.Add(StartCoroutine(CheckIdel()));
        ReturnAtk();
    }
    public void OnAroundAttack(InputAction.CallbackContext context)
    {

    }

    public void OnSupperAttack(InputAction.CallbackContext context)
    {

    }

    void SetStopMove()
    {
        canMove = false;
        rb.velocity = Vector2.zero;
        rbGround.velocity = Vector2.zero;
    }
}
