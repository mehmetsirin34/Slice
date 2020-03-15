using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform point0, point1, point2;
    public Transform center;

    private Vector3 MiddlePoint;

    public int lenght;

    public GameObject ArrowParent;
    public GameObject Arrow;

    private int numPoints;
    private Vector3[] positions;

    public float MinValue;
    public float MaxValue;

    public float GoBackMinValue;

    private Vector3 SavePos;

    // İsimlerini değiştir 
    float mouseX;
    float test;

    bool onetime = true;
    bool goBack;
    bool forward;

    float ArrowPower;

    private void Start()
    {
        numPoints = 51;
        positions = new Vector3[52];
        lineRenderer.positionCount = numPoints + 1;

        Arrow.GetComponent<Rigidbody>().isKinematic = true;
        DrawQuadraticCurve();
        SetArrowPos();
    }

    private void Update()
    {
        Bow();
    }

    private void LateUpdate()
    {
        if ((SavePos - center.position).z > MinValue && (SavePos - center.position).z < MaxValue && goBack == false && forward == false)
        {
            point1.position = SavePos;
            DrawQuadraticCurve();
        }
        else
        {
            if ((SavePos - center.position).z > GoBackMinValue && goBack == true)
            {
                point1.position = SavePos;
                DrawQuadraticCurve();
            }
            else if(goBack == true)
            {
                goBack = false;
                forward = true;
            }

            if ((SavePos - center.position).z < MinValue && forward == true)
            {
                point1.position = SavePos;
                DrawQuadraticCurve();
            }
            else
            {
                forward = false;
            }

        }
    }

    private void SetArrowPos()
    {
        SavePosPoint(mouseX);
        ArrowParent.transform.position = MiddlePoint;
    }


    private void SavePosPoint(float x)
    {
        SavePos = Vector3.Lerp(point1.transform.position, point1.transform.position + new Vector3(0, 0, x), 1 * Time.deltaTime);
    }

    private void Bow()
    {
        if (Input.GetMouseButton(0))
        {
            goBack = false;
            forward = false;

            mouseX = Input.GetAxis("Mouse X");

            if (mouseX != 0)
            {
                SetArrowPos();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Arrow.GetComponent<Rigidbody>().isKinematic = false;
            Arrow.transform.parent = null;
            ArrowPower = ((MiddlePoint.z * 2) - center.position.z) * 1000;
            Arrow.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, 1) * ArrowPower);
            goBack = true;
            forward = false;
        }

        if (goBack)
            SavePosPoint(-5);
        else if (forward)
            SavePosPoint(2);


    }

    private Vector3 mDirect;
    private float mDistance;
    private void DrawQuadraticCurve()
    {
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions[i] = CalculateQuadraticBezierPoint(t, point0.position, point1.position, point2.position);
        }
        positions[positions.Length - 1] = point2.transform.position;

        MiddlePoint = positions[positions.Length / 2];

        lineRenderer.SetPositions(positions);
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        //return   (1-t)2 P0 + 2(1-t)  t   P1 + t2   P2
        //           u           u              tt
        //           uu * P0 + 2 * u * t * P1 + tt * P2
        float u = (1 - t);
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }
}
