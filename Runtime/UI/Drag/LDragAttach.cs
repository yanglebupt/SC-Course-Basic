using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YLBasic
{
  /// <summary>
  /// 接收拖拽，将可拖拽物体放入
  /// </summary>
  public class LDragAttach : AnimationUI
  {
    [Tooltip("使用默认的事件Action")]
    public bool useDefaultAction = true;
    [Tooltip("当有draggable对象 hover 改变颜色")]
    public Color hoverColor = Color.red;
    [Tooltip("一旦吸附了draggable对象，则其不可再次拖出，除非有 dragspawn 组件")]
    public bool cancelDrag = true;
    public UnityEvent OnHover;
    public UnityEvent OnLeave;
    public UnityEvent OnEnter;
    public UnityEvent OnAttached;

    [Tooltip("end 的时候 attached 是否冲突")]
    public UnityEvent<GameObject, GameObject, GameObject> OnAttachedOverlap;
    public UnityEvent<GameObject> OnHoverParams;
    public UnityEvent<GameObject> OnLeaveParams;
    public UnityEvent<GameObject> OnEnterParams;
    public UnityEvent<GameObject> OnAttachedParams;

    private Color defaultColor;
    private bool isActive = false;
    // 是否接触了draggable对象
    public bool IsActive { get => isActive; }
    private bool isAttached = false;
    // 是否已经存在物品了
    public bool IsAttached { get => isAttached; }

    private GameObject attachedGameObject;
    public GameObject AttachedGameObject { get => attachedGameObject; }

    void Start()
    {
      Image image = GetComponent<Image>();
      defaultColor = image.color;
      if (useDefaultAction)
      {
        OnEnter.AddListener(() =>
        {
          TransitionColorTo<Image>(image, hoverColor);
          isActive = true;
        });
        OnLeave.AddListener(() =>
        {
          TransitionColorTo<Image>(image, defaultColor);
          isActive = false;
        });
        OnAttached.AddListener(() =>
        {
          TransitionColorTo<Image>(image, defaultColor);
          isAttached = true;
        });
      }
    }

    public static void Attach(RectTransform draggableRect, RectTransform dragAttachRect, Action<GameObject, GameObject, GameObject> InvokeOnAttachedOverlap = null)
    {
      LDraggable draggable = draggableRect.GetComponent<LDraggable>();
      LDragAttach dragAttach = dragAttachRect.GetComponent<LDragAttach>();
      if (draggable == null)
      {
        throw new System.Exception("does not has LDraggable component");
      }
      if (dragAttach == null)
      {
        throw new System.Exception("does not has LDragAttach component");
      }

      if (!dragAttach.IsActive)
      {
        throw new System.Exception("Not Intersect with draggable");
      }
      if (dragAttach.IsAttached)
      {
        InvokeOnAttachedOverlap?.Invoke(dragAttach.AttachedGameObject, draggableRect.gameObject, dragAttachRect.gameObject);
        dragAttach.isAttached = false;
      }
      else
      {
        Tools.RemoveAllChildren(dragAttachRect.gameObject);
        draggableRect.position = dragAttachRect.position;
        draggableRect.SetParent(dragAttachRect);
        dragAttach.InvokeOnAttached(draggableRect.gameObject);
        dragAttach.attachedGameObject = draggableRect.gameObject;
        if (dragAttach.cancelDrag)
          draggable.CancelDrag();
      }
    }

    public void Attach(RectTransform src)
    {
      LDraggable draggable = src.GetComponent<LDraggable>();
      if (draggable == null)
      {
        throw new System.Exception("src does not has LDraggable component");
      }
      Attach(src, transform as RectTransform, (GameObject src, GameObject tar, GameObject attach) => OnAttachedOverlap?.Invoke(src, tar, attach));
    }

    public void InvokeOnEnter(GameObject obj = null)
    {
      OnEnter?.Invoke();
      OnEnterParams?.Invoke(obj);
    }
    public void InvokeOnLeave(GameObject obj = null)
    {
      OnLeave?.Invoke();
      OnLeaveParams?.Invoke(obj);
    }
    public void InvokeOnHover(GameObject obj = null)
    {
      OnHover?.Invoke();
      OnHoverParams?.Invoke(obj);
    }
    public void InvokeOnAttached(GameObject obj = null)
    {
      OnAttached?.Invoke();
      OnAttachedParams?.Invoke(obj);
    }

    public override void GenerateStructure()
    {
    }

    public override void DrawEditorPreview(UnityEditor.SerializedObject serializedObject)
    {
    }

    public override void DrawEditorPreview()
    {
    }

    public override void InitComponents()
    {
    }
  }

}

