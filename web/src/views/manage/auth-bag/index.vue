<script setup lang="tsx">
import { ElMessage, ElButton, ElPopconfirm, ElSpace } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import {
  fetchAuthDutyPage,
  fetchAuthGoodsBagPage,
  fetchAuthVisitorBagPage,
  fetchAuthWordBagPage,
  fetchAuthWordDefinePage,
  fetchCleanAuthCache,
  fetchDeleteAuthDuty,
  fetchDeleteAuthGoodsBag,
  fetchDeleteAuthVisitorBag,
  fetchDeleteAuthWordBag,
  fetchDeleteAuthWordDefine,
  type AuthDutyItem,
  type AuthGoodsBag,
  type AuthVisitorBag,
  type AuthWordBag,
  type AuthWordDefine
} from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';
import DutyEditor from './components/DutyEditor.vue';
import VisitorBagEditor from './components/VisitorBagEditor.vue';
import GoodsBagEditor from './components/GoodsBagEditor.vue';
import WordBagEditor from './components/WordBagEditor.vue';
import WordDefineEditor from './components/WordDefineEditor.vue';

const message = ElMessage;
const activeTab = ref('duty');

const dutySearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const goodsSearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const visitorBagSearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const wordBagSearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const wordDefineSearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });

function makeTable<T>(
  api: () => Promise<FlatResponseData<any, Api.Common.PaginatingQueryRecord<T>>>,
  search: { page: number; pageSize: number },
  columns: () => NaiveUI.TableColumn<T>[]
) {
  return useNaivePaginatedTable<FlatResponseData<any, Api.Common.PaginatingQueryRecord<T>>, T>({
    api,
    transform: defaultTransform,
    onPaginationParamsChange: ({ page, pageSize }) => {
      search.page = page ?? 1;
      search.pageSize = pageSize ?? 10;
    },
    columns
  });
}

type EditorKind = 'duty' | 'visitor' | 'goods' | 'word' | 'wordDefine';
const drawerVisible = ref(false);
const drawerTitle = ref('');
const editorKind = ref<EditorKind>('duty');
const dutyModel = ref<AuthDutyItem | null>(null);
const visitorModel = ref<AuthVisitorBag | null>(null);
const goodsModel = ref<AuthGoodsBag | null>(null);
const wordModel = ref<AuthWordBag | null>(null);
const wordDefineModel = ref<AuthWordDefine | null>(null);

function openDuty(row?: AuthDutyItem) {
  editorKind.value = 'duty';
  dutyModel.value = row ? { ...row } : null;
  drawerTitle.value = row ? '编辑权限条目' : '新增权限条目';
  drawerVisible.value = true;
}

function openVisitor(row?: AuthVisitorBag) {
  editorKind.value = 'visitor';
  visitorModel.value = row ? { ...row } : null;
  drawerTitle.value = row ? '编辑访客集合' : '新增访客集合';
  drawerVisible.value = true;
}

function openGoods(row?: AuthGoodsBag) {
  editorKind.value = 'goods';
  goodsModel.value = row ? { ...row } : null;
  drawerTitle.value = row ? '编辑资源集合' : '新增资源集合';
  drawerVisible.value = true;
}

function openWord(row?: AuthWordBag) {
  editorKind.value = 'word';
  wordModel.value = row ? { ...row } : null;
  drawerTitle.value = row ? '编辑词条集合' : '新增词条集合';
  drawerVisible.value = true;
}

function openWordDefine(row?: AuthWordDefine) {
  editorKind.value = 'wordDefine';
  wordDefineModel.value = row ? { ...row } : null;
  drawerTitle.value = row ? '编辑词条定义' : '新增词条定义';
  drawerVisible.value = true;
}

