using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;


public class ControlCutScenes : MonoBehaviour
{
   [HideInInspector] public SpawnCutScenes spawnCutScenes;
   [SerializeField] bool canSkipCutScenes;
   public TextEffect textEffect;
   void Start()
   {
      if (textEffect != null)
      {
         textEffect.controlCutScenes = this;
      }
   }
   public void EndCutSceens()
   {
      spawnCutScenes.StartSpawnCutScenes();
   }
   public void SpeedCutScenes()
   {
      textEffect.SkipTextEffect();
   }
}
