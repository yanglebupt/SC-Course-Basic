using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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
  public class SliderThresholdColors : IComparable<SliderThresholdColors>
  {
    public float threshold;
    public Color fillColor;
    public Color bgColor;
    public Color textColor;

    private bool _isActive;
    public bool IsActive { get => _isActive; set => _isActive = value; }

    public int CompareTo(SliderThresholdColors other)
    {
      return threshold > other.threshold ? 1 : -1;
    }
  }

  public class LSlider : AnimationUI
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
    [Tooltip("正常状态颜色")]
    public SliderThresholdColors normalColors = new SliderThresholdColors()
    {
      threshold = 1,
      bgColor = Color.black,
      fillColor = Color.white,
      textColor = Color.blue,
      IsActive = true
    };
    [Tooltip("滑动条类型")]
    public SliderMode sliderMode = SliderMode.Rect;
    [Tooltip("填充方式")]
    public Image.FillMethod fillMethod = Image.FillMethod.Horizontal; // FillMethod 和 FillOrigin 是联动的
    [Tooltip("文本显示方式")]
    public TextMode textMode = TextMode.PERCENTAGE;
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
    #endregion

    #region Editor
#if UNITY_EDITOR
    public override void GenerateStructure()
    {
      LSlider slider = GenerateStructure<LSlider>("Packages/com.ylbupt.sc-course-basic/Resources/UI/Prefabs/Slider.prefab");
      Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.ylbupt.sc-course-basic/Resources/UI/Sprites/circle.png");
      Sprite rect = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.ylbupt.sc-course-basic/Resources/UI/Sprites/rect.png");
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

      // 把 threshold=1 合并进来
      List<SliderThresholdColors> _threshold_list = new List<SliderThresholdColors>();
      _threshold_list.AddRange(sliderThresholdColors);
      _threshold_list.Add(normalColors);

      for (int i = 0; i < _threshold_list.Count; i++)
      {
        SliderThresholdColors curThresholdColors = _threshold_list[i];
        if (fillValue <= curThresholdColors.threshold)
        {
          if (!curThresholdColors.IsActive)
          {
            for (int j = 0; j < _threshold_list.Count; j++) // 其余置 false，自己置 true
            {
              _threshold_list[j].IsActive = j == i;
            }
            TransitionColorTo<Image>(background, curThresholdColors.bgColor);
            TransitionColorTo<Image>(fillarea, curThresholdColors.fillColor);
            TransitionColorTo<TMP_Text>(text, curThresholdColors.textColor);
          }
          return;
        }
      }
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

    public override void InitComponents()
    {
      background = transform.Find("Bg").GetComponent<Image>();
      fillarea = transform.Find("Fill").GetComponent<Image>();
      text = transform.Find("Text").GetComponent<TMP_Text>();
    }
  }
}

