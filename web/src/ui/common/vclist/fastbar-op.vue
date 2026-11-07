<template>
    <el-popover trigger="click" placement="bottom" :width="120">
        <el-radio-group v-model="logic" size="small" @change="onChange">
            <el-radio v-for="item in logicList" :key="item.value" :value="item.value">
                {{ item.label }}
            </el-radio>
        </el-radio-group>
        <template #reference>
            <span class="q-op">
                <svg
                    class="icon"
                    style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;"
                    viewBox="0 0 1024 1024"
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <path
                        d="M516.973714 950.857143a67.584 67.584 0 0 1-30.427428-7.606857l-111.616-53.248a64.073143 64.073143 0 0 1-38.034286-58.368V499.346286v-2.56L100.937143 179.712a65.609143 65.609143 0 0 1-5.046857-71.021714c10.166857-22.893714 32.914286-35.547429 58.368-35.547429h611.328c25.380571 0 48.274286 12.653714 58.368 35.547429 10.166857 22.820571 10.166857 48.201143-5.12 68.461714L582.948571 494.226286v388.169143c0 22.820571-12.653714 43.154286-30.427428 55.808a55.442286 55.442286 0 0 1-35.547429 12.653714z"
                        fill-opacity=".65"
                    />
                </svg>
            </span>
        </template>
    </el-popover>
</template>

<script setup>
defineOptions({
    name: 'FastbarOp',
})

const logicList = [
    { label: '默认', value: '' },
    { label: '包含', value: 'like' },
    { label: '以...开头', value: 'likeLeft' },
    { label: '等于', value: '=' },
    { label: '不等于', value: '<>' },
    { label: '大于', value: '>' },
    { label: '小于', value: '<' },
    { label: '大于等于', value: '>=' },
    { label: '小于等于', value: '<=' },
]

const logic = defineModel({ type: String, default: '' })

const emit = defineEmits(['change'])

function onChange() {
    emit('change', logic.value)
}
</script>

<style scoped>
.q-op {
    color: #999;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
}
.q-op:hover {
    color: #333;
}
</style>
