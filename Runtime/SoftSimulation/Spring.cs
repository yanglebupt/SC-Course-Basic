using UnityEngine;

namespace YLBasic
{
  /// <summary>
  /// 模拟质点之间的弹簧，也就是胡克定律
  /// </summary>
  public class Spring
  {
    /// <summary>
    /// 获取两点之间由于弹簧拉伸产生的力
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="resetLength"></param>
    /// <param name="k"></param>
    /// <returns>返回target的受力，source的受力相反</returns>
    public static Vector3 GetForce(Transform source, Transform target, float resetLength, float k)
    {
      Vector3 dir = source.position - target.position;
      float currentLength = dir.magnitude;
      Vector3 force = k * (currentLength - resetLength) * dir.normalized;
      return force;
    }

    public static void CalcSpeed(ref Vector3 origin, Vector3 force, float mass, float time, float damping)
    {
      Vector3 acceleration = force / mass;
      Vector3 speed = acceleration * time;
      origin += speed;
      origin *= 1 - damping;
    }
  }
}

