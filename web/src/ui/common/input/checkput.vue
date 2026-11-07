<template>
    <div>
        <div class="options-group" v-for="option in options">
            <el-checkbox :key="option.value" :label="option.label" :value="option.value"
                :model-value="selectedValue === option.value" @update:model-value="handleSelect(option.value)"
                class="checkbox-item">
                {{ option.label }}
            </el-checkbox>
        </div>
    </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({
    modelValue: null,
    options: Array
})

const emit = defineEmits(['change', 'update:modelValue'])
// 选中的值
const selectedValue = ref('')

// 处理选择事件
const handleSelect = (value) => {
    if (selectedValue.value === value) {
        // 如果点击已选中的选项，则取消选择
        selectedValue.value = ''
    } else {
        // 选中新的选项
        selectedValue.value = value
    }
    emit('update:modelValue', selectedValue.value)
}

// 获取选中项的标签
const getSelectedLabel = () => {
    const selectedOption = options.value.find(opt => opt.value === selectedValue.value)
    return selectedOption ? selectedOption.label : ''
}

// 监听选中值的变化（可选，用于调试或其他操作）
watch(selectedValue, (newVal) => {
    console.log('选中值发生变化:', newVal)
})
</script>

<style scoped>
.checkbox-radio-container {
    padding: 20px;
    max-width: 400px;
    margin: 0 auto;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.checkbox-radio-container h3 {
    margin-bottom: 20px;
    color: #303133;
    font-weight: 500;
}

.options-group {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.checkbox-item {
    padding: 8px 12px;
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    transition: all 0.3s ease;
}

.checkbox-item:hover {
    background-color: #f5f7fa;
    border-color: #c0c4cc;
}

.checkbox-item:deep(.el-checkbox__input.is-checked .el-checkbox__inner) {
    background-color: #409eff;
    border-color: #409eff;
}

.checkbox-item:deep(.el-checkbox__input.is-checked + .el-checkbox__label) {
    color: #409eff;
    font-weight: 500;
}

.selected-info {
    margin-top: 20px;
    padding: 12px;
    background-color: #f0f9ff;
    border: 1px solid #d1ecff;
    border-radius: 4px;
    color: #606266;
}

.selected-info p {
    margin: 0;
}
</style>