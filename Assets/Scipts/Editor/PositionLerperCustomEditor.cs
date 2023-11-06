using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scipts.Editor
{
    [CustomEditor(typeof(PositionLerper))]
    public class PositionLerperCustomEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return base.CreateInspectorGUI();
        }


        private void OnSceneGUI()
        {
            var positionLerper = target as PositionLerper;

            for (int i = 0; i < positionLerper.localPositions.Count; i++)
            {
                var globalPos = positionLerper.transform.TransformPoint(positionLerper.localPositions[i]);
                globalPos = Handles.PositionHandle(globalPos, Quaternion.identity);

                positionLerper.localPositions[i] = positionLerper.transform.InverseTransformPoint(globalPos);
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}