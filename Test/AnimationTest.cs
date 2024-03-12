using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YLBasic
{
  public class AnimationTest : MonoBehaviour
  {
    public Vector3 start;
    public Vector3 end;

    [SerializeField]
    public AnimationOptions opt;

    // 两种动画类型
    private LAnimation move;
    private string aniId;

    void Start()
    {
      // 方式一Update的方法
      move = new LAnimation((t) =>
      {
        transform.position = Vector3.Lerp(start, end, t);
      }, opt);

      // 方式二协程的方法
      aniId = LAnimationCoroutine.MakeAnimation((t) =>
      {
        transform.position = Vector3.Lerp(start, end, t);
      }, opt);
    }

    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.A))
      {
        // move.RePlay();
        LAnimationCoroutine.RePlayC(aniId);
      }
      if (Input.GetKeyDown(KeyCode.P))
      {
        // move.Pause();
        LAnimationCoroutine.PauseC(aniId);
      }
      if (Input.GetKeyDown(KeyCode.R))
      {
        // move.Resume();
        LAnimationCoroutine.ResumeC(aniId);
      }
      if (Input.GetKeyDown(KeyCode.S))
      {
        // move.Stop();
        LAnimationCoroutine.StopC(aniId);
        // LAnimationCoroutine.RemoveC(aniId);
      }
    }

    private void FixedUpdate()
    {
      move.Update(Time.fixedDeltaTime);
    }
  }

}