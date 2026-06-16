using UnityEngine;
using UnityEditor;

public class InteractablePrefabCreator
{
    [MenuItem("GameObject/Interactable Item", false, 0)]
    public static void CreateInteractable(MenuCommand menuCommand)
    {
        GameObject prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item.prefab");
        GameObject root = PrefabUtility.InstantiatePrefab(prefabBase) as GameObject;
        GameObjectUtility.SetParentAndAlign(root, menuCommand.context as GameObject);
        Transform anchorTransform = root.transform.Find("Anchor");

        if (anchorTransform != null)
        {
            var actionComp = root.GetComponent<Action_Interactable>();
            if (actionComp != null)
            {
                var field = typeof(Action_Interactable).GetField("inspectionAnchor",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                field?.SetValue(actionComp, anchorTransform);
            }
        }
        Undo.RegisterCreatedObjectUndo(root, "Create Interactable");
        Selection.activeGameObject = root;
    }
}