using UnityEngine;

namespace MeshGeneration
{
    /*************************************************************************************************/
    /// <summary>
    /// BezierPathを設定するためのハンドル
    /// </summary>
    /*************************************************************************************************/
    [System.Serializable]
    public class BezierHandle
    {
        public Vector3 position;
        public Vector3 control0;
        public Vector3 control1;
        public bool isCorner; // 直線で結ぶかどうか

        [System.NonSerialized] public int posId; // エディタ用
        [System.NonSerialized] public int c0Id; // エディタ用
        [System.NonSerialized] public int c1Id; // エディタ用

        public BezierHandle()
        {
        }

        public BezierHandle(Vector3 position)
        {
            this.position = position;
            control0 = Vector3.up * 3 + position;
            control1 = Vector3.up * -3 + position;
        }

        public void SetHandlePositoin(Vector3 targetPos)
        {
            var offset = targetPos - position;
            position = targetPos;
            control0 += offset;
            control1 += offset;
        }

        public void MoveHandleByOffset(Vector3 offset)
        {
            position += offset;
            control0 += offset;
            control1 += offset;
        }
    }
}