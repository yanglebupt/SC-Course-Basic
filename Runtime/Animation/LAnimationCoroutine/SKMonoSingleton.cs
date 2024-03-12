using UnityEngine;


namespace YLBasic
{
  // https://github.com/Skyrim07/SKCell/blob/main/Assets/SKCell/Common/SKMonoSingleton.cs

  // 这里只能 abstract，不能使用 static 类，当然也可以不加 abstract
  public abstract class SKMonoSingleton<T> : MonoBehaviour where T : SKMonoSingleton<T>
  {
    private static T _inst = null;

    public static T instance
    {
      get
      {
        return _inst;
      }
    }

    protected virtual void Awake()
    {
      _inst = (T)this;
    }
  }
}