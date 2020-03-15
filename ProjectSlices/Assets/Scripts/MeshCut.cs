using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace Blade
{
    public class MeshCut
    {
        private static Plane _blade;
        private static Mesh _victim_mesh;

        private static Mesh_Maker _leftSide = new Mesh_Maker();
        private static Mesh_Maker _rightSide = new Mesh_Maker();
        private static Mesh_Maker.Triangle _triangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static List<Vector3> _newVerticesCache = new List<Vector3>();
        private static bool[] _isLeftSideCache = new bool[3];
        private static int _capMatSub = 1;


        private static int test = 0;

        /// <summary>
        /// Yeah
        /// </summary>
        /// <param name="victim">Dışarıdan alınır</param>
        /// <param name="anchorPoint">Bıçağın dünyadaki pozisyonu</param>
        /// <param name="normalDirection">bıçağın normal yönü</param>
        /// <param name="capMaterial">Kesildikten sonra açık kalan yerin doldurmak için gereken materyal.</param>
        /// <returns></returns>
        public static GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial,TextMeshProUGUI a, TextMeshProUGUI aa, TextMeshProUGUI aaa)
        {
            try
            {
                // Bıçağın kesildiği yere göre bir plane oluşturur
                _blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
                    victim.transform.InverseTransformPoint(anchorPoint));

                // Dışarıdan verilen objenin meshi alınır
                _victim_mesh = victim.GetComponent<MeshFilter>().mesh;

                // 2 yeni mesh
                _leftSide.Clear();
                _rightSide.Clear();
                _newVerticesCache.Clear();


                int index_1, index_2, index_3;

                var mesh_vertices = _victim_mesh.vertices;
                var mesh_normals = _victim_mesh.normals;
                var mesh_uvs = _victim_mesh.uv;
                var mesh_tangents = _victim_mesh.tangents;
                if (mesh_tangents != null && mesh_tangents.Length == 0)
                    mesh_tangents = null;

                for (int submeshIterator = 0; submeshIterator < _victim_mesh.subMeshCount; submeshIterator++)
                {

                    // Triangles
                    var indices = _victim_mesh.GetTriangles(submeshIterator);

                    for (int i = 0; i < indices.Length; i += 3)
                    {

                        index_1 = indices[i];
                        index_2 = indices[i + 1];
                        index_3 = indices[i + 2];

                        // Verts
                        _triangleCache.vertices[0] = mesh_vertices[index_1];
                        _triangleCache.vertices[1] = mesh_vertices[index_2];
                        _triangleCache.vertices[2] = mesh_vertices[index_3];

                        // Normals
                        _triangleCache.normals[0] = mesh_normals[index_1];
                        _triangleCache.normals[1] = mesh_normals[index_2];
                        _triangleCache.normals[2] = mesh_normals[index_3];

                        // Uvs
                        _triangleCache.uvs[0] = mesh_uvs[index_1];
                        _triangleCache.uvs[1] = mesh_uvs[index_2];
                        _triangleCache.uvs[2] = mesh_uvs[index_3];

                        // Tangents
                        if (mesh_tangents != null)
                        {
                            _triangleCache.tangents[0] = mesh_tangents[index_1];
                            _triangleCache.tangents[1] = mesh_tangents[index_2];
                            _triangleCache.tangents[2] = mesh_tangents[index_3];
                        }
                        else
                        {
                            _triangleCache.tangents[0] = Vector4.zero;
                            _triangleCache.tangents[1] = Vector4.zero;
                            _triangleCache.tangents[2] = Vector4.zero;
                        }

                        _isLeftSideCache[0] = _blade.GetSide(mesh_vertices[index_1]);
                        _isLeftSideCache[1] = _blade.GetSide(mesh_vertices[index_2]);
                        _isLeftSideCache[2] = _blade.GetSide(mesh_vertices[index_3]);


                        // Üçgen
                        if (_isLeftSideCache[0] == _isLeftSideCache[1] && _isLeftSideCache[0] == _isLeftSideCache[2])
                        {

                            if (_isLeftSideCache[0]) // sol yüz
                                _leftSide.AddTriangle(_triangleCache, submeshIterator);
                            else // sağ yüz
                                _rightSide.AddTriangle(_triangleCache, submeshIterator);

                        }
                        else
                        { // Üçgeni keser

                            Cut_this_Face(ref _triangleCache, submeshIterator);
                        }
                    }
                }

                //  Doldurulan yere materyal veriliyor.
                Material[] mats = victim.GetComponent<MeshRenderer>().sharedMaterials;
                if (capMaterial != null)
                {
                    if (mats[mats.Length - 1].name != capMaterial.name)
                    {
                        Material[] newMats = new Material[mats.Length + 1];
                        mats.CopyTo(newMats, 0);
                        newMats[mats.Length] = capMaterial;
                        mats = newMats;
                    }
                    _capMatSub = mats.Length - 1;
                }

                // Açık olan yerleri kapatan method
                Cap_the_Cut();


                // Sol Mesh
                Mesh left_HalfMesh = _leftSide.GetMesh();
                left_HalfMesh.name = "Split Mesh Left";

                // Sağ Mesh
                Mesh right_HalfMesh = _rightSide.GetMesh();
                right_HalfMesh.name = "Split Mesh Right";

                // Dışardan girdiğimiz obje ile eşitleme

                victim.name = "left side";
                victim.GetComponent<MeshFilter>().mesh = left_HalfMesh;

                GameObject leftSideObj = victim;

                GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
                rightSideObj.transform.position = victim.transform.position;
                rightSideObj.transform.rotation = victim.transform.rotation;
                rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;

                // Burayı Unutma
                if (rightSideObj.transform.parent == null)
                {
                    GameObject clone = new GameObject();
                    clone.name = "Slice";
                    clone.transform.position = rightSideObj.transform.position;
                    rightSideObj.transform.parent = clone.transform;

                    clone.AddComponent<FruitMovement>();
                }


                rightSideObj.transform.localScale = victim.transform.localScale;


                // Materyaller eşitlenir
                leftSideObj.GetComponent<MeshRenderer>().materials = mats;
                rightSideObj.GetComponent<MeshRenderer>().materials = mats;

                // hata mesajı için 
                //if (1 / test == 2)
                //{

                //}

                return new GameObject[] { leftSideObj, rightSideObj };
            }
            catch (System.Exception ex)
            {

                Debug.Log(ex);
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);




                a.text = ex.ToString();
                //aa.text = ex.Message;
                //aaa.text = ex.StackTrace;

                return null;

            }
        }

        #region Cutting

        private static Mesh_Maker.Triangle _leftTriangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static Mesh_Maker.Triangle _rightTriangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static Mesh_Maker.Triangle _newTriangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        // Method
        private static void Cut_this_Face(ref Mesh_Maker.Triangle triangle, int submesh)
        {

            _isLeftSideCache[0] = _blade.GetSide(triangle.vertices[0]); // true = left
            _isLeftSideCache[1] = _blade.GetSide(triangle.vertices[1]);
            _isLeftSideCache[2] = _blade.GetSide(triangle.vertices[2]);


            int leftCount = 0;
            int rightCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if (_isLeftSideCache[i])
                { // Sol

                    _leftTriangleCache.vertices[leftCount] = triangle.vertices[i];
                    _leftTriangleCache.uvs[leftCount] = triangle.uvs[i];
                    _leftTriangleCache.normals[leftCount] = triangle.normals[i];
                    _leftTriangleCache.tangents[leftCount] = triangle.tangents[i];

                    leftCount++;
                }
                else
                { // right

                    _rightTriangleCache.vertices[rightCount] = triangle.vertices[i];
                    _rightTriangleCache.uvs[rightCount] = triangle.uvs[i];
                    _rightTriangleCache.normals[rightCount] = triangle.normals[i];
                    _rightTriangleCache.tangents[rightCount] = triangle.tangents[i];

                    rightCount++;
                }
            }

            // Yeni Üçgenleri bulur x3
            // Vertices bulur


            //bir üçgen
            if (leftCount == 1)
            {
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[1] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[1] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _rightTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _rightTriangleCache.uvs[1];
                _triangleCache.normals[2] = _rightTriangleCache.normals[1];
                _triangleCache.tangents[2] = _rightTriangleCache.tangents[1];
            }
            else // rightCount == 1
            {
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[1] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[1] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _leftTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _leftTriangleCache.uvs[1];
                _triangleCache.normals[2] = _leftTriangleCache.normals[1];
                _triangleCache.tangents[2] = _leftTriangleCache.tangents[1];
            }

            // Keşisim noktalarını bulmaya geldik
            float distance = 0;
            float normalizedDistance = 0.0f;
            Vector3 edgeVector = Vector3.zero; // kenar uzunluğu ve yönü ölçer

            edgeVector = _triangleCache.vertices[1] - _triangleCache.vertices[0];
            _blade.Raycast(new Ray(_triangleCache.vertices[0], edgeVector.normalized), out distance);

            normalizedDistance = distance / edgeVector.magnitude;
            _newTriangleCache.vertices[0] = Vector3.Lerp(_triangleCache.vertices[0], _triangleCache.vertices[1], normalizedDistance);
            _newTriangleCache.uvs[0] = Vector2.Lerp(_triangleCache.uvs[0], _triangleCache.uvs[1], normalizedDistance);
            _newTriangleCache.normals[0] = Vector3.Lerp(_triangleCache.normals[0], _triangleCache.normals[1], normalizedDistance);
            _newTriangleCache.tangents[0] = Vector4.Lerp(_triangleCache.tangents[0], _triangleCache.tangents[1], normalizedDistance);

            edgeVector = _triangleCache.vertices[2] - _triangleCache.vertices[0];
            _blade.Raycast(new Ray(_triangleCache.vertices[0], edgeVector.normalized), out distance);

            normalizedDistance = distance / edgeVector.magnitude;
            _newTriangleCache.vertices[1] = Vector3.Lerp(_triangleCache.vertices[0], _triangleCache.vertices[2], normalizedDistance);
            _newTriangleCache.uvs[1] = Vector2.Lerp(_triangleCache.uvs[0], _triangleCache.uvs[2], normalizedDistance);
            _newTriangleCache.normals[1] = Vector3.Lerp(_triangleCache.normals[0], _triangleCache.normals[2], normalizedDistance);
            _newTriangleCache.tangents[1] = Vector4.Lerp(_triangleCache.tangents[0], _triangleCache.tangents[2], normalizedDistance);

            if (_newTriangleCache.vertices[0] != _newTriangleCache.vertices[1])
            {
                _newVerticesCache.Add(_newTriangleCache.vertices[0]);
                _newVerticesCache.Add(_newTriangleCache.vertices[1]);
            }
            // Yeni üçgenler oluşturur

            if (leftCount == 1)
            {
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // Doğru yöne bakıp bakmadığını kontrol eder.
                NormalCheck(ref _triangleCache);

                // Ekleme
                _leftSide.AddTriangle(_triangleCache, submesh);

                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // Doğru yöne bakıp bakmadığını kontrol eder.
                NormalCheck(ref _triangleCache);

                // Ekleme
                _rightSide.AddTriangle(_triangleCache, submesh);

                // Üçüncü
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _rightTriangleCache.vertices[1];
                _triangleCache.uvs[1] = _rightTriangleCache.uvs[1];
                _triangleCache.normals[1] = _rightTriangleCache.normals[1];
                _triangleCache.tangents[1] = _rightTriangleCache.tangents[1];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // Doğru yöne bakıp bakmadığını kontrol eder.
                NormalCheck(ref _triangleCache);

                // Ekleme
                _rightSide.AddTriangle(_triangleCache, submesh);
            }
            else
            {
                // Sağ İlk
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // Doğru yöne bakıp bakmadığını kontrol eder.
                NormalCheck(ref _triangleCache);

                // Ekleme
                _rightSide.AddTriangle(_triangleCache, submesh);

                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // Doğru yöne bakıp bakmadığını kontrol eder.
                NormalCheck(ref _triangleCache);

                // Ekleme
                _leftSide.AddTriangle(_triangleCache, submesh);

                // Üçüncü
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _leftTriangleCache.vertices[1];
                _triangleCache.uvs[1] = _leftTriangleCache.uvs[1];
                _triangleCache.normals[1] = _leftTriangleCache.normals[1];
                _triangleCache.tangents[1] = _leftTriangleCache.tangents[1];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // Doğru yöne bakıp bakmadığını kontrol eder.
                NormalCheck(ref _triangleCache);

                // Ekleme
                _leftSide.AddTriangle(_triangleCache, submesh);
            }

        }
        #endregion

        #region Capping

        private static List<int> _capUsedIndicesCache = new List<int>();
        private static List<int> _capPolygonIndicesCache = new List<int>();

        private static void Cap_the_Cut()
        {

            _capUsedIndicesCache.Clear();
            _capPolygonIndicesCache.Clear();

            // Gerekli olan polygonları bulur
            // kesilen yüzler, kenar oluşturmak için her seferinde 2 yeni köşe eklenir
            //iki kenar aynı Vector3 noktasını içeriyorsa kontrol
            for (int i = 0; i < _newVerticesCache.Count; i += 2)
            {
                // Kenarı kontrol et
                if (!_capUsedIndicesCache.Contains(i)) // eğer bir tane varsa, bu kenar vardır
                {
                    // bu kenarla yeni çokgen oluştu
                    _capPolygonIndicesCache.Clear();
                    _capPolygonIndicesCache.Add(i);
                    _capPolygonIndicesCache.Add(i + 1);

                    _capUsedIndicesCache.Add(i);
                    _capUsedIndicesCache.Add(i + 1);

                    Vector3 connectionPointLeft = _newVerticesCache[i];
                    Vector3 connectionPointRight = _newVerticesCache[i + 1];
                    bool isDone = false;

                    // Kenar arama
                    while (!isDone)
                    {
                        isDone = true;

                        // Kenar sayısınca for
                        for (int index = 0; index < _newVerticesCache.Count; index += 2)
                        {  // eğer bir tane varsa, bu kenar vardır
                            if (!_capUsedIndicesCache.Contains(index))
                            {
                                Vector3 nextPoint1 = _newVerticesCache[index];
                                Vector3 nextPoint2 = _newVerticesCache[index + 1];

                                // Bir sonraki noktayı kontrol et
                                if (connectionPointLeft == nextPoint1 ||
                                    connectionPointLeft == nextPoint2 ||
                                    connectionPointRight == nextPoint1 ||
                                    connectionPointRight == nextPoint2)
                                {
                                    _capUsedIndicesCache.Add(index);
                                    _capUsedIndicesCache.Add(index + 1);

                                    // Diğerini ekele
                                    if (connectionPointLeft == nextPoint1)
                                    {
                                        _capPolygonIndicesCache.Insert(0, index + 1);
                                        connectionPointLeft = _newVerticesCache[index + 1];
                                    }
                                    else if (connectionPointLeft == nextPoint2)
                                    {
                                        _capPolygonIndicesCache.Insert(0, index);
                                        connectionPointLeft = _newVerticesCache[index];
                                    }
                                    else if (connectionPointRight == nextPoint1)
                                    {
                                        _capPolygonIndicesCache.Add(index + 1);
                                        connectionPointRight = _newVerticesCache[index + 1];
                                    }
                                    else if (connectionPointRight == nextPoint2)
                                    {
                                        _capPolygonIndicesCache.Add(index);
                                        connectionPointRight = _newVerticesCache[index];
                                    }

                                    isDone = false;
                                }
                            }
                        }
                    }// while isDone = False

                    // bağlantının kapalı olup olmadığını kontrol et
                    // first == last
                    if (_newVerticesCache[_capPolygonIndicesCache[0]] == _newVerticesCache[_capPolygonIndicesCache[_capPolygonIndicesCache.Count - 1]])
                        _capPolygonIndicesCache[_capPolygonIndicesCache.Count - 1] = _capPolygonIndicesCache[0];
                    else
                        _capPolygonIndicesCache.Add(_capPolygonIndicesCache[0]);

                    //  kaplama methodu
                    FillCap_Method2(_capPolygonIndicesCache);
                }
            }
        }

        private static void FillCap_Method2(List<int> indices)
        {

            // kapağın merkezi bulunur
            Vector3 center = Vector3.zero;
            foreach (var index in indices)
                center += _newVerticesCache[index];

            center = center / indices.Count;

            // kapağa dayalı bir eksen
            Vector3 upward = Vector3.zero;
            // 90 derece dönüş
            upward.x = _blade.normal.y;
            upward.y = -_blade.normal.x;
            upward.z = _blade.normal.z;
            Vector3 left = Vector3.Cross(_blade.normal, upward);

            Vector3 displacement = Vector3.zero;
            Vector2 newUV1 = Vector2.zero;
            Vector2 newUV2 = Vector2.zero;

            for (int i = 0; i < indices.Count - 1; i++)
            {

                displacement = _newVerticesCache[indices[i]] - center;
                newUV1 = Vector3.zero;
                newUV1.x = 0.5f + Vector3.Dot(displacement, left);
                newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
                //newUV1.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

                displacement = _newVerticesCache[indices[i + 1]] - center;
                newUV2 = Vector3.zero;
                newUV2.x = 0.5f + Vector3.Dot(displacement, left);
                newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
                //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);



                _newTriangleCache.vertices[0] = _newVerticesCache[indices[i]];
                _newTriangleCache.uvs[0] = newUV1;
                _newTriangleCache.normals[0] = -_blade.normal;
                _newTriangleCache.tangents[0] = Vector4.zero;

                _newTriangleCache.vertices[1] = _newVerticesCache[indices[i + 1]];
                _newTriangleCache.uvs[1] = newUV2;
                _newTriangleCache.normals[1] = -_blade.normal;
                _newTriangleCache.tangents[1] = Vector4.zero;

                _newTriangleCache.vertices[2] = center;
                _newTriangleCache.uvs[2] = new Vector2(0.5f, 0.5f);
                _newTriangleCache.normals[2] = -_blade.normal;
                _newTriangleCache.tangents[2] = Vector4.zero;


                NormalCheck(ref _newTriangleCache);

                _leftSide.AddTriangle(_newTriangleCache, _capMatSub);

                _newTriangleCache.normals[0] = _blade.normal;
                _newTriangleCache.normals[1] = _blade.normal;
                _newTriangleCache.normals[2] = _blade.normal;

                NormalCheck(ref _newTriangleCache);

                _rightSide.AddTriangle(_newTriangleCache, _capMatSub);

            }

        }
        #endregion

        #region Misc.
        // Normalleri kontrol eder. Normaller ışığın doğru yansıması ve gölgelerin doğru oluşması için gereklidir.

        private static void NormalCheck(ref Mesh_Maker.Triangle triangle)
        {
            Vector3 crossProduct = Vector3.Cross(triangle.vertices[1] - triangle.vertices[0], triangle.vertices[2] - triangle.vertices[0]);
            Vector3 averageNormal = (triangle.normals[0] + triangle.normals[1] + triangle.normals[2]) / 3.0f;
            float dotProduct = Vector3.Dot(averageNormal, crossProduct);
            if (dotProduct < 0)
            {
                Vector3 temp = triangle.vertices[2];
                triangle.vertices[2] = triangle.vertices[0];
                triangle.vertices[0] = temp;

                temp = triangle.normals[2];
                triangle.normals[2] = triangle.normals[0];
                triangle.normals[0] = temp;

                Vector2 temp2 = triangle.uvs[2];
                triangle.uvs[2] = triangle.uvs[0];
                triangle.uvs[0] = temp2;

                Vector4 temp3 = triangle.tangents[2];
                triangle.tangents[2] = triangle.tangents[0];
                triangle.tangents[0] = temp3;
            }

        }
        #endregion
    }
}