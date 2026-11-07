# Sail 设计器侧栏 vs 引擎 / n_flowable 缺口表

> 日期：2026-11-01 · 配套实施：图元覆盖（D）+ 各图元齿轮弹窗（C）  
> 原则：坚持 `sf:` + Host 消费；不整体移植芋道 `bpmnProcessDesigner`。

## 1. 矩阵

| 图元/能力 | 引擎落点 | 侧栏/弹窗 | vs n_flowable | 结论 |
|-----------|----------|-----------|---------------|------|
| UserTask ConfigPair | `sf:assigneeResolve` / `sf:leaveJudge` | 策略化 Params + 弹窗 | CandidateStrategy 选择器 | **做**（Type 仍为 string） |
| 会签/按钮/通知/动作/边界超时 | Multisign / Buttons / Notify / Actions / `sf:Boundaries`→`XmlConvert.ToTimeSpan` | 已有；超时文案 ISO8601 | 自定义配置 | **保留** |
| NodePolicy | Host JSON | 节点弹窗 | UserTaskCustomConfig | **保留** |
| 表单 `sf:Forms` | 流程 `GetFormList`；`ConvertForms()` **空实现**；节点扩展以 Host 读为主 | Process + UserTask/StartEvent | StartEvent/UserTask 表单 | **做**侧栏扩展；运行时勿假装 BPMN `formKey` |
| SequenceFlow | GroupBehaviours + condition | 侧栏+弹窗 | 流转条件 | **保留** |
| Gateway | And/Or/XOr/ApprovalOrSplit/EOrJoin/MI | 侧栏+齿轮弹窗 | — | **做**弹窗 |
| Service/Script/Rule/AI | 对应 Mediator + UseXxx | 侧栏+齿轮弹窗 | 任务折叠 | **做**弹窗 |
| SubProcess | SubProcess + `sf:SubInfoes` | 侧栏+弹窗 | CallActivity | **做**弹窗；**不做** CallActivity |
| CallActivity / ReceiveTask | **无** Mediator | 无 | 有面板 | **缺口·不做** |
| Timer/Message/Signal/Conditional | NodeMediator*Timer/Msg/Signal/Conditional | Timer 三态 + 事件弹窗 | TimeEventConfig | **做**；与用户任务 `sf:Boundaries` 超时语义分离 |
| Process 消息/信号库 | rootElements Message/Signal | Process 侧栏列表 | SignalAndMessage | **做** |
| 执行/任务监听器 | 非 Gate/Hook 主路径 | 无 | Listeners | **暂缓** |
| 扩展属性 KV | 无统一契约 | 无 | ElementProperties | **暂缓** |
| 转办按钮 | 须有（todo §F）；引擎命令待补 | 无 | 操作按钮 | **做**（码表+命令后挂侧栏启停） |
| 委派按钮 | 暂缓 | 无 | 操作按钮 | **暂缓** |

## 2. 语义差异（必读）

1. **两套超时**：用户任务逾期 = `sf:Boundaries` + ISO8601 Duration（`PT10M`）；中间定时事件 = BPMN `TimerEventDefinition`（`timeDate` / `timeDuration` / `timeCycle`）。
2. **表单**：侧栏写 `sf:Forms`；引擎 `ConvertForms()` 仍空——发起/办理以 Host Workbench 读扩展 + Form 服务为准。
3. **子流程**：Sail 用 `bpmn:SubProcess` + `sf:SubInfoes`，不是 Camunda `CallActivity`/`calledElement`。
4. **待办人**：不映射芋道 CandidateStrategy 数值枚举；表单字段算人走 `Expression` / `BusinessCallback`。

## 3. 本轮不做 / 另开

- CallActivity / ReceiveTask 伪面板  
- 监听器、任意扩展属性 KV  
- 委派；修复引擎 `ConvertForms()`（另开引擎债）  
- **转办**不在本侧栏轮次实现，但产品口径已改为 **须有**（见 sailflow `todo-产品缺口.md` §F）；侧栏启停随码表落地另开
