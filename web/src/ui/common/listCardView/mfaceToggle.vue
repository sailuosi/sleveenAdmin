<template>
    <el-button
        class="toolbar-action-btn list-card-view-toggle-btn"
        :class="{ 'is-mini': mini }"
        :title="toggleTitle"
        :text="mini"
        @click="onToggle"
    >
        <SvgIcon :name="toggleIcon" />
        <template v-if="showLabel && !mini">{{ toggleLabel }}</template>
    </el-button>
</template>

<script setup>
import { computed, onMounted } from "vue";
import { isCardView, VIEW_CARD, VIEW_TABLE } from "./mfaceToggle.shared.js";
import { getFaceCache, setFaceCache } from "./utils/faceCache.js";

const props = defineProps({
    /** 为 false 时仅显示图标 */
    showLabel: { type: Boolean, default: false },
    /** mini：仅小图标、无边框 */
    mini: { type: Boolean, default: false },
    listTitle: { type: String, default: "列表" },
    cardTitle: { type: String, default: "卡片" },
    switchToListTitle: { type: String, default: "切换为列表视图" },
    switchToCardTitle: { type: String, default: "切换为卡片视图" },
    /** 传入 VCName 等标识时读写 face 缓存；不传则不缓存 */
    cacheKey: { type: String, default: "" },
});

/** 与 vclist.face 一致：card | table */
const mface = defineModel({ type: String, default: VIEW_TABLE });

const emit = defineEmits(["toggled"]);

const isCard = computed(() => isCardView(mface));

const toggleTitle = computed(() =>
    isCard.value ? props.switchToListTitle : props.switchToCardTitle
);
const toggleIcon = computed(() =>
    isCard.value ? "fa fa-list" : "fa fa-th-large"
);
const toggleLabel = computed(() =>
    isCard.value ? props.listTitle : props.cardTitle
);

function isValidFace(face) {
    return face === VIEW_CARD || face === VIEW_TABLE;
}

function restoreFaceFromCache() {
    if (!props.cacheKey) return;
    const cached = getFaceCache(props.cacheKey);
    if (isValidFace(cached)) {
        mface.value = cached;
    }
}

function onToggle() {
    const next = isCard.value ? VIEW_TABLE : VIEW_CARD;
    mface.value = next;
    if (props.cacheKey) {
        setFaceCache(props.cacheKey, next);
    }
    emit("toggled", next);
}

onMounted(() => {
    restoreFaceFromCache();
});
</script>

<style scoped>
/* 浅灰悬浮，不用 toolbar-btn-neutral，避免各页父级蓝色 hover 覆盖 */
.list-card-view-toggle-btn.el-button {
    --el-button-text-color: #344054;
    --el-button-bg-color: #fff;
    --el-button-border-color: #d0d5dd;
    --el-button-hover-text-color: #1d2939;
    --el-button-hover-bg-color: #f9fafb;
    --el-button-hover-border-color: #c4cad4;
    --el-button-active-text-color: #1d2939;
    --el-button-active-bg-color: #f2f4f7;
    --el-button-active-border-color: #b9c0cc;
    --el-button-focus-text-color: #344054;
    --el-button-focus-bg-color: #fff;
    --el-button-focus-border-color: #d0d5dd;
}

/* mini：小图标、无边框 */
.list-card-view-toggle-btn.is-mini.el-button {
    width: 24px;
    height: 24px;
    min-height: 24px;
    padding: 0;
    margin: 0;
    border: none;
    border-color: transparent;
    background: transparent;
    box-shadow: none;
    --el-button-bg-color: transparent;
    --el-button-border-color: transparent;
    --el-button-hover-bg-color: transparent;
    --el-button-hover-border-color: transparent;
    --el-button-active-bg-color: transparent;
    --el-button-active-border-color: transparent;
    --el-button-focus-bg-color: transparent;
    --el-button-focus-border-color: transparent;
    --el-button-text-color: #667085;
    --el-button-hover-text-color: #1d2939;
    --el-button-active-text-color: #1d2939;
}
.list-card-view-toggle-btn.is-mini.el-button:hover,
.list-card-view-toggle-btn.is-mini.el-button:focus,
.list-card-view-toggle-btn.is-mini.el-button:active {
    border: none;
    background: transparent;
    box-shadow: none;
}
.list-card-view-toggle-btn.is-mini :deep(.svg-icon),
.list-card-view-toggle-btn.is-mini :deep(i) {
    font-size: 13px;
    width: 13px;
    height: 13px;
}
</style>
