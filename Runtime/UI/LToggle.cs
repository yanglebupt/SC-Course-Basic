using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace YLBasic
{
  public class LToggle : CustomUI, IPointerClickHandler
  {

    #region Fields
    private bool isOn;
    [HideInInspector]
    public bool _isOn;
    public Image bgImage;
    public Image selectedImage;
    public TMP_Text text;

    [Tooltip("未选中时背景颜色")]
    public Color bgNormalColor = Color.white;

    [Tooltip("选中时背景颜色")]
    public Color bgSelectedColor = Color.red;

    public bool useTransition = false;
    public float transitionTime = 0.1f;
    #endregion

    #region Events
    public UnityEvent<bool> onValueChanged;
    public UnityEvent onOn;
    public UnityEvent onOff;
    #endregion

    private List<LAnimation> anis = new List<LAnimation>();

    #region Editor
#if UNITY_EDITOR
  public override void DrawEditorPreview()
  {
    InitValues();  // 直接在面板上修改 isOn 不行
  }

  public override void DrawEditorPreview(SerializedObject serializedObject)
  {
    serializedObject.Update();
    SerializedProperty o = serializedObject.FindProperty("_isOn");
    if (!EditorApplication.isPlaying)
    {
      EditorGUILayout.PropertyField(o, new GUIContent("isOn"));
      isOn = o.boolValue;
    }
    serializedObject.ApplyModifiedProperties();
  }

  public override void GenerateStructure()
  {
    base.GenerateStructure<LToggle>("Assets/Scripts/07-UI操作/CustomUI/Prefabs/Toggle.prefab");
  }
#endif
    #endregion

    public void InitValues()
    {
      // 无法记录面板的值是否改变，导致每一帧都要调用，生成动画
      if (isOn) TurnOn();
      else TurnOff();
    }

    public override void InitComponents()
    {
      bgImage = transform.Find("Bg").GetComponent<Image>();
      selectedImage = transform.Find("Selected").GetComponent<Image>();
      text = transform.Find("Text").GetComponent<TMP_Text>();
    }

    public void TurnOn()
    {
      selectedImage.gameObject.SetActive(true);
      TransitionColor<Image>(bgImage, bgImage.color, bgSelectedColor);
      TransitionAlpha<Image>(selectedImage, () =>
      {
        // if (!selectedImage.gameObject.activeInHierarchy && isOn) { selectedImage.gameObject.SetActive(true); }
        // else if (!isOn)
        // {
        //   selectedImage.gameObject.SetActive(false);
        // }
      }, selectedImage.color.a, 1);
    }
    public void TurnOff()
    {
      TransitionColor<Image>(bgImage, bgImage.color, bgNormalColor);
      TransitionAlpha<Image>(selectedImage, selectedImage.color.a, 0, () =>
      {
        if (!isOn)
          selectedImage.gameObject.SetActive(false);
      });
    }

    public void TransitionColor(Action<Color> onUpdate, Action onCompleted, Color src, Color tar)
    {
      AnimationOptions opt = new AnimationOptions();
      opt.duration = transitionTime;
      opt.onCompleted = onCompleted;
      LAnimation ani = new LAnimation((v) =>
      {
        onUpdate(Color.Lerp(src, tar, v));
      }, opt);
      anis.Add(ani);
      ani.Play();
    }
    public void TransitionAlpha(Action<float> onUpdate, Action onCompleted, float src, float tar)
    {
      AnimationOptions opt = new AnimationOptions();
      opt.duration = transitionTime;
      opt.onCompleted = onCompleted;
      LAnimation ani = new LAnimation((v) =>
      {
        onUpdate(Mathf.Lerp(src, tar, v));
      }, opt);
      anis.Add(ani);
      ani.Play();
    }

    public void TransitionColor<T>(T t, Color src, Color tar) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionColor((Color c) => t.color = c, () => { }, src, tar);
      else
        t.color = tar;
    }

    public void TransitionAlpha<T>(T t, float src, float tar) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionAlpha((float c) => t.color = GetColorByAlpha(t.color, c), () => { }, src, tar);
      else
        t.color = GetColorByAlpha(t.color, tar);
    }

    public void TransitionAlpha<T>(T t, float src, float tar, Action onCompleted) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionAlpha((float c) => t.color = GetColorByAlpha(t.color, c), onCompleted, src, tar);
      else
      { t.color = GetColorByAlpha(t.color, tar); onCompleted(); }
    }

    public void TransitionAlpha<T>(T t, Action onUpdate, float src, float tar) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionAlpha((float c) => { t.color = GetColorByAlpha(t.color, c); onUpdate(); }, () => { }, src, tar);
      else
      { t.color = GetColorByAlpha(t.color, tar); }
    }

    public Color GetColorByAlpha(Color color, float alpha)
    {
      return new Color(color.r, color.g, color.b, alpha);
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
      else
      {
        if (isOn) onOn.Invoke();
        else onOff.Invoke();
      }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      SetIsOn(!isOn);
    }

    void Start()
    {
      InitValues();
    }

    private void FixedUpdate()
    {
      for (int i = anis.Count - 1; i >= 0; i--)  // 一边遍历，一边删除
      {
        LAnimation item = anis[i];
        if (item.Completed)
        {
          anis.Remove(item);
          continue;
        }
        item.Update(Time.fixedDeltaTime);
      }
    }
  }
}

