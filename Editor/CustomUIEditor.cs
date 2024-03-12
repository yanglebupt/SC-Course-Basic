using UnityEngine;
using UnityEditor;

namespace YLBasic
{
  public class CustomUIEditor<T> : Editor where T : CustomUI
  {
    public override void OnInspectorGUI()
    {
      T script = (T)(object)target;
      if (script.Initialized)
      {
        base.OnInspectorGUI();
        script.DrawEditorPreview(serializedObject);
        script.DrawEditorPreview();
      }
      else
      {
        // 为对应的脚本类在面板中添加按钮(未初始化状态)
        if (GUILayout.Button("GenerateStructure"))
        {
          script.GenerateStructure();
        }
      }
    }
  };


  [CustomEditor(typeof(LButton))]
  public class CustomButtonEditor : CustomUIEditor<LButton> { };

  [CustomEditor(typeof(LSlider))]
  public class CustomSliderEditor : CustomUIEditor<LSlider> { };

  [CustomEditor(typeof(LToggle))]
  public class CustomToggleEditor : CustomUIEditor<LToggle> { };
}

