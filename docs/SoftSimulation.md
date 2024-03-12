# SoftSimulation

## MyPlaneMesh

自己实现的一个平面 Mesh，`GetMesh()` 方法创建一个平面 `Mesh`

## Spring

弹簧模拟静态类

#### GetForce()

获取 `source` 和 `target` 两点之间弹簧作用的受力，注意返回的是 `target` 的受力，`source` 的受力相反

```csharp
Vector3 GetForce(Transform source, Transform target, float resetLength, float k)
```

#### CalcSpeed()

计算点由于受力导致的速度变化，`damping` 阻力系数

```csharp
void CalcSpeed(ref Vector3 origin, Vector3 force, float mass, float time, float damping)
```

#### 案例

可以参考 `Test/SoftSimulation/SpringLineTest.cs` 的使用