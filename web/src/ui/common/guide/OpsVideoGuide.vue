<template>
    <span
        class="ops-video-guide-link"
        role="button"
        tabindex="0"
        :aria-label="label"
        :title="label"
        @click.stop="open"
        @keydown.enter.stop.prevent="open"
    >
        <span class="ops-video-guide-icon" aria-hidden="true"></span>
        <span class="ops-video-guide-sr">{{ label }}</span>
    </span>

    <!-- 页面加载后后台预取，首次打开弹窗即可走浏览器缓存 -->
    <el-dialog
        v-model="visible"
        :title="title"
        width="64vw"
        top="6vh"
        append-to-body
        :destroy-on-close="false"
        class="ops-video-guide-dialog"
        @close="onClose"
    >
        <div class="ops-video-guide-player-wrap">
            <video
                ref="player"
                class="ops-video-guide-player"
                controls
                playsinline
                preload="auto"
                :src="videoSrc"
                @error="onVideoError"
            />
        </div>
    </el-dialog>
</template>

<script setup>
import { computed, ref, nextTick } from "vue";
import { ElMessage } from "element-plus";

const unavailableMessage = "\u64cd\u4f5c\u89c6\u9891\u672a\u914d\u7f6e\u6216\u4e0d\u53ef\u8bbf\u95ee\uff0c\u8bf7\u8054\u7cfb\u7ba1\u7406\u5458";

const props = defineProps({
    src: { type: String, default: "" },
    label: { type: String, default: "观看操作说明" },
    title: { type: String, default: "操作说明" },
});

const visible = ref(false);
const player = ref(null);
const videoSrc = computed(() => String(props.src || "").trim());

function open() {
    if (!videoSrc.value) {
        ElMessage.warning(unavailableMessage);
        return;
    }
    visible.value = true;
    nextTick(() => {
        player.value?.play();
    });
}

function onClose() {
    player.value?.pause();
    if (player.value) {
        player.value.currentTime = 0;
    }
}

function onVideoError() {
    ElMessage.warning(unavailableMessage);
    visible.value = false;
}
</script>

<style scoped>
.ops-video-guide-link {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    width: 16px;
    height: 16px;
    margin-left: 8px;
    font-size: 0;
    line-height: 16px;
    color: #338fd1;
    cursor: pointer;
    user-select: none;
    vertical-align: middle;
    transition: transform 0.15s ease, color 0.15s ease;
}

.ops-video-guide-link:hover {
    color: #2f80ba;
    transform: translateY(-1px);
}

.ops-video-guide-icon {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 14px;
    height: 14px;
    box-sizing: border-box;
    border: 1.5px solid currentColor;
    border-radius: 50%;
}

.ops-video-guide-icon::before {
    content: "";
    display: block;
    width: 0;
    height: 0;
    margin-left: 1px;
    border-top: 3.5px solid transparent;
    border-bottom: 3.5px solid transparent;
    border-left: 5px solid currentColor;
}

.ops-video-guide-sr {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
}

.ops-video-guide-player-wrap {
    display: block;
    width: 100%;
    max-height: 70vh;
    aspect-ratio: 16 / 9;
    overflow: hidden;
    background: #fff;
    border-radius: 6px;
}

.ops-video-guide-player {
    display: block;
    width: 100%;
    height: 100%;
    object-fit: cover;
    background: transparent;
}
</style>
