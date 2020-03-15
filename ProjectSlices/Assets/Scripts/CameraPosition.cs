using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField] private Transform Camera;

    [SerializeField] private Transform ModeBladeCameraPoint;
    [SerializeField] private Transform ModeArrowCameraPoint;

    private Vector3 BladePoint;
    private Vector3 ArrowPoint;

    [SerializeField] private GetScript GetScript;

    private void Start()
    {
        BladePoint = ModeBladeCameraPoint.transform.position;
        ArrowPoint = ModeArrowCameraPoint.transform.position;
    }

    public void CameraPosChange()
    {
        Vector3 camPos = Camera.transform.position;

        if (GetScript.GameManager.GameModeBlade)
        {
            camPos = Vector3.Lerp(camPos, BladePoint, 1 * Time.deltaTime);
        }
        else if (GetScript.GameManager.GameModeArrow)
        {
            camPos = Vector3.Lerp(camPos, ArrowPoint, 1 * Time.deltaTime);
        }

        Camera.transform.position = camPos;
    }

    public bool GetControl()
    {
        Vector3 camPos = Camera.transform.position;

        if (camPos == BladePoint && GetScript.GameManager.GameModeBlade || camPos == ArrowPoint && GetScript.GameManager.GameModeArrow)
            return true;
        else
            return false;
    }
}
