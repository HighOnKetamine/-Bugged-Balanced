using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared helpers for editor-only tools that programmatically build uGUI
/// prefab hierarchies (see ShopUIBuilder, PlayerInventoryUIBuilder).
/// </summary>
public static class UIPrefabBuilderUtils
{
    private const string MedievalFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/MedievalSharp-Regular SDF.asset";

    public static void ApplyMedievalFont(TMP_Text text)
    {
        if (text == null) return;
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MedievalFontPath);
        if (font != null)
            text.font = font;
        else
            Debug.LogWarning($"[UIPrefabBuilderUtils] Medieval font asset not found at {MedievalFontPath}.");
    }

    public static GameObject LoadOrCreatePrefabRoot(string path, string rootName, out bool isNew)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            isNew = false;
            return PrefabUtility.LoadPrefabContents(path);
        }

        isNew = true;
        return new GameObject(rootName, typeof(RectTransform));
    }

    public static void SavePrefabRoot(GameObject root, string path, bool wasNew)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        if (wasNew)
            Object.DestroyImmediate(root);
        else
            PrefabUtility.UnloadPrefabContents(root);
    }

    public static Transform FindRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    public static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    public static void RemoveChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) Object.DestroyImmediate(child.gameObject);
    }

    public static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    public static void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    public static void AssignSerialized(Object target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[UIPrefabBuilderUtils] Field '{fieldName}' not found on {target.GetType().Name}.");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
