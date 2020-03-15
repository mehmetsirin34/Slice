using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSlice : MonoBehaviour
{
    [SerializeField] private MeshClipComponent MeshClip;
    //public TriggerControl control;

    public bool onetime = true;
    public bool kesildi;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.distance <5f && hit.collider.gameObject.tag == "Fruit" && onetime == true)
            {
                GameObject victim = hit.collider.gameObject;

                MeshClip.Target = victim.GetComponent<TargetComponent>();
                MeshClip.Clip(victim, transform.position, transform.right);

                kesildi = true;
                onetime = false;
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
    }
}
