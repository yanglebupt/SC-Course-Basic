using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YLBasic;

public class CustomInventoryItem : InventoryItem<ItemProperty, ItemData>, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
  public CanvasGroup descPlane; // 描述面板
  public Image bgImage;  // 背景图片
  public Image itemImage; // 物品图片
  public TMP_Text count; // 个数
  public Color bgHoverColor; // hover 的背景颜色
  private ItemProperty itemProperty; // 物品属性
  private ItemData itemData; // 物品的动态属性，例如个数，等级

  private bool isOpenDesc = false;
  private bool locked = false;
  private string bg_name;
  private string item_image_name;
  private Color bgColor;

  private void Awake()
  {
    descPlane.alpha = 0;
    descPlane.gameObject.SetActive(false);

    bg_name = bgImage.name;
    item_image_name = itemImage.name;

    bgColor = bgImage.color;
  }

  public override void Init(ItemProperty itemProperty, ItemData itemData)
  {
    this.itemProperty = itemProperty;
    this.itemData = itemData;
    itemImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(itemProperty.SpritePath);
    descPlane.GetComponentInChildren<TMP_Text>().text = itemProperty.Desc;
    count.text = itemData.count + "";
  }

  public void ShowDescPanel()
  {
    TransitionColorTo(bgImage, bgHoverColor);
    descPlane.gameObject.SetActive(true);
    TransitionAlphaTo(descPlane, 1);
    isOpenDesc = true;
  }

  public void CloseDescPanel()
  {
    TransitionColorTo(bgImage, bgColor);
    isOpenDesc = false;
    TransitionAlphaTo(descPlane, 0, () =>
    {
      descPlane.gameObject.SetActive(false);
    });
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (locked) return;
    if (eventData.pointerEnter.name == bg_name || eventData.pointerEnter.name == item_image_name)
      ShowDescPanel();
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (locked) return;
    CloseDescPanel();
  }

  public void OnPointerMove(PointerEventData eventData)
  {
    if (locked) return;
    if ((eventData.pointerEnter.name == bg_name || eventData.pointerEnter.name == item_image_name) && !isOpenDesc)
      ShowDescPanel();
  }

  public override void OnBeginDrag()
  {
    locked = true;
    descPlane.gameObject.SetActive(false);
    descPlane.alpha = 0;
    Destroy(count);
    isOpenDesc = false;
    GetComponent<LayoutElement>().ignoreLayout = true;
  }

  public override void OnEndDrag()
  {
    locked = false;
    GetComponent<LayoutElement>().ignoreLayout = false;
  }

  public override void OnAttached()
  {
    itemData.count -= 1;
    count.text = itemData.count + "";
  }

  public override void DisAttached()
  {
    itemData.count += 1;
    count.text = itemData.count + "";
  }
}
