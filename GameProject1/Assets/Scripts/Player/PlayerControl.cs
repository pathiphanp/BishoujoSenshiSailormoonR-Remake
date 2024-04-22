using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerActionController;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public enum ActionType
{
    NORMAL, MOVE, JUMP, JUMPKICK, MOVEJUMP, ATTACK, ATTACKAROUND,
    SPECIALATTACK, GRAB, TAKEGRAB, TAKEGRABTHORW, KNOCKBACK, DIE, GAMEOVER
    , GETHIT, MOVEBACK, STUN
}
public class PlayerControl : MonoBehaviour, IGamePlayControlActions
{
    [Header("ActionType")]
    [SerializeField] ActionType actionType;
    [Header("Animator")]
    [SerializeField] public Animator anim;
    [SerializeField] public Sprite icon;
    [Header("Collider")]
    [SerializeField] Collider2D attack;
    [SerializeField] Collider2D kick;
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
    [Header("Status")]
    [SerializeField] public int hp;
    int maxhp;
    [SerializeField] float immortalDuration;
    [SerializeField] public int life;
    [SerializeField] public int specialAttackNum;
    [Header("Control")]
    [Header("Idel")]
    [SerializeField] float delayToIdel;
    Coroutine checkIdel;
    Coroutine countIdel;
    [Header("Move")]
    [SerializeField] float speedMove;
    [SerializeField] float clampYMax;
    [SerializeField] float clampYMin;
    [SerializeField] public float clampXMax;
    [SerializeField] public float clampXMin;
    Vector2 inputVector;
    [Header("Jump")]
    [SerializeField] float jump;
    [SerializeField] float moveJump;
    [SerializeField] float jumpDown;
    bool canJump = true;
    [Header("Knockback Force")]
    [SerializeField] public float finishKnockbackForce;
    [Header("Attack")]
    [SerializeField] int damage;
    [SerializeField] float delayAttack;
    int attackIndex;
    [SerializeField] bool canAtk = true;
    Coroutine _CountAtk;
    [Header("AroundAttack")]
    [SerializeField] AttackAround attackAround;
    [Header("Grab")]
    [SerializeField] Transform grabDetector;
    float detectionDistanceGrab;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float grabDuration;
    bool canGrab = true;
    int countGrabKick;
    Coroutine grabCount;
    [SerializeField] GameObject enemyPositionGrab;
    [Header("Enemy")]
    [HideInInspector] public List<Enemy> enemyList = new List<Enemy>();
    public Enemy enemyGrab;
    [Header("TakeDamage")]
    [SerializeField] int countTakedamage;
    Coroutine countTakeKnockback;
    [SerializeField] bool canTakeDamage = true;
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
        rb = GetComponent<Rigidbody2D>();
        rbGround = ground.GetComponent<Rigidbody2D>();
        playerControl = new PlayerActionController();
        playerControl.GamePlayControl.SetCallbacks(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        maxhp = hp;
        countIdel = StartCoroutine(CheckIdel());
    }
    // Update is called once per frame
    void Update()
    {
        GroundFollowPlayer();
        DetectBase();
        DetectGrabEnemy();
        if(Input.GetKeyDown(KeyCode.B))
        {

        }
    }
    void FixedUpdate()
    {
        MoveControl();
    }
    #region Trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            enemyList.Add(other.gameObject.GetComponent<Enemy>());
            foreach (Enemy e in enemyList)
            {
                e.player = this;
            }
            if (actionType == ActionType.JUMPKICK)
            {
                Enemy _enemy = other.GetComponent<Enemy>();
                _enemy.player = this;
                _enemy.TakeDamage(damage);
                Vector2 knockbackDirection = (transform.position - _enemy.gameObject.transform.position).normalized;
                _enemy.AddKnockback(knockbackDirection, finishKnockbackForce, false);
            }
        }
        if (other.gameObject.tag == "SpawnEnemy")
        {
        }
        if (other.gameObject.tag == "ClampMap")
        {
            other.gameObject.SetActive(false);
            ControlClampPlayer.Instance.CheckClamp();
        }
        if (other.gameObject.tag == "BossMap")
        {
            SceneManager.LoadScene("Boss");
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
    #endregion

    void MoveControl()
    {
        //Set Clamp Y Position Player
        // clampXMin = ControlClampPlayer.Instance.leftClamp.position.x;
        // clampXMax = ControlClampPlayer.Instance.rightClamp.position.x;
        float _YClampP = transform.position.y;
        _YClampP = Mathf.Clamp(_YClampP, clampYMin, clampYMax);
        float _YClampG = ground.transform.position.y;
        _YClampG = Mathf.Clamp(_YClampG, clampYMin, clampYMax);
        float _XClamp = transform.position.x;
        _XClamp = Mathf.Clamp(_XClamp, clampXMin, clampXMax);
        //Move
        if (actionType == ActionType.MOVE)
        {
            //Move Character
            rb.velocity = new Vector2(inputVector.x * speedMove, inputVector.y * speedMove);
            //Move Up Down Ground
            rbGround.velocity = new Vector2(rbGround.velocity.x, inputVector.y * speedMove);
            //Clamp Y Position
            ground.transform.position = new Vector3(ground.transform.position.x, _YClampG, ground.transform.position.z);
            transform.position = new Vector3(transform.position.x, _YClampP, transform.position.z);
            //Check Side
            ChangeSide(false);
            //Play Animation Walk
            if (inputVector != Vector2.zero)
            {
                anim.Play("Walk");
            }
        }
    }
    void ChangeSide(bool reverse)
    {
        //CheckStopCameraBack
        if (inputVector.x < 0)
        {
            // ControlClampPlayer.Instance.StopFollowPlayer();
            if (!reverse)
            {
                FlipX(true);
            }
            else
            {
                FlipX(false);
            }
        }
        else if (inputVector.x > 0)
        {
            // ControlClampPlayer.Instance.StartFollowPlayer();
            if (!reverse)
            {
                FlipX(false);
            }
            else
            {
                FlipX(true);
            }
        }
    }
    void FlipX(bool flip)
    {
        if (flip)
        {
            body.GetComponent<SpriteRenderer>().flipX = true;
            checkEnemy.transform.localRotation = Quaternion.Euler(0, -180f, 0);
            detectionDistanceGrab = -0.6f;
        }
        else
        {
            body.GetComponent<SpriteRenderer>().flipX = false;
            checkEnemy.transform.localRotation = Quaternion.Euler(0, 0, 0);
            detectionDistanceGrab = 0.1f;
        }

    }
    public void OnMoveMent(InputAction.CallbackContext context)
    {
        if (context.started && actionType == ActionType.NORMAL)
        {
            actionType = ActionType.MOVE;
            CallStopCheckIdel();
        }
        inputVector = context.ReadValue<Vector2>();
        if (context.canceled && actionType == ActionType.MOVE)
        {
            actionType = ActionType.NORMAL;
            SetStopMove();
            countIdel = StartCoroutine(CountIdel());
        }
    }
    void SetStopMove()
    {
        rb.velocity = Vector2.zero;
        rbGround.velocity = Vector2.zero;
    }
    IEnumerator CountIdel()
    {
        yield return new WaitForSeconds(0.1f);
        checkIdel = StartCoroutine(CheckIdel());
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
        if (hit && rb.velocity.y <= 0 && actionType == ActionType.JUMP || hit && rb.velocity.y <= 0 && actionType == ActionType.JUMPKICK)
        {
            kick.enabled = false;
            attack.enabled = true;
            rb.velocity = Vector2.zero;
            checkIdel = StartCoroutine(CheckIdel());
            rb.gravityScale = 0f;
            canFollowPlayer = true;
            canJump = true;
            canAtk = true;
            StartCoroutine(CanGrabRetrun());
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && actionType == ActionType.NORMAL || actionType == ActionType.MOVE)
        {
            actionType = ActionType.JUMP;
        }
        if (actionType == ActionType.JUMP && canJump)
        {
            canJump = false;
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
    IEnumerator CheckIdel()
    {
        if (inputVector != Vector2.zero)
        {
            actionType = ActionType.MOVE;
        }
        else
        {
            actionType = ActionType.NORMAL;
            anim.Play("Normal");
            yield return new WaitForSeconds(delayToIdel);
            anim.Play("Idel");
        }
    }
    void CallStopCheckIdel()
    {
        if (countIdel != null)
        {
            StopCoroutine(countIdel);
        }
        if (checkIdel != null)
        {
            StopCoroutine(checkIdel);
        }
    }
    public void OnNormalAttack(InputAction.CallbackContext context)
    {
        if (context.started && canAtk)
        {
            CallStopCheckIdel();
            if (actionType == ActionType.JUMP)
            {
                kick.enabled = true;
                attack.enabled = false;
                canGrab = false;
                canAtk = false;
                actionType = ActionType.JUMPKICK;
                anim.Play("KickJump");
            }
            else if (actionType == ActionType.GRAB)
            {
                canAtk = false;
                StopCoroutine(grabCount);
                if (inputVector.x == 0)
                {
                    anim.Play("GrabAttack");
                }
                else
                {
                    anim.Play("GrabTrow");
                }
            }
            else
            {
                actionType = ActionType.ATTACK;
                if (actionType == ActionType.ATTACK)
                {
                    canAtk = false;
                    SetStopMove();
                    if (_CountAtk != null)
                    {
                        StopCoroutine(_CountAtk);
                    }
                    anim.SetBool("OnAtk", true);
                    attackIndex++;
                    anim.SetInteger("Attack", attackIndex);
                    if (enemyList.Count > 0)
                    {
                        for (int i = 0; i < enemyList.Count; i++)
                        {
                            if (enemyList[i] != null)
                            {
                                enemyList[i].TakeDamage(damage);
                            }
                        }
                    }
                }
            }
        }
    }
    #region Scripts For Animation
    void ReturnAtk()
    {
        if (enemyList.Count == 0)
        {
            ResetAtk();
        }
        else
        {
            anim.SetBool("OnAtk", false);
            anim.Play("WaitAttack");
            canAtk = true;
        }
    }
    void EndWaitAttack()
    {
        ResetAtk();
    }
    void EndComboAttack()
    {
        ResetAtk();
        if (enemyList.Count > 0)
        {
            foreach (Enemy e in enemyList)
            {
                Vector2 knockbackDirection = (transform.position - e.transform.position).normalized;
                e.AddKnockback(knockbackDirection, finishKnockbackForce, false);
            }
        }
    }
    void ReturnGrabAtk()
    {
        canAtk = true;
        if (enemyGrab == null)
        {
            CancelGrab();
            checkIdel = StartCoroutine(CheckIdel());
        }
        else
        {
            if (countGrabKick < 3)
            {
                countGrabKick++;
                enemyGrab.TakeDamage(damage);
            }
            if (countGrabKick == 3)
            {
                enemyGrab.TakeDamage(damage);
                Vector2 knockbackDirection = (transform.position - enemyGrab.transform.position).normalized;
                enemyGrab.AddKnockback(knockbackDirection, finishKnockbackForce, false);
                CancelGrab();
                checkIdel = StartCoroutine(CheckIdel());
            }
            else
            {
                grabCount = StartCoroutine(CountGrabDuration());
            }
        }
    }
    void ReturnThrowGrab()
    {
        canAtk = true;
        Vector2 knockbackDirection = (transform.position - enemyGrab.transform.position).normalized;
        enemyGrab.AddKnockback(-knockbackDirection, finishKnockbackForce * 1.5f, true);
        enemyGrab.TakeDamage(damage);
        CancelGrab();
    }
    void ThorwGrabFlip()
    {
        ChangeSide(true);
        if (enemyGrab != null)
        {
            enemyGrab.SetOnGrab(enemyPositionGrab, ActionType.TAKEGRABTHORW, false);
        }
    }
    void RetrunAroundAttack()
    {
        canAtk = true;
        attackAround.CancelCastSkill();
        checkIdel = StartCoroutine(CheckIdel());
    }
    void ResetAtk()
    {
        anim.SetBool("OnAtk", false);
        anim.SetInteger("Attack", 0);
        attackIndex = 0;
        canAtk = true;
        checkIdel = StartCoroutine(CheckIdel());
    }
    #endregion
    public void OnAroundAttack(InputAction.CallbackContext context)
    {
        if (context.started && hp > (maxhp * 0.05))
        {
            if (actionType == ActionType.NORMAL || actionType == ActionType.MOVE)
            {
                CallStopCheckIdel();
                SetStopMove();
                anim.Play("AttackAround");
                actionType = ActionType.ATTACKAROUND;
                attackAround.CastAroundAttack(damage, finishKnockbackForce, false);
            }
        }
    }
    public void OnSupperAttack(InputAction.CallbackContext context)
    {
        if (context.started && specialAttackNum > 0)
        {
            if (actionType == ActionType.NORMAL || actionType == ActionType.MOVE)
            {
                CallStopCheckIdel();
                SetStopMove();
                anim.Play("SpecialAttack");
                HpUIManager.Instance.UpdateSpecialAttackStack();
                actionType = ActionType.SPECIALATTACK;
                attackAround.CastAroundAttack(99999, finishKnockbackForce, true);
            }
        }
    }
    void DetectGrabEnemy()
    {
        RaycastHit2D hit = Physics2D.Raycast(grabDetector.position, -Vector2.left, detectionDistanceGrab, enemyLayer);
        if (hit)
        {
            if (actionType == ActionType.MOVE && canGrab)
            {
                actionType = ActionType.GRAB;
                canGrab = false;
                SetStopMove();
                CallStopCheckIdel();
                enemyGrab = hit.collider.gameObject.GetComponent<Enemy>();
                enemyGrab.SetOnGrab(enemyPositionGrab, ActionType.TAKEGRAB, true);
                anim.Play("Grab");
                grabCount = StartCoroutine(CountGrabDuration());
            }
        }
    }
    IEnumerator CountGrabDuration()
    {
        yield return new WaitForSeconds(grabDuration);
        CancelGrab();
    }
    public IEnumerator CanGrabRetrun()
    {
        yield return new WaitForSeconds(1f);
        canGrab = true;
    }
    public void CancelGrab()
    {
        actionType = ActionType.NORMAL;
        countGrabKick = 0;
        if (enemyGrab != null)
        {
            enemyPositionGrab.transform.DetachChildren();
            enemyGrab.transform.localRotation = Quaternion.Euler(0, 0, 0);
            enemyGrab = null;
        }
        StartCoroutine(CanGrabRetrun());
        countIdel = StartCoroutine(CheckIdel());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(jumpDetector.transform.position, -Vector2.up * detectionDistance);
        Gizmos.DrawRay(grabDetector.transform.position, -Vector2.left * detectionDistanceGrab);
    }
    #region  Takedamage
    public void Takedamage(int _damage, Enemy _enemy)
    {
        if (canTakeDamage)
        {
            hp -= _damage;
            if (enemyGrab != null)
            {
                enemyGrab.transform.SetParent(null);
                enemyGrab.GetComponent<Enemy>().ReturnKnockbackToMove();
                enemyGrab.GetComponent<Enemy>().actionType = ActionType.MOVEBACK;
                enemyGrab.GetComponent<Enemy>().MoveBack();
                enemyGrab = null;
            }
            if (hp > 0)
            {
                actionType = ActionType.GETHIT;
                if (countTakeKnockback != null)
                {
                    StopCoroutine(countTakeKnockback);
                }
                CallStopCheckIdel();
                SetStopMove();
                playerControl.GamePlayControl.Disable();
                HpUIManager.Instance.UpdateHpPlayer();
                if (_enemy != null)
                {
                    HpUIManager.Instance.CallShowHpEnemy(_enemy);
                }
                if (_enemy != null)
                {
                    countTakeKnockback = StartCoroutine(CountTakeDamageKnockback());
                    countTakedamage++;
                    if (countTakedamage == 3 || _enemy.specialAttack)
                    {
                        if (_enemy.specialAttack)
                        {
                            countTakedamage = 0;
                        }
                        anim.Play("KnockBack");
                        canTakeDamage = false;
                        countTakeKnockback = null;
                        Vector2 knockbackDiraction = (transform.position - _enemy.gameObject.transform.position).normalized;
                        knockbackDiraction.x = -knockbackDiraction.x;
                        StartCoroutine(Knockback(knockbackDiraction, -0.005f));
                    }
                    else
                    {
                        if (_enemy.transform.position.x < transform.position.x)
                        {
                            FlipX(true);
                        }
                        else
                        {
                            FlipX(false);
                        }
                        anim.Play("GetHit");
                    }
                }
            }
            else if (hp <= 0)//Check Knockback;
            {
                StopAllCoroutines();
                CancelGrab();
                canTakeDamage = false;
                hp = 0;
                HpUIManager.Instance.UpdateHpPlayer();
                //Die Animation
                actionType = ActionType.DIE;
                SetStopMove();
                anim.Play("Die");
            }
        }
    }
    #endregion
    void ReturnGetHit()
    {
        actionType = ActionType.NORMAL;
        playerControl.GamePlayControl.Enable();
        canTakeDamage = true;
        ResetAtk();
        CheckIdel();
    }
    #region AnimationCheck
    void CheckLife()
    {
        //Check have life 
        if (life > 0)
        {
            //Spawn player to Die Positon
            actionType = ActionType.JUMP;
            transform.position = new Vector3(transform.position.x, 15f, 0);
            rb.gravityScale = 10;
            ResetStatusPlyer();
            life--;
            HpUIManager.Instance.UpdateLife();
            StartCoroutine(ImmortalDuration());
        }
        else
        {
            //Count player play Agine
            actionType = ActionType.GAMEOVER;
            transform.position = new Vector3(transform.position.x, 15f, 0);
            HpUIManager.Instance.CallStartCountGameOver();
            //Spawn player to Die Positon ane Reset life
            //Retrun To main manu 
        }
    }
    void ResetStatusPlyer()
    {
        canAtk = true;
        canGrab = true;
        hp = maxhp;
        HpUIManager.Instance.ResetHpPlayer();
    }
    IEnumerator ImmortalDuration()
    {
        anim.SetBool("Immortal", true);
        anim.Play("Immortal");
        yield return new WaitForSeconds(immortalDuration);
        anim.SetBool("Immortal", false);
        canTakeDamage = true;
    }
    #endregion
    IEnumerator CountTakeDamageKnockback()
    {
        yield return new WaitForSeconds(1f);
        countTakedamage = 0;
        countTakeKnockback = null;
    }
    public IEnumerator Knockback(Vector3 knockbackDiraction, float knockbackForce)
    {
        actionType = ActionType.KNOCKBACK;
        rb.velocity = Vector3.zero;
        knockbackDiraction.y = 0;
        rb.AddForce(knockbackDiraction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.05f);
        rb.Sleep();
        rb.velocity = Vector3.zero;
        anim.Play("RetrunToNormal");
    }
    void RetrunToImmortal()
    {
        anim.Play("Immortal");
    }
    void RetrunToNormal()
    {
        canTakeDamage = true;
        countIdel = StartCoroutine(CheckIdel());
    }

    public void OnContinue(InputAction.CallbackContext context)
    {
        if (context.started && actionType == ActionType.GAMEOVER)
        {
            HpUIManager.Instance.StopGameOver();
            RestartGameOver();
        }
    }
    void RestartGameOver()
    {
        ResetStatusPlyer();
        actionType = ActionType.JUMP;
        StartCoroutine(ImmortalDuration());
        life = 3;
        rb.gravityScale = 10;
        HpUIManager.Instance.UpdateLife();
    }
    void Win()
    {
        StartCoroutine(DelayWin());
    }
    IEnumerator DelayWin()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Win");
    }
}
