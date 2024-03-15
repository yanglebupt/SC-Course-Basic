# Reflect

该静态类实现了一些常用的反射操作，API 设计参考 <a href="https://developer.mozilla.org/zh-CN/docs/Web/JavaScript/Reference/Global_Objects/Reflect">MDN Reflect</a>。传入的 `target` 如果是对象实例，访问实例成员；传入的是 `Type` 类型，则访问静态成员，例如下面。

```csharp
Person p = new Person();
Reflect.get(p, "age");  // 传入对象实例，访问实例成员
Reflect.get(p.GetType(), "age");  // 传入`Type` 类型，访问静态成员
```

无说明，默认公共、私有、继承的都可以访问。同时对于访问出错，也就是不存在的情况，统一返回 `ReflectNotFoundError` 对象，可以通过 `Message` 查看错误信息

## apply()

根据方法名，调用方法，返回结果。如果调用失败，返回 `ReflectNotFoundError` 对象。暂不支持泛型方法的调用

```csharp
static object? apply(object target, string propertyKey) // 调用无参方法
static object? apply(object target, string propertyKey, object?[]? args) // 调用有参方法
```

## construct()

根据实例对象或者类型，调用构造函数，返回生成一个新对象。如果调用失败，返回 `ReflectNotFoundError` 对象

```csharp
static object? construct(object target) // 调用无参构造函数
static object? construct(object target, object?[]? args) // 调用有参构造函数
```

## get()

根据属性或者字段名字，获取值。如果获取失败，返回 `ReflectNotFoundError` 对象

```csharp
object? get(object target, string propertyKey)
```

## set()

根据属性或者字段名字，设置值。成功设置返回 `true`，设置失败返回 `false`

```csharp
bool set(object target, string propertyKey, object value)
bool set(object target, string propertyKey, int index, object value) // 设置数组中的某一项，int index 代表索引
```

## getPrototypeOf()

获取继承的父类型

```csharp
static Type? getPrototypeOf(object target)
```

## has() 

判断是否存在某个成员，包括继承的成员

```csharp
static bool has(object target, string propertyKey)
```

## ownKeys()

获取自身的成员变量名字，不包括继承

```csharp
static string[] ownKeys(object target)
```