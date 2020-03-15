using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private int Force;
    [Space]
    [SerializeField] private int DirectionX;
    [SerializeField] private int DirectionY;
    [SerializeField] private int DirectionZ;

    void Update()
    {
        transform.Rotate(new Vector3(DirectionX, DirectionY, DirectionZ) * Force * Time.deltaTime);
    }
}
