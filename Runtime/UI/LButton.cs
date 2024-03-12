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
  [Serializable]
  public enum ButtonTransitionMode
  {
    ColorImageOnly,
    ColorTextOnly,
    ColorImageAndText,
    None
  }


  public class LButton : CustomUI, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
  {
    #region Fields
    [Tooltip("是否禁用按钮")]
    public bool disabled = false;
    [Tooltip("按钮背景组件")]
    public Image buttonImage;
    [Tooltip("按钮文字组件")]
    public TMP_Text buttonText;

    [Header("Transition")]
    [Tooltip("过渡时间")]
    public ButtonTransitionMode transitionType = ButtonTransitionMode.None;
    [Tooltip("过渡时间")]
    public float transitionTime = 0.1f;
    [Tooltip("按钮正常背景颜色")]
    public Color imageNormalColor = Color.white;
    [Tooltip("按钮悬浮背景颜色")]
    public Color imageHoverColor = Color.blue;
    [Tooltip("按钮按下背景颜色")]
    public Color imagePressedColor = Color.red;
    [Tooltip("按钮禁用背景颜色")]
    public Color imageDisabledColor = Color.gray;

    [Tooltip("按钮正常文字颜色")]
    public Color textNormalColor = Color.black;
    [Tooltip("按钮悬浮文字颜色")]
    public Color textHoverColor = Color.black;
    [Tooltip("按钮按下文字颜色")]
    public Color textPressedColor = Color.black;
    [Tooltip("按钮禁用文字颜色")]
    public Color textDisabledColor = Color.black;

    [Header("Events")]
    public UnityEvent onPointClick;
    public UnityEvent onPointDoubleClick;
    public UnityEvent onPointerMove;
    public UnityEvent onPointerEnter;
    public UnityEvent onPointerExit;
    public UnityEvent onPointerDown;
    public UnityEvent onPointerUp;
    #endregion

    #region Private Fields
    private Color ct;
    private Color tt;
    private Color ci;
    private Color ti;
    private Color CurrentTextColor { get => ct; set { ct = value; buttonText.color = value; } }
    private Color CurrentImageColor { get => ci; set { ci = value; buttonImage.color = value; } }
    private Color TargetTextColor { get => tt; set { TransitionTextColor(ct, value); } }
    private Color TargetImageColor { get => ti; set { TransitionImageColor(ci, value); } }
    private bool isIn = false;
    private List<LAnimation> anis = new List<LAnimation>();
    private bool inspectorControl = true;
    #endregion

    void Start()
    {
      InitComponents();
      InitColors();
    }

    private void InitColors()
    {
      buttonImage.color = CurrentImageColor = disabled ? imageDisabledColor : imageNormalColor;
      buttonText.color = CurrentTextColor = disabled ? textDisabledColor : textNormalColor;
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

    public override void InitComponents()
    {
      buttonImage = GetComponent<Image>();
      buttonText = GetComponentInChildren<TMP_Text>();
    }

    #region Editor
#if UNITY_EDITOR
  public override void GenerateStructure()
  {
    base.GenerateStructure<LButton>("Assets/Scripts/07-UI操作/CustomUI/Prefabs/Button.prefab");
  }
  public override void DrawEditorPreview()
  {
    if (inspectorControl)
      InitColors();
  }
  public override void DrawEditorPreview(SerializedObject serializedObject)
  {
    return;
  }
#endif
    #endregion

    public void TransitionTextColor(Color src, Color tar)
    {
      if (transitionType == ButtonTransitionMode.None || transitionType == ButtonTransitionMode.ColorImageOnly)
      {
        CurrentTextColor = tar;
        if (!isIn) inspectorControl = true;
      }
      else
        TransitionColor((c) => buttonText.color = c, () => CurrentTextColor = tar, src, tar);
    }

    public void TransitionImageColor(Color src, Color tar)
    {
      if (transitionType == ButtonTransitionMode.None || transitionType == ButtonTransitionMode.ColorTextOnly)
      {
        CurrentImageColor = tar;
        if (!isIn) inspectorControl = true;
      }
      else
        TransitionColor((c) => buttonImage.color = c, () => CurrentImageColor = tar, src, tar);
    }

    public void TransitionColor(Action<Color> onUpdate, Action onCompleted, Color src, Color tar)
    {
      AnimationOptions opt = new AnimationOptions();
      opt.duration = transitionTime;
      opt.onCompleted = () =>
      {
        if (!isIn) inspectorControl = true;
        onCompleted();   // 离开后可以由面板控制颜色（前提是动画结束了）
      };
      LAnimation ani = new LAnimation((v) =>
      {
        onUpdate(Color.Lerp(src, tar, v));
      }, opt);
      anis.Add(ani);
      ani.Play();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
      if (disabled) return;
      onPointerMove.Invoke();
      inspectorControl = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      if (disabled) return;
      onPointerEnter.Invoke();
      isIn = true;
      inspectorControl = false;
      TargetImageColor = imageHoverColor;
      TargetTextColor = textHoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      if (disabled) return;
      onPointerExit.Invoke();
      isIn = false; // 离开后可以由面板控制颜色（前提是动画结束了）
      TargetImageColor = imageNormalColor;
      TargetTextColor = textNormalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      if (disabled) return;
      onPointerDown.Invoke();
      inspectorControl = false;
      TargetImageColor = imagePressedColor;
      TargetTextColor = textPressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      if (disabled) return;
      onPointerUp.Invoke();
      // 判断松开点是否在按钮
      if (isIn)
      {
        TargetImageColor = imageHoverColor;
        TargetTextColor = textHoverColor;
      }
      else
      {
        TargetImageColor = imageNormalColor;
        TargetTextColor = textNormalColor;
      }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      if (eventData.clickCount == 1) onPointClick.Invoke();
      else if (eventData.clickCount == 2) onPointDoubleClick.Invoke();
    }
  }

}
