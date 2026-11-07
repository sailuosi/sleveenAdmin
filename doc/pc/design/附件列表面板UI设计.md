# 附件列表面板 UI 设计

本文档固化业务页采用的「附件列表面板」视觉与接入方式。设计对齐 OA 风格面板（`panel-heading` + 链接行列表），**只约束样式与结构，不改变上传 / 预览 / 下载 / 删除业务逻辑**。

| 文档 / 代码 | 说明 |
|-------------|------|
| `web/src/ui/common/attachsimshell.vue` | 附件外壳：标题栏 + 插槽 |
| 业务域 `VC_*FileList.vue` | 附件列表；`displayMode="panel"` 为链接行 |
| 业务编辑 / 只读页 | 用 shell 包住列表 VC |

---

## 1. 目标与边界

### 1.1 目标

- 统一「附件」区块外观：边框面板 + 灰底标题栏 + 浅灰列表区 + 链接行操作。
- 标题栏提供数量、上传、批量下载；列表行提供类型图标、文件名、预览 / 下载 / 删除。
- 外壳与列表解耦：`attachsimshell` 只管壳与工具栏；列表组件负责数据与行操作。

### 1.2 适用 / 不适用

| 适用 | 不适用 |
|------|--------|
| 业务编辑 / 只读页中的附件嵌入区 | 独立全表管理页（仍用 `displayMode="table"`） |
| 需要 OA 风格附件条的其它业务嵌入 | 需要 zip 打包批量下载的后端接口 |
| 复用已有预览 / 单文件下载逻辑 | 新增「收藏」、文件大小展示（无字段则不做） |

### 1.3 明确不做

- 不改后端附件 Biz / 字段与上传 API 契约（本 UI 文档范围外）。
- 不引入真实「收藏」能力（参考图中的「收藏」对应现网「删除」文案位；只读不显示删除）。
- 批量下载仅为 UI 入口：循环调用已有单文件 `downloadFile`，不做新打包接口。
- 库表无大小字段时不展示 `(xx.xxk)`。

---

## 2. 结构与分层

```
┌─────────────────────────────────────────────────────────────┐
│ attachsimshell（.attsim-shell）                              │
│  ┌─ header（.attsim-header）─────────────────────────────┐  │
│  │ 📎 附件 (N个)          [上传附件]  [批量下载]          │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌─ list-wrap ───────────────────────────────────────────┐  │
│  │  #list → VC_*FileList displayMode=panel               │  │
│  │    .tmf-panel-list（浅灰底）                            │  │
│  │      .tmf-panel-row … 虚线分隔 …                       │  │
│  │  #upload-tip（可编辑时）                                │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

父页面职责：

- 传入 `count`（如 `attachTotal`）、`disabled` / `empty`。
- `@upload-click` → 列表 `triggerUpload`。
- `@batch-download` → 列表 `downloadAll`。
- 列表加 `displayMode="panel"`、`hideToolbar`，外边框交给 shell。

---

## 3. 视觉规范

参考 Bootstrap `panel-default` / `panel-heading` 与业务侧附件列表量纲。

### 3.1 外层面板（shell）

| 项 | 值 |
|----|-----|
| 边框 | `1px solid #e8e8e8` |
| 圆角 | `3px` |
| 背景 | `#fff` |

### 3.2 标题栏（panel-heading）

| 项 | 值 |
|----|-----|
| padding | `10px 15px` |
| 文字色 | `#333` |
| 背景 | `#f5f5f5` |
| 底边 | `1px solid #e8e8e8` |
| 顶角圆角 | `3px` |
| 左侧 | `fa-paperclip` + 标题 + `({{ count }}个)`，数量色 `#8c8c8c` |
| 右侧工具 | 上传（可编辑）在前，批量下载在后；间距 `16px`；链接色 `#1890ff` |

### 3.3 附件区总容器（列表 panel）

| 项 | 值 |
|----|-----|
| margin | `10px 0` |
| 背景 | `rgba(240, 242, 245, 0.6)` |

### 3.4 附件行

