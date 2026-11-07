<template>
    <el-table-column fixed="left" width="44" align="center" class-name="vc-drag-col">
        <template #default>
            <span class="vc-drag-handle" title="拖动调整顺序">⋮⋮</span>
        </template>
    </el-table-column>
</template>

<script setup>
/**

 */
import { useSlots, useAttrs, provide, inject, onMounted ,onBeforeUnmount} from 'vue'
import Sortable from "sortablejs";


const props = defineProps({
    foo: String
})

const emit = defineEmits(['change', 'delete'])

const dragSortBody = ref(null);

function clearDragSort() {
    if (dragSortBody.value) {
        dragSortBody.value.destroy();
        dragSortBody.value = null;
    }
}

function tryInitDragSort() {
    // if (!props.dragable) {
    //     clearDragSort();
    //     return;
    // }
    nextTick(() => {
        console.log("tryInitDragSort");
        const t = table.value;
        const root = t && t.$el;
        if (!root) return;

        const ftbody = root.querySelector(".el-table__fixed-body-wrapper tbody");
        const tbody = root.querySelector(".el-table__body-wrapper tbody");
        /* el-table 固定列与主表各有一套 tr；两处同时绑 Sortable 会只拖动一侧 tbody，另一侧不同步，表现为列错位、合并行错乱。只绑一处：优先固定区（拖拽手柄在左侧固定列）。无固定列时用主表体。 */
        const target = ftbody || tbody;
        if (target && !dragSortBody.value) {
            dragSortBody.value = Sortable.create(target, {
                animation: 150,
                handle: ".vc-drag-handle",
                draggable: "tr",
                ghostClass: "vc-drag-ghost",
                onEnd: (evt) => {
                    const oi = evt && typeof evt.oldIndex === "number" ? evt.oldIndex : -1;
                    const ni = evt && typeof evt.newIndex === "number" ? evt.newIndex : -1;
                    if (oi < 0 || ni < 0 || oi === ni) return;
                    /** 供父组件在校验失败时撤回 Sortable 已移动的 tr，与数据顺序恢复一致 */
                    const revertDragDom = () => {
                        try {
                            const from = evt.from;
                            const item = evt.item;
                            if (!from || !item || !from.contains(item)) return;
                            if (oi < ni) {
                                from.insertBefore(item, from.children[oi] || null);
                            } else {
                                from.insertBefore(item, from.children[oi + 1] || null);
                            }
                        } catch (e) {
                            /* noop */
                        }
                    };
                    emit("drag-sort", oi, ni, revertDragDom);
                },
            });
        }
    });
}


onMounted(() => {
    //tryInitDragSort();
})
onBeforeUnmount(() => {
    //clearDragSort();
});

defineExpose({

})
</script>