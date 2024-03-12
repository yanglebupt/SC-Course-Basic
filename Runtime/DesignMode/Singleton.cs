// https://www.cnblogs.com/deatharthas/p/13047660.html，考虑线程单例

namespace YLBasic
{
  public static class Singleton<T> where T : class, new() // 约束泛型 T 必须是引用类型+无参构造
  {
    // 私有字段
    private static T _instance;

    // 公开属性
    public static T Instance => _instance ?? (_instance = new T()); // getter 使用 lambda 表达式

    public static void Clear()
    {
      _instance = null;
    }
  }
}