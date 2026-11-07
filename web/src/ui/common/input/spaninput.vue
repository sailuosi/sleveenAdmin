<template>
    <span v-if="disabled" class="spaninput spaninput--readonly">{{ displayValue }}</span>
    <input
        v-else
        class="spaninput spaninput--edit"
        :class="themeClass"
        :type="type"
        :value="modelValue"
        :style="widthStyle"
        @input="onInput"
        @blur="emit('blur', $event)"
    />
</template>

<script setup>
import { computed } from 'vue'

defineOptions({ name: 'spaninput' })

const emit = defineEmits(['blur'])

const modelValue = defineModel({ default: '' })

const props = defineProps({
    disabled: { type: Boolean, default: false },
    type: { type: String, default: 'text' },
    width: { type: [String, Number], default: undefined },
    theme: { type: String, default: undefined },
})

const themeClass = computed(() => {
    var res=[];
    if (!!props.theme) res.push(props.theme);
    return res;
})

const displayValue = computed(() => {
    const v = modelValue.value
    return v == null ? '' : v
})

const widthStyle = computed(() => {
    if (props.width === undefined || props.width === null || props.width === '') {
        return undefined
    }
    const w = isNaN(props.width) ? props.width : `${props.width}px`
    return { width: w }
})

function onInput(e) {
    modelValue.value = e.target.value
}
</script>

<style scoped>


.spaninput--readonly {
    display: inline;
    word-break: break-all;
}

.spaninput--edit {
    display: inline-block;
    margin: 0;
    padding: 0;
    border: none;
    outline: none;
    background: transparent;
    min-width: 0;
}




.spaninput--edit.uline {
    border-bottom: 1px solid gray;
    padding-bottom: 1px;
    border-radius: 0;
}
</style>
