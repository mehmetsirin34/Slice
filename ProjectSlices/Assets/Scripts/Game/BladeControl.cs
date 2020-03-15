using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class BladeControl : MonoBehaviour
{
    public GetScript Get;

    public GameObject Blade;
    private Rigidbody mBladeRB;
    public Transform mBladeTR;

    const float MaxLimitFirstAngelZ = 30;
    const float MaxLimitSecondAngelZ = -30;
    const float MinLimitSecondAngelZ = 85;

    private bool mForwardForce;
    private bool mBackForce;

    private bool NotChangingDirectionOnce;

    public bool ComingBack;

    public bool ClickControl;    float BladeAngle;

    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        mBladeRB = Blade.GetComponent<Rigidbody>();
        mBladeTR = Blade.transform;

        NotChangingDirectionOnce = true;

        mForwardForce = false;
        mBackForce = true;

        mBladeRB.maxAngularVelocity = 10000;
    }

    private void Update()
    {
        BladeInputControl();
    }

    private float GetAngel()
    {
        BladeAngle = mBladeTR.eulerAngles.z;
        BladeAngle = (BladeAngle > 180) ? BladeAngle - 360 : BladeAngle;
        BladeAngle = (int)BladeAngle;
        return BladeAngle;
    }

    private void BladeInputControl()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Stationary)
                ClickControl = true;
        }
        else if (Input.GetMouseButton(0))
            ClickControl = true;

        if (GetAngel() >= 90)
        {
            mBackForce = true;
            ComingBack = true;
        }

        if (ClickControl && mBackForce == false)
        {
            mForwardForce = true;
            ComingBack = false;
        }

        if (mBackForce)
        {
            Quaternion test = Quaternion.Euler(mBladeTR.eulerAngles.x, mBladeTR.eulerAngles.y, -30);
            mBladeTR.rotation = Quaternion.RotateTowards(mBladeTR.rotation, test, Get.GameManager.BladeSpeedMovement * Time.deltaTime);

            if (GetAngel() <= 50)
            {
                if (ClickControl)
                {
                    mBackForce = false;
                }

                if (GetAngel() <= -28)
                {
                    mBackForce = false;
                    ClickControl = false;
                }
            }
        }
        if (mForwardForce)
        {
            Quaternion test = Quaternion.Euler(mBladeTR.eulerAngles.x, mBladeTR.eulerAngles.y, 90);
            mBladeTR.rotation = Quaternion.RotateTowards(mBladeTR.rotation, test, Get.GameManager.BladeSpeedMovement * Time.deltaTime);

            if (GetAngel() >= 90)
            {
                mForwardForce = false;
                ClickControl = false;
            }
        }
    }
}
