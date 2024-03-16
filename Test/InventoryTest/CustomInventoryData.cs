using System;
using UnityEngine;
using YLBasic;


[Serializable]
public struct ItemData
{
  public string Id;
  public int count;
}

[CreateAssetMenu(fileName = "InventoryData", menuName = "InventoryData", order = 3)]
public class CustomInventoryData : InventoryData<ItemData>
{
  public override string GetIdentify { get => "Id"; }
}