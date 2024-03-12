using System;
using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  [Serializable]
  public class MyPlaneMesh
  {
    private Vector3 startPoint = Vector3.zero;

    [Tooltip("宽度多少个正/长方形")]
    public int width = 10;
    [Tooltip("高度多少个正/长方形")]
    public int height = 10;
    [Tooltip("每个正/长方形的宽度多少")]
    public float widthScale = 1f;
    [Tooltip("每个正/长方形的高度多少")]
    public float heightScale = 1f;

    [Tooltip("y 是否使用噪声进行浮起")]
    public bool useNoise = true;

    [Tooltip("y 噪声缩放大小")]
    public float noiseScale = 1f;

    /// <summary>
    /// Mesh 包含
    /// vertices  顶点数组 Vector3[]
    /// normals  法线数组  Vector3[]
    /// triangles  三角面片数组  int[]
    /// uv  纹理 uv  Vector2[]
    /// 点从宽度开始，索引为 S 形，初始点为原点，平面在 x(width)-z(height) 
    /// 共用点，重合的点不声明多份
    /// 索引为 S 形 搞复杂了，可以直接二维数组存
    /// </summary>
    /// <returns></returns>
    public Mesh GetMesh()
    {
      Mesh mesh = new Mesh();
      List<Vector3> vertices = new List<Vector3>();
      List<Vector2> uv0 = new List<Vector2>();
      List<Vector3> norm = new List<Vector3>();
      List<int> triangles = new List<int>();
      for (int i = 0; i < height + 1; i++)
      {
        float w = i % 2 == 0 ? 0 : widthScale * width;
        float h = i * heightScale;
        float v = i % 2 == 0 ? 1 : -1;
        for (int j = 0; j < width + 1; j++)
        {
          Vector2 uv = new Vector2(1.0f * (i % 2 == 0 ? (width - j) : j) / (width + 1), 1.0f * i / (height + 1));
          Vector3 normal = Vector3.up;
          uv0.Add(uv);
          norm.Add(normal);
          vertices.Add(startPoint + new Vector3(w + v * j * widthScale, useNoise ? noiseScale * GetDisplacement(uv) : 0, h));
        }
        if (i > 0)
        {
          // 可以边添加三角形
          int topIndex = (i - 1) / 2 * 2 * (width + 1) + (i - 1) % 2 * (2 * (width + 1) - 1);
          int bottomIndex = topIndex + (i % 2 == 0 ? 1 : (2 * (width + 1) - 1));
          int topGasp = -1 * (int)v * 1;
          for (int j = 0; j < width; j++)
          {
            triangles.Add(topIndex + j * topGasp);
            triangles.Add(bottomIndex + j * -1 * topGasp);
            triangles.Add(bottomIndex + (j + 1) * -1 * topGasp);

            triangles.Add(bottomIndex + (j + 1) * -1 * topGasp);
            triangles.Add(topIndex + (j + 1) * topGasp);
            triangles.Add(topIndex + j * topGasp);
          }
        }
      }
      mesh.name = "MyPlane";
      mesh.vertices = vertices.ToArray();
      mesh.triangles = triangles.ToArray();
      mesh.normals = norm.ToArray();
      mesh.uv = uv0.ToArray();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
      mesh.RecalculateBounds();
      return mesh;
    }

    float GetDisplacement(float x, float y)
    {
      float p1 = Mathf.PerlinNoise(x, y);
      float p2 = Mathf.PerlinNoise(x * 1000, y * 200);
      float p = Mathf.Lerp(p1, p2, 0.5f);
      p *= 2;
      p = Mathf.Pow(p, 3);
      return p;
    }
    float GetDisplacement(Vector2 uv)
    {
      float x = uv.x, y = uv.y;
      return GetDisplacement(x, y);
    }
  }

}