function onEditorSaved() {
  drawerVisible.value = false;
  if (editorKind.value === 'duty') getDutyData();
  if (editorKind.value === 'visitor') getVisitorBagData();
  if (editorKind.value === 'goods') getGoodsData();
  if (editorKind.value === 'word') getWordBagData();
  if (editorKind.value === 'wordDefine') getWordDefineData();
}

const {
  columns: dutyColumns,
  data: dutyData,
  loading: dutyLoading,
  getData: getDutyData,
  mobilePagination: dutyPagination
} = makeTable(() => fetchAuthDutyPage(dutySearch), dutySearch, () => [
  { key: 'name', title: '名称', minWidth: 140 },
  { key: 'code', title: '编号', width: 120 },
  { key: 'visitorBagName', title: '访客集合', minWidth: 120 },
  { key: 'goodsBagName', title: '资源集合', minWidth: 120 },
  { key: 'wordBagName', title: '词条集合', minWidth: 120 },
  { key: 'idx', title: '排序', width: 70 },
  {
    key: 'operate',
    title: '操作',
    width: 160,
    render: (row: AuthDutyItem) => (
      <ElSpace>
        <ElButton size="small" onClick={() => openDuty(row)}>
          编辑
        </ElButton>
        <ElPopconfirm title={'确认删除?'} onConfirm={() => removeDuty(row.id!)}>{{ reference: () => (
              <ElButton size="small" type="danger">
                删除
              </ElButton>
            )
          }}
        </ElPopconfirm>
      </ElSpace>
    )
  }
]);

const {
  columns: goodsColumns,
  data: goodsData,
  loading: goodsLoading,
  getData: getGoodsData,
  mobilePagination: goodsPagination
} = makeTable(() => fetchAuthGoodsBagPage(goodsSearch), goodsSearch, () => [
  { key: 'name', title: '名称', minWidth: 140 },
  { key: 'code', title: '编号', width: 120 },
  { key: 'note', title: '说明', minWidth: 160 },
  { key: 'menuBindCount', title: '已绑菜单', width: 90 },
  { key: 'idx', title: '排序', width: 70 },
  {
    key: 'operate',
    title: '操作',
    width: 160,
    render: (row: AuthGoodsBag) => (
      <ElSpace>
        <ElButton size="small" onClick={() => openGoods(row)}>
          编辑
        </ElButton>
        <ElPopconfirm title={'确认删除?'} onConfirm={() => removeGoods(row.id!)}>{{ reference: () => (
              <ElButton size="small" type="danger">
                删除
              </ElButton>
            )
          }}
        </ElPopconfirm>
      </ElSpace>
    )
  }
]);

const {
  columns: visitorBagColumns,
  data: visitorBagData,
  loading: visitorBagLoading,
  getData: getVisitorBagData,
  mobilePagination: visitorBagPagination
} = makeTable(() => fetchAuthVisitorBagPage(visitorBagSearch), visitorBagSearch, () => [
  { key: 'name', title: '名称', minWidth: 140 },
  { key: 'code', title: '编号', width: 120 },
  { key: 'note', title: '描述', minWidth: 160 },
  { key: 'keyWord', title: '关键成员', minWidth: 120 },
  { key: 'idx', title: '排序', width: 70 },
  {
    key: 'operate',
    title: '操作',
    width: 160,
    render: (row: AuthVisitorBag) => (
      <ElSpace>
        <ElButton size="small" onClick={() => openVisitor(row)}>
          编辑
        </ElButton>
        <ElPopconfirm title={'确认删除?'} onConfirm={() => removeVisitorBag(row.id!)}>{{ reference: () => (
              <ElButton size="small" type="danger">
                删除
              </ElButton>
            )
          }}
        </ElPopconfirm>
      </ElSpace>
    )
  }
]);

