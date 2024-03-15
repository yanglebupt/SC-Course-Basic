using UnityEngine;
using UnityEditor;

namespace YLBasic
{
  [CustomEditor(typeof(LocalizationComponent))]
  public class LocalizationComponentEditor : Editor
  {
    public override void OnInspectorGUI()
    {
      LocalizationComponent script = target as LocalizationComponent;
      base.OnInspectorGUI();
      script.DrawEditorPreview(serializedObject);
      GUI.contentColor = new Color(0.9f, 0.8f, 0.7f);
      GUILayout.Label("To use language localization: {ID}, Only the ID in the package will be updated its language text");

    }
  };
}


