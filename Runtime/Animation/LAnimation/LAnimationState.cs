using System;
using System.Collections.Generic;

namespace YLBasic
{
  public struct LAnimationStateStruct
  {
    public AnimationOptions options;

    public bool Paused;
    public bool Stoped;
    public bool Completed;
    public int iter_count;
    public float t;
  }

  /// <summary>
  /// 抽象出一个属性类，因为 LAnimationCoroutine 也需要使用
  /// </summary>
  public class LAnimationState
  {
    public string id;
    // 动画配置
    public AnimationOptions options;
    // 动画流逝的总时间
    protected float elapsed_time = 0;
    // 当前在第几次循环动画
    protected int iter_count = 1;
    // 当前循环流逝的总时间
    protected float elapsed_time_in_iter = 0;
    // 当前采样点
    protected float t;
    // 正方向
    protected float positive_direction;
    // 当前循环的方向
    protected float ani_direction;
    // 每秒增加多少采样比例 1/duration
    protected float delta_t;
    // 是否已经执行了 帧函数
    protected Dictionary<float, bool> frameActionPerformedDict = new Dictionary<float, bool>();

    public bool Paused = true;
    public bool Stoped = true;
    public bool Completed = false;

    public LAnimationStateStruct Deconstruction()
    {
      return new LAnimationStateStruct()
      {
        options = options,
        Paused = Paused,
        Stoped = Stoped,
        Completed = Completed,
        iter_count = iter_count,
        t = t,
      };
    }

    public virtual void Play()
    {
      Paused = false;
      Stoped = false;
    }
    public virtual void RePlay(bool reset = true)
    {
      Paused = false;
      Stoped = false;
      Completed = false;
      if (reset)
      {
        InitAnimationState();
      }
    }
    public virtual void Stop()
    {
      Stoped = true;
      InitAnimationState();
    }
    public virtual void Resume()
    {
      Paused = false;
      Stoped = false;
    }
    public virtual void Pause()
    {
      Paused = true;
      Stoped = false;
    }

    public virtual void Over()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action">onUpdate</param>
    /// <param name="_options">包含 onCompleted</param>
    public LAnimationState(AnimationOptions _options)
    {
      options = _options;
      delta_t = 1.0f / options.duration;
      InitAnimationState();
    }

    public virtual void InitAnimationState()
    {
      t = options.direction == AnimationDirection.REVERSE ? 1 : 0;
      positive_direction = ani_direction = options.direction == AnimationDirection.REVERSE ? -1 : 1;
      elapsed_time = 0;
      iter_count = 1;
      elapsed_time_in_iter = 0;
      InitFrameActionPerformed();
    }

    public virtual void InitFrameActionPerformed()
    {
      if (options.onFrameDict == null) return;
      foreach (KeyValuePair<float, Action> item in options.onFrameDict)
      {
        if (frameActionPerformedDict.ContainsKey(item.Key))
        {
          frameActionPerformedDict[item.Key] = false;
        }
        else
        {
          frameActionPerformedDict.Add(item.Key, false);
        }
      };
      if (options.onFrameDictParam == null) return;
      foreach (KeyValuePair<float, Action<LAnimationStateStruct>> item in options.onFrameDictParam)
      {
        if (frameActionPerformedDict.ContainsKey(item.Key))
        {
          frameActionPerformedDict[item.Key] = false;
        }
        else
        {
          frameActionPerformedDict.Add(item.Key, false);
        }
      }
    }

    public virtual void TryPerformFrameAction(float ani_direction, float t)
    {
      if (options.onFrameDict == null) return;
      foreach (KeyValuePair<float, Action> item in options.onFrameDict)
      {
        if (frameActionPerformedDict[item.Key]) continue;
        float frameRate = item.Key;
        if ((ani_direction > 0 && t >= frameRate) || (ani_direction < 0 && t <= 1 - frameRate))
        {
          options.onFrameDict[frameRate]?.Invoke();
          options.onFrameDictParam[frameRate]?.Invoke(Deconstruction());
          frameActionPerformedDict[frameRate] = true;
        }
      };
      if (options.onFrameDictParam == null) return;
      foreach (KeyValuePair<float, Action<LAnimationStateStruct>> item in options.onFrameDictParam)
      {
        if (frameActionPerformedDict[item.Key]) continue;
        float frameRate = item.Key;
        if ((ani_direction > 0 && t >= frameRate) || (ani_direction < 0 && t <= 1 - frameRate))
        {
          options.onFrameDictParam[frameRate]?.Invoke(Deconstruction());
          frameActionPerformedDict[frameRate] = true;
        }
      };
    }

    public virtual bool ArriveEnd(float direction, ref float t)
    {
      if (direction > 0 && t >= 1)
      {
        t = 1;
        return true;
      }
      else if (direction < 0 && t <= 0)
      {
        t = 0;
        return true;
      }
      return false;
    }
  }

  [Serializable]
  public enum AnimationDirection
  {
    // 前向动画
    NORMAL,
    // 反向动画
    REVERSE,
    // 每次都会改变方向
    ALTERNATE
  }

  [Serializable]
  public enum AnimationFill
  {
    // 保留动画结束
    FORWARDS,
    // 动画结束回到初始
    BACKWARDS,
  }

  [Serializable]
  public struct AnimationOptions
  {
    // 动画时长
    public float duration;
    // 动画延迟
    public float delay;
    public bool delayInIterations;
    // 动画方向
    public AnimationDirection direction;
    // 动画曲线
    public LCurveType curve;
    // 动画结束是否保留
    public AnimationFill fill;

    // -1 代表循环播放
    public int iterations;
    // 动画结束删除
    public bool removeOnCompleted;

    // 动画事件
    public Action onCompleted;
    public Action onWaitDelay;
    public Action onUpdate;
    public Dictionary<float, Action> onFrameDict;

    public Action<LAnimationStateStruct> onCompletedParam;
    public Action<LAnimationStateStruct> onUpdateParam;
    public Action<LAnimationStateStruct> onWaitDelayParam;
    public Dictionary<float, Action<LAnimationStateStruct>> onFrameDictParam;

  }
}

