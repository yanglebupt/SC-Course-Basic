# Tools

静态工具类，提供了一系列方法

## ScreenToWorldPoint()

屏幕坐标转世界坐标，并可以调整 `z` 值

```csharp
Vector3 ScreenToWorldPoint(Vector2 screenPosition, float z = 1)
```

## WorldToScreenPoint()

世界坐标转屏幕坐标

```csharp
Vector2 WorldToScreenPoint(Vector3 worldPosition)
```

## GetRectSize()

获取组件 `RectTransform` 的尺寸

```csharp
Vector2 GetRectSize(RectTransform rectTransform)
```

## GetRectCorners()

获取组件 `RectTransform` 的四个角坐标

```csharp
Vector3[] GetRectCorners(RectTransform rectTransform)
```

## GetRectBounds()

获取组件 `RectTransform` 的四个边界

```csharp
(float left, float top, float right, float bottom) GetRectBounds(RectTransform rectTransform)
(float left, float top, float right, float bottom) GetRectBounds(Vector3[] corners)
```

## RectIntersect()

判断两个 `RectTransform` 组件是否相交

```csharp
bool RectIntersect(RectTransform src, RectTransform tar)
```

## RemoveAllChildren()

清除所有子物体

```csharp
void RemoveAllChildren(GameObject parent)
```