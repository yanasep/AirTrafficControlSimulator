using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace MeshGeneration
{
    /*************************************************************************************************/
    /// <summary>
    /// 線に沿ったメッシュを生成する
    /// BezierPathコンポーネントがついたオブジェクトにアタッチすると、自動で生成される
    /// </summary>
    /*************************************************************************************************/
    [ExecuteInEditMode]
    public class RoadMeshGenerator : MonoBehaviour
    {
        public Material material;
        public float width = 3;
        public float height = 1;
        public float minPositionStep = 0.5f; // 近すぎる点を消す
        public bool hasFloor = true;

        [Header("Edge")] public bool hasEdge = true;
        public Material edgeMaterial;
        public float edgeWidth = 0.3f;
        public float edgeHeight = 0.2f;

        public enum NormalDirection
        {
            Upward,
            Smooth
        }

        [Header("表面方向")] public NormalDirection normalDir;

        [Header("nullにすると新規生成")] public GameObject meshObject;
        [SerializeField] bool validate;

        private MeshFilter filter;
        private MeshRenderer rend;
        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris0 = new List<int>();
        private List<int> tris1 = new List<int>();
        private readonly Material[] materials = new Material[2];
        private List<Vector2> uvs = new List<Vector2>();

        private BezierPath path;

        private enum Part
        {
            Floor,
            RightEdge,
            LeftEdge
        }

        private void OnValidate()
        {
            if (validate)
            {
                OnPathChanged();
            }
        }

        public void SetComponents()
        {
            if (path == null)
            {
                path = GetComponent<BezierPath>();
                if (path != null)
                {
                    path.OnChanged -= OnPathChanged; // 二度入れ防止
                    path.OnChanged += OnPathChanged;
                }
            }

            if (meshObject == null)
            {
                meshObject = new GameObject("Road Mesh");
            }

            filter = GetOrAddComponent<MeshFilter>(meshObject);
            rend = GetOrAddComponent<MeshRenderer>(meshObject);
            materials[0] = material;
            materials[1] = edgeMaterial;
            rend.materials = materials;
        }

        /// <summary>
        /// 線に沿ったメッシュを生成する
        /// </summary>
        /// <param name="positions">線上の点のリスト</param>
        /// <param name="right">横幅方向</param>
        public void Generate(List<Vector3> positions, List<Vector3> tangents)
        {
            SetComponents();

            // ---------------- メッシュ計算 -------------------------
            verts.Clear();
            tris0.Clear();
            tris1.Clear();
            uvs.Clear();
            RemoveSmallSteps(ref positions, ref tangents);
            // 床
            if (hasFloor)
                AddLineRectangle(ref verts, ref tris0, ref uvs, positions, width, height, tangents, Part.Floor);
            // エッジ
            if (hasEdge)
            {
                AddLineRectangle(ref verts, ref tris1, ref uvs, positions, edgeWidth, height + edgeHeight, tangents,
                    Part.LeftEdge);
                AddLineRectangle(ref verts, ref tris1, ref uvs, positions, edgeWidth, height + edgeHeight, tangents,
                    Part.RightEdge);
            }

            // -------------- メッシュセット -------------------
            var mesh = new Mesh();
            mesh.subMeshCount = 2;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris0, 0);
            mesh.SetTriangles(tris1, 1);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            filter.mesh = mesh;

            // --------------- gizmo ------------------
            //GizmosDrawer.Clear();
            //foreach (var vert in verts)
            //{
            //    GizmosDrawer.DrawSphere(vert, 0.05f, Color.red, 100);
            //}
        }

        private T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var c))
            {
                return c;
            }
            else
            {
                return go.AddComponent<T>();
            }
        }

        private void AddLineRectangle(ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs,
            in List<Vector3> positions,
            float width, float height, in List<Vector3> tangents, Part part)
        {
            if (positions.Count <= 0) return;

            Vector3 heightDir = Vector3.down;
            float distance = 0; // uv用、最初の点から進んだ距離
            for (int i = 0; i < positions.Count; i++)
            {
                var center = positions[i];

                // 厚み方向、横幅方向を求める
                var prevHeightDir = heightDir;
                var usedNormal = normalDir == NormalDirection.Upward ? Vector3.down : prevHeightDir;
                heightDir = Vector3.ProjectOnPlane(usedNormal, tangents[i]).normalized;
                Vector3 right = Vector3.Cross(heightDir, tangents[i]);
                float heightMgn = height;
                float widthMgn = width;
                float thisWidth = this.width;

                // 厚み方向が同じ側になるようにする
                if (i > 0)
                {
                    Plane plane = new Plane(center, positions[i - 1], center + right);
                    if (!plane.SameSide(center + heightDir, positions[i - 1] + prevHeightDir))
                    {
                        heightDir *= -1;
                        right *= -1;
                    }

                    // 高さ調整
                    heightMgn = height / Mathf.Sin(Vector3.Angle(center - positions[i - 1], heightDir) * Mathf.Deg2Rad);
                    widthMgn = width / Mathf.Sin(Vector3.Angle(center - positions[i - 1], right) * Mathf.Deg2Rad);
                    thisWidth = thisWidth / Mathf.Sin(Vector3.Angle(center - positions[i - 1], right) * Mathf.Deg2Rad);
                }

                int rectStartIndex = vertices.Count;

                // Mainとエッジで点をずらす分け
                Vector3 offset = Vector3.zero;
                switch (part)
                {
                    case Part.LeftEdge:
                        offset = right * (thisWidth * 0.5f + edgeWidth * 0.5f) - heightDir * (edgeHeight * 0.5f);
                        break;
                    case Part.RightEdge:
                        offset = right * -(thisWidth * 0.5f + edgeWidth * 0.5f) - heightDir * (edgeHeight * 0.5f);
                        break;
                }

                vertices.Add(center + 0.5f * widthMgn * right + offset);
                vertices.Add(center + -0.5f * widthMgn * right + offset);
                vertices.Add(center + -0.5f * widthMgn * right + heightDir * heightMgn + offset);
                vertices.Add(center + 0.5f * widthMgn * right + heightDir * heightMgn + offset);
                if (part == Part.LeftEdge)
                {
                    vertices[vertices.Count - 3] += widthMgn * 0.8f * right;
                }
                else if (part == Part.RightEdge)
                {
                    vertices[vertices.Count - 4] += -widthMgn * 0.8f * right;
                }

                if (i > 0) distance += Vector3.Distance(positions[i], positions[i - 1]);
                // widthがuvの長さ1分
                float uvY = distance / width;
                uvs.Add(new Vector2(0, uvY));
                uvs.Add(new Vector2(1, uvY));
                uvs.Add(new Vector2(1, uvY));
                uvs.Add(new Vector2(0, uvY));

                // triangleセット
                if (i == 0) // 最初の面
                {
                    triangles.AddRange(new int[]
                    {
                        rectStartIndex, rectStartIndex + 1, rectStartIndex + 2,
                        rectStartIndex, rectStartIndex + 2, rectStartIndex + 3
                    });
                }
                else
                {
                    for (int j = rectStartIndex; j < rectStartIndex + 4; j++)
                    {
                        if (j < rectStartIndex + 3)
                        {
                            triangles.AddRange(new int[]
                            {
                                j - 4, j, j + 1,
                                j - 4, j + 1, j - 3
                            });
                        }
                        else
                        {
                            triangles.AddRange(new int[]
                            {
                                j - 4, j, j - 3,
                                j - 4, j - 3, j - 7
                            });
                        }
                    }
                }

                if (i == positions.Count - 1) // 最後の面
                {
                    triangles.AddRange(new int[]
                    {
                        rectStartIndex + 1, rectStartIndex, rectStartIndex + 2,
                        rectStartIndex + 2, rectStartIndex, rectStartIndex + 3
                    });
                }
            }
        }

        // 近すぎる点を削除。 頂点数を減らし、綺麗に
        private void RemoveSmallSteps(ref List<Vector3> positions, ref List<Vector3> tangents)
        {
            for (int i = 1; i < positions.Count; i++)
            {
                if ((positions[i] - positions[i - 1]).magnitude < minPositionStep)
                {
                    if (i < positions.Count - 1)
                    {
                        positions.RemoveAt(i);
                        tangents.RemoveAt(i);
                        i--;
                    }
                    else // 最後
                    {
                        positions.RemoveAt(i - 1);
                        tangents.RemoveAt(i - 1);
                    }
                }
            }
        }

        private void OnEnable()
        {
            path = GetComponent<BezierPath>();
            if (path != null)
            {
                path.OnChanged -= OnPathChanged; // 二度入れ防止
                path.OnChanged += OnPathChanged;
                OnPathChanged();
            }
        }

        public void OnPathChanged()
        {
            // メッシュ更新
            Generate(path.GetPoints().ToList(), path.GetTangents().ToList());
            if (meshObject.TryGetComponent<MeshCollider>(out var col))
            {
                col.sharedMesh = filter.sharedMesh;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RoadMeshGenerator))]
    public class RoadMeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            if (GUILayout.Button("Manual Update"))
            {
                var gen = target as RoadMeshGenerator;
                gen.OnPathChanged();
            }
        }
    }
#endif
}