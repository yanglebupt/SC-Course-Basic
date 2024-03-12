using UnityEngine;

namespace YLBasic
{
  public static class Tools
  {
    public static Vector3 ScreenToWorldPoint(Vector2 screenPosition, float z = 1)
    {
      Vector3 pos = Camera.main.WorldToScreenPoint(new Vector3(0, 0, z));
      Vector3 wpos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, pos.z));
      wpos.z = z;
      return wpos;
    }
    public static Vector2 WorldToScreenPoint(Vector3 worldPosition)
    {
      return Camera.main.WorldToScreenPoint(worldPosition);
    }

    /// <summary>
    /// PosX  PosY 都是 pivot 相对于 anchor 的定位 (当 anchor 重合至两个点)
    /// Left Top Right Bottom  都是 rect 的边界 相对于 四个 anchor 的定位，和 pivot 无关了
    /// https://zhuanlan.zhihu.com/p/413446225  https://blog.csdn.net/qq_38913715/article/details/123049180  https://zhuanlan.zhihu.com/p/642327972
    /// rect 中的属性 是相对于 pivot 的位置
    /// sizeDelta 是相对于 anchor 的大小，而 anchor 是相对于父元素归一化 0-1 的
    /// sizeDelta = rect.size - parent.rect.size * anchor
    /// 
    /// position localPosition 是以 pivot 为中心计算的屏幕坐标
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <returns></returns>
    public static Vector2 GetRectSize(RectTransform rectTransform)
    {
      // Debug.Log(rectTransform.rect.size);
      // Vector2 anchorSize = rectTransform.anchorMax - rectTransform.anchorMin;
      // Vector2 s = rectTransform.sizeDelta + (rectTransform.parent.transform as RectTransform).rect.size * anchorSize;
      // Debug.Log(s);
      // Debug.Log(rectTransform.sizeDelta);
      // Debug.Log(rectTransform.position);
      // Debug.Log(rectTransform.localPosition);

      // Vector3[] edges = new Vector3[4];
      // rectTransform.GetWorldCorners(edges);  // 左下角开始，顺时针
      // for (int i = 0; i < 4; i++)
      // {
      //   Debug.Log(edges[i]);
      // }
      return rectTransform.rect.size;
    }

    public static Vector3[] GetRectCorners(RectTransform rectTransform)
    {
      Vector3[] corners = new Vector3[4];
      rectTransform.GetWorldCorners(corners);
      return corners;
    }

    public static (float left, float top, float right, float bottom) GetRectBounds(RectTransform rectTransform)
    {
      Vector3[] corners = new Vector3[4];
      rectTransform.GetWorldCorners(corners);
      return GetRectBounds(corners);
    }
    public static (float left, float top, float right, float bottom) GetRectBounds(Vector3[] corners)
    {
      if (corners.Length != 4)
      {
        throw new System.Exception("Invalid number of corners");
      }
      Vector3 leftBottom = corners[0];
      Vector3 rightTop = corners[2];
      float left = leftBottom.x, bottom = leftBottom.y;
      float right = rightTop.x, top = rightTop.y;
      return (left, top, right, bottom);
    }

    public static bool RectIntersect(RectTransform src, RectTransform tar)
    {
      (float left, float top, float right, float bottom) = Tools.GetRectBounds(src);
      (float t_l, float t_t, float t_r, float t_b) = Tools.GetRectBounds(tar);
      bool notIntersect = right < t_l || bottom > t_t || left > t_r || top < t_b;
      return !notIntersect;
    }

    public static void RemoveAllChildren(GameObject parent)
    {
      Transform transform;
      for (int i = 0; i < parent.transform.childCount; i++)
      {
        transform = parent.transform.GetChild(i);
        GameObject.Destroy(transform.gameObject);
      }
    }
  }
}