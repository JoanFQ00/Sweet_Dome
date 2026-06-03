using UnityEngine;
using UnityEditor;

public class InteractablePrefabCreator
{
    [MenuItem("GameObject/Interactable Item", false, 0)]
    public static void CreateInteractable(MenuCommand menuCommand)
    {
        GameObject prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item.prefab");

        if (prefabBase == null)
        {
            Debug.LogError("No se encontró el prefab en: Assets/Prefabs/Item.prefab");
            return;
        }

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
        else
        {
            Debug.LogWarning("El prefab no tiene un hijo llamado 'Anchor'.");
        }

        Undo.RegisterCreatedObjectUndo(root, "Create Interactable");
        Selection.activeGameObject = root;
    }
}