const {
  columns: wordBagColumns,
  data: wordBagData,
  loading: wordBagLoading,
  getData: getWordBagData,
  mobilePagination: wordBagPagination
} = makeTable(() => fetchAuthWordBagPage(wordBagSearch), wordBagSearch, () => [
  { key: 'name', title: '名称', minWidth: 140 },
  { key: 'code', title: '编号', width: 120 },
  { key: 'note', title: '描述', minWidth: 160 },
  { key: 'type', title: '类型', width: 80 },
  {
    key: 'operate',
    title: '操作',
    width: 160,
    render: (row: AuthWordBag) => (
      <ElSpace>
        <ElButton size="small" onClick={() => openWord(row)}>
          编辑
        </ElButton>
        <ElPopconfirm title={'确认删除?'} onConfirm={() => removeWordBag(row.id!)}>{{ reference: () => (
              <ElButton size="small" type="danger">
                删除
              </ElButton>
            )
          }}
        </ElPopconfirm>
      </ElSpace>
    )
  }
]);

const {
  columns: wordDefineColumns,
  data: wordDefineData,
  loading: wordDefineLoading,
  getData: getWordDefineData,
  mobilePagination: wordDefinePagination
} = makeTable(() => fetchAuthWordDefinePage(wordDefineSearch), wordDefineSearch, () => [
  { key: 'name', title: '名称', minWidth: 140 },
  { key: 'code', title: '编号', width: 120 },
  { key: 'type', title: '类型', width: 80 },
  { key: 'isOn', title: '启用', width: 70 },
  {
    key: 'operate',
    title: '操作',
    width: 160,
    render: (row: AuthWordDefine) => (
      <ElSpace>
        <ElButton size="small" onClick={() => openWordDefine(row)}>
          编辑
        </ElButton>
        <ElPopconfirm title={'确认删除?'} onConfirm={() => removeWordDefine(row.id!)}>{{ reference: () => (
              <ElButton size="small" type="danger">
                删除
              </ElButton>
            )
          }}
        </ElPopconfirm>
      </ElSpace>
    )
  }
]);

async function removeDuty(id: string) {
  await fetchDeleteAuthDuty(id);
  getDutyData();
}
async function removeGoods(id: string) {
  await fetchDeleteAuthGoodsBag(id);
  getGoodsData();
}
async function removeVisitorBag(id: string) {
  await fetchDeleteAuthVisitorBag(id);
  getVisitorBagData();
}
async function removeWordBag(id: string) {
  await fetchDeleteAuthWordBag(id);
  getWordBagData();
}
async function removeWordDefine(id: string) {
  await fetchDeleteAuthWordDefine(id);
  getWordDefineData();
}

const cacheOptions = [
  { label: '清理访客/组织缓存', key: '1' },
  { label: '清理职责/角色缓存', key: '2' }
];

async function handleCleanCache(key: string) {
  const { data } = await fetchCleanAuthCache(key);
  ElMessage.success(`已清理 ${data?.count ?? 0} 项缓存`);
}

watch(activeTab, tab => {
  if (tab === 'duty') getDutyData();
  if (tab === 'goods') getGoodsData();
  if (tab === 'visitorBag') getVisitorBagData();
  if (tab === 'wordBag') getWordBagData();
  if (tab === 'wordDefine') getWordDefineData();
});

getDutyData();
</script>

