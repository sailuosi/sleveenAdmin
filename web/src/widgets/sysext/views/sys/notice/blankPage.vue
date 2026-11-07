<script setup lang="ts">
import { useRouter } from "vue-router";

defineOptions({ name: "BlankDevelopingPage" });

const router = useRouter();
</script>

<template>
  <div class="dev-page">
    <div class="bg-grid" aria-hidden="true" />
    <div class="bg-blobs" aria-hidden="true">
      <span class="blob blob-a" />
      <span class="blob blob-b" />
      <span class="blob blob-c" />
    </div>

    <div class="content">
      <div class="panel">
        <div class="swingWrap" role="status" aria-live="polite">
          <div class="pin" aria-hidden="true" />
          <svg
            class="ropes"
            viewBox="0 0 100 100"
            preserveAspectRatio="none"
            aria-hidden="true"
          >
            <defs>
              <linearGradient id="ropeGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="rgba(0,0,0,0.35)" />
                <stop offset="100%" stop-color="rgba(0,0,0,0.08)" />
              </linearGradient>
            </defs>
            <!-- from top pin (50, 6) to bottom pins (5/95, 98) -->
            <line
              x1="50"
              y1="6"
              x2="10"
              y2="98"
              stroke="url(#ropeGradient)"
              stroke-width="2.8"
              stroke-linecap="round"
            />
            <line
              x1="50"
              y1="6"
              x2="90"
              y2="98"
              stroke="url(#ropeGradient)"
              stroke-width="2.8"
              stroke-linecap="round"
            />
          </svg>

          <div class="sign">
            <div class="signInner">
              <SvgIcon class="signIcon" icon="mdi:tools" />
              <div class="signText">
                <span class="title">页面正在开发中...</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="actions">
        <ElButton @click="router.push('/home')">回到首页</ElButton>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dev-page {
  position: relative;
  height: 100%;
  min-height: 520px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 28px;
}

.content {
  position: relative;
  width: min(920px, 100%);
  z-index: 2;
}

.panel {
  position: relative;
  padding: 44px 18px 24px 18px;
  min-height: 360px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 14px;
}

.actions {
  margin-top: 50px;
  display: flex;
  justify-content: center;
}

.title {
  font-size: 30px;
  font-weight: 800;
  letter-spacing: 0.2px;
}

.sub {
  opacity: 0.68;
  font-size: 13px;
  letter-spacing: 0.2px;
}

.pin {
  position: absolute;
  left: 50%;
  top: -2px;
  width: 20px;
  height: 20px;
  transform: translateX(-50%) translateZ(0);
  border-radius: 999px;
  background: radial-gradient(
    circle at 30% 30%,
    #ffffff 0%,
    #d1d5db 40%,
    #9ca3af 72%,
    #97a0b3 100%
  );
  border: 1px solid rgba(0, 0, 0, 0.12);
  box-shadow: 0 10px 22px rgba(0, 0, 0, 0.01),
    inset 0 1px 0 rgba(255, 255, 255, 0.75);
  z-index: 50;
  pointer-events: none;
  overflow: hidden;
}

.pin::after {
  content: "";
  position: absolute;
  left: 5px;
  top: 4px;
  width: 6px;
  height: 6px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.85);
  filter: blur(0.2px);
}

.swingWrap {
  position: relative;
  width: min(760px, calc(100vw - 96px));
  padding-top: 86px;
  transform-origin: 50% 0%;
  animation: signSwing 3.2s ease-in-out infinite;
  will-change: transform;
  display: inline-block;
  transform: translateZ(0);
  z-index: 1;
}

.ropes {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 86px;
  filter: drop-shadow(0 10px 16px rgba(0, 0, 0, 0.08));
  pointer-events: none;
  z-index: 1;
}

