<template>
  <el-dialog
    v-model="opened"
    :title="title"
    :width="width"
    :top="top"
    :destroy-on-close="destroyOnClose"
    :close-on-click-modal="closeOnClickModal"
    :class="[extraClass, customClass]"
    :style="maxWidth ? { maxWidth } : undefined"
    @closed="emit('close')"
  >
    <slot />
    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </el-dialog>
</template>

<script setup>
import { computed } from 'vue';

defineOptions({ name: 'MDialog' });

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: '' },
  width: { type: [String, Number], default: '50%' },
  maxWidth: { type: String, default: '' },
  top: { type: String, default: '15vh' },
  destroyOnClose: { type: Boolean, default: false },
  closeOnClickModal: { type: Boolean, default: true },
  extraClass: { type: String, default: '' },
  customClass: { type: String, default: '' }
});

const emit = defineEmits(['update:modelValue', 'close']);

const opened = computed({
  get: () => props.modelValue,
  set: v => emit('update:modelValue', v)
});
</script>
