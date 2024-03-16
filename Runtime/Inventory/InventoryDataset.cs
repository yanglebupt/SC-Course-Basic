using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public abstract class InventoryDataset<P> : ScriptableObject
  {
    public List<P> Sheet1;

    public abstract P GetItemProperty(string Id);
  }
}