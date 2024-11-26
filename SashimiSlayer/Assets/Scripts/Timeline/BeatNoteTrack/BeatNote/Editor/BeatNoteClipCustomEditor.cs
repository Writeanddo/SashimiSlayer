using UnityEditor;
using UnityEngine;

namespace Timeline.BeatNoteTrack.BeatNote.Editor
{
    [CustomEditor(typeof(BeatNoteClip))]
    public class BeatNoteClipCustomEditor : UnityEditor.Editor
    {
        private GUIStyle _labelStyle;

        private void OnEnable()
        {
            _labelStyle = new GUIStyle
            {
                normal = { textColor = Color.black },
                fontStyle = FontStyle.Bold
            };
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView view)
        {
            var actionClip = target as BeatNoteClip;
            if (actionClip == null)
            {
                return;
            }

            BeatNoteBehavior actionData = actionClip.Template;
            if (actionData == null)
            {
                return;
            }

            for (var i = 0; i < actionData.NoteData.Positions.Length; i++)
            {
                Vector3 pos = actionData.NoteData.Positions[i];

                Handles.color = Color.red;
                Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);

                Handles.DrawWireDisc(newPos, Vector3.forward, 0.5f);

                Handles.Label(newPos, $"Pos {i}", _labelStyle);

                if (newPos != pos)
                {
                    Undo.RecordObject(actionClip, "Edit BnH Action Clip");
                    actionData.NoteData.Positions[i] = newPos;
                    Repaint();
                }
            }
        }
    }
}