<template>
  <div class="p-16px">
    <ElCard title="权限配置 (AuthBag)" shadow="never">
      <ElTabs v-model="activeTab" type="line">
        <ElTabPane name="duty" label="权限条目">
          <ElSpace class="mb-12px">
            <ElInput v-model="dutySearch.keyword" placeholder="搜索名称/编号" clearable style="width: 220px" />
            <ElButton type="primary" @click="getDutyData()">查询</ElButton>
            <ElButton type="warning" @click="openDuty()">新增</ElButton>
            <ElDropdown @command="handleCleanCache">
              <ElButton>缓存清理</ElButton>
              <template #dropdown>
                <ElDropdownMenu>
                  <ElDropdownItem v-for="item in cacheOptions" :key="item.key" :command="item.key">
                    {{ item.label }}
                  </ElDropdownItem>
                </ElDropdownMenu>
              </template>
            </ElDropdown>
          </ElSpace>
          <TableData
            :columns="dutyColumns"
            :data="dutyData"
            :loading="dutyLoading"
            :pagination="dutyPagination"
          />
        </ElTabPane>

        <ElTabPane name="goods" label="资源集合">
          <ElSpace class="mb-12px">
            <ElInput v-model="goodsSearch.keyword" placeholder="搜索" clearable style="width: 220px" />
            <ElButton type="primary" @click="getGoodsData()">查询</ElButton>
            <ElButton type="warning" @click="openGoods()">新增</ElButton>
          </ElSpace>
          <TableData
            :columns="goodsColumns"
            :data="goodsData"
            :loading="goodsLoading"
            :pagination="goodsPagination"
          />
        </ElTabPane>

        <ElTabPane name="visitorBag" label="访客集合">
          <ElSpace class="mb-12px">
            <ElInput
              v-model="visitorBagSearch.keyword"
              placeholder="搜索"
              clearable
              style="width: 220px"
            />
            <ElButton type="primary" @click="getVisitorBagData()">查询</ElButton>
            <ElButton type="warning" @click="openVisitor()">新增</ElButton>
          </ElSpace>
          <TableData
            :columns="visitorBagColumns"
            :data="visitorBagData"
            :loading="visitorBagLoading"
            :pagination="visitorBagPagination"
          />
        </ElTabPane>

        <ElTabPane name="wordBag" label="词条集合">
          <ElSpace class="mb-12px">
            <ElInput v-model="wordBagSearch.keyword" placeholder="搜索" clearable style="width: 220px" />
            <ElButton type="primary" @click="getWordBagData()">查询</ElButton>
            <ElButton type="warning" @click="openWord()">新增</ElButton>
          </ElSpace>
          <TableData
            :columns="wordBagColumns"
            :data="wordBagData"
            :loading="wordBagLoading"
            :pagination="wordBagPagination"
          />
        </ElTabPane>

        <ElTabPane name="wordDefine" label="词条定义">
          <ElSpace class="mb-12px">
            <ElInput
              v-model="wordDefineSearch.keyword"
              placeholder="搜索"
              clearable
              style="width: 220px"
            />
            <ElButton type="primary" @click="getWordDefineData()">查询</ElButton>
            <ElButton type="warning" @click="openWordDefine()">新增</ElButton>
          </ElSpace>
          <TableData
            :columns="wordDefineColumns"
            :data="wordDefineData"
            :loading="wordDefineLoading"
            :pagination="wordDefinePagination"
          />
        </ElTabPane>
      </ElTabs>
    </ElCard>

    <ElDrawer v-model="drawerVisible" size="640px" direction="rtl" :title="drawerTitle">
      <DutyEditor
          v-if="editorKind === 'duty'"
          :model="dutyModel"
          @saved="onEditorSaved"
          @cancel="drawerVisible = false"
        />
        <VisitorBagEditor
          v-else-if="editorKind === 'visitor'"
          :model="visitorModel"
          @saved="onEditorSaved"
          @changed="getVisitorBagData"
          @cancel="drawerVisible = false"
        />
        <GoodsBagEditor
          v-else-if="editorKind === 'goods'"
          :model="goodsModel"
          @saved="onEditorSaved"
          @changed="getGoodsData"
          @cancel="drawerVisible = false"
        />
        <WordBagEditor
          v-else-if="editorKind === 'word'"
          :model="wordModel"
          @saved="onEditorSaved"
          @changed="getWordBagData"
          @cancel="drawerVisible = false"
        />
        <WordDefineEditor
          v-else-if="editorKind === 'wordDefine'"
          :model="wordDefineModel"
          @saved="onEditorSaved"
          @cancel="drawerVisible = false"
        />
    </ElDrawer>
  </div>
</template>
