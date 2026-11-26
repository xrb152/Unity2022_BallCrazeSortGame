using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayerData))]
public class LayerDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LayerData layerData = (LayerData)target;

        EditorGUILayout.LabelField("Layer Configuration", EditorStyles.boldLabel);

        // Display trays
        for (int j = 0; j < layerData.trays.Count; j++)
        {
            var tray = layerData.trays[j];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Tray: {j + 1}", EditorStyles.boldLabel);

            // Modify tray properties
            tray.size = (TrayType)EditorGUILayout.EnumPopup("Size", tray.size);
            tray.trayColor = (ECoffeeColor)EditorGUILayout.EnumPopup("Color", tray.trayColor);
            tray.position = EditorGUILayout.Vector2Field("Position", tray.position);
            tray.direction = (TrayDirection)EditorGUILayout.EnumPopup("Direction", tray.direction);

            // Remove tray button
            if (GUILayout.Button("Remove Tray"))
            {
                Undo.RecordObject(layerData, "Remove Tray");
                layerData.trays.RemoveAt(j);
                break; // Exit loop to avoid layout errors
            }

            EditorGUILayout.EndVertical();
        }

        // Add trays button
        if (GUILayout.Button("Add Tray"))
        {
            Undo.RecordObject(layerData, "Add Tray");
            layerData.trays.Add(new LayerData.TrayData());
        }

        // Mark as dirty if changes are made
        if (GUI.changed)
        {
            EditorUtility.SetDirty(layerData);
        }
    }
}
