<script setup lang="tsx">
import { computed } from 'vue';

defineOptions({ name: 'TableData' });

type TablePagination = {
  page?: number;
  pageSize?: number;
  itemCount?: number;
  pageSizes?: number[];
  onUpdatePage?: (page: number) => void;
  onUpdatePageSize?: (pageSize: number) => void;
};

const props = withDefaults(
  defineProps<{
    columns: NaiveUI.TableColumn<any>[];
    data: any[];
    loading?: boolean;
    rowKey?: string | ((row: any) => string | number);
    pagination?: TablePagination | null;
    size?: 'small' | 'default' | 'large';
    /** 树表：传入后启用折叠展开（默认 children 字段） */
    tree?: boolean | { children?: string; hasChildren?: string };
    defaultExpandAll?: boolean;
  }>(),
  {
    loading: false,
    pagination: null,
    size: 'small',
    tree: false,
    defaultExpandAll: false
  }
);

const treeProps = computed(() => {
  if (!props.tree) return undefined;
  if (props.tree === true) return { children: 'children', hasChildren: 'hasChildren' };
  return {
    children: props.tree.children ?? 'children',
    hasChildren: props.tree.hasChildren ?? 'hasChildren'
  };
});

const checkedRowKeys = defineModel<Array<string | number>>('checkedRowKeys', { default: () => [] });

function getRowKey(row: Record<string, unknown>) {
  if (typeof props.rowKey === 'function') {
    return String(props.rowKey(row));
  }
  if (typeof props.rowKey === 'string') {
    return String(row[props.rowKey] ?? '');
  }
  return String(row.id ?? '');
}

const tableRowKey = computed(() => {
  if (typeof props.rowKey === 'string') return props.rowKey;
  return getRowKey;
});

function onSelectionChange(rows: Record<string, unknown>[]) {
  checkedRowKeys.value = rows.map(row => getRowKey(row));
}

function isHiddenColumn(column: NaiveUI.TableColumn<any>) {
  if (column.type === 'selection' || column.type === 'expand') return false;
  if ('key' in column && column.key === 'id' && column.width === 0) return true;
  return false;
}

function visibleColumns() {
  return props.columns.filter(col => !isHiddenColumn(col));
}

function columnWidth(column: NaiveUI.TableColumn<any>) {
  return column.width ?? undefined;
}

function columnMinWidth(column: NaiveUI.TableColumn<any>) {
  return column.minWidth ?? undefined;
}

function columnLabel(column: NaiveUI.TableColumn<any>) {
  return column.title ?? '';
}

function columnProp(column: NaiveUI.TableColumn<any>) {
  if ('key' in column && column.key) return String(column.key);
  return undefined;
}

function onPageChange(page: number) {
  props.pagination?.onUpdatePage?.(page);
}

function onPageSizeChange(pageSize: number) {
  props.pagination?.onUpdatePageSize?.(pageSize);
}
</script>

<template>
  <div v-loading="loading" class="table-data flex-col-stretch gap-12px">
    <ElTable
      :data="data"
      :row-key="tableRowKey"
      :size="size"
      :tree-props="treeProps"
      :default-expand-all="defaultExpandAll"
      border
      stripe
      class="sm:flex-1"
      @selection-change="onSelectionChange"
    >
      <template v-for="(column, index) in visibleColumns()" :key="index">
        <ElTableColumn
          v-if="column.type === 'selection'"
          type="selection"
          :reserve-selection="true"
          :width="columnWidth(column) ?? 48"
          :fixed="column.fixed === true || column.fixed === 'left' ? 'left' : column.fixed === 'right' ? 'right' : false"
        />
        <ElTableColumn
          v-else
          :prop="columnProp(column)"
          :label="columnLabel(column)"
          :width="columnWidth(column)"
          :min-width="columnMinWidth(column)"
          :fixed="column.fixed === true || column.fixed === 'left' ? 'left' : column.fixed === 'right' ? 'right' : false"
          :show-overflow-tooltip="Boolean(column.ellipsis)"
        >
          <template #default="{ row, $index }">
            <template v-if="typeof column.render === 'function'">
              <component :is="() => column.render!(row as never, $index)" />
            </template>
            <span v-else>{{ row[columnProp(column)!] }}</span>
          </template>
        </ElTableColumn>
      </template>
    </ElTable>
    <div v-if="pagination" class="flex justify-end pt-8px">
      <ElPagination
        :current-page="pagination.page ?? 1"
        :page-size="pagination.pageSize ?? 10"
        :total="pagination.itemCount ?? 0"
        :page-sizes="pagination.pageSizes ?? [10, 15, 20, 25, 30]"
        layout="total, sizes, prev, pager, next"
        background
        @update:current-page="onPageChange"
        @update:page-size="onPageSizeChange"
      />
    </div>
  </div>
</template>

<style scoped>
.table-data :deep(.el-table) {
  width: 100%;
}
</style>
