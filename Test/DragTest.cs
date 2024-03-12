using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public class DragTest : MonoBehaviour
  {
    public RectTransform draggableRect;
    public RectTransform dragAttachRect;

    void Start()
    {
      LDraggable draggable = draggableRect.GetComponent<LDraggable>();
      draggable.onDrag.AddListener(() =>
      {
        LDragAttach dragAttach = dragAttachRect.GetComponent<LDragAttach>();
        if (Tools.RectIntersect(draggableRect, dragAttachRect))
        {
          if (!dragAttach.IsActive)
            dragAttach.OnEnter.Invoke();
          else
            dragAttach.OnHover.Invoke();
        }
        else
        {
          if (dragAttach.IsActive)
            dragAttach.OnLeave.Invoke();
        }
      });
      draggable.onEndDrag.AddListener(() =>
      {
        draggable.AttachTo(dragAttachRect);
      });
    }
  }

}
