using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace YLBasic
{
  /// <summary>
  /// 可拖拽对象
  /// </summary>
  public class LDraggable : AnimationUI, IBeginDragHandler, IDragHandler, IEndDragHandler
  {
    #region Fields
    [Tooltip("拖拽限制")]
    public RectTransform constraint;
    #endregion

    #region Private Fields
    private Vector3 offset;
    private bool stickH = false;
    private bool stickV = false;
    private bool isDragging = false;
    public bool IsDragging { get => isDragging; }
    public bool canDrag = true;
    // 可拖拽目标
    private RectTransform target;
    #endregion

    [HideInInspector]
    public object data = new object();

    #region Events
    [Tooltip("可以用来初始化设置一些拖拽数据")]
    public UnityEvent onBeginDrag = new UnityEvent();
    [Tooltip("可以用来判断是否接触到可接受区域的")]
    public UnityEvent onDrag = new UnityEvent();
    [Tooltip("end 判断是否 attached")]
    public UnityEvent onEndDrag = new UnityEvent();
    [Tooltip("end 后 attached 触发 ")]
    public UnityEvent<GameObject> onEndDragIsAttached = new UnityEvent<GameObject>();
    [Tooltip("end 的时候 attached 是否冲突")]
    public UnityEvent<GameObject, GameObject, GameObject> onEndDragAttachedOverlap = new UnityEvent<GameObject, GameObject, GameObject>();
    #endregion

    // 可放置目标集合
    private List<RectTransform> dragAttachList = new List<RectTransform>();
    // 当前的放置目标
    private RectTransform currentDragAttachRect;

    public RectTransform CurrentDragAttachRect { get => currentDragAttachRect; }

    public void CancelDrag()
    {
      canDrag = false;
    }

    private void Awake()
    {
      target = transform as RectTransform;
      currentDragAttachRect = null;

      onDrag.AddListener(() =>
      {
        foreach (RectTransform dragAttachRect in dragAttachList)
        {
          LDragAttach dragAttach = dragAttachRect.GetComponent<LDragAttach>();
          if (Tools.RectIntersect(target, dragAttachRect))
          {
            currentDragAttachRect = dragAttachRect;
            if (!dragAttach.IsActive)
              dragAttach.InvokeOnEnter(target.gameObject);
            else
              dragAttach.InvokeOnHover(target.gameObject);
            break;
          }
          else
          {
            currentDragAttachRect = null;
            if (dragAttach.IsActive)
              dragAttach.InvokeOnLeave(target.gameObject);
          }
        }
      });

      onEndDrag.AddListener(() =>
      {
        if (currentDragAttachRect == null)
        {
          DestroyImmediate(target.gameObject);
          onEndDragIsAttached?.Invoke(null);
        }
        else
        {
          target.GetComponent<LDraggable>().AttachTo(currentDragAttachRect);
          onEndDragIsAttached?.Invoke(currentDragAttachRect.gameObject);
        }
      });
    }

    public void UpdateDragAttachList(List<RectTransform> attachList)
    {
      dragAttachList = attachList;
    }

    /// <summary>
    /// 如果在限制内返回 true
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public (bool inV, bool inH, float left, float top, float right, float bottom) ApplyConstraint(Vector3 target)
    {
      Vector3[] constraintCorners = new Vector3[4];
      constraint.GetWorldCorners(constraintCorners);

      Vector3[] targetCorners = new Vector3[4];
      this.target.GetWorldCorners(targetCorners);
      Vector3 centerOffset = target - transform.position;
      for (int i = 0; i < 4; i++)
      {
        targetCorners[i] = targetCorners[i] + centerOffset;
      }

      (float c_l, float c_t, float c_r, float c_b) = Tools.GetRectBounds(constraint);
      (float t_l, float t_t, float t_r, float t_b) = Tools.GetRectBounds(targetCorners);
      Vector2 size = Tools.GetRectSize(this.target);
      (float half_w, float half_h) = (size.x / 2.0f, size.y / 2.0f);

      bool inH = t_l >= c_l && t_r <= c_r;
      bool inV = t_t <= c_t && t_b >= c_b;
      float left = c_l + half_w;
      float right = c_r - half_w;
      float top = c_t - half_h;
      float bottom = c_b + half_h;
      return (inH, inV, left, top, right, bottom);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
      if (!canDrag) return;
      isDragging = true;
      Vector3 downPoint = new Vector3(eventData.position.x, eventData.position.y, transform.position.z);
      offset = transform.position - downPoint;
      onBeginDrag?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)  // 注意不要使用 OnPointerMove 因为拖动太快会出范围，导致不触发了
    {
      if (!canDrag) return;
      // Vector3 move = new Vector3(eventData.delta.x, eventData.delta.y, transform.position.z);  // 注意不要使用 delta 因为当鼠标在范围外，又回来了物体无法跟随鼠标了
      // Vector3 target = transform.position + move;
      // rect transform 就是屏幕坐标
      isDragging = true;
      Vector3 mousePoint = new Vector3(eventData.position.x, eventData.position.y, transform.position.z);
      Vector3 target = mousePoint + offset;
      (bool inH, bool inV, float left, float top, float right, float bottom) = ApplyConstraint(target); // x,y 分别判断
      if (inH)
      {
        transform.position = new Vector3(target.x, transform.position.y, target.z);
        stickH = false;
      }
      else
      {
        if (!stickH)
        {
          // 存在滑动过快，鼠标超出区域，这时需要物体自动吸附到边界
          transform.position = new Vector3(eventData.delta.x < 0 ? left : right, transform.position.y, target.z);
          stickH = true;
        }
      }
      if (inV)
      {
        transform.position = new Vector3(transform.position.x, target.y, target.z);
        stickV = false;
      }
      else
      {
        if (!stickV) // 防止重复 stick 出现跳变
        {
          // 存在滑动过快，鼠标超出区域，这时需要物体自动吸附到边界
          transform.position = new Vector3(transform.position.x, eventData.delta.y > 0 ? top : bottom, target.z);
          stickV = true;
        }
      }
      onDrag?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
      if (!canDrag) return;
      isDragging = false;
      onEndDrag?.Invoke();
    }

    public void AttachTo(RectTransform dst)
    {
      LDragAttach dragAttach = dst.GetComponent<LDragAttach>();
      if (dragAttach == null)
      {
        throw new System.Exception("dst does not has LDragAttach component");
      }
      LDragAttach.Attach(target, dst, (GameObject src, GameObject tar, GameObject attach) => onEndDragAttachedOverlap?.Invoke(src, tar, attach));
    }
  }

}
