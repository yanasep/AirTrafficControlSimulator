using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace MeshGeneration
{
    /*************************************************************************************************/
    /// <summary>
    /// ベジェ曲線を作るためのコンポーネント
    /// </summary>
    /*************************************************************************************************/
    public class BezierPath : MonoBehaviour
    {
        public float handleSize = 0.3f;
        [SerializeField] int divisions = 50;

        float step;
        public List<BezierHandle> handles = new List<BezierHandle>();

        public Action OnChanged;

        public enum HandleMode
        {
            Free,
            Axis
        }

        public HandleMode handleMode;

        // 線を構成する点を取得
        public IEnumerable<Vector3> GetPoints()
        {
            step = 1f / divisions;
            for (int i = 0; i < handles.Count - 1; i++)
            {
                BezierHandle left = handles[i], right = handles[i + 1];
                if (left.isCorner && right.isCorner) // 直線の場合、分割しない
                {
                    if (i > 0 && handles[i - 1].isCorner) // 前も直線の場合、右だけ追加する
                    {
                        yield return right.position;
                    }
                    else
                    {
                        yield return left.position;
                        yield return right.position;
                    }
                }
                else
                {
                    for (int j = 0; j < divisions; j++)
                    {
                        yield return CalcPoint(left, right, step * j);
                    }
                }
            }
        }

        // ハンドル間の点を計算する
        private Vector3 CalcPoint(BezierHandle handle0, BezierHandle handle1, float t)
        {
            if (!handle0.isCorner && !handle1.isCorner) // 両方制御点を持つ
            {
                return CubicBezier(handle0.position, handle0.control1, handle1.control0, handle1.position, t);
            }
            else if (!handle0.isCorner && handle1.isCorner) // 左だけ制御点を持つ
            {
                return CubicBezier(handle0.position, handle0.control1, handle0.control1, handle1.position, t);
            }
            else if (handle0.isCorner && !handle1.isCorner) // 右だけ制御点を持つ
            {
                return CubicBezier(handle0.position, handle1.control0, handle1.control0, handle1.position, t);
            }
            else // 両方制御点なし、直線
            {
                return Vector3.Lerp(handle0.position, handle1.position, t);
            }
        }

        // 3次ベジェ曲線
        private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }

        // 接線
        public IEnumerable<Vector3> GetTangents()
        {
            step = 1f / divisions;
            for (int i = 0; i < handles.Count - 1; i++)
            {
                BezierHandle left = handles[i], right = handles[i + 1];

                if (left.isCorner && right.isCorner) // 直線の場合、分割しない
                {
                    if (i > 0 && handles[i - 1].isCorner) // 前も直線の場合、右だけ追加する
                    {
                        if (i == handles.Count - 2)
                        {
                            yield return -(left.position - right.position).normalized;
                        }
                        else
                        {
                            var rightNext = CalcPoint(right, handles[i + 2], step);
                            yield return -((left.position - right.position).normalized -
                                           (rightNext - right.position).normalized).normalized;
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            yield return -(left.position - right.position).normalized;
                        }
                        else
                        {
                            var leftPrev = CalcPoint(handles[i - 1], left, step * (divisions - 1));
                            yield return -((leftPrev - left.position).normalized -
                                           (right.position - left.position).normalized).normalized;
                        }

                        if (i == handles.Count - 2)
                        {
                            yield return -(left.position - right.position).normalized;
                        }
                        else
                        {
                            var rightNext = CalcPoint(right, handles[i + 2], step);
                            yield return -((left.position - right.position).normalized -
                                           (rightNext - right.position).normalized).normalized;
                        }
                    }
                }
                else // ベジェ曲線の場合
                {
                    for (int j = 0; j < divisions; j++)
                    {
                        yield return CalcTangent(left, right, step * j);
                    }
                }
            }
        }

        private Vector3 CalcTangent(BezierHandle handle0, BezierHandle handle1, float t)
        {
            if (!handle0.isCorner && !handle1.isCorner) // 両方制御点を持つ
            {
                return ComputeTangent(handle0.position, handle0.control1, handle1.control0, handle1.position, t);
            }
            else if (!handle0.isCorner && handle1.isCorner) // 左だけ制御点を持つ
            {
                return ComputeTangent(handle0.position, handle0.control1, handle0.control1, handle1.position, t);
            }
            else if (handle0.isCorner && !handle1.isCorner) // 右だけ制御点を持つ
            {
                return ComputeTangent(handle0.position, handle1.control0, handle1.control0, handle1.position, t);
            }
            else // 両方制御点なし、直線
            {
                var dir = (handle1.position - handle0.position).normalized;
                return ComputeTangent(handle0.position, handle0.position + dir * 0.3f, handle0.position + dir * 0.7f,
                    handle1.position, t);
            }
        }

        private Vector3 ComputeTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return (-3f * Mathf.Pow(1f - t, 2f) * p0 + 3f * Mathf.Pow(1f - t, 2) * p1
                    - 6f * t * (1f - t) * p1 - 3f * Mathf.Pow(t, 2) * p2 + 6f * t * (1f - t) * p2 +
                    3f * Mathf.Pow(t, 2f) * p3).normalized;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BezierPath))]
    public class BezierPathEditor : Editor
    {
        private BezierPath bezier;
        private BezierHandle lastSelectedHandle = null;
        private int lastSelectedId;
        private bool idAllSet;

        private void OnEnable()
        {
            bezier = target as BezierPath;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add handle"))
            {
                Undo.RecordObject(bezier, "Add handle");
                // 1つ前のハンドルを取得
                int insertIndex = 0;
                if (lastSelectedHandle == null) insertIndex = bezier.handles.Count;
                else insertIndex = bezier.handles.FindIndex(h => h == lastSelectedHandle) + 1;
                var newHandle = new BezierHandle(bezier.transform.position + Vector3.down * 5f);
                if (bezier.handles.Count > 0)
                {
                    var prevHandle = bezier.handles[insertIndex - 1];
                    newHandle.isCorner = prevHandle.isCorner;

                    if (insertIndex < bezier.handles.Count) // 間に入れる場合は、ちょうど真ん中に
                    {
                        var nextHandle = bezier.handles[insertIndex];
                        newHandle.position = Vector3.Lerp(prevHandle.position, nextHandle.position, 0.5f);
                        newHandle.control0 = newHandle.position + prevHandle.control0 - prevHandle.position;
                        newHandle.control1 = newHandle.position + prevHandle.control1 - prevHandle.position;
                    }
                    else if (insertIndex >= 2) // 最後に入れる場合
                    {
                        var prev2Handle = bezier.handles[insertIndex - 2];
                        newHandle.position = (prevHandle.position - prev2Handle.position) * 0.3f + prevHandle.position;
                        newHandle.control0 = newHandle.position + (prevHandle.control1 - prevHandle.position);
                        newHandle.control1 = newHandle.position + (prevHandle.control0 - prevHandle.position);
                    }
                }

                bezier.handles.Insert(insertIndex, newHandle);
                lastSelectedHandle = newHandle;
                idAllSet = false;
                Undo.FlushUndoRecordObjects();
                HandleUtility.Repaint();
                EditorUtility.SetDirty(bezier);
                bezier.OnChanged?.Invoke();
            }

            if (GUILayout.Button($"Change Handle Mode")) // 直線、曲線の切り替え
            {
                if (lastSelectedHandle == null) return;

                Undo.RecordObject(bezier, "Change handle mode");
                lastSelectedHandle.isCorner = !lastSelectedHandle.isCorner;
                HandleUtility.Repaint();
                EditorUtility.SetDirty(bezier);
                Undo.FlushUndoRecordObjects();
                bezier.OnChanged?.Invoke();
            }

            if (GUILayout.Button("Remove handle"))
            {
                if (lastSelectedHandle != null)
                {
                    Undo.RecordObject(bezier, "Remove handle");
                    bezier.handles.Remove(lastSelectedHandle);
                    lastSelectedHandle = null;
                    HandleUtility.Repaint();
                    EditorUtility.SetDirty(bezier);
                    Undo.FlushUndoRecordObjects();
                    bezier.OnChanged?.Invoke();
                }
            }

            if (GUILayout.Button($"Clear"))
            {
                Undo.RecordObject(bezier, "Clear bezier");
                lastSelectedHandle = null;
                idAllSet = false;
                bezier.handles.Clear();
                EditorUtility.SetDirty(bezier);
                Undo.FlushUndoRecordObjects();
                SceneView.RepaintAll();
                bezier.OnChanged?.Invoke();
            }
        }

        public void OnSceneGUI()
        {
            var bezier = target as BezierPath;

            // control idのセット
            if (!idAllSet)
            {
                foreach (var handle in bezier.handles)
                {
                    handle.posId = GUIUtility.GetControlID(FocusType.Passive);
                    handle.c0Id = GUIUtility.GetControlID(FocusType.Passive);
                    handle.c1Id = GUIUtility.GetControlID(FocusType.Passive);
                    if (handle == lastSelectedHandle) lastSelectedId = handle.posId;
                }

                idAllSet = true;
            }

            // ハンドル描画
            foreach (var handle in bezier.handles)
            {
                // MouseDownでハンドル選択
                if (Event.current.type == EventType.MouseDown)
                {
                    int id = HandleUtility.nearestControl;
                    if (id == handle.posId || id == handle.c0Id || id == handle.c1Id)
                    {
                        lastSelectedHandle = handle;
                        lastSelectedId = id;
                    }
                }

                EditorGUI.BeginChangeCheck();

                Handles.color = handle == lastSelectedHandle ? Color.yellow : Color.blue;

                //bool drawAxis = bezier.handleMode == BezierPath.HandleMode.Axis && handle != lastSelectedHandle;

                // -------------- ハンドルの中心 -------------------
                if (bezier.handleMode == BezierPath.HandleMode.Axis && lastSelectedId != handle.posId)
                {
                    var fmh_315_75_638274549437548407 = Quaternion.identity; Handles.FreeMoveHandle(handle.posId, handle.position, bezier.handleSize,
                        Vector3.one * 0.5f, Handles.SphereHandleCap);
                }
                else
                {
                    Vector3 newPos;
                    switch (bezier.handleMode)
                    {
                        case BezierPath.HandleMode.Free:
                            var fmh_324_92_638274549438733173 = Quaternion.identity; newPos = Handles.FreeMoveHandle(handle.posId, handle.position,
                                bezier.handleSize, Vector3.one * 0.5f, Handles.SphereHandleCap);
                            break;
                        case BezierPath.HandleMode.Axis:
                            newPos = Handles.PositionHandle(handle.position, Quaternion.identity);
                            Handles.SphereHandleCap(handle.posId, handle.position, Quaternion.identity,
                                bezier.handleSize, EventType.Repaint);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (handle.position != newPos)
                        {
                            Undo.RecordObject(bezier, "Move handle");
                            handle.SetHandlePositoin(newPos);
                            EditorUtility.SetDirty(bezier);
                            Undo.FlushUndoRecordObjects();
                            bezier.OnChanged?.Invoke();
                        }
                    }
                }

                // --------------- 制御点 -------------------------
                if (!handle.isCorner)
                {
                    Handles.color = Color.red;

                    // 制御点0 -----
                    if (bezier.handleMode == BezierPath.HandleMode.Axis && lastSelectedId != handle.c0Id)
                    {
                        var fmh_357_78_638274549438740664 = Quaternion.identity; Handles.FreeMoveHandle(handle.c0Id, handle.control0, bezier.handleSize,
                            Vector3.one * 0.5f, Handles.SphereHandleCap);
                    }
                    else
                    {
                        Vector3 control0Pos;
                        switch (bezier.handleMode)
                        {
                            case BezierPath.HandleMode.Free:
                                var fmh_366_100_638274549438746052 = Quaternion.identity; control0Pos = Handles.FreeMoveHandle(handle.c0Id, handle.control0,
                                    bezier.handleSize, Vector3.one * 0.5f, Handles.SphereHandleCap);
                                break;
                            case BezierPath.HandleMode.Axis:
                                control0Pos = Handles.PositionHandle(handle.control0, Quaternion.identity);
                                Handles.SphereHandleCap(handle.c0Id, handle.control0, Quaternion.identity,
                                    bezier.handleSize, EventType.Repaint);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (handle.control0 != control0Pos)
                            {
                                Undo.RecordObject(bezier, "Move control point");
                                handle.control0 = control0Pos;
                                handle.control1 = (handle.position - handle.control0) + handle.position;
                                EditorUtility.SetDirty(bezier);
                                Undo.FlushUndoRecordObjects();
                                bezier.OnChanged?.Invoke();
                            }
                        }
                    }

                    // 制御点1 -----
                    if (bezier.handleMode == BezierPath.HandleMode.Axis && lastSelectedId != handle.c1Id)
                    {
                        var fmh_395_78_638274549438750590 = Quaternion.identity; Handles.FreeMoveHandle(handle.c1Id, handle.control1, bezier.handleSize,
                            Vector3.one * 0.5f, Handles.SphereHandleCap);
                    }
                    else
                    {
                        Vector3 control1Pos;
                        switch (bezier.handleMode)
                        {
                            case BezierPath.HandleMode.Free:
                                var fmh_404_100_638274549438757788 = Quaternion.identity; control1Pos = Handles.FreeMoveHandle(handle.c1Id, handle.control1,
                                    bezier.handleSize, Vector3.one * 0.5f, Handles.SphereHandleCap);
                                break;
                            case BezierPath.HandleMode.Axis:
                                control1Pos = Handles.PositionHandle(handle.control1, Quaternion.identity);
                                Handles.SphereHandleCap(handle.c1Id, handle.control1, Quaternion.identity,
                                    bezier.handleSize, EventType.Repaint);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (handle.control1 != control1Pos)
                            {
                                Undo.RecordObject(bezier, "Move control point");
                                handle.control1 = control1Pos;
                                handle.control0 = (handle.position - handle.control1) + handle.position;
                                EditorUtility.SetDirty(bezier);
                                Undo.FlushUndoRecordObjects();
                                bezier.OnChanged?.Invoke();
                            }
                        }
                    }

                    Handles.color = Color.black;
                    Handles.DrawLine(handle.control0, handle.control1);
                }
            }

            // 曲線の描画
            Handles.color = Color.green;
            bool isFirst = true;
            Vector3 prev = Vector3.zero;
            foreach (var pos in bezier.GetPoints())
            {
                if (isFirst)
                {
                    prev = pos;
                    isFirst = false;
                    continue;
                }

                Handles.DrawLine(prev, pos);
                prev = pos;
            }
        }
    }
#endif
}