<template>
  <div class="app-logo">
    <img :src="logoUrl" alt="logo" class="logo" />
    <div></div>
  </div>
</template>

<script setup>
const base = import.meta.env.BASE_URL || '/';
const logoUrl = `${base.endsWith('/') ? base : `${base}/`}images/logo.png`;
</script>

<style scoped>
.app-logo {
  --logo-color-300: rgb(var(--primary-300-color));
  --logo-color-400: rgb(var(--primary-400-color));
  --logo-color-500: rgb(var(--primary-500-color));
  --logo-color-600: rgb(var(--primary-600-color));
  --logo-color-700: rgb(var(--primary-700-color));
}
img.logo {
  width: auto;
  height: 40px;
  max-width: 120px;
}
</style>
