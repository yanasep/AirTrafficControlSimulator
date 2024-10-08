﻿using System.Linq;
using UnityEngine;

namespace MeshGeneration
{
    public class BezierPathTracker
    {
        private BezierPath path;
        private Vector3[] points;
        private Vector3[] tangents;
        public Vector3 StartPosition { get; private set; }
        public Vector3 StartTangent { get; private set; }
        private int pointIndex;
        private float pointDelta;
        public bool IsEnd { get; private set; }

        public void Init(BezierPath path, bool reverse = false)
        {
            this.path = path;
            points = path.GetPoints().ToArray();
            tangents = path.GetTangents().ToArray();

            if (reverse)
            {
                points = points.Reverse().ToArray();
                tangents = tangents.Reverse().Select(dir => -dir).ToArray();
            }

            StartPosition = points[0];
            StartTangent = tangents[0];

            Reset();
        }

        public void Reset()
        {
            pointIndex = 0;
            pointDelta = 0;
            IsEnd = false;
        }

        public (Vector3 position, Vector3 tangent) Step(float maxDistanceDelta)
        {
            var prev = points[pointIndex];
            float deltaRemain = maxDistanceDelta;
            while (deltaRemain > 0 && pointIndex < points.Length - 1)
            {
                var next = points[pointIndex + 1];
                var distToNext = Vector3.Distance(next, prev) - pointDelta;
                if (distToNext > deltaRemain)
                {
                    pointDelta += deltaRemain;
                    deltaRemain = 0;
                }
                else
                {
                    // move to next point  
                    deltaRemain -= distToNext;
                    pointIndex++;
                    pointDelta = 0;
                }
            }

            if (pointIndex >= points.Length - 1)
            {
                IsEnd = true;
                return (points[^1], tangents[^1]);
            }

            float posRate = pointDelta / (points[pointIndex + 1] - points[pointIndex]).magnitude;
            if (float.IsNaN(posRate)) posRate = 1;

            var pos = Vector3.Lerp(points[pointIndex], points[pointIndex + 1], posRate);

            var prevAngle = Vector3.SignedAngle(Vector3.up, tangents[pointIndex], Vector3.back);
            var newAngle = Vector3.SignedAngle(Vector3.up, tangents[pointIndex + 1], Vector3.back);
            var angle = Mathf.LerpAngle(prevAngle, newAngle, posRate);
            var tan = Quaternion.AngleAxis(angle, Vector3.back) * Vector3.up;

            return (pos, tan);
        }
    }
}