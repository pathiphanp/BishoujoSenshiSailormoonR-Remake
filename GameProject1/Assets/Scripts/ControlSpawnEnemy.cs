using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSpawnEnemy : Singleton<ControlSpawnEnemy>
{
    [Header("Enemy in Zone")]
    [SerializeField] Zone[] zones;
    public int indexZone;
    [SerializeField] int enemyCount;
    [SerializeField] int enemyIndexCount;
    void Start()
    {
        // zones[0].set[0].enemyInSet[0].spawnPosition;
        // zones[0].set[0].enemyInSet[0].enemy;
    }
    public void FirstSpawnEnemy()
    {
        enemyCount = zones[indexZone].set[0].enemyInSet.Length;
        enemyIndexCount = 0;
        for (int i = 0; i < zones[indexZone].firstEnemySet; i++)
        {
            enemyIndexCount = i;
            //ObjectSpawn
            Instantiate(zones[indexZone].set[0].enemyInSet[i].enemy,
             zones[indexZone].set[0].enemyInSet[i].spawnPosition.transform.position,//PositionSpawn
             zones[indexZone].set[0].enemyInSet[i].enemy.transform.localRotation);
        }
    }

    public void CheckSpawnEnemy()
    {
        enemyCount--;
        if (enemyCount == 0)
        {
            Debug.Log(indexZone);
            if (zones[indexZone].nextMap)
            {
                Debug.Log("Next Map");
                StartCoroutine(ControlClampPlayer.Instance.NextMap());
            }
            else
            {
                Debug.Log("Next Point");
                ControlClampPlayer.Instance.SetNextCheckPoint();
                ControlClampPlayer.Instance.onClamp = false;
            }
        }
        else if (enemyIndexCount < zones[indexZone].set[0].enemyInSet.Length - 1)
        {
            //ObjectSpawn
            enemyIndexCount++;
            Instantiate(zones[indexZone].set[0].enemyInSet[enemyIndexCount].enemy,
             zones[indexZone].set[0].enemyInSet[enemyIndexCount].spawnPosition.transform.position, //PositionSpawn
             zones[indexZone].set[0].enemyInSet[enemyIndexCount].enemy.transform.localRotation);
        }
    }
}