| 项 | 值 |
|----|-----|
| padding | `8px 6px` |
| 左右 margin | `10px` |
| 非末行分隔 | `border-bottom: 1px dashed #d9d9d9` |
| hover | `rgba(240, 247, 255, 0.9)` |
| 文件名 | `#1890ff`，点击预览 |
| 操作链接 | 预览 / 下载 /（可编辑）删除；色 `#1890ff`，删除可用危险色 |
| 类型图标 | 按后缀：`doc/docx`、`xls/xlsx`、`ppt`、`pdf`、图片、压缩包等 |

---

## 4. 组件契约

### 4.1 `attachsimshell`

**主要 props**

| Prop | 类型 | 默认 | 说明 |
|------|------|------|------|
| `title` | string | `附件` | 标题栏文案 |
| `count` | number | `0` | 数量，展示为 `(N个)` |
| `showHeader` | boolean | `true` | 是否显示标题栏 |
| `showBatchDownload` | boolean | `true` | 是否允许批量下载入口 |
| `disabled` | boolean | `false` | `true` 时隐藏上传 |
| `empty` | boolean | `false` | 空态标记（插槽可用） |
| `showEmptyDecor` | boolean | `false` | 装饰性空态 |

`uploadInCard` / `compactUploadButton` / `alwaysCardUpload` 已废弃（上传固定在标题栏），仅为兼容保留。

**事件**

| 事件 | 说明 |
|------|------|
| `upload-click` | 点击「上传附件」 |
| `batch-download` | 点击「批量下载」；默认仅在 `count > 0` 时显示 |

**插槽**

| 插槽 | 说明 |
|------|------|
| `list` | 附件列表内容 |
| `upload-trigger` | 覆盖默认上传链接 |
| `batch-download` | 覆盖默认批量下载 |
| `upload-tip` | 上传说明（可编辑时渲染在列表下方） |

### 4.2 列表 `displayMode="panel"`

以业务域附件 ListVC 为例（其它附件 VC 可仿照）：

| 项 | 说明 |
|----|------|
| `displayMode` | `table`（默认，独立列表页）/ `panel`（嵌入面板） |
| `downloadAll` | `defineExpose`；对 `mlist` 逐个调用已有 `downloadFile` |
| cover 区 | panel 模式对 columns 使用 `[cover-break] [已改]` |

行操作复用既有 `previewFile` / `downloadFile` / `deleteFileRow`，不改路径与鉴权。

---

## 5. 接入示例

```vue
<attachsimshell
  title="附件"
  :count="attachTotal"
  :disabled="formNoedit"
  :empty="attachEmpty"
  :showBatchDownload="attachTotal > 0"
  @upload-click="onUploadFileClick"
  @batch-download="onBatchDownloadClick"
>
  <template #upload-tip>
    <div v-if="attachEmpty" class="upload-box-tip">可上传 …，且不超过 20MB</div>
  </template>
  <template #list>
    <VC_XxxFileList
      displayMode="panel"
      :hideToolbar="true"
      :disabled="formNoedit"
      :fk="focusRow.XxxOID"
      childed
      ref="fileListBody"
      @loaded="onFileListLoaded"
      ...
    />
  </template>
</attachsimshell>
```

```js
function onUploadFileClick() {
  fileListBody.value?.triggerUpload?.();
}
function onBatchDownloadClick() {
  fileListBody.value?.downloadAll?.();
}
function onFileListLoaded(res) {
  attachTotal.value = typeof res?.Total === "number" ? res.Total : 0;
}
```

只读页：附件 slot 内同样包一层 `attachsimshell`（`disabled`），**不要**再画「回形针 + 附件」标题，避免双标题。

嵌入时去掉列表自身粗边框（如 `.custom-file-list { border: none }`），外边框由 shell 承担；分页可隐藏。

---

## 6. 推广其它业务时的清单

1. 用 `attachsimshell` 包住现有附件列表。
2. 列表增加 `panel` 展示（或抽公共行组件），样式对齐 §3.3 / §3.4。
3. 接 `count`、`upload-click`、`batch-download`；批量下载优先复用已有单文件下载。
4. 不要在 panel 模式改动生成器 cover 而不打 `[cover-break]`。
5. 与存储 / 流式下载 / 预览地址相关的变更不在本 UI 文档范围。

---

## 7. 修订记录

| 日期 | 说明 |
|------|------|
| 2026-11-07 | 按 sleveen-admin（`web/src/ui/common/attachsimshell`）改写；去掉来源业务样板路径 |
