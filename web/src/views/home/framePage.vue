<script setup lang="ts">

import { useRoute } from 'vue-router'
import { computed } from 'vue'

interface Props {
  url: string;
}

defineProps<Props>();
const route = useRoute()
const url = computed(() => route.meta.isLink)

</script>

<template>
  <div class="h-full">
    <iframe id="iframePage" class="size-full" :src="url"></iframe>
  </div>
</template>

<style scoped></style>
