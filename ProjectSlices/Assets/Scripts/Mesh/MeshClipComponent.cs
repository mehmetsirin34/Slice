using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshClipComponent : MonoBehaviour
{
    public Transform PlanePoint0, PlanePoint1, PlanePoint2;
    private Plane clipPlane = new Plane();


    public TargetComponent Target;
    public Material SlicedMaterial;

    GameObject Blade;

    private void Awake()
    {
        Blade = new GameObject("Blade", typeof(MeshFilter), typeof(MeshRenderer));
        clipPlane = new Plane(PlanePoint0.transform.position, PlanePoint1.transform.position, PlanePoint2.transform.position);
    }

    public void Clip(GameObject targetGameObject, Vector3 anchorPoint, Vector3 normalDirection)
    {
        WMesh slicedMesh, remainedMesh;

        if (ClipByPlane(clipPlane, Target.GetWMesh(), out slicedMesh, out remainedMesh))
        {
            CreateNewObject("Sliced", slicedMesh.ToUnityMesh("Sliced", Target), Target.GetPosition(), Target.SlicedMaterial);

            CreateNewObject("Remained", remainedMesh.ToUnityMesh("Remained", Target), Target.GetPosition(), Target.SlicedMaterial);

            Target.Hide();
        }
    }

    private void CreateNewObject(string name, Mesh newMesh, Vector3 position, Material slicedMaterial)
    {
        GameObject newObject = Instantiate(Target.gameObject, position, Target.GetRotation());
        newObject.name = name;

        GameObject newObjectParent = new GameObject();
        newObjectParent.transform.position = position;
        newObjectParent.transform.rotation = Target.GetRotation();

        newObjectParent.name = name;

        newObject.transform.parent = newObjectParent.transform;

        if (newObjectParent.GetComponent<FruitMovement>() == false)
            newObjectParent.AddComponent<FruitMovement>();

        MeshFilter meshFilter = newObject.GetComponent<MeshFilter>();
        meshFilter.mesh.Clear();
        meshFilter.mesh = newMesh;

        MeshCollider meshCollider = newObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = newMesh;

        MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();
        meshRenderer.materials = new Material[] { meshRenderer.sharedMaterial, slicedMaterial };

        int value = 1;
        if (newObject.GetComponent<Rigidbody>() == false)
        {
            Rigidbody rb = newObject.AddComponent<Rigidbody>();

            if (name == "Sliced")
                value = -1;
            else if (name == "Remained")
                value = 1;


            rb.AddForce(new Vector3(0, 1, 0) * 100);
            rb.AddTorque(new Vector3(0, 0, value) * 100);
        }
    }

    private bool ClipByPlane(Plane plane, WMesh originMesh, out WMesh slicedMesh, out WMesh remainedMesh)
    {
        slicedMesh = null;
        remainedMesh = null;

        List<WTriangle> bodyTris = originMesh.GetBodyTris();

        List<WTriangle> slicedTris = new List<WTriangle>();
        List<WTriangle> remainedTris = new List<WTriangle>();

        List<WClipEdge> clipedEdges = new List<WClipEdge>();

        //Plane clipPlane = new Plane(plane.normal, Vector3.zero);
        Plane clipPlane = plane;

        for (int i = 0; i < bodyTris.Count; i++)
        {
            WTriangle tri = bodyTris[i];

            Vector3 worldPos = Target.GetPosition();
            // Using modified Sutherland-Hodgman
            Vector3 a = tri.a + worldPos;
            Vector3 b = tri.b + worldPos;
            Vector3 c = tri.c + worldPos;

            float da = clipPlane.GetDistanceToPoint(a);
            float db = clipPlane.GetDistanceToPoint(b);
            float dc = clipPlane.GetDistanceToPoint(c);


            if (WMath.InFront(da) && WMath.InFront(db) && WMath.InFront(dc))
            {
                slicedTris.Add(tri);
            }
            else if (WMath.Behind(da) && WMath.Behind(db) && WMath.Behind(dc))
            {
                remainedTris.Add(tri);
            }
            else if (WMath.InFront(da) && WMath.InFront(db) && WMath.Behind(dc))
            {
                Vector3 ac = WMath.Intersect(a, c, da, dc);
                Vector3 bc = WMath.Intersect(b, c, db, dc);

                slicedTris.Add(new WTriangle(ac, a, b, worldPos));
                slicedTris.Add(new WTriangle(b, bc, ac, worldPos));
                remainedTris.Add(new WTriangle(ac, bc, c, worldPos));

                clipedEdges.Add(new WClipEdge(ac, bc, worldPos));
            }
            else if (WMath.Behind(da) && WMath.Behind(db) && WMath.InFront(dc))
            {
                Vector3 ac = WMath.Intersect(a, c, da, dc);
                Vector3 bc = WMath.Intersect(b, c, db, dc);

                remainedTris.Add(new WTriangle(ac, a, b, worldPos));
                remainedTris.Add(new WTriangle(b, bc, ac, worldPos));
                slicedTris.Add(new WTriangle(ac, bc, c, worldPos));

                clipedEdges.Add(new WClipEdge(bc, ac, worldPos));
            }
            else if (WMath.InFront(da) && WMath.Behind(db) && WMath.InFront(dc))
            {
                Vector3 ab = WMath.Intersect(a, b, da, db);
                Vector3 bc = WMath.Intersect(b, c, db, dc);

                slicedTris.Add(new WTriangle(a, ab, c, worldPos));
                slicedTris.Add(new WTriangle(ab, bc, c, worldPos));
                remainedTris.Add(new WTriangle(ab, b, bc, worldPos));

                clipedEdges.Add(new WClipEdge(bc, ab, worldPos));
            }
            else if (WMath.Behind(da) && WMath.InFront(db) && WMath.Behind(dc))
            {
                Vector3 ab = WMath.Intersect(a, b, da, db);
                Vector3 bc = WMath.Intersect(b, c, db, dc);

                remainedTris.Add(new WTriangle(a, ab, c, worldPos));
                remainedTris.Add(new WTriangle(ab, bc, c, worldPos));
                slicedTris.Add(new WTriangle(ab, b, bc, worldPos));

                clipedEdges.Add(new WClipEdge(ab, bc, worldPos));
            }
            else if (WMath.Behind(da) && WMath.InFront(db) && WMath.InFront(dc))
            {
                Vector3 ab = WMath.Intersect(a, b, da, db);
                Vector3 ac = WMath.Intersect(a, c, da, dc);

                slicedTris.Add(new WTriangle(ab, b, c, worldPos));
                slicedTris.Add(new WTriangle(c, ac, ab, worldPos));
                remainedTris.Add(new WTriangle(ac, a, ab, worldPos));

                clipedEdges.Add(new WClipEdge(ab, ac, worldPos));
            }
            else if (WMath.InFront(da) && WMath.Behind(db) && WMath.Behind(dc))
            {
                Vector3 ab = WMath.Intersect(a, b, da, db);
                Vector3 ac = WMath.Intersect(a, c, da, dc);

                remainedTris.Add(new WTriangle(ab, b, c, worldPos));
                remainedTris.Add(new WTriangle(c, ac, ab, worldPos));
                slicedTris.Add(new WTriangle(ac, a, ab, worldPos));

                clipedEdges.Add(new WClipEdge(ac, ab, worldPos));
            }
        }

        MakeCap(clipedEdges, out List<WTriangle> slicedCap, out List<WTriangle> remainedCap);

        if (slicedTris.Count > 0 && remainedTris.Count > 0)
        {
            slicedMesh = new WMesh(slicedTris, slicedCap);
            remainedMesh = new WMesh(remainedTris, remainedCap);
            return true;
        }
        else return false;
    }

    private void MakeCap(List<WClipEdge> edges, out List<WTriangle> slicedCap, out List<WTriangle> remainedCap)
    {
        Vector3 capCenter = Vector3.zero;

        for (int i = 0; i < edges.Count; i++)
        {
            capCenter += edges[i].end;
        }

        capCenter /= edges.Count;

        slicedCap = new List<WTriangle>();
        remainedCap = new List<WTriangle>();

        for (int i = 0; i < edges.Count; i++)
        {
            slicedCap.Add(new WTriangle(capCenter, edges[i].start, edges[i].end));
            remainedCap.Add(new WTriangle(capCenter, edges[i].end, edges[i].start));
        }
    }
}
