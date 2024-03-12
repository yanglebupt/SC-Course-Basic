using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YLBasic
{
  public class AnimationAndCoroutine
  {
    public string id;
    public LAnimation animationState;
    public Coroutine coroutine;
    public IEnumerator animation;
  }

  // 再提供一个 static 协程的方式
  public static class LAnimationCoroutine
  {
    public static readonly Dictionary<string, AnimationAndCoroutine> CoroutineMap = new Dictionary<string, AnimationAndCoroutine>();

#nullable enable
    public static IEnumerator Animation(LAnimation animationState, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      while (!animationState.Completed) // Completed 结束了协程也结束了
      {
        animationState.Update(Time.fixedDeltaTime);
        yield return new WaitForFixedUpdate(); // 使用 Unity 自带的，那个必须在 StartCoroutine 中调用
      }
      if (animationState.options.removeOnCompleted)
      {
        Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
        CoroutineMap.Remove(animationState.id);
      }
    }

    public static IEnumerator Animation(Action<float> action, AnimationOptions options)
    {
      float dd_t = Time.fixedDeltaTime; // 固定住间隔时间
      int count = (int)(options.duration / dd_t);
      float t = 0;
      for (int i = 0; i < count + 1; i++)
      {
        float x = LCurveSampler.Evaluate(t, options.curve);
        action(x);
        yield return new WaitForFixedUpdate(); // 使用 Unity 自带的，那个必须在 StartCoroutine 中调用
        t += 1.0f / count;
      }
    }

    public static string MakeAnimation(Action<float> action, AnimationOptions options, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      return MakeAnimation(action, options, MakeCoroutineId(), _CoroutineMap);
    }

    public static string MakeAnimation(Action<float> action, AnimationOptions options, string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (CoroutineMap.ContainsKey(id))
        throw new SystemException("ID has already in used");
      LAnimation animationState = new LAnimation(action, options);
      animationState.id = id;
      IEnumerator animation = Animation(animationState, CoroutineMap);
      CoroutineMap.Add(id, new AnimationAndCoroutine()
      {
        id = id,
        animationState = animationState,
        animation = animation
      });
      return id;
    }

    public static string MakeAnimationAndPlay(Action<float> action, AnimationOptions options, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      string _id = MakeAnimation(action, options, _CoroutineMap);
      RePlayC(_id, _CoroutineMap);
      return _id;
    }

    public static void RePlayC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      AnimationAndCoroutine animationAndCoroutine = CoroutineMap[id];
      if (animationAndCoroutine.coroutine != null)
      {
        StopCoroutine(animationAndCoroutine.coroutine);
        animationAndCoroutine.coroutine = null;
      }
      if (animationAndCoroutine.animationState.Completed)
      {
        animationAndCoroutine.animationState.RePlay();
        // 完成了，需要重新来一个迭代器
        animationAndCoroutine.animation = Animation(animationAndCoroutine.animationState);
      }
      else
      {
        animationAndCoroutine.animationState.RePlay();
      }
      animationAndCoroutine.coroutine = StartCoroutine(animationAndCoroutine.animation);
    }

    public static void PauseC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      AnimationAndCoroutine animationAndCoroutine = CoroutineMap[id];
      animationAndCoroutine.animationState.Pause();
    }

    public static void StopC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      AnimationAndCoroutine animationAndCoroutine = CoroutineMap[id];
      animationAndCoroutine.animationState.Stop();
      StopCoroutine(animationAndCoroutine.coroutine);
      animationAndCoroutine.coroutine = null;
    }

    public static void OverC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      AnimationAndCoroutine animationAndCoroutine = CoroutineMap[id];
      animationAndCoroutine.animationState.Over();
    }

    public static void RemoveC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      CoroutineMap.Remove(id);
    }

    public static void StopAndRemoveC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      StopC(id, _CoroutineMap);
      RemoveC(id, _CoroutineMap);
    }

    public static void StopAllC(Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      foreach (string id in CoroutineMap.Keys.ToArray<string>())
      {
        StopC(id, _CoroutineMap);
      }
    }

    public static void RemoveAllC(Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      foreach (string id in CoroutineMap.Keys.ToArray<string>())
      {
        RemoveC(id, _CoroutineMap);
      }
    }

    public static void StopAndRemoveAllC(Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      foreach (string id in CoroutineMap.Keys.ToArray<string>())
      {
        StopAndRemoveC(id, _CoroutineMap);
      }
    }

    public static void ResumeC(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      AnimationAndCoroutine animationAndCoroutine = CoroutineMap[id];
      animationAndCoroutine.animationState.Resume();
    }

    public static LAnimationStateStruct GetAnimationState(string id, Dictionary<string, AnimationAndCoroutine>? _CoroutineMap = null)
    {
      Dictionary<string, AnimationAndCoroutine> CoroutineMap = _CoroutineMap ?? LAnimationCoroutine.CoroutineMap;
      if (!CoroutineMap.ContainsKey(id)) throw new SystemException("ID not found");
      return CoroutineMap[id].animationState.Deconstruction();
    }
