using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ControlClampPlayer : Singleton<ControlClampPlayer>
{
    PlayerControl playerControl;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
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
        StopFollowPlayer();
        setZone++;
        ControlSpawnEnemy.Instance.indexZone = setZone;
        ControlSpawnEnemy.Instance.FirstSpawnEnemy();
    }
    public void StartFollowPlayer()
    {
        if (!onClamp)
        {
            if (virtualCamera.gameObject.transform.position.x < playerControl.gameObject.transform.position.x)
            {
                virtualCamera.Follow = playerControl.gameObject.transform;
            }
        }
    }
    public void StopFollowPlayer()
    {
        virtualCamera.Follow = null;
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

    public IEnumerator NextMap()
    {
        fade.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        playerControl.transform.position = startTransform.position;
        boxmap.transform.position = ChangePositionboxmap.position;
        yield return new WaitForSeconds(0.5f);
        fade.SetActive(false);
        SetNextCheckPoint();
    }

}
