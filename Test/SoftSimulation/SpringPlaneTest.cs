using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public class SpringPlaneTest : MonoBehaviour
  {
    [SerializeField]
    public MyPlaneMesh myPlaneMesh;

    private float step;
    public float k = 100;
    public float mass = 1f;
    public float damping = 0.1f;

    private MeshFilter meshFilter;
    private Mesh mesh;
    private int pointCount;

    private GameObject sphere;
    private Transform[] allSphere;

    private Vector3[] speedList;
    void Start()
    {
      meshFilter = GetComponent<MeshFilter>();
      mesh = myPlaneMesh.GetMesh();
      pointCount = mesh.vertices.Length;
      meshFilter.mesh = mesh;
      sphere = transform.GetChild(0).gameObject;
      sphere.SetActive(false);
      allSphere = new Transform[pointCount];
      speedList = new Vector3[pointCount];
      initPoint();
      step = Mathf.Min(myPlaneMesh.heightScale, myPlaneMesh.widthScale);
    }

    void Update()
    {
      Point2PlaneRender();
    }

    void FixedUpdate()
    {
      int index = 0;
      for (int i = 0; i < myPlaneMesh.height + 1; i++)
      {
        for (int j = 0; j < myPlaneMesh.width + 1; j++)
        {
          Vector3 force = Vector3.zero;
          int bp = 1 + 2 * (i % 2 == 0 ? j : (myPlaneMesh.width - j));
          int tp = 2 * (myPlaneMesh.width + 1) - bp;  // 注意点是 S 形的
          if (j > 0 && i > 0 && j < myPlaneMesh.width && i < myPlaneMesh.height)
          {
            // 受到左弹簧的力
            force += Spring.GetForce(allSphere[index - 1], allSphere[index], step, k);
            // 受到右弹簧的力
            force += Spring.GetForce(allSphere[index + 1], allSphere[index], step, k);
            // 受到上弹簧的力
            force += Spring.GetForce(allSphere[index - (i % 2 == 0 ? bp : tp)], allSphere[index], step, k);
            // 受到下弹簧的力
            force += Spring.GetForce(allSphere[index + (i % 2 == 0 ? tp : bp)], allSphere[index], step, k);
          }
          else if (i == 0)
          {
            // 受到下弹簧的力
            force += Spring.GetForce(allSphere[index + (i % 2 == 0 ? tp : bp)], allSphere[index], step, k);
            if (j == 0)
            {
              // 受到右弹簧的力
              force += Spring.GetForce(allSphere[index + 1], allSphere[index], step, k);
            }
            else if (j == myPlaneMesh.width)
            {
              // 受到左弹簧的力
              force += Spring.GetForce(allSphere[index - 1], allSphere[index], step, k);
            }
            else
            {
              // 受到左弹簧的力
              force += Spring.GetForce(allSphere[index - 1], allSphere[index], step, k);
              // 受到右弹簧的力
              force += Spring.GetForce(allSphere[index + 1], allSphere[index], step, k);
            }
          }
          else if (i == myPlaneMesh.height)
          {
            // 受到上弹簧的力
            force += Spring.GetForce(allSphere[index - (i % 2 == 0 ? bp : tp)], allSphere[index], step, k);
            if (j == 0)
            {
              // 受到右弹簧的力
              force += Spring.GetForce(allSphere[index + 1], allSphere[index], step, k);
            }
            else if (j == myPlaneMesh.width)
            {
              // 受到左弹簧的力
              force += Spring.GetForce(allSphere[index - 1], allSphere[index], step, k);
            }
            else
            {
              // 受到左弹簧的力
              force += Spring.GetForce(allSphere[index - 1], allSphere[index], step, k);
              // 受到右弹簧的力
              force += Spring.GetForce(allSphere[index + 1], allSphere[index], step, k);
            }
          }
          else if (j == 0)
          {
            // 受到上弹簧的力
            force += Spring.GetForce(allSphere[index - (i % 2 == 0 ? bp : tp)], allSphere[index], step, k);
            // 受到下弹簧的力
            force += Spring.GetForce(allSphere[index + (i % 2 == 0 ? tp : bp)], allSphere[index], step, k);
            // 受到右弹簧的力
            force += Spring.GetForce(allSphere[index + 1], allSphere[index], step, k);
          }
          else if (j == myPlaneMesh.width)
          {
            // 受到上弹簧的力
            force += Spring.GetForce(allSphere[index - (i % 2 == 0 ? bp : tp)], allSphere[index], step, k);
            // 受到下弹簧的力
            force += Spring.GetForce(allSphere[index + (i % 2 == 0 ? tp : bp)], allSphere[index], step, k);
            // 受到左弹簧的力
            force += Spring.GetForce(allSphere[index - 1], allSphere[index], step, k);
          }
          Spring.CalcSpeed(ref speedList[index], force, mass, Time.fixedDeltaTime, damping);
          allSphere[index].transform.position += speedList[index] * Time.fixedDeltaTime;
          index++;
        }
      }
    }


    void Point2PlaneRender()
    {
      Vector3[] points = new Vector3[pointCount];
      for (int i = 0; i < pointCount; i++)
      {
        points[i] = allSphere[i].localPosition;
      }
      mesh.vertices = points;
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
    }

    void initPoint()
    {
      for (int i = 0; i < pointCount; i++)
      {
        Vector3 ps = mesh.vertices[i];
        GameObject s = Instantiate(sphere, transform);
        s.SetActive(true);
        s.transform.localPosition = ps;
        allSphere[i] = s.transform;
      }
    }
  }

}