#nullable disable
    public static string MakeCoroutineId()
    {
      return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Starts a coroutine.
    /// static 方法是不可以调用 非 static 方法，因此这里需要拿到 MonoBehaviour 的示例来调用 StartCoroutine
    /// </summary>
    /// <param name="cr"></param>
    /// <param name="allowMultipleInstances">If not, previous instances of the coroutine will be stopped.</param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Coroutine StartCoroutine(IEnumerator cr)
    {
      return SKCommonTimer.instance.StartCoroutine(cr);
    }
#nullable enable
    public static void StopCoroutine(Coroutine? cr)
    {
      if (cr != null)
        SKCommonTimer.instance.StopCoroutine(cr);
    }
#nullable disable
  }

  // 提供一个 Group 的方式
  public class LAnimationCoroutineGroup
  {
    public readonly Dictionary<string, AnimationAndCoroutine> CoroutineMap = new Dictionary<string, AnimationAndCoroutine>();
    public string GroupId = LAnimationCoroutine.MakeCoroutineId();

    public string MakeAnimation(Action<float> action, AnimationOptions options)
    {
      return LAnimationCoroutine.MakeAnimation(action, options, LAnimationCoroutine.MakeCoroutineId(), CoroutineMap);
    }

    public string MakeAnimation(Action<float> action, AnimationOptions options, string id)
    {
      return LAnimationCoroutine.MakeAnimation(action, options, id, CoroutineMap);
    }

    public string MakeAnimationAndPlay(Action<float> action, AnimationOptions options)
    {
      return LAnimationCoroutine.MakeAnimationAndPlay(action, options, CoroutineMap);
    }

    public void RePlayC(string id)
    {
      LAnimationCoroutine.RePlayC(id, CoroutineMap);
    }

    public void PauseC(string id)
    {
      LAnimationCoroutine.PauseC(id, CoroutineMap);
    }

    public void StopC(string id)
    {
      LAnimationCoroutine.StopC(id, CoroutineMap);
    }

    public void RemoveC(string id)
    {
      LAnimationCoroutine.RemoveC(id, CoroutineMap);
    }

    public void StopAndRemoveC(string id)
    {
      LAnimationCoroutine.StopAndRemoveC(id, CoroutineMap);
    }

    public void StopAllC()
    {
      LAnimationCoroutine.StopAllC(CoroutineMap);
    }

    public void RemoveAllC()
    {
      LAnimationCoroutine.RemoveAllC(CoroutineMap);
    }

    public void StopAndRemoveAllC()
    {
      LAnimationCoroutine.StopAndRemoveAllC(CoroutineMap);
    }

    public void ResumeC(string id)
    {
      LAnimationCoroutine.ResumeC(id, CoroutineMap);
    }

    public void OverC(string id)
    {
      LAnimationCoroutine.OverC(id, CoroutineMap);
    }

    public LAnimationStateStruct GetAnimationState(string id)
    {
      return LAnimationCoroutine.GetAnimationState(id, CoroutineMap);
    }
  }
}

