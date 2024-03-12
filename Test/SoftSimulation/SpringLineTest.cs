using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public class SpringLineTest : MonoBehaviour
  {
    public float step = 2;  // 可以看成弹簧初始长度
    public int pointCount = 10;

    public float k = 100;
    public float mass = 1f;
    public float damping = 0.1f;
    private LineRenderer lineRenderer;
    private GameObject sphere;
    private Transform[] allSphere;

    private Vector3[] speedList;
    void Start()
    {
      lineRenderer = GetComponent<LineRenderer>();
      sphere = transform.GetChild(0).gameObject;
      sphere.SetActive(false);
      allSphere = new Transform[pointCount];
      speedList = new Vector3[pointCount];
      initPoint();
    }

    void Update()
    {
      Point2LineRender();
    }

    void FixedUpdate()
    {
      for (int i = 0; i < pointCount; i++)
      {
        Vector3 force = Vector3.zero;
        if (i > 0)
        {
          // 受到左边弹簧的力
          force += Spring.GetForce(allSphere[i - 1], allSphere[i], step, k);
        }
        if (i < pointCount - 1)
        {
          // 受到右边弹簧的力
          force += Spring.GetForce(allSphere[i + 1], allSphere[i], step, k);
        }
        Spring.CalcSpeed(ref speedList[i], force, mass, Time.fixedDeltaTime, damping);
        allSphere[i].transform.position += speedList[i] * Time.fixedDeltaTime;
      }
    }

    void Point2LineRender()
    {
      lineRenderer.positionCount = pointCount;
      Vector3[] points = new Vector3[pointCount];
      for (int i = 0; i < pointCount; i++)
      {
        points[i] = allSphere[i].localPosition;
      }
      lineRenderer.SetPositions(points);
    }

    void initPoint()
    {
      float left = -1 * (pointCount / 2) * step;
      for (int i = 0; i < pointCount; i++)
      {
        Vector3 ps = new Vector3(left + i * step, 0, 0);
        GameObject s = Instantiate(sphere, transform);
        s.SetActive(true);
        s.transform.localPosition = ps;
        allSphere[i] = s.transform;
      }
    }
  }
}
