<template>
  <el-select v-model="model" multiple filterable placeholder="选择角色" style="width: 100%">
    <slot />
  </el-select>
</template>

<script>
export default {
  name: 'rolesearch',
  props: {
    modelValue: { type: Array, default: () => [] }
  },
  emits: ['update:modelValue'],
  computed: {
    model: {
      get() {
        return this.modelValue;
      },
      set(val) {
        this.$emit('update:modelValue', val);
      }
    }
  }
};
</script>
