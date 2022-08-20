using UnityEditor;
using UnityEngine;

public static class CreateEmptyScriptableObject
{
    /// <summary>
    /// 空のScriptableObjectを作成するエディタ拡張
    /// </summary>
    [MenuItem("Assets/Create/ScriptableObjects/Empty", priority = 1)]
    private static void Create()
    {
        ScriptableObject obj = ScriptableObject.CreateInstance<ScriptableObject>();

        string defaultName = "NewScriptableObject.asset";

        ProjectWindowUtil.CreateAsset(obj, defaultName);
    }
}
