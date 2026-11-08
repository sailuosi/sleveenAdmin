<template>
  <div
    class="glass-div"
    :class="rootClass"
    :style="rootStyle"
  >
    <div v-if="theme !== 'scene'" class="glass-div__shine" aria-hidden="true" />
    <div v-if="theme === 'scene'" class="glass-div__arcs" aria-hidden="true">
      <span class="glass-div__arc glass-div__arc--a" />
      <span class="glass-div__arc glass-div__arc--b" />
    </div>
    <header v-if="$slots.title || title" class="glass-div__head">
      <slot name="title">
        <span class="glass-div__title">{{ title }}</span>
      </slot>
      <div v-if="$slots.extra" class="glass-div__extra">
        <slot name="extra" />
      </div>
    </header>
    <div class="glass-div__body">
      <slot />
    </div>
  </div>
</template>

<script setup>
/**
 * GlassDiv — Liquid Glass 卡片
 *
 * theme:
 *   glass   主面板毛玻璃（默认）
 *   module  内部模块嵌套玻璃
 *   solid   信息/列表实底 #F3F1F1
 *   dark    深色锚点 #1B1B19 + 铜光
 *   accent  强调态 #B66D44
 *   scene   页面氛围底（暖杏渐变 + 光弧）
 *   muted   未完成/弱态半透明灰
 */
import { computed } from 'vue'

defineOptions({ name: 'GlassDiv' })

const props = defineProps({
  theme: {
    type: String,
    default: 'glass',
    validator: (v) =>
      ['glass', 'module', 'solid', 'dark', 'accent', 'scene', 'muted'].includes(v)
  },
  title: { type: String, default: '' },
  padding: { type: [String, Number], default: '20px' },
  radius: { type: [String, Number], default: 24 },
  /** 关闭顶部内高光线 */
  noShine: { type: Boolean, default: false },
  /** 关闭投影 */
  flat: { type: Boolean, default: false },
  /** 暗色主题顶部铜光强度 0–1 */
  glow: { type: [Number, String], default: 1 },
  hoverable: { type: Boolean, default: false }
})

const rootClass = computed(() => [
  `glass-div--${props.theme}`,
  {
    'glass-div--no-shine': props.noShine || props.theme === 'scene',
    'glass-div--flat': props.flat,
    'glass-div--hoverable': props.hoverable
  }
])

const rootStyle = computed(() => {
  const pad =
    typeof props.padding === 'number' ? `${props.padding}px` : props.padding
  const rad =
    typeof props.radius === 'number' ? `${props.radius}px` : props.radius
  const glow = Math.min(1, Math.max(0, Number(props.glow) || 0))
  return {
    '--gd-padding': pad,
    '--gd-radius': rad,
    '--gd-glow': String(glow)
  }
})
</script>

<style scoped>
.glass-div {
  --gd-copper: #b66d44;
  --gd-peach: #e4c0a6;
  --gd-rose: #d4b8ac;
  --gd-apricot: #ebd7cc;
  --gd-solid: #f3f1f1;
  --gd-ink: #1b1b19;
  --gd-icon-from: #e7a471;
  --gd-icon-to: #c5a0ab;
  --gd-fg: #2a2420;
  --gd-fg-muted: rgba(42, 36, 32, 0.55);

  position: relative;
  isolation: isolate;
  box-sizing: border-box;
  border-radius: var(--gd-radius, 24px);
  padding: var(--gd-padding, 20px);
  color: var(--gd-fg);
  font-family: 'Plus Jakarta Sans', 'Manrope', 'Segoe UI', system-ui, sans-serif;
  overflow: hidden;
  transition:
    box-shadow 0.25s ease,
    transform 0.25s ease,
    background 0.25s ease,
    border-color 0.25s ease;
}

.glass-div__shine {
  pointer-events: none;
  position: absolute;
  inset: 0 0 auto;
  height: 1px;
  z-index: 2;
  margin: 1px 12px 0;
  border-radius: 1px;
  background: linear-gradient(
    90deg,
    transparent 0%,
    rgba(255, 255, 255, 0.85) 20%,
    rgba(255, 255, 255, 0.95) 50%,
    rgba(255, 255, 255, 0.85) 80%,
    transparent 100%
  );
  opacity: 0.9;
}

.glass-div--no-shine > .glass-div__shine {
  display: none;
}

.glass-div__arcs {
  pointer-events: none;
  position: absolute;
  inset: 0;
  z-index: 0;
  overflow: hidden;
}

.glass-div__arc {
  position: absolute;
  border-radius: 50%;
  border: 1px solid rgba(255, 255, 255, 0.35);
  box-shadow: inset 0 0 40px rgba(255, 255, 255, 0.18);
}

.glass-div__arc--a {
  width: 140%;
  height: 90%;
  left: -20%;
  top: -45%;
  background: radial-gradient(
    ellipse at center,
    rgba(255, 255, 255, 0.28) 0%,
    transparent 65%
  );
}

.glass-div__arc--b {
  width: 100%;
  height: 70%;
  right: -30%;
  bottom: -35%;
  background: radial-gradient(
    ellipse at center,
    rgba(255, 255, 255, 0.18) 0%,
    transparent 60%
  );
}

.glass-div__head {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}

.glass-div__title {
  font-size: 18px;
  font-weight: 650;
  letter-spacing: -0.02em;
  line-height: 1.3;
}

