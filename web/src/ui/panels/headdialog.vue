<template>
	<el-dialog
		v-model="opened"
		:title="''"
		:width="resolvedWidth"
		:top="top"
		:fullscreen="fullscreen"
		:modal="modal"
		:append-to-body="appendToBody"
		:destroy-on-close="destroyOnClose"
		:close-on-click-modal="closeOnClickModal"
		:close-on-press-escape="closeOnPressEscape"
		:show-close="false"
		:draggable="false"
		:align-center="alignCenter"
		class="headless-dialog"
		:class="dialogClassList"
		@closed="onClosed"
		@close="onNativeClose"
	>
		<button
			v-if="showClose"
			type="button"
			class="headless-dialog__close"
			title="关闭"
			aria-label="关闭"
			@click.stop="close"
		>
			<svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
				<path
					fill="currentColor"
					d="M764.288 214.592 512 466.88 259.712 214.592a31.936 31.936 0 0 0-45.12 45.12L466.752 512 214.528 764.224a31.936 31.936 0 1 0 45.12 45.184L512 557.184l252.288 252.288a31.936 31.936 0 0 0 45.12-45.12L557.12 512.064l252.288-252.352a31.936 31.936 0 1 0-45.12-45.184z"
				/>
			</svg>
		</button>
		<div class="headless-dialog__body" :style="bodyStyle">
			<slot v-if="!bodyNotLive || opened" :close="close"></slot>
		</div>
		<template v-if="$slots.footer" #footer>
			<slot name="footer" :close="close"></slot>
		</template>
	</el-dialog>
</template>

<script setup>
/**
 * 无头弹窗：基于 el-dialog，无标题栏以最大化内容区；关闭按钮贴右侧边显示。
 *
 * @example
 * import headdialog from "@/ui/panels/headdialog.vue";
 * <headdialog v-model="visible" width="90vw" height="85vh">
 *   <YourContent />
 * </headdialog>
 */
import { computed } from "vue";

defineOptions({ name: "headdialog" });

const props = defineProps({
	/** 宽度；不传时可用 size 预设 */
	width: { type: String, default: "" },
	/** big / middle / small */
	size: { type: String, default: "" },
	top: { type: String, default: "5vh" },
	alignCenter: { type: Boolean, default: false },
	fullscreen: { type: Boolean, default: false },
	modal: { type: Boolean, default: true },
	appendToBody: { type: Boolean, default: true },
	destroyOnClose: { type: Boolean, default: false },
	closeOnClickModal: { type: Boolean, default: false },
	closeOnPressEscape: { type: Boolean, default: true },
	showClose: { type: Boolean, default: true },
	/** true 时关闭后不保留默认插槽内容 */
	bodyNotLive: { type: Boolean, default: false },
	/** 内容区高度，如 80vh；与 maxHeight 同时传时以 height 为准 */
	height: { type: String, default: "" },
	/** 内容区最大高度；无 height 时内容自然增高，超出后滚动 */
	maxHeight: { type: String, default: "" },
	customClass: { type: [String, Array, Object], default: "" },
});

const emit = defineEmits(["close", "closed"]);

const opened = defineModel({ type: Boolean, default: false });

const sizeWidthMap = {
	big: "90vw",
	middle: "70vw",
	small: "50vw",
};

const resolvedWidth = computed(() => {
	if (props.width) return props.width;
	if (props.size && sizeWidthMap[props.size]) return sizeWidthMap[props.size];
	return "80vw";
});

const dialogClassList = computed(() => {
	const list = [];
	if (props.size) list.push(`headless-dialog--${props.size}`);
	if (props.maxHeight && !props.height) list.push("headless-dialog--auto-height");
	if (props.customClass) list.push(props.customClass);
	return list;
});

const bodyStyle = computed(() => {
	const s = {};
	if (props.height) {
		s.height = props.height;
		s.maxHeight = props.height;
		return s;
	}
	if (props.maxHeight) {
		s.maxHeight = props.maxHeight;
	}
	return Object.keys(s).length ? s : undefined;
});

function close() {
	if (!opened.value) return;
	opened.value = false;
}

function onNativeClose() {
	emit("close");
}

function onClosed() {
	emit("closed");
}

defineExpose({ close, opened });
</script>

<style lang="scss">
/* el-dialog 根节点与 class 同级，需非 scoped */
.el-overlay-dialog:has(.el-dialog.headless-dialog) {
	overflow: visible;
}

.el-dialog.headless-dialog {
	padding: 0;
	overflow: visible;
	display: flex;
	flex-direction: column;
	border-radius: 10px;
	box-shadow: 0 16px 48px rgba(15, 23, 42, 0.18);
}

.el-dialog.headless-dialog > .el-dialog__header {
	display: none !important;
	height: 0 !important;
	padding: 0 !important;
	margin: 0 !important;
	border: none !important;
}

.el-dialog.headless-dialog > .el-dialog__body {
	position: relative;
	flex: 1 1 auto;
	min-height: 0;
	padding: 0 !important;
	margin: 0;
	/* 允许关闭钮伸出右侧外缘，内容滚动交给内层 */
	overflow: visible;
	display: flex;
	flex-direction: column;
	border-radius: 10px 10px 0 0;
}

.el-dialog.headless-dialog > .el-dialog__footer {
	padding: 10px 16px;
	border-top: 1px solid rgba(15, 23, 42, 0.06);
	border-radius: 0 0 10px 10px;
}

.el-dialog.headless-dialog .headless-dialog__body {
	flex: 1 1 auto;
	min-height: 0;
	width: 100%;
	box-sizing: border-box;
	overflow: auto;
	border-radius: 10px 10px 0 0;
}

.el-dialog.headless-dialog.headless-dialog--auto-height {
	height: auto;
	max-height: none;
}

.el-dialog.headless-dialog.headless-dialog--auto-height > .el-dialog__body {
	flex: 0 1 auto;
}

.el-dialog.headless-dialog.headless-dialog--auto-height .headless-dialog__body {
	flex: 0 1 auto;
}

/* 无 footer 时内容区四角统一 */
.el-dialog.headless-dialog:not(:has(> .el-dialog__footer)) > .el-dialog__body,
.el-dialog.headless-dialog:not(:has(> .el-dialog__footer)) .headless-dialog__body {
	border-radius: 10px;
}

/* 关闭钮：贴弹窗外右侧边缘（伸出对话框外） */
.el-dialog.headless-dialog .headless-dialog__close {
	position: absolute;
	top: 12px;
	right: -36px;
	z-index: 20;
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 36px;
	height: 36px;
	margin: 0;
	padding: 0;
	border: none;
	border-radius: 0 8px 8px 0;
	background: rgba(239, 68, 68, 0.92);
	color: #fff;
	cursor: pointer;
	box-shadow: 2px 2px 8px rgba(15, 23, 42, 0.14);
	transition: background-color 0.15s ease, transform 0.15s ease;
}

.el-dialog.headless-dialog .headless-dialog__close:hover {
	background: #dc2626;
	transform: translateX(1px);
}

.el-dialog.headless-dialog .headless-dialog__close:active {
	background: #b91c1c;
}

.el-dialog.headless-dialog .headless-dialog__close svg {
	width: 16px;
	height: 16px;
	display: block;
}

.el-dialog.headless-dialog--big {
	--headless-dialog-prefer-width: 90vw;
}
.el-dialog.headless-dialog--middle {
	--headless-dialog-prefer-width: 70vw;
}
.el-dialog.headless-dialog--small {
	--headless-dialog-prefer-width: 50vw;
}
</style>
