using System;
using UnityEngine;
using YLBasic;

[Serializable]
public struct ItemProperty
{
  public string Id;
  public string Name;
  public string Desc;
  public string EquipType;
  public string SpritePath;
  public string Effect;
}


[CreateAssetMenu(fileName = "InventoryDataset", menuName = "InventoryDataset", order = 2)]
public class CustomInventoryDataset : InventoryDataset<ItemProperty>
{
  public override ItemProperty GetItemProperty(string Id)
  {
    return Sheet1.Find(item => item.Id == Id);
  }
}