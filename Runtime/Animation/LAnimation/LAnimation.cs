using System;
using UnityEngine;

namespace YLBasic
{
  public class LAnimation : LAnimationState
  {
    private Action<float> _action;
    public override void RePlay(bool reset = true)
    {
      base.RePlay(reset);
      if (reset)
      {
        _action(LCurveSampler.Evaluate(t, options.curve));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action">onUpdate</param>
    /// <param name="_options">包含 onCompleted</param>
    public LAnimation(Action<float> action, AnimationOptions _options) : base(_options)
    {
      _action = action;
    }

    // 由 Unity 引擎调用
    public void Update(float deltaTime)
    {
      // 尝试迭代
      if (!Stoped && !Paused && !Completed)
      {
        elapsed_time += deltaTime;
        elapsed_time_in_iter += deltaTime;
        if (elapsed_time_in_iter < options.delay)
        {
          if (!(iter_count > 1 && !options.delayInIterations))
          {
            options.onWaitDelay?.Invoke();
            options.onWaitDelayParam?.Invoke(Deconstruction());
            return;
          }
        }
        bool arriveEnd = ArriveEnd(ani_direction, ref t);
        _action(LCurveSampler.Evaluate(t, options.curve));
        // 执行帧
        options.onUpdate?.Invoke();
        options.onUpdateParam?.Invoke(Deconstruction());
        TryPerformFrameAction(ani_direction, t);
        if (arriveEnd)
        {
          if (options.iterations == 1 || options.iterations == 0 || options.iterations == iter_count)
          {
            if (options.fill == AnimationFill.BACKWARDS)
            {
              // 重置采样点
              t = Mathf.Abs(t - 1);
              _action(LCurveSampler.Evaluate(t, options.curve));
            }
            Completed = true;
            options.onCompleted?.Invoke();
            options.onCompletedParam?.Invoke(Deconstruction());
            return;
          }
          iter_count++;
          InitFrameActionPerformed();
          if (options.direction != AnimationDirection.ALTERNATE)
          {
            // 重置采样点
            t = Mathf.Abs(t - 1);
            _action(LCurveSampler.Evaluate(t, options.curve));
          }
          elapsed_time_in_iter = 0;
        }
        // 更新下一帧
        ani_direction = positive_direction;
        if (options.direction == AnimationDirection.ALTERNATE)
        {
          ani_direction = iter_count % 2 == 1 ? positive_direction : -1 * positive_direction;
        }
        t += ani_direction * deltaTime * delta_t;
      }
    }
  }

}


