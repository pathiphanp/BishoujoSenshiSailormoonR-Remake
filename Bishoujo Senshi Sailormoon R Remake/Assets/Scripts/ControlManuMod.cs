using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum InputMode
{
    CUTSCENES, STARTGAME, SETNUMBERPLAYER, CHOOSECHARACTER, PLAYGAME

}
public class ControlManuMod : MonoBehaviour
{
    [SerializeField] public InputMode actiontype;
    [SerializeField] public GameObject startManuGame;
    [SerializeField] GameObject setPlayer;
    [SerializeField] GameObject chooseCharacter;
    [SerializeField] SpawnCutScenes spawnCutScenes;
    [SerializeField] Animator chooseCharacterAnimator;
    bool VENUS = true;
    bool MERCURY;
    void Start()
    {
        spawnCutScenes.controlManuMod = this;
    }
    void Update()
    {
        ControlMod();
    }
    void ControlMod()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            InputControl();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            InputControl();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            InputControl();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            chooseCharacterAnimator.Play("ChooseVenusSelect");
            VENUS = true;
            MERCURY = false;
            SceneManager.LoadScene("AllGamePlay1");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            chooseCharacterAnimator.Play("ChooseMercurySelect");
            VENUS = false;
            MERCURY = true;
            // SceneManager.LoadScene("AllGamePlay2");
        }
    }
    void InputControl()
    {
        if (actiontype == InputMode.CUTSCENES)
        {
            spawnCutScenes.SpeedCutScenes();
            return;
        }
        if (actiontype == InputMode.STARTGAME)
        {
            actiontype = InputMode.SETNUMBERPLAYER;
            startManuGame.SetActive(false);
            setPlayer.SetActive(true);
            return;
        }
        if (actiontype == InputMode.SETNUMBERPLAYER)
        {
            actiontype = InputMode.CHOOSECHARACTER;
            setPlayer.SetActive(false);
            chooseCharacter.SetActive(true);
            return;
        }
        if (actiontype == InputMode.CHOOSECHARACTER)
        {
            if (MERCURY)
            {
                Debug.Log("Play MERCURY");
            }
            if (VENUS)
            {
                Debug.Log("Play VENUS");
            }
            return;
        }
    }
}