.signInner {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  padding: 32px 44px;
  border-radius: 22px;
  min-height: 96px;
  width: min(720px, calc(100vw - 96px));
  justify-content: center;
  background: radial-gradient(
      circle at 20% 15%,
      rgba(24, 160, 255, 0.16),
      transparent 55%
    ),
    radial-gradient(circle at 80% 0%, rgba(124, 58, 237, 0.14), transparent 60%),
    linear-gradient(
      180deg,
      rgba(255, 255, 255, 0.92),
      rgba(255, 255, 255, 0.78)
    );
  border: 1px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 24px 56px rgba(0, 0, 0, 0.14),
    inset 0 1px 0 rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(8px);
  position: relative;
}

.sign {
  display: flex;
  justify-content: center;
}

.signInner::before,
.signInner::after {
  content: "";
  position: absolute;
  top: 10px;
  width: 10px;
  height: 10px;
  border-radius: 999px;
  background: radial-gradient(
    circle at 30% 30%,
    rgba(255, 255, 255, 0.92),
    rgba(0, 0, 0, 0.25)
  );
  box-shadow: 0 10px 18px rgba(0, 0, 0, 0.12),
    inset 0 1px 0 rgba(255, 255, 255, 0.65);
  opacity: 0.75;
  z-index: 2;
}

.signInner::before {
  left: 16px;
}

.signInner::after {
  right: 16px;
}

:deep(.signIcon) {
  width: 30px;
  height: 30px;
  line-height: 1;
  opacity: 0.9;
  /* animation: toolPulse 1.6s ease-in-out infinite; */
}

.signText {
  display: inline-flex;
  align-items: baseline;
  gap: 2px;
}

.ellipsis {
  display: inline-block;
  font-weight: 800;
  letter-spacing: 2px;
  opacity: 0.85;
  width: 3ch;
  overflow: hidden;
  vertical-align: baseline;
  animation: ellipsisClip 1.15s steps(4, end) infinite;
}

.bg-grid {
  position: absolute;
  inset: 0;
  z-index: 0;
  background-image: linear-gradient(rgba(0, 0, 0, 0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 0, 0, 0.04) 1px, transparent 1px);
  background-size: 24px 24px;
  mask-image: radial-gradient(
    circle at 50% 40%,
    rgba(0, 0, 0, 0.75),
    transparent 66%
  );
  opacity: 0.65;
}

.bg-blobs {
  position: absolute;
  inset: 0;
  z-index: 1;
  pointer-events: none;
}

.blob {
  position: absolute;
  width: 420px;
  height: 420px;
  border-radius: 50%;
  filter: blur(40px);
  opacity: 0.24;
  animation: blobMove 10s ease-in-out infinite;
}

.blob-a {
  left: -140px;
  top: -120px;
  background: radial-gradient(circle at 30% 30%, #18a0ff, transparent 60%);
}

.blob-b {
  right: -160px;
  top: 40px;
  background: radial-gradient(circle at 30% 30%, #7c3aed, transparent 60%);
  animation-delay: -3.2s;
}

.blob-c {
  left: 10%;
  bottom: -220px;
  background: radial-gradient(circle at 30% 30%, #22c55e, transparent 60%);
  animation-delay: -6.1s;
  opacity: 0.18;
}

@keyframes signSwing {
  0%,
  100% {
    transform: rotate(-3.2deg) translateY(0);
  }
  50% {
    transform: rotate(3.2deg) translateY(1px);
  }
}

@keyframes toolPulse {
  0%,
  100% {
    transform: rotate(-6deg);
    opacity: 0.86;
  }
  50% {
    transform: rotate(6deg);
    opacity: 0.98;
  }
}

@keyframes ellipsisClip {
  0% {
    clip-path: inset(0 100% 0 0);
  }
  100% {
    clip-path: inset(0 0 0 0);
  }
}

@keyframes blobMove {
  0%,
  100% {
    transform: translate(0, 0) scale(1);
  }
  33% {
    transform: translate(28px, -18px) scale(1.05);
  }
  66% {
    transform: translate(-18px, 22px) scale(0.98);
  }
}
</style>
