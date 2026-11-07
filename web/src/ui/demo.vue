<template>
<div>
    <template #[it]="data" v-for="it in Object.keys(slots).filter((p) => p != 'default')">
      <slot :name="it" v-bind="data || {}"></slot>
    </template>
</div>
</template>

<script setup>
import { useSlots, useAttrs,provide,inject } from 'vue'

const props = defineProps({
  foo: String
})

const emit = defineEmits(['change', 'delete'])

const count = defineModel("count", { type: Number, default: 0 })

provide('path', '/project/')
const path = inject('path')

defineOptions({
  inheritAttrs: false,
  customOptions: {
    /* ... */
  }
})

const slots = useSlots()
const attrs = useAttrs()

const vMyDirective = {
  beforeMount: (el) => {
    // 在元素上做些操作
  }
}

defineExpose({

})
</script>
