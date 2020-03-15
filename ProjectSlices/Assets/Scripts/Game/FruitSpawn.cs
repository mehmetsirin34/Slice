using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Objects
{
    Sphere,
    Cylinder,
    CubeYellow,
    CubeGreen 
}

public class FruitSpawn : MonoBehaviour,IPooledObject
{
    Objects randomObjectName;

    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private GameObject Prefab;
    [SerializeField] GameObject[] Fruit;
    [SerializeField] GameObject Block;

    public float SpawnTime = 1.5f;

    private int RandomNumber;
    private int SameFruitCount;
    private int GroupFruitCount = 0;

    int random;
    bool BlockSpawn;

    private void Start()
    {
        StartCoroutine(WaitSpawn());

        RandomNumber = Random.Range(0, Fruit.Length);

        random = Random.Range(1, 3);

    }
    private void Spawn()
    {
        if (SameFruitCount == 3)
        {
            RandomNumber = Random.Range(0, Fruit.Length);
            SameFruitCount = 0;
            GroupFruitCount++;
        }

        if (GroupFruitCount != random)
            SameFruitCount++;

        if (GroupFruitCount == 0)
        {
            BlockSpawn = false;
            randomObjectName = (Objects)RandomNumber; 
        }
        else if (GroupFruitCount == random)
        {
            BlockSpawn = true;
            GroupFruitCount = 0;

            random = Random.Range(1, 3);
        }

        OnObjectSpawn();
    }
    public void OnObjectSpawn()
    {
        if (BlockSpawn)
            ObjectPooler.Instance.SpawnFromPool("Block", SpawnPoint.transform.position, Quaternion.identity);
        else
            ObjectPooler.Instance.SpawnFromPool(randomObjectName.ToString(), SpawnPoint.transform.position, Quaternion.identity);
    }
    IEnumerator WaitSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(SpawnTime);
            Spawn();
        }
    }
}
