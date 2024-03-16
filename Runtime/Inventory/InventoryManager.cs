using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YLBasic
{
  public class InventoryManager : MonoBehaviour
  {
    public ScriptableObject itemPropertyDataset;
    public ScriptableObject itemDataDataset;
    public GameObject itemPrefab;
    public QuerySelection querySelection;
    public GameObject root;
    public List<RectTransform> equipmentSlots;
    private Dictionary<string, Component> Id2Cpn = new Dictionary<string, Component>();

    private void Awake()
    {
      foreach (RectTransform item in equipmentSlots)
      {
        if (item.GetComponent<LDragAttach>() == null)
        {
          LDragAttach dragAttach = item.gameObject.AddComponent<LDragAttach>();
          dragAttach.OnAttachedParams.AddListener((GameObject draggable) =>
          {
            draggable.name = $"{draggable.name}-Equipped";
            (draggable.transform as RectTransform).sizeDelta = (draggable.transform.Find("BG") as RectTransform).sizeDelta = YLBasic.Tools.GetRectSize(item.transform as RectTransform);
          });
        }
      }
    }
    void Start()
    {
      var itemDataList = Reflect.get(itemDataDataset, "Sheet1");
      string identifyName = Reflect.get(itemDataDataset, "GetIdentify") as string;

      foreach (var itemData in itemDataList as IEnumerable)
      {
        string id = Reflect.get(itemData, identifyName) as string;
        var itemProperty = Reflect.apply(itemPropertyDataset, "GetItemProperty", new object[] { id });
        GameObject itemObj = Instantiate(itemPrefab, root.transform);
        Component cpn = GetInventoryItemComponent(itemObj);
        // itemObj.name = id;
        itemObj.name = Reflect.get(itemProperty, "Name") as string;
        AddDrag(id, itemObj, cpn);
        Id2Cpn.Add(id, cpn);
        Reflect.apply(cpn, "Init", new object[] { itemProperty, itemData });
      }
    }

    public Component GetInventoryItemComponent(GameObject obj)
    {
      foreach (Component cpn in obj.GetComponents<Component>())
      {
        if (Reflect.has(cpn, "IsInventoryItem"))
        {
          return cpn;
        }
      }
      return null;
    }

    // 在这个方法中控制二行拖动有关的
    public void AddDrag(string id, GameObject obj, Component cpn)
    {
      LDragSpawn spawn = obj.AddComponent<LDragSpawn>();
      spawn.useSelf = true;
      spawn.constraint = transform.parent as RectTransform;

      // 注册方法
      spawn.initDraggableGameObject.AddListener((GameObject newObj) =>
      {
        newObj.name = obj.name;
        Component newCpn = GetInventoryItemComponent(newObj);
        Reflect.apply(newCpn, "OnBeginDrag");
        LDraggable draggable = newObj.GetComponent<LDraggable>();
        draggable.data = new { id };
        draggable.onEndDragIsAttached.AddListener((GameObject attached) =>
        {
          Reflect.apply(newCpn, "OnEndDrag");
          if (attached != null)
          {
            Reflect.apply(cpn, "OnAttached");
          }
        });
        draggable.UpdateDragAttachList(equipmentSlots);
      });

      spawn.onEndDragAttachedOverlap.AddListener((GameObject existed, GameObject newIn, GameObject attach) =>
      {
        querySelection.OnSlected.RemoveAllListeners(); // 先清除旧的监听
        querySelection.OnSlected.AddListener((bool yes) =>
        {
          // Yes newIn 直接 attach 就行，existed + 1
          // No existed 直接 attach 就行，newIn + 1，还有销毁 newIn
          LDraggable draggable = (yes ? existed : newIn).GetComponent<LDraggable>();
          Component cpn = Id2Cpn[Reflect.get(draggable.data, "id") as string];
          Reflect.apply(cpn, "DisAttached");
          (yes ? newIn : existed).GetComponent<LDraggable>().AttachTo(attach.transform as RectTransform);
          if (!yes)
            Destroy(newIn);
        });
        querySelection.UpdateContentText((string template) =>
        {
          return string.Format(template, new object[] { newIn.name, existed.name });
        });
        querySelection.Open();
      });
    }
  }
}
