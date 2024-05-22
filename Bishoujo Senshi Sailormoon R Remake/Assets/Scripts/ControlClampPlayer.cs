using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ControlClampPlayer : Singleton<ControlClampPlayer>
{
    PlayerControl playerControl;
    [SerializeField] CinemachineVirtualCamera virtualCameraFollowPlayer;
    [SerializeField] public CinemachineVirtualCamera virtualCameraClampFightPosition;
    [SerializeField] GameObject[] clampPosition;
    public int indexClamp = 0;
    [SerializeField] public Transform rightClamp;
    [SerializeField] public Transform leftClamp;
    public bool onClamp;
    int setZone = 0;

    [Header("NextMapSet")]
    [SerializeField] GameObject fade;
    [SerializeField] Transform startTransform;
    [SerializeField] GameObject boxmap;
    [SerializeField] Transform ChangePositionboxmap;
    [Header("BossMap")]
    [SerializeField] Transform startTransformBoss;
    [SerializeField] GameObject boxmapBoss;
    [SerializeField] Transform ChangePositionboxmapBoss;
    [SerializeField] GameObject boss;
    [SerializeField] GameObject Fileboss;
    //8.5 //18.8 //30.5 //41.48 //52.3
    private void Start()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        setZone = -1;
        CheckClamp();
    }
    public void CheckClamp()
    {
        onClamp = true;
        StartClamp();
        setZone++;
        ControlSpawnEnemy.Instance.indexZone = setZone;
        ControlSpawnEnemy.Instance.FirstSpawnEnemy();
        ControlSpawnEnemy.Instance.controlClampPlayer = this;
    }
    public void StartFollowPlayer()
    {
        if (!onClamp)
        {
            if (virtualCameraFollowPlayer.gameObject.transform.position.x < playerControl.gameObject.transform.position.x)
            {
                virtualCameraFollowPlayer.Follow = playerControl.gameObject.transform;
            }
        }
    }
    void StartClamp()
    {
        virtualCameraFollowPlayer.Priority = 0;
        virtualCameraClampFightPosition.Priority = 1;
        Vector3 clampFightPosition = clampPosition[indexClamp].gameObject.transform.position;
        clampFightPosition.z = -10;
        virtualCameraClampFightPosition.gameObject.transform.position = clampFightPosition;
    }
    public void StopClamp()
    {
        virtualCameraFollowPlayer.Priority = 1;
        virtualCameraClampFightPosition.Priority = 0;
    }
    public void StopFollowPlayer()
    {
        virtualCameraFollowPlayer.Follow = null;
    }
    public void SetNextCheckPoint()
    {
        clampPosition[indexClamp].gameObject.SetActive(false);
        indexClamp++;
        if (indexClamp < clampPosition.Length)
        {
            clampPosition[indexClamp].gameObject.SetActive(true);
        }
    }

    public IEnumerator NextMap(bool bossMap)
    {
        if (!bossMap)
        {
            fade.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            playerControl.transform.position = startTransform.position;
            boxmap.transform.position = ChangePositionboxmap.position;
            yield return new WaitForSeconds(0.5f);
            fade.SetActive(false);
            SetNextCheckPoint();
        }
        else
        {
            fade.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            playerControl.transform.position = startTransformBoss.position;
            boxmap.transform.position = ChangePositionboxmapBoss.position;
            yield return new WaitForSeconds(0.5f);
            fade.SetActive(false);
            boss.gameObject.SetActive(true);
            
        }
    }

    

}
