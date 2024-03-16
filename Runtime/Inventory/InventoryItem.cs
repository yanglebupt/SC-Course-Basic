using UnityEngine;


namespace YLBasic
{
  public abstract class InventoryItem<P, D> : AnimationUI
  {
    [HideInInspector]
    public bool IsInventoryItem = true;
    public abstract void Init(P itemProperty, D itemData);
    // 这两个方法是用于在生成的物体上
    public abstract void OnBeginDrag();
    public abstract void OnEndDrag();
    // 这个方法是作用在原始物体上
    public abstract void OnAttached(); // 装载
    public abstract void DisAttached(); // 卸载
  }
}