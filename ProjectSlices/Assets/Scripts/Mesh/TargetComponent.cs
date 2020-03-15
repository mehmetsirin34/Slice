using UnityEngine;
using System.Collections;

public class TargetComponent : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;

    WMesh wMesh;

    public Material SlicedMaterial;

    private void Update()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public WMesh GetWMesh()
    {
        wMesh = new WMesh(meshFilter.mesh);
        return wMesh;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }

    public void Hide()
    {
        string name = gameObject.name;

        if (name != "Remained" && name != "Sliced")
        {
            ObjectPooler.Instance.ReturnToPool(transform.parent.gameObject);
        }
        else
            Destroy(gameObject);
    }
}
