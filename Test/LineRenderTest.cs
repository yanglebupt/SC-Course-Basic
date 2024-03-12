using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public class LineRenderTest : MonoBehaviour
  {
    public int maxLinePoints = 50;
    public float minPointDistance = 0.5f;
    private Queue<Vector3> linePoints;
    private Vector3 previousPoint;
    private int curPointNums;
    private LineRenderer lineRenderer;
    private PolygonCollider2D polygonCollider2D;
    void Start()
    {
      linePoints = new Queue<Vector3>(maxLinePoints);
      lineRenderer = GetComponent<LineRenderer>();
      polygonCollider2D = GetComponent<PolygonCollider2D>();
      curPointNums = 0;
      previousPoint = new Vector3(0, 0, 0);
    }

    void Update()
    {
      // 1. 收集鼠标点，只保留50个点
      if (Input.GetMouseButton(0))
      {
        Vector3 pos = Tools.ScreenToWorldPoint(Input.mousePosition, 1);
        pos.z = 0;
        AddPoint(pos);
      }
      UpdateLineRenderPoints();  // 2. 将点给 LineRender 渲染
      UpdatePolygonColliderPath();  // 3. 生成贴合的碰撞盒
    }

    void UpdatePolygonColliderPath()
    {
      Vector3[] points = linePoints.ToArray();
      int size = points.Length;
      List<Vector2> colliderPoints = new List<Vector2>();

      for (int i = 1; i < size; i++)
      {
        float width = lineRenderer.widthCurve.Evaluate(i / (float)size);  // 注意点是位于宽度中心的
                                                                          // 获取当前点的法向
        Vector3 t = points[i - 1] - points[i];
        Vector3 n = Vector3.Cross(t, Vector3.forward).normalized;

        if (i == 1)
        {
          Vector2 lp = points[i - 1] - width * 0.5f * n;
          Vector2 rp = points[i - 1] + width * 0.5f * n;
          colliderPoints.Insert(0, lp);
          colliderPoints.Add(rp);
        }
        Vector2 leftPoint = points[i] - width * 0.5f * n;
        Vector2 rightPoint = points[i] + width * 0.5f * n;
        colliderPoints.Insert(0, leftPoint);
        colliderPoints.Add(rightPoint);
      }
      if (size < 2) return;
      polygonCollider2D.SetPath(0, colliderPoints.ToArray());
    }

    void UpdateLineRenderPoints()
    {
      Vector3[] points = linePoints.ToArray();
      lineRenderer.positionCount = points.Length;
      lineRenderer.SetPositions(points);
    }

    void AddPoint(Vector3 pos, bool enabled = true)
    {
      if (enabled)
      {
        linePoints.Enqueue(pos);
        curPointNums++;
        previousPoint = pos;
      }
      else
      {
        AddPoint(pos);
      }
    }

    void AddPoint(Vector3 pos)
    {
      if (curPointNums == 0)
      {
        // 第一个点，直接添加
        AddPoint(pos, true);
        return;
      }
      // 移动大于最小距离才添加
      if (Vector3.Distance(previousPoint, pos) > minPointDistance)
      {
        if (curPointNums >= maxLinePoints)  // 如果满了，挤出第一个，再添加
        {
          linePoints.Dequeue();
        }
        AddPoint(pos, true);
      }
    }
  }
}

