using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BnHActionClip))]
public class BnHActionClipCustomEditor : Editor
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
        var actionClip = target as BnHActionClip;
        if (actionClip == null)
        {
            return;
        }

        BnHActionBehavior actionData = actionClip.Template;
        if (actionData == null)
        {
            return;
        }

        for (var i = 0; i < actionData.ActionData.Positions.Length; i++)
        {
            Vector3 pos = actionData.ActionData.Positions[i];

            Handles.color = Color.red;
            Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);

            Handles.DrawWireDisc(newPos, Vector3.forward, 0.5f);

            Handles.Label(newPos, $"Pos {i}", _labelStyle);

            if (newPos != pos)
            {
                Undo.RecordObject(actionClip, "Edit BnH Action Clip");
                actionData.ActionData.Positions[i] = newPos;
                Repaint();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}