<template>
    <el-popover
        v-model:visible="visible"
        trigger="click"
        placement="bottom-start"
        :width="660"
        :disabled="disabled"
        :show-arrow="false"
        transition="el-zoom-in-top"
        popper-class="business-date-range__popper"
    >
        <template #reference>
            <div class="business-date-range__input" :class="{ 'is-disabled': disabled, 'is-active': visible }">
                <SvgIcon name="fa fa-clock-o" :size="13" />
                <span :class="{ 'is-placeholder': !start }">{{ start || startPlaceholder }}</span>
                <span class="business-date-range__separator">至</span>
                <span :class="{ 'is-placeholder': !end }">{{ end || endPlaceholder }}</span>
            </div>
        </template>

        <div class="business-date-range__panel">
            <div v-if="showTime" class="business-date-range__editors">
                <el-input :model-value="startDate" readonly placeholder="开始日期" />
                <el-time-picker
                    :model-value="startTime"
                    value-format="HH:mm:ss"
                    format="HH:mm:ss"
                    placeholder="开始时间"
                    @update:model-value="setStartTime"
                />
                <span class="business-date-range__arrow">›</span>
                <el-input :model-value="endDate" readonly placeholder="结束日期" />
                <el-time-picker
                    :model-value="endTime"
                    value-format="HH:mm:ss"
                    format="HH:mm:ss"
                    placeholder="结束时间"
                    @update:model-value="setEndTime"
                />
            </div>

            <div class="business-date-range__calendars" @mousedown.prevent>
                <el-date-picker-panel
                    :model-value="startDate"
                    type="date"
                    value-format="YYYY-MM-DD"
                    :default-value="defaultValue"
                    :disabled-date="startDisabledDate"
                    :show-footer="false"
                    :border="false"
                    @update:model-value="setStartDate"
                />
                <div class="business-date-range__divider" aria-hidden="true"></div>
                <el-date-picker-panel
                    :model-value="endDate"
                    type="date"
                    value-format="YYYY-MM-DD"
                    :default-value="defaultValue"
                    :disabled-date="endDisabledDate"
                    :show-footer="false"
                    :border="false"
                    @update:model-value="setEndDate"
                />
            </div>

            <div class="business-date-range__footer">
                <el-button type="primary" size="small" @click="confirm">确定</el-button>
            </div>
        </div>
    </el-popover>
</template>

<script setup>
import { computed, ref } from 'vue';

const props = defineProps({
    disabled: { type: Boolean, default: false },
    defaultValue: { type: Date, default: null },
    showTime: { type: Boolean, default: false },
    disabledDateFn: { type: Function, default: () => false },
});

const emit = defineEmits(['change']);
const start = defineModel('start');
const end = defineModel('end');
const visible = ref(false);

const startDate = computed(() => getPart(start.value, 0));
const startTime = computed(() => getPart(start.value, 1) || '00:00:00');
const endDate = computed(() => getPart(end.value, 0));
const endTime = computed(() => getPart(end.value, 1) || '00:00:00');
const startPlaceholder = computed(() => props.showTime ? '开始时间' : '开始日期');
const endPlaceholder = computed(() => props.showTime ? '结束时间' : '结束日期');

function getPart(val, idx) {
    if (!val) return '';
    return String(val).replace('T', ' ').split(' ')[idx] || '';
}

function setStartDate(val) {
    start.value = val ? joinValue(val, startTime.value) : null;
}

function setEndDate(val) {
    end.value = val ? joinValue(val, endTime.value) : null;
}

function setStartTime(val) {
    if (startDate.value) start.value = joinValue(startDate.value, val || '00:00:00');
}

function setEndTime(val) {
    if (endDate.value) end.value = joinValue(endDate.value, val || '00:00:00');
}

function joinValue(date, time) {
    return props.showTime ? date + ' ' + time : date;
}

function startDisabledDate(time) {
    if (props.disabledDateFn(time)) return true;
    return !!endDate.value && time.getTime() > new Date(endDate.value).setHours(23, 59, 59, 999);
}

function endDisabledDate(time) {
    if (props.disabledDateFn(time)) return true;
    return !!startDate.value && time.getTime() < new Date(startDate.value).setHours(0, 0, 0, 0);
}

function confirm() {
    visible.value = false;
    emit('change', [start.value, end.value]);
}
</script>

<style scoped>
.business-date-range__input {
    display: grid;
    grid-template-columns: auto minmax(0, 1fr) auto minmax(0, 1fr);
    align-items: center;
    gap: 7px;
    width: 100%;
    min-height: 32px;
    padding: 0 10px;
    color: var(--el-text-color-regular);
    background: var(--el-fill-color-blank);
    box-shadow: 0 0 0 1px var(--el-border-color) inset;
    border-radius: 6px;
    cursor: pointer;
    box-sizing: border-box;
    transition: box-shadow var(--el-transition-duration-fast) ease;
}

.business-date-range__input:hover {
    box-shadow: 0 0 0 1px var(--el-border-color-hover) inset;
}

.business-date-range__input.is-active {
    box-shadow: 0 0 0 1px var(--el-color-primary) inset;
}

.business-date-range__input.is-disabled {
    color: var(--el-disabled-text-color);
    background: var(--el-disabled-bg-color);
    cursor: not-allowed;
}

.business-date-range__input span {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.business-date-range__input .is-placeholder,
.business-date-range__separator {
    color: var(--el-text-color-placeholder);
}

.business-date-range__editors {
    display: grid;
    grid-template-columns: 140px 140px 24px 140px 140px;
    align-items: center;
    justify-content: center;
    gap: 8px;
    padding: 10px 12px;
    border-bottom: 1px solid var(--el-border-color-light);
}

.business-date-range__editors :deep(.el-date-editor) {
    width: 140px;
}

.business-date-range__arrow {
    color: var(--el-text-color-placeholder);
    text-align: center;
}

.business-date-range__calendars {
    display: grid;
    grid-template-columns: 322px 12px 322px;
    justify-content: center;
}

.business-date-range__calendars :deep(.el-picker-panel) {
    width: 322px;
    border: 0;
    box-shadow: none;
}

.business-date-range__divider {
    width: 1px;
    height: 100%;
    justify-self: center;
    background: var(--el-border-color-light);
}

.business-date-range__footer {
    min-height: 40px;
    padding: 7px 10px;
    border-top: 1px solid var(--el-border-color-light);
    text-align: right;
    box-sizing: border-box;
}
</style>

<style>
.business-date-range__popper.el-popper {
    padding: 0;
    overflow: hidden;
    border: 1px solid var(--el-border-color);
    border-radius: var(--el-border-radius-base);
    box-shadow:
        0 4px 12px rgba(15, 23, 42, 0.16),
        0 14px 32px rgba(15, 23, 42, 0.18);
}

@media (prefers-reduced-motion: reduce) {
    .business-date-range__popper.el-zoom-in-top-enter-active,
    .business-date-range__popper.el-zoom-in-top-leave-active {
        transition-duration: 0.01ms;
    }
}
</style>
