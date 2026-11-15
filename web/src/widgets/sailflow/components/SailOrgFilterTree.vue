<template>
  <div class="sail-org-filter">
    <el-input v-model="filterText" clearable placeholder="筛选组织" size="small" class="sail-org-filter__input" />
    <el-tree
      ref="treeRef"
      class="sail-org-filter__tree"
      :data="treeData"
      node-key="id"
      highlight-current
      :props="{ label: 'label', children: 'children' }"
      :filter-node-method="filterNode"
      default-expand-all
      @node-click="onClick"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';

defineOptions({ name: 'SailOrgFilterTree' });

type OrgFlat = { id: string; parentId?: string | null; name?: string; label?: string };

const props = defineProps<{
  orgs?: OrgFlat[];
}>();

const emit = defineEmits<{ select: [unitId: string | null] }>();

const filterText = ref('');
const treeRef = ref();

const treeData = computed(() => buildTree(props.orgs || []));

watch(filterText, (v) => {
  treeRef.value?.filter?.(v);
});

function filterNode(value: string, data: { label?: string }) {
  if (!value) return true;
  return String(data.label || '').toLowerCase().includes(value.toLowerCase());
}

function onClick(data: { id?: string }) {
  emit('select', data?.id ? String(data.id) : null);
}

/** 扁平 → 树；无父或父不在集合内的为根（与后端种子清 Parent 一致） */
function buildTree(list: OrgFlat[]) {
  const map = new Map<string, any>();
  for (const o of list) {
    const id = String(o.id);
    if (!id) continue;
    map.set(id, {
      id,
      label: o.label || o.name || id,
      parentId: o.parentId || null,
      children: [] as any[]
    });
  }
  const roots: any[] = [];
  for (const node of map.values()) {
    const pid = node.parentId;
    if (pid && map.has(pid)) map.get(pid).children.push(node);
    else roots.push(node);
  }
  return roots;
}

defineExpose({
  clear() {
    treeRef.value?.setCurrentKey?.(null);
    emit('select', null);
  }
});
</script>

<style scoped>
.sail-org-filter {
  display: flex;
  flex-direction: column;
  min-height: 0;
  height: 100%;
}
.sail-org-filter__input {
  margin-bottom: 8px;
  flex: 0 0 auto;
}
.sail-org-filter__tree {
  flex: 1 1 auto;
  min-height: 0;
  overflow: auto;
}
</style>
