using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(WorldItem))]
public class WorldItemEditor : Editor {

    WorldItem myWorldItem;

    public override void OnInspectorGUI() {
        myWorldItem = (WorldItem)target;

        base.OnInspectorGUI();

        /*if (myWorldItem.GetItem().GetType() == typeof(CombinedItems)) {
            var combinedItems = serializedObject.FindProperty("itemComponents");
            EditorGUILayout.PropertyField(combinedItems, new GUIContent("Components"), true);
        }*/
    }
}