.glass-div__extra {
  flex: 0 0 auto;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.glass-div__body {
  position: relative;
  z-index: 1;
  min-width: 0;
}

/* —— glass：主面板 —— */
.glass-div--glass {
  background: rgba(255, 255, 255, 0.52);
  border: 1px solid rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(28px) saturate(140%);
  -webkit-backdrop-filter: blur(28px) saturate(140%);
  box-shadow:
    0 18px 48px rgba(90, 60, 40, 0.12),
    0 2px 8px rgba(90, 60, 40, 0.06);
}

/* —— module：内部嵌套模块 —— */
.glass-div--module {
  background: rgba(255, 255, 255, 0.38);
  border: 1px solid rgba(255, 255, 255, 0.45);
  backdrop-filter: blur(18px) saturate(130%);
  -webkit-backdrop-filter: blur(18px) saturate(130%);
  box-shadow:
    0 10px 28px rgba(90, 60, 40, 0.08),
    inset 0 1px 0 rgba(255, 255, 255, 0.35);
}

/* —— solid：列表/信息实底 —— */
.glass-div--solid {
  background: var(--gd-solid);
  border: 1px solid rgba(255, 255, 255, 0.7);
  box-shadow: 0 8px 24px rgba(90, 60, 40, 0.06);
}

.glass-div--solid > .glass-div__shine {
  opacity: 0.35;
}

/* —— dark：深色锚点 + 顶部铜光 —— */
.glass-div--dark {
  --gd-fg: #f5f0eb;
  --gd-fg-muted: rgba(245, 240, 235, 0.55);
  background:
    radial-gradient(
      ellipse 90% 55% at 50% -10%,
      rgba(182, 109, 68, calc(0.45 * var(--gd-glow, 1))) 0%,
      transparent 70%
    ),
    var(--gd-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow:
    0 22px 56px rgba(0, 0, 0, 0.35),
    0 0 40px rgba(182, 109, 68, calc(0.18 * var(--gd-glow, 1)));
  color: var(--gd-fg);
}

.glass-div--dark > .glass-div__shine {
  background: linear-gradient(
    90deg,
    transparent 0%,
    rgba(231, 164, 113, 0.55) 25%,
    rgba(255, 220, 190, 0.75) 50%,
    rgba(231, 164, 113, 0.55) 75%,
    transparent 100%
  );
}

/* —— accent：铜橙强调 —— */
.glass-div--accent {
  --gd-fg: #fff8f3;
  background: linear-gradient(
    165deg,
    #c47a4f 0%,
    var(--gd-copper) 55%,
    #9a5a38 100%
  );
  border: 1px solid rgba(255, 255, 255, 0.22);
  box-shadow:
    0 16px 40px rgba(182, 109, 68, 0.35),
    inset 0 1px 0 rgba(255, 255, 255, 0.25);
  color: var(--gd-fg);
}

.glass-div--accent > .glass-div__shine {
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.55),
    transparent
  );
}

/* —— scene：页面氛围底 —— */
.glass-div--scene {
  --gd-padding: 0;
  background:
    radial-gradient(
      ellipse 70% 55% at 18% 12%,
      rgba(255, 255, 255, 0.45),
      transparent 55%
    ),
    radial-gradient(
      ellipse 55% 45% at 82% 78%,
      rgba(255, 255, 255, 0.28),
      transparent 50%
    ),
    linear-gradient(
      160deg,
      var(--gd-peach) 0%,
      var(--gd-rose) 48%,
      var(--gd-apricot) 100%
    );
  border: none;
  box-shadow: none;
  filter: saturate(1.05);
}

/* —— muted：弱态 —— */
.glass-div--muted {
  --gd-fg: rgba(42, 36, 32, 0.45);
  background: rgba(243, 241, 241, 0.55);
  border: 1px solid rgba(255, 255, 255, 0.4);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  box-shadow: none;
  color: var(--gd-fg);
}

.glass-div--flat {
  box-shadow: none !important;
}

.glass-div--hoverable {
  cursor: pointer;
}

.glass-div--hoverable:hover {
  transform: translateY(-2px);
}

.glass-div--glass.glass-div--hoverable:hover,
.glass-div--module.glass-div--hoverable:hover {
  background: rgba(255, 255, 255, 0.62);
  box-shadow:
    0 22px 56px rgba(90, 60, 40, 0.16),
    0 4px 12px rgba(90, 60, 40, 0.08);
}

.glass-div--dark.glass-div--hoverable:hover {
  box-shadow:
    0 26px 64px rgba(0, 0, 0, 0.4),
    0 0 48px rgba(182, 109, 68, calc(0.28 * var(--gd-glow, 1)));
}

.glass-div--accent.glass-div--hoverable:hover {
  filter: brightness(1.04);
}

/* 工具类：图标渐变描边（子内容可挂 class） */
.glass-div :deep(.gd-icon-grad) {
  background: linear-gradient(180deg, var(--gd-icon-from), var(--gd-icon-to));
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.glass-div :deep(.gd-num) {
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum' 1;
  font-family: ui-monospace, 'SF Mono', Menlo, Consolas, monospace;
}

.glass-div :deep(.gd-tag) {
  display: inline-flex;
  align-items: center;
  padding: 2px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  color: #fff;
  background: var(--gd-copper);
}

.glass-div--dark :deep(.gd-tag),
.glass-div--accent :deep(.gd-tag) {
  background: rgba(255, 255, 255, 0.18);
  color: inherit;
}
</style>
