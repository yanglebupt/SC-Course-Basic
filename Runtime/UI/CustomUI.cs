using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace YLBasic
{
  public abstract class CustomUI : MonoBehaviour
  {
    [HideInInspector]
    public bool Initialized = false;
    public abstract void GenerateStructure();
    /// <summary>
    /// 初始化组件结构
    /// </summary>
    public T GenerateStructure<T>(string prefabPath) where T : CustomUI
    {
      GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
      GameObject obj = Instantiate(prefab);
      RectTransform oriTransform = GetComponent<RectTransform>();
      obj.name = transform.gameObject.name;
      RectTransform tarTransform = obj.GetComponent<RectTransform>();
      tarTransform.SetParent(oriTransform.parent);

      tarTransform.localPosition = oriTransform.localPosition;
      tarTransform.localScale = oriTransform.localScale;
      tarTransform.localRotation = oriTransform.localRotation;
      tarTransform.anchorMin = oriTransform.anchorMin;
      tarTransform.anchorMax = oriTransform.anchorMax;
      tarTransform.pivot = oriTransform.pivot;

      obj.transform.SetSiblingIndex(transform.GetSiblingIndex());
      T script = obj.GetComponent<T>();
      script.Initialized = true;
      script.InitComponents();
      Selection.activeGameObject = obj;
      DestroyImmediate(gameObject);
      return script;
    }
    /// <summary>
    /// 动态修改响应
    /// </summary>
    public abstract void DrawEditorPreview(SerializedObject serializedObject);
    public abstract void DrawEditorPreview();
    /// <summary>
    /// 初始化组件
    /// </summary>
    public abstract void InitComponents();
  }
}
