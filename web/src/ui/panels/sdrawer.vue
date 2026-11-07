<template>
    <el-drawer :title="title" v-model="showMe" :direction="direction" :size="mySize" :append-to-body="bodyed" :destroy-on-close="destroyOnClose"
        @close="onclose" :customClass="customClass" :close-on-press-escape="!hold" :wrapperClosable="!hold" class="sframe"
    >
        <template #header>
            <span><slot name="title">{{title}}</slot></span>
            <el-button plain @click="toggleMaxed" style="border: none;">
                <template #icon>
                    <i :class="mymaxed?'far fa-window-restore':'far fa-square'"></i>
                </template>
            </el-button>
        </template>
        <template #default="pd">
            <slot v-bind="pd"></slot>
        </template>
        <template #footer>
            <slot name="footer"></slot>
        </template>
    </el-drawer>
</template>

<script setup>

import { ref, computed, watch } from 'vue'

defineOptions({ name: 'sdrawer' })

const props = defineProps({
    title: { type: String, default: '' },
    value: { type: Boolean, default: false }, // 是否显示，默认隐藏
    size: { type: String, default: '80vw' },

    destroyOnClose: { type: Boolean, default: false },
    hold: { type: Boolean, default: false },
    customClass: { type: String, default: '' },
    direction: { type: String, default: 'rtl' }, // 方向
    bodyed: { type: Boolean, default: true },
    maxable: { type: Boolean, default: true }, // 是否可以最大化
    maxed: { type: Boolean, default: false }, // 初始时是否最大化
})

const showMe=defineModel();

const emit = defineEmits(['input', 'close'])

const mlist = ref([])
const total = ref(0)
const mymaxed = defineModel('maxed',{default:false})

const mySize = computed(() => {
    if (mymaxed.value) {
        return '100%'
    }
    return props.size
})

function toggleMaxed() {
    mymaxed.value = !mymaxed.value
}

function onclose() {
    emit('input', false)
    emit('close')
}
</script>

<style>
.el-drawer__wrapper.sframe>.el-drawer__container>.el-drawer{
    display: flex;
}
.el-drawer__wrapper.sframe>.el-drawer__container>.el-drawer>.el-drawer__body{
    flex:1;
}
</style>