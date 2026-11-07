<template>
    <div ref="rootRef" class="mts" :class="{ 'is-disabled': disabled, 'is-focused': focused }">
        <div class="mts-box" @mousedown="onBoxMouseDown">
            <div class="mts-inner">
                <div v-if="tags.length" class="mts-tags">
                    <span
                        v-for="(item, index) in tags"
                        :key="itemKey(item, index)"
                        class="mts-tag"
                    >
                        <slot name="tag" :item="item" :index="index" :remove="() => removeAt(index)">
                            <span class="mts-tag-text">{{ itemLabel(item) }}</span>
                        </slot>
                        <button
                            type="button"
                            class="mts-tag-close"
                            tabindex="-1"
                            aria-label="移除"
                            :disabled="disabled"
                            @click.stop="removeAt(index)"
                        >
                            ×
                        </button>
                    </span>
                </div>
                <input
                    ref="inputRef"
                    v-model="query"
                    class="mts-input"
                    type="text"
                    :disabled="disabled"
                    :placeholder="inputPlaceholder"
                    autocomplete="off"
                    @focus="onInputFocus"
                    @blur="onInputBlur"
                    @input="onQueryInput"
                    @keydown="onInputKeydown"
                />
            </div>
            <div class="mts-divider" aria-hidden="true" />
            <button
                type="button"
                class="mts-more"
                tabindex="-1"
                :disabled="disabled"
                @click.stop="emitMore"
            >
                <span class="mts-more-dots">⋯</span>
            </button>
        </div>
        <transition name="mts-dd">
            <div
                v-show="dropdownVisible"
                class="mts-dropdown"
                role="listbox"
            >
                <div
                    v-for="(row, idx) in suggestions"
                    :key="itemKey(row, idx)"
                    class="mts-dd-item"
                    :class="{ 'is-active': idx === activeIndex }"
                    role="option"
                    @mousedown.prevent="pick(row)"
                >
                    <slot name="option" :item="row" :index="idx">
                        {{ itemLabel(row) }}
                    </slot>
                </div>
            </div>
        </transition>
    </div>
</template>

<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'

defineOptions({ name: 'MultiTagSearchInput' })

const props = defineProps({
    /** 已选条目，v-model */
    modelValue: { type: Array, default: () => [] },
    /** 当前搜索建议列表（由父组件根据 @search 结果传入） */
    suggestions: { type: Array, default: () => [] },
    /** 对象项时用于唯一标识与去重的字段 */
    valueKey: { type: String, default: 'id' },
    /** 对象项时用于标签与默认下拉文案的字段 */
    labelKey: { type: String, default: 'label' },
    placeholder: { type: String, default: '请输入搜索' },
    disabled: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue', 'search', 'more', 'select'])

const rootRef = ref(null)
const inputRef = ref(null)
const query = ref('')
const focused = ref(false)
const activeIndex = ref(-1)

const tags = computed(() => props.modelValue || [])

const inputPlaceholder = computed(() => (tags.value.length ? '' : props.placeholder))

const dropdownVisible = computed(() => {
    if (props.disabled || !focused.value) return false
    const q = query.value.trim()
    if (!q) return false
    return props.suggestions.length > 0
})

function itemKey(item, index) {
    if (item != null && typeof item === 'object' && props.valueKey in item) {
        return String(item[props.valueKey])
    }
    return String(item) + ':' + index
}

function itemLabel(item) {
    if (item == null) return ''
    if (typeof item !== 'object') return String(item)
    const v = item[props.labelKey]
    return v != null && v !== '' ? String(v) : String(item[props.valueKey] ?? '')
}

function valueOf(item) {
    if (item != null && typeof item === 'object' && props.valueKey in item) {
        return item[props.valueKey]
    }
    return item
}

function isDuplicate(candidate) {
    const k = valueOf(candidate)
    return tags.value.some((t) => valueOf(t) === k)
}

function removeAt(index) {
    if (props.disabled) return
    const next = tags.value.slice()
    next.splice(index, 1)
    emit('update:modelValue', next)
}

function pick(row) {
    if (props.disabled) return
    if (!isDuplicate(row)) {
        emit('update:modelValue', [...tags.value, row])
        emit('select', row)
    }
    query.value = ''
    activeIndex.value = -1
    emit('search', '')
    nextTick(() => inputRef.value?.focus())
}

function emitMore() {
    if (props.disabled) return
    emit('more')
}

function onBoxMouseDown(e) {
    if (props.disabled) return
    const t = e.target
    if (t.closest('.mts-more')) return
    if (t.closest('.mts-tag-close')) return
    const inp = inputRef.value
    if (inp && (t === inp || inp.contains(t))) return
    e.preventDefault()
    inp?.focus()
}

function onInputFocus() {
    focused.value = true
}

function onInputBlur() {
    focused.value = false
    activeIndex.value = -1
}

function onQueryInput() {
    emit('search', query.value)
}

function onInputKeydown(e) {
    if (!dropdownVisible.value) {
        if (e.key === 'Backspace' && !query.value && tags.value.length) {
            removeAt(tags.value.length - 1)
        }
        return
    }
    const len = props.suggestions.length
    if (e.key === 'ArrowDown') {
        e.preventDefault()
        activeIndex.value = activeIndex.value < len - 1 ? activeIndex.value + 1 : 0
    } else if (e.key === 'ArrowUp') {
        e.preventDefault()
        activeIndex.value = activeIndex.value > 0 ? activeIndex.value - 1 : len - 1
    } else if (e.key === 'Enter') {
        e.preventDefault()
        const i = activeIndex.value >= 0 ? activeIndex.value : 0
        if (len && i >= 0 && i < len) {
            pick(props.suggestions[i])
        }
    } else if (e.key === 'Escape') {
        e.preventDefault()
        query.value = ''
        emit('search', '')
        activeIndex.value = -1
    }
}

watch(
    () => props.suggestions,
    () => {
        activeIndex.value = props.suggestions.length ? 0 : -1
    },
)

function onDocMouseDown(e) {
    const el = rootRef.value
    if (!el || el.contains(e.target)) return
    query.value = ''
    emit('search', '')
    activeIndex.value = -1
}

onMounted(() => {
    document.addEventListener('mousedown', onDocMouseDown, true)
})

onBeforeUnmount(() => {
    document.removeEventListener('mousedown', onDocMouseDown, true)
})
</script>

<style scoped lang="scss">
.mts {
    position: relative;
    width: 100%;
    font-size: 14px;
    line-height: 1.5;
}

.mts-box {
    display: flex;
    align-items: stretch;
    min-height: 32px;
    padding: 2px 0 2px 8px;
    background: #fff;
    border: 1px solid #d9d9d9;
    border-radius: 4px;
    box-sizing: border-box;
    transition: border-color 0.2s, box-shadow 0.2s;
}

.mts-inner {
    flex: 1 1 auto;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;
    min-width: 0;
    padding-right: 4px;
}

.mts.is-focused .mts-box {
    border-color: #409eff;
    box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.15);
}

.mts.is-disabled .mts-box {
    background: #f5f7fa;
    cursor: not-allowed;
}

.mts-tags {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;
    max-width: 100%;
}

.mts-tag {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    max-width: 100%;
    padding: 0 2px 0 8px;
    background: #e6f7ff;
    border: 1px solid #91d5ff;
    border-radius: 4px;
    color: rgba(0, 0, 0, 0.88);
    font-size: 13px;
    line-height: 22px;
}

.mts-tag-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 200px;
}

.mts-tag-close {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 20px;
    height: 20px;
    margin: 0;
    padding: 0;
    border: none;
    background: transparent;
    color: rgba(0, 0, 0, 0.45);
    font-size: 16px;
    line-height: 1;
    cursor: pointer;
    border-radius: 2px;
}

.mts-tag-close:hover:not(:disabled) {
    color: #409eff;
    background: rgba(64, 158, 255, 0.12);
}

.mts-tag-close:disabled {
    cursor: not-allowed;
    opacity: 0.5;
}

.mts-input {
    flex: 1 1 80px;
    min-width: 60px;
    height: 26px;
    margin: 0;
    padding: 0 4px 0 0;
    border: none;
    outline: none;
    background: transparent;
    font-size: 14px;
    color: rgba(0, 0, 0, 0.88);
}

.mts-input::placeholder {
    color: #bfbfbf;
}

.mts-input:disabled {
    cursor: not-allowed;
}

.mts-divider {
    flex: 0 0 1px;
    width: 1px;
    align-self: stretch;
    margin: 4px 0;
    background: #d9d9d9;
}

.mts-more {
    flex: 0 0 36px;
    width: 36px;
    align-self: stretch;
    margin: -2px -1px -2px 0;
    padding: 0;
    border: none;
    border-radius: 0 3px 3px 0;
    background: #fff;
    cursor: pointer;
    color: rgba(0, 0, 0, 0.45);
}

.mts-more:hover:not(:disabled) {
    color: #409eff;
    background: #fafafa;
}

.mts-more:disabled {
    cursor: not-allowed;
    opacity: 0.5;
}

.mts-more-dots {
    display: block;
    font-size: 18px;
    line-height: 1;
    letter-spacing: 1px;
}

.mts-dropdown {
    position: absolute;
    left: 0;
    right: 0;
    top: calc(100% + 4px);
    z-index: 2000;
    max-height: 280px;
    overflow: auto;
    margin: 0;
    padding: 4px 0;
    background: #fff;
    border: 1px solid #d9d9d9;
    border-radius: 4px;
    box-shadow: 0 3px 12px rgba(0, 0, 0, 0.12);
}

.mts-dd-item {
    padding: 8px 12px;
    font-size: 13px;
    color: rgba(0, 0, 0, 0.88);
    cursor: pointer;
    line-height: 1.4;
}

.mts-dd-item:hover,
.mts-dd-item.is-active {
    background: #409eff;
    color: #fff;
}

.mts-dd-enter-active,
.mts-dd-leave-active {
    transition: opacity 0.12s ease, transform 0.12s ease;
}

.mts-dd-enter-from,
.mts-dd-leave-to {
    opacity: 0;
    transform: translateY(-4px);
}
</style>
