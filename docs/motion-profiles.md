# 三档动效规则

`MotionStyleProfile` 为已有 46 项时间轴、BX 按钮和新页面配方提供同一套推荐起点。档位不是新效果编号，也不是逐项重新制作动画；从同一个目录基准推导参数，再按用途微调。保持原素材的暖奶油、薄荷、淡紫、金边与柔软体积语言；需要新按钮时延续材质和光照，不限制为演示中已有的造型。

| 档位 | 时长倍数 | 强度 | 距离 | 间隔 | 越界量 | 弹簧 Hz / 阻尼 |
|---|---:|---:|---:|---:|---:|---|
| 克制 Restrained | 0.85 | 0.60 | 0.65 | 0.75 | 0.45 | 9 / 0.90 |
| 标准 Standard | 1 | 1 | 1 | 1 | 1 | 7 / 0.68 |
| 庆祝 Celebratory | 1.18 | 1.25 | 1.15 | 1.15 | 1.45 | 5.8 / 0.55 |

克制适合高频、次要或密集操作，标准适合通用页面和主要按钮，庆祝适合低频奖励和解锁。错误提示与高频输入不应默认使用庆祝。装饰脉冲长度为 `0.55 * timeScale` 秒；弹簧反馈本身不承诺固定结束时刻。减弱动效是独立可访问性设置，可叠加到任意档位。

```csharp
var canonical = MotionSettings.FromDefinition(MotionCatalog.Find("A04"));
var profile = MotionStyleProfile.Get(MotionStyleKind.Restrained);
var effective = profile.Apply(canonical); // 克隆，不修改 canonical
player.Play("A04", effective);
feedback.frequency = profile.springFrequency;
feedback.damping = profile.springDamping;
feedback.intensity = profile.strengthScale;
feedback.accentDuration = .55f * profile.timeScale;
```

Apply 对时长保留下限 .05 秒、强度和越界量限制 0–3，间隔不小于 0。不要对上次的 effective 再 Apply。展馆每次切换档位均从目录默认值重新生成；会替换当前微调，之后可继续调节。导出配置中的 settings、interaction 以及 recipe 的反馈字段已经是实际生效值，profile 仅保留来源，消费者不要重复相乘。所选素材独立于档位保留。

主展馆右侧三个档位按钮对原有条目和页面配方共同生效。不要只为新示例接入规则而遗漏已有按钮、弹窗和奖励。存量宿主配置若需保持旧表现，应直接使用其已保存参数，而不是强行套用档位。
