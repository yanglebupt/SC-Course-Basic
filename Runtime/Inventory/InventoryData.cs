using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public abstract class InventoryData<D> : ScriptableObject
  {
    public List<D> Sheet1;

    public abstract string GetIdentify { get; }
  }
}