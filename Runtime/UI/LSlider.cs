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
  public enum SliderMode
  {
    Rect,
    Circle,
  }

  public enum FillOriginHorizontal
  {
    Left,
    Right,
  }

  public enum FillOriginVertical
  {
    Bottom,
    Top,
  }
  public enum FillOriginR90
  {
    BottomLeft,
    TopLeft,
    TopRight,
    BottomRight,
  }

  public enum FillOrigin
  {
    Bottom,
    Right,
    Top,
    Left,
  }

  public enum TextMode
  {
    PERCENTAGE,
    COUNTER,
  }


  [Serializable]
  public struct SliderThresholdColors : IComparable<SliderThresholdColors>
  {
    public float threshold;
    public Color fillColor;
    public Color bgColor;
    public Color textColor;

    public int CompareTo(SliderThresholdColors other)
    {
      return threshold > other.threshold ? 1 : -1;
    }
  }

  public class LSlider : CustomUI
  {
    #region Fields
    [Tooltip("滑动值")]
    [Range(0, 1)]
    public float fillValue = 1;
    [Tooltip("背景图片")]
    public Image background;
    [Tooltip("填充图片")]
    public Image fillarea;
    [Tooltip("进度文字")]
    public TMP_Text text;
    [Tooltip("填充颜色")]
    public Color fillColor = Color.white;
    [Tooltip("背景颜色")]
    public Color bgColor = Color.black;
    [Tooltip("文字颜色")]
    public Color textColor = Color.black;
    [Tooltip("滑动条类型")]
    public SliderMode sliderMode = SliderMode.Rect;
    [Tooltip("填充方式")]
    public Image.FillMethod fillMethod = Image.FillMethod.Horizontal; // FillMethod 和 FillOrigin 是联动的
    [HideInInspector]
    public int fillOrigin = 0;
    [HideInInspector]
    public FillOrigin fillOriginA;
    [HideInInspector]
    public FillOriginR90 fillOriginR90;
    [HideInInspector]
    public FillOriginHorizontal fillOriginH;
    [HideInInspector]
    public FillOriginVertical fillOriginV;

    [Tooltip("滑动阈值颜色")]
    [SerializeField]
    public List<SliderThresholdColors> sliderThresholdColors = new List<SliderThresholdColors>();
    [Tooltip("设置好阈值后勾选排序，设置过程中取消勾选")]
    public bool needSorted = false;
    public bool useTransition = true;
    public float transitionTime = 0.05f;

    public TextMode textMode = TextMode.PERCENTAGE;

    [HideInInspector]
    public int textCount = 100;

    #endregion

    #region Events
    public UnityEvent<float> onValueChange;
    #endregion

    #region Private Fields
    [HideInInspector]
    public Sprite rect;
    [HideInInspector]
    public Sprite circle;
    private List<LAnimation> anis = new List<LAnimation>();
    #endregion

    #region Editor
#if UNITY_EDITOR
  public override void GenerateStructure()
  {
    LSlider slider = base.GenerateStructure<LSlider>("Assets/Scripts/07-UI操作/CustomUI/Prefabs/Slider.prefab");
    Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Scripts/07-UI操作/CustomUI/Sprites/circle.png");
    Sprite rect = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Scripts/07-UI操作/CustomUI/Sprites/rect.png");
    slider.rect = rect;
    slider.circle = circle;
  }
  public override void DrawEditorPreview(SerializedObject serializedObject)
  {
    serializedObject.Update();
    SerializedProperty p = serializedObject.FindProperty("fillMethod");
    SerializedProperty originA = serializedObject.FindProperty("fillOriginA");
    SerializedProperty originR90 = serializedObject.FindProperty("fillOriginR90");
    SerializedProperty originH = serializedObject.FindProperty("fillOriginH");
    SerializedProperty originV = serializedObject.FindProperty("fillOriginV");
    SerializedProperty textMode = serializedObject.FindProperty("textMode");
    SerializedProperty textCount = serializedObject.FindProperty("textCount");
    if (textMode.enumValueIndex == 1)
    {
      EditorGUILayout.PropertyField(textCount);
    }
    if (p.enumValueIndex == 0)
    {
      EditorGUILayout.PropertyField(originH, new GUIContent("Fill Origin"));
      fillOrigin = originH.enumValueIndex;
    }
    else if (p.enumValueIndex == 1)
    {
      EditorGUILayout.PropertyField(originV, new GUIContent("Fill Origin"));
      fillOrigin = originV.enumValueIndex;
    }
    else if (p.enumValueIndex == 2)
    {
      EditorGUILayout.PropertyField(originR90, new GUIContent("Fill Origin"));
      fillOrigin = originR90.enumValueIndex;
    }
    else
    {
      EditorGUILayout.PropertyField(originA, new GUIContent("Fill Origin"));
      fillOrigin = originA.enumValueIndex;
    }
    serializedObject.ApplyModifiedProperties();
  }
  public override void DrawEditorPreview() // Update
  {
    InitValues();
  }
#endif
    #endregion

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

    public void InitValues()
    {
      Sprite s = sliderMode == SliderMode.Circle ? circle : rect;
      fillarea.sprite = background.sprite = s;
      fillarea.fillAmount = fillValue;
      onValueChange.Invoke(fillValue);
      fillarea.fillMethod = fillMethod;
      fillarea.fillOrigin = fillOrigin;
      text.text = GetTextContent();
      // 根据 value 选择颜色
      if (needSorted)
        sliderThresholdColors.Sort((SliderThresholdColors A, SliderThresholdColors B) => A.CompareTo(B)); // 升序
      for (int i = 0; i < sliderThresholdColors.Count; i++)
      {
        if (fillValue <= sliderThresholdColors[i].threshold)
        {
          TransitionColor<Image>(background, background.color, sliderThresholdColors[i].bgColor);
          TransitionColor<Image>(fillarea, fillarea.color, sliderThresholdColors[i].fillColor);
          TransitionColor<TMP_Text>(text, text.color, sliderThresholdColors[i].textColor);
          return;
        }
      }
      TransitionColor<Image>(background, background.color, bgColor);
      TransitionColor<Image>(fillarea, fillarea.color, fillColor);
      TransitionColor<TMP_Text>(text, text.color, textColor);
    }

    public string GetTextContent()
    {
      if (textMode == TextMode.PERCENTAGE)
      {
        string v = (fillValue * 100).ToString("0.00");
        return $"{v}%";
      }
      else if (textMode == TextMode.COUNTER)
      {
        int v = (int)(fillValue * textCount);
        return $"{v}/{textCount}";
      }
      else
      {
        return "";
      }
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

    public void TransitionColor<T>(T t, Color src, Color tar) where T : MaskableGraphic
    {
      if (EditorApplication.isPlaying && useTransition)
        TransitionColor((Color c) => t.color = c, () => { }, src, tar);
      else
        t.color = tar;
    }

    public override void InitComponents()
    {
      background = transform.Find("Bg").GetComponent<Image>();
      fillarea = transform.Find("Fill").GetComponent<Image>();
      text = transform.Find("Text").GetComponent<TMP_Text>();
    }
  }
}

