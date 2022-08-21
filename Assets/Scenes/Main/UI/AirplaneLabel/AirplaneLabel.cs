using System;
using TMPro;
using UnityEngine;

namespace AirTraffic.Main.UI
{
    public class AirplaneLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RectTransform positionRect;
        [SerializeField] private RectTransform lineRect;
        [SerializeField] private Vector2 positionOffset;
        [SerializeField] private float lineLengthAdd;
        private Transform followWorld;
        private Camera mainCamera;
        private RectTransform canvasRectTransform;

        private void Awake()
        {
            mainCamera = Camera.main;
            canvasRectTransform = GetComponentInParent<Canvas>().transform as RectTransform;
        }

        private void Update()
        {
            UpdatePosition(followWorld.transform.position);
        }

        public void SetName(string name)
        {
            nameText.SetText(name);
        }

        public void SetFollow(Transform followWorld)
        {
            this.followWorld = followWorld;
            UpdatePosition(followWorld.transform.position);
        }

        private void UpdatePosition(Vector3 worldPos)
        {
            var targetScreenPos = mainCamera.WorldToScreenPoint(worldPos);
            positionRect.transform.position = GetScreenSafePosition(targetScreenPos, positionOffset);

            // line
            var center = GetRectTransformCenter(positionRect);
            var lineDir = (targetScreenPos - center).normalized;
            var lineEnd = center - (Vector3)PointOnRect(positionRect.rect, lineDir);
            float localScale = positionRect.localScale.x;
            SetLinePosition(targetScreenPos + lineDir * (lineLengthAdd * localScale),
                lineEnd - lineDir * (lineLengthAdd * localScale));
        }

        /// <summary>
        /// 指定スクリーン位置及び・ずらし位置からスクリーン内に必ず収まるスクリーン位置を取得する
        /// </summary>
        private Vector2 GetScreenSafePosition(Vector3 screenPosition, Vector2 additionalVector)
        {
            // スクリーン位置へ指定ベクトルを加算
            var addX = additionalVector.x * canvasRectTransform.localScale.x;
            var addY = additionalVector.y * canvasRectTransform.localScale.y;
            screenPosition.x += addX;
            screenPosition.y += addY;

            var rectTran = positionRect;
            var xMin = rectTran.sizeDelta.x * rectTran.pivot.x * canvasRectTransform.localScale.x;
            var xMax = Screen.width - rectTran.sizeDelta.x * (1 - rectTran.pivot.x) * canvasRectTransform.localScale.x;
            var yMin = rectTran.sizeDelta.y * rectTran.pivot.y * canvasRectTransform.localScale.y;
            var yMax = Screen.height - rectTran.sizeDelta.y * (1 - rectTran.pivot.y) * canvasRectTransform.localScale.y;

            screenPosition.x = Mathf.Clamp(screenPosition.x, xMin, xMax);
            screenPosition.y = Mathf.Clamp(screenPosition.y, yMin, yMax);

            return screenPosition;
        }

        /// <summary>
        /// 線の始点と終点をセット
        /// </summary>
        public void SetLinePosition(Vector3 startPos, Vector3 endPos)
        {
            // 線の位置、長さ、向きをセット
            lineRect.position = startPos;
            var diff = endPos - startPos;
            var len = diff.magnitude;
            var size = lineRect.sizeDelta;
            size.y = len;
            lineRect.sizeDelta = size;
            if (len > 0)
            {
                var deg = Vector3.SignedAngle(diff / len, Vector3.up, Vector3.back);
                lineRect.eulerAngles = new Vector3(0, 0, deg);
            }
        }

        public static Vector3 GetRectTransformCenter(RectTransform rectTransform)
        {
            var position = rectTransform.position;
            return position - new Vector3(
                Mathf.Lerp(-rectTransform.rect.size.x / 2f, rectTransform.rect.size.x / 2f, rectTransform.pivot.x) *
                rectTransform.transform.lossyScale.x,
                Mathf.Lerp(-rectTransform.rect.size.y / 2f, rectTransform.rect.size.y / 2f, rectTransform.pivot.y) *
                rectTransform.transform.lossyScale.y
            );
        }

        // https://answers.unity.com/questions/1221879/finding-point-on-a-bounds-2d-rectangle-using-its-c.html
        public static Vector2 PointOnRect(Rect rect, Vector2 direction)
        {
            direction.Normalize();
            var extents = rect.size / 2f;
            var v = direction;
            float y = extents.x * v.y / v.x;
            if (Mathf.Abs(y) < extents.y)
                return new Vector2(extents.x, y);
            return new Vector2(extents.y * v.x / v.y, extents.y);
        }
    }
}