using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeSlice : MonoBehaviour
{
    public GetScript Get;

    int oneTime;

    void Update()
    {
        RaycastHit hit;

        if (Get.BladeControl.ComingBack == true)
            oneTime = 0;

        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.distance < 10 && hit.collider.gameObject.tag == "Fruit" && Get.BladeControl.ComingBack == false)
            {
                oneTime++;

                if (oneTime == 1)
                {
                    GameObject victim = hit.collider.gameObject;

                    Get.MeshClip.Target = victim.GetComponent<TargetComponent>();

                    Get.MeshClip.Clip(victim, transform.position, transform.right);
                    Get.UIManager.ScoreIncreaseAndAssigned(10);
                }
            }
            else if (hit.distance < 0.8f && hit.collider.gameObject.tag == "Block" && Get.BladeControl.ComingBack == false)
            {
                Get.GameManager.GameOver = true;
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f);
    }
}
