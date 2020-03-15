using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFalse : MonoBehaviour
{
    public float destructionTime;
    void Start()
    {
       // ObjectPooler.Instance.ReturnToPool(this.gameObject, destructionTime);
    }
}
