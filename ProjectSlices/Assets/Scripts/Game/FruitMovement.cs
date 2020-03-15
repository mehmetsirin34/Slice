using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitMovement : MonoBehaviour
{
    private float LimitZ = -2.5f;
    private float LimitY = -1;

    private GameManager GameManager;

    private void Start()
    {
        GameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        transform.Translate(new Vector3(0, 0, -1 * GameManager.SpeedMovement) * Time.deltaTime);
        DestroyControl();
    }

    private void DestroyControl()
    {
        string name = gameObject.name;
        if (transform.position.z < -2.5f || transform.position.y < -1)
        {
            if (name != "Remained" && name != "Sliced")
            {
                ObjectPooler.Instance.ReturnToPool(gameObject);
            }
            else
                Destroy(gameObject);
        }
    }
}