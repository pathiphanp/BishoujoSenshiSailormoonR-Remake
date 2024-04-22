using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;


public class SpawnCutScenes : MonoBehaviour
{
    [HideInInspector] public ControlManuMod controlManuMod;
    [SerializeField] GameObject[] cutScenes;
    [SerializeField] ControlCutScenes controlCutScenes;
    [SerializeField] bool openGame;
    [SerializeField] GameObject playGame;
    int indexCutScenes;
    void Start()
    {
        StartSpawnCutScenes();
    }
    public void StartSpawnCutScenes()
    {

        if (indexCutScenes == cutScenes.Length)
        {
            if (controlManuMod != null)
            {
                controlManuMod.startManuGame.SetActive(true);
                controlManuMod.actiontype = InputMode.STARTGAME;
            }
            this.gameObject.SetActive(false);
            if (openGame)
            {
                playGame.SetActive(true);
            }
            return;
        }
        GameObject _cutscenes = Instantiate(cutScenes[indexCutScenes], transform);
        controlCutScenes = _cutscenes.GetComponent<ControlCutScenes>();
        controlCutScenes.spawnCutScenes = this;
        indexCutScenes++;

    }
    public void SpeedCutScenes()
    {
        if (controlCutScenes != null)
        {
            controlCutScenes.SpeedCutScenes();
        }
    }

}
