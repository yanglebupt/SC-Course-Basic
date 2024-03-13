using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YLBasic
{
  /// <summary>
  /// 发生拖拽，生成一个可拖拽物体
  /// </summary>
  public class LDragSpawn : AnimationUI, IBeginDragHandler, IDragHandler, IEndDragHandler
  {
    [Tooltip("生成的可拖拽游戏对象")]
    public GameObject prefab;

    public bool useSelf = false;

    [Tooltip("生成的可拖拽游戏对象")]
    public RectTransform constraint;

    private LDraggable draggable;

    #region Events
    public UnityEvent<GameObject> initDraggableGameObject;
    public UnityEvent onBeginDrag;
    [Tooltip("可以用来判断是否接触到可接受区域的")]
    public UnityEvent onDrag;
    public UnityEvent onEndDrag;
    [Tooltip("end 是否 attached")]
    public UnityEvent<GameObject> onEndDragIsAttached;
    [Tooltip("end 的时候 attached 是否冲突")]
    public UnityEvent<GameObject, GameObject, GameObject> onEndDragAttachedOverlap;
    #endregion

    void Start()
    {
      if (useSelf)
      {
        prefab = gameObject;
      }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
      GameObject gameObject = Instantiate(prefab, transform.parent);
      if (useSelf)
      {
        Destroy(gameObject.GetComponent<LDragSpawn>());
        draggable = gameObject.AddComponent<LDraggable>();
      }
      else
      {
        draggable = gameObject.GetComponent<LDraggable>();
      }
      if (draggable == null)
      {
        throw new System.Exception("GameObject does not has LDraggable component");
      }
      initDraggableGameObject?.Invoke(gameObject);
      gameObject.transform.position = eventData.position;
      draggable.constraint = constraint;
      draggable.onBeginDrag.AddListener(() => onBeginDrag?.Invoke()); // 注意不要直接赋值
      draggable.onDrag.AddListener(() => onDrag?.Invoke());
      draggable.onEndDrag.AddListener(() => onEndDrag?.Invoke());
      draggable.onEndDragIsAttached.AddListener((GameObject attached) => onEndDragIsAttached?.Invoke(attached));
      draggable.onEndDragAttachedOverlap.AddListener((GameObject src, GameObject tar, GameObject attach) => onEndDragAttachedOverlap?.Invoke(src, tar, attach));
      draggable.OnBeginDrag(eventData);  // 注意将事件传播下去
    }

    public void OnDrag(PointerEventData eventData)
    {
      draggable.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
      draggable.OnEndDrag(eventData);
    }

    public override void GenerateStructure()
    {
    }

    public override void DrawEditorPreview(SerializedObject serializedObject)
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
