using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YLBasic
{
  public class SpringTest : MonoBehaviour
  {
    [Tooltip("质点位置")]
    public Transform massTransform;

    [Tooltip("质点质量")]
    public float mass = 1f;

    [Tooltip("胡克系数")]
    public float k = 1.5f;

    [Tooltip("阻力系数")]
    public float damping = 0.1f;

    [Tooltip("空闲的长度")]
    public float reset = 1f;

    private Vector3 speed = Vector3.zero;

    void FixedUpdate()
    {
      Vector3 force = Spring.GetForce(transform, massTransform, reset, k);
      Spring.CalcSpeed(ref speed, force, mass, Time.fixedDeltaTime, damping);
      massTransform.position += speed * Time.fixedDeltaTime;
    }
  }

}
