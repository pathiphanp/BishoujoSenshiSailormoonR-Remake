using System.Diagnostics.Contracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public enum Hptye
{
    MIN, MEDIUM, MAX
}
public class HpUIManager : Singleton<HpUIManager>
{
    [Header("Player")]
    [SerializeField] Slider playerHP;
    [SerializeField] Image iconPlayer;
    PlayerControl player;
    [SerializeField] TMP_Text life;
    [SerializeField] TMP_Text specialAttackStack;
    [SerializeField] TMP_Text countPlayAgain;
    Coroutine countGameOver;
    [Header("Enemy")]
    [SerializeField] DataHpSlider[] enemyHp;
    [SerializeField] Image enemyIcon;
    [SerializeField] float delayEnemyHp;
    Coroutine showHpEnemy;
    void Start()
    {
        player = FindObjectOfType<PlayerControl>();
        iconPlayer.sprite = player.icon;
        playerHP.maxValue = player.hp;
        playerHP.value = playerHP.maxValue;
        UpdateSpecialAttackStack();
        UpdateLife();
    }
    public void ResetHpPlayer()
    {
        playerHP.value = playerHP.maxValue;
    }
    public void UpdateSpecialAttackStack()
    {
        specialAttackStack.text = player.specialAttackNum.ToString();
    }
    public void UpdateLife()
    {
        life.text = "X " + player.life.ToString();
    }
    public void UpdateHpPlayer()
    {
        playerHP.value = player.hp;
    }
    public void CallShowHpEnemy(Enemy enemy)
    {
        if (showHpEnemy != null)
        {
            StopCoroutine(showHpEnemy);
        }
        for (int i = 0; i < enemyHp.Length; i++)
        {
            if (enemy.enemyData.hptye == enemyHp[i].hptye)
            {
                enemyHp[i].hpslider.maxValue = enemy.enemyData.hp;
                enemyHp[i].hpslider.value = enemy.hp;
                enemyIcon.sprite = enemy.enemyData.icon;
                showHpEnemy = StartCoroutine(ShowHpEnemy(enemyHp[i].hpslider));
            }
        }
    }
    IEnumerator ShowHpEnemy(Slider hpShow)
    {
        hpShow.gameObject.SetActive(true);
        enemyIcon.gameObject.SetActive(true);
        yield return new WaitForSeconds(delayEnemyHp);
        hpShow.gameObject.SetActive(false);
        enemyIcon.gameObject.SetActive(false);
    }
    public void CallStartCountGameOver()
    {
        StopAllCoroutines();
        countGameOver = StartCoroutine(CountPlayAgain(10));
    }
    public IEnumerator CountPlayAgain(int timeToOver)
    {
        countPlayAgain.gameObject.SetActive(true);
        life.gameObject.SetActive(false);
        specialAttackStack.gameObject.SetActive(false);
        playerHP.gameObject.SetActive(false);
        foreach (DataHpSlider e in enemyHp)
        {
            e.hpslider.gameObject.SetActive(false);
        }
        while (timeToOver > 0)
        {
            yield return new WaitForSeconds(1f);
            timeToOver--;
            countPlayAgain.text = "CONTINUE? " + timeToOver.ToString();
            yield return true;
        }
        //GameOver Retrun to main manu
        SceneManager.LoadScene(0);
    }

    public void StopGameOver()
    {
        StopCoroutine(countGameOver);
        countPlayAgain.gameObject.SetActive(false);
        life.gameObject.SetActive(true);
        specialAttackStack.gameObject.SetActive(true);
        playerHP.gameObject.SetActive(true);
    }
}
