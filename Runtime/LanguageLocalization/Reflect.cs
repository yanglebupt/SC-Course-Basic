using System;
using System.Reflection;
using System.Linq;

namespace YLBasic
{
  public class ReflectNotFoundError
  {
    public string Message;
    public ReflectNotFoundError(string mess)
    {
      Message = mess;
    }
  }

#nullable enable
  // API设计参考：https://developer.mozilla.org/zh-CN/docs/Web/JavaScript/Reference/Global_Objects/Reflect
  // 实例代表实例属性，类型代表静态属性
  public static class Reflect
  {
    /* 对一个函数进行调用操作，同时可以传入一个数组作为调用参数 */
    public static object? apply(object target, string propertyKey)
    {
      return apply(target, propertyKey, new object[] { });
    }
    public static object? apply(object target, string propertyKey, object?[]? args)
    {
      bool isStatic = target is Type;
      Type? type = isStatic ? target as Type : target.GetType();
      BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public
                          | (isStatic ? BindingFlags.Static | BindingFlags.FlattenHierarchy : BindingFlags.Instance);
      object? obj = isStatic ? null : target;
      try
      {
        return type!.InvokeMember(propertyKey, flags, null, obj, args);
      }
      catch (Exception ex)
      {
        string mes = isStatic ? "Static" : "";
        return new ReflectNotFoundError($"{mes} {ex.Message}");
      }
    }
    public static object? construct(object target)
    {
      return construct(target, new object[] { });
    }
    public static object? construct(object target, object?[]? args)
    {
      Type? type = target is Type ? target as Type : target.GetType();
      try
      {
        return type!.
          InvokeMember(type.Name, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, args);
      }
      catch (Exception ex)
      {
        return new ReflectNotFoundError(ex.Message);
      }
    }
    public static object? get(object target, string propertyKey)
    {
      bool isStatic = target is Type;
      Type? type = isStatic ? target as Type : target.GetType();
      BindingFlags flags = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public
                          | (isStatic ? BindingFlags.Static | BindingFlags.FlattenHierarchy : BindingFlags.Instance);
      object? obj = isStatic ? null : target;
      try
      {
        return type!.InvokeMember(propertyKey, flags, null, obj, new object[] { });
      }
      catch (Exception ex)
      {
        string mes = isStatic ? "Static" : "";
        string mes2 = ex.Message.Replace("Method", "Field|Property");
        return new ReflectNotFoundError($"{mes} {mes2}");
      }
    }
    private static bool set(object target, string propertyKey, object?[]? args)
    {
      bool isStatic = target is Type;
      Type? type = isStatic ? target as Type : target.GetType();
      BindingFlags flags = BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Public
                          | (isStatic ? BindingFlags.Static | BindingFlags.FlattenHierarchy : BindingFlags.Instance);
      object? obj = isStatic ? null : target;
      try
      {
        type!.InvokeMember(propertyKey, flags, null, obj, args);
        return true;
      }
      catch
      {
        return false;
      }
    }
    public static bool set(object target, string propertyKey, object value)
    {
      return set(target, propertyKey, new object[] { value });
    }
    public static bool set(object target, string propertyKey, int index, object value)
    {
      return set(target, propertyKey, new object[] { index, value });
    }
    public static Type? getPrototypeOf(object target)
    {
      Type? type = target is Type ? target as Type : target.GetType();
      return type!.BaseType;
    }
    /* 考虑继承 */
    public static bool has(object target, string propertyKey)
    {
      bool isStatic = target is Type; ;
      Type? type = isStatic ? target as Type : target.GetType();
      BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public
                            | (isStatic ? BindingFlags.Static | BindingFlags.FlattenHierarchy : BindingFlags.Instance);
      foreach (MemberInfo item in type!.GetMembers(flags))
      {
        if (item.Name == propertyKey) return true;
      }
      return false;
    }
    /* 返回一个包含所有自身属性（不包含继承属性）的数组 */
    public static string[] ownKeys(object target)
    {
      bool isStatic = target is Type;
      Console.WriteLine(isStatic);
      Type? type = isStatic ? target as Type : target.GetType();
      BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly
                            | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
      return type!
        .GetMembers(flags)
        .Select<MemberInfo, string>(m => m.Name).ToArray<string>();
    }
  }
#nullable disable
}
