<template>
    <mfaceToggle v-model="mfaceBridge" v-bind="forwardAttrs" @toggled="(v) => emit('toggled', v)" />
</template>

<script setup>
/**
 * @deprecated 请改用 mfaceToggle + v-model（modelValue）；保留 listViewFace 命名以兼容旧列表 VC。
 */
import { computed, useAttrs } from "vue";
import mfaceToggle from "./mfaceToggle.vue";
import { VIEW_CARD } from "./mfaceToggle.shared.js";

defineOptions({ inheritAttrs: false });

const listViewFace = defineModel("listViewFace", { type: String, default: "table" });
const emit = defineEmits(["toggled"]);
const forwardAttrs = useAttrs();

const mfaceBridge = computed({
    get() {
        const v = listViewFace.value;
        if (v === "list") return VIEW_CARD;
        return v === VIEW_CARD || v === "table" ? v : VIEW_CARD;
    },
    set(v) {
        listViewFace.value = v === VIEW_CARD ? "list" : v;
    },
});
</script>
