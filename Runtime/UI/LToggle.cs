using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace YLBasic
{
  public class LToggle : AnimationUI, IPointerClickHandler
  {

    #region Fields
    private bool isOn;  // 这个是内部用的
    public bool IsOn;  // 面板调整的
    public Image bgImage;
    public Image selectedImage;
    public TMP_Text text;

    [Tooltip("未选中时背景颜色")]
    public Color bgNormalColor = Color.white;

    [Tooltip("选中时背景颜色")]
    public Color bgSelectedColor = Color.red;
    #endregion

    #region Events
    public UnityEvent<bool> onValueChanged;
    public UnityEvent onOn;
    public UnityEvent onOff;
    #endregion

    #region Editor
#if UNITY_EDITOR
    public override void DrawEditorPreview()
    {
      return;
    }

    public override void DrawEditorPreview(SerializedObject serializedObject)
    {
      serializedObject.Update();
      SerializedProperty o = serializedObject.FindProperty("IsOn");
      SetIsOn(o.boolValue);
      serializedObject.ApplyModifiedProperties();
    }

    public override void GenerateStructure()
    {
      GenerateStructure<LToggle>("Packages/com.ylbupt.sc-course-basic/Resources/UI/Prefabs/Toggle.prefab");
    }
#endif
    #endregion

    public override void InitComponents()
    {
      bgImage = transform.Find("Bg").GetComponent<Image>();
      selectedImage = transform.Find("Selected").GetComponent<Image>();
      text = transform.Find("Text").GetComponent<TMP_Text>();
    }

    public void TurnOn()
    {
      selectedImage.gameObject.SetActive(true);
      TransitionColorTo<Image>(bgImage, bgSelectedColor);
      TransitionAlphaTo<Image>(selectedImage, 1);
    }
    public void TurnOff()
    {
      TransitionColorTo<Image>(bgImage, bgNormalColor);
      TransitionAlphaTo<Image>(selectedImage, 0, () =>
      {
        if (!isOn)
          selectedImage.gameObject.SetActive(false);
      });
    }

    public void SetIsOn(bool _isOn)
    {
      bool changed = isOn != _isOn;
      if (changed)
      {
        isOn = _isOn;
        onValueChanged.Invoke(isOn);
        if (isOn) { TurnOn(); onOn.Invoke(); }
        else { TurnOff(); onOff.Invoke(); }
      }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      SetIsOn(!isOn);
    }
  }
}

