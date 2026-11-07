<template>
    <div class="attsim-shell">
        <div v-if="showHeader" class="attsim-header">
            <div class="attsim-header-left">
                <i class="fa fa-paperclip" aria-hidden="true"></i>
                <span class="attsim-header-title">{{ title }}</span>
                <span class="attsim-header-count">({{ count }}个)</span>
            </div>
            <div class="attsim-header-right">
                <slot name="upload-trigger" :empty="empty">
                    <a
                        v-if="!disabled"
                        href="javascript:void(0)"
                        class="attsim-upload-link"
                        @click.prevent="emit('upload-click')"
                    >
                        <i class="fa fa-cloud-upload" aria-hidden="true"></i>
                        上传附件
                    </a>
                </slot>
                <slot name="batch-download">
                    <a
                        v-if="showBatchDownloadBtn"
                        href="javascript:void(0)"
                        class="attsim-batch-down"
                        @click.prevent="emit('batch-download')"
                    >
                        <i class="fa fa-download" aria-hidden="true"></i>
                        批量下载
                    </a>
                </slot>
            </div>
        </div>

        <div class="attsim-list-wrap">
            <slot name="list"></slot>
            <div v-if="!disabled" class="attsim-upload-tip-wrap">
                <slot name="upload-tip"></slot>
            </div>
            <div v-if="showEmptyDecor" class="attsim-empty-wrap">
                <div class="hit-the-floor">{{ emptyText }}</div>
                <a
                    v-if="showLogo"
                    :href="logoHref"
                    class="dr-url"
                    target="_blank"
                    rel="noopener noreferrer"
                >
                    <img class="dr" :src="logoSrc" alt="logo" />
                </a>
            </div>
        </div>
    </div>
</template>

<script setup>
import { computed } from "vue";

const emit = defineEmits(["upload-click", "batch-download"]);

const props = defineProps({
    title: { type: String, default: "附件" },
    count: { type: Number, default: 0 },
    showHeader: { type: Boolean, default: true },
    showBatchDownload: { type: Boolean, default: true },
    disabled: { type: Boolean, default: false },
    empty: { type: Boolean, default: false },
    /** @deprecated 上传已固定在标题栏，保留兼容 */
    uploadInCard: { type: Boolean, default: false },
    /** @deprecated 上传已固定在标题栏，保留兼容 */
    compactUploadButton: { type: Boolean, default: false },
    /** @deprecated 上传已固定在标题栏，保留兼容 */
    alwaysCardUpload: { type: Boolean, default: false },
    showEmptyDecor: { type: Boolean, default: false },
    emptyText: { type: String, default: "暂无文件" },
    emptyUploadText: { type: String, default: "暂无文件，请上传" },
    continueUploadText: { type: String, default: "继续上传" },
    showLogo: { type: Boolean, default: true },
    logoHref: { type: String, default: "https://dribbble.com/YancyMin" },
    logoSrc: {
        type: String,
        default: "https://cdn.dribbble.com/assets/logo-footer-hd-a05db77841b4b27c0bf23ec1378e97c988190dfe7d26e32e1faea7269f9e001b.png",
    },
});

const showBatchDownloadBtn = computed(() => {
    return props.showBatchDownload && props.count > 0;
});
</script>

<style scoped>
.attsim-shell {
    position: relative;
    border: 1px solid #e8e8e8;
    border-radius: 3px;
    background: #fff;
    overflow: hidden;
}

.attsim-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 15px;
    color: #333;
    background-color: #f5f5f5;
    border-bottom: 1px solid #e8e8e8;
    border-top-left-radius: 3px;
    border-top-right-radius: 3px;
    font-size: 14px;
}

.attsim-header-left {
    display: flex;
    align-items: center;
    gap: 6px;
}

.attsim-header-left .fa-paperclip {
    color: #8c8c8c;
    font-size: 14px;
}

.attsim-header-title {
    font-weight: 500;
    color: #333;
}

.attsim-header-count {
    color: #8c8c8c;
}

.attsim-header-right {
    display: flex;
    align-items: center;
    gap: 16px;
}

.attsim-upload-link,
.attsim-batch-down {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    color: #1890ff;
    text-decoration: none;
    font-size: 13px;
    cursor: pointer;
}

.attsim-upload-link:hover,
.attsim-batch-down:hover {
    color: #40a9ff;
}

.attsim-list-wrap {
    background: #fff;
}

.attsim-upload-tip-wrap {
    padding: 0 15px 8px;
}

.attsim-empty-wrap {
    margin-top: 12px;
    min-height: 64px;
    position: relative;
    display:none;
}

.dr {
    position: absolute;
    bottom: 16px;
    right: 16px;
    width: 100px;
}

.hit-the-floor {
    color: #fff;
    font-size: 4em;
    font-weight: bold;
    font-family: Helvetica;
    text-align: center;
    text-shadow:
        0 1px 0 #ccc,
        0 2px 0 #c9c9c9,
        0 3px 0 #bbb,
        0 4px 0 #b9b9b9,
        0 5px 0 #aaa,
        0 6px 1px rgba(0, 0, 0, 0.1),
        0 0 5px rgba(0, 0, 0, 0.1),
        0 1px 3px rgba(0, 0, 0, 0.3),
        0 3px 5px rgba(0, 0, 0, 0.2),
        0 5px 10px rgba(0, 0, 0, 0.25),
        0 10px 10px rgba(0, 0, 0, 0.2),
        0 20px 20px rgba(0, 0, 0, 0.15);
}
</style>
