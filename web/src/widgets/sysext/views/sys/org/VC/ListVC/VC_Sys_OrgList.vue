<template>
    <flexpan :flex="flex" :height="height">
        <template #header>
            <teleport :to="position" :disabled="!position">
                <el-button @click="onBtnAddClick" class="toolbar-btn-add-yellow"><SvgIcon name="fa fa-plus-circle" /> 添加</el-button>
            </teleport>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height" :flex="flex" v-loading="loading" :stripe="true">
                <vccol field="op" caption="操作" width="100px" align="center">
                    <template #default="cellp">
                        <el-link @click="openRow(cellp.row)" style="margin-right: 12px; color: #338fd1;"><SvgIcon name="fa fa-pencil-square-o" :size="18" /></el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)"><SvgIcon name="fa fa-trash-o" :size="18" /></el-link>
                    </template>
                </vccol>
                <vccol field="name" caption="名称" min-width="140px"></vccol>
                <vccol field="code" caption="编码" width="120px"></vccol>
                <vccol field="pid" caption="上级" width="120px"></vccol>
                <vccol field="orderNo" caption="排序" width="80px"></vccol>
                <vccol field="createTime" caption="创建时间" width="168px" type="datetime"></vccol>
            </vclist>
            <el-dialog v-model="diaEditOpened" width="520px" append-to-body>
                <template #header>
                    <div class="dialog-header">
                        <span>编辑组织</span>
                        <el-button class="toolbar-btn-primary" @click="onDialogSave"><SvgIcon name="fa fa-save" /> 保存</el-button>
                    </div>
                </template>
                <livevc name="VC_Sys_OrgEdit" :row="editingRow" @refresh="getlist" @close="closeRowEdit" :api="api" ref="editLiveVCRef" />
            </el-dialog>
        </template>
    </flexpan>
</template>

<script setup>
import { ref, onMounted, unref } from 'vue'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import livevc from "@/uiext/gene/live/livevc.vue"

const props = defineProps({
    api: null, page: null, bus: null, height: null, position: null,
    flex: { type: Boolean, default: false }, pageSize: { type: Number, default: 30 },
})
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const diaEditOpened = ref(false)
const editingRow = ref({})
const editLiveVCRef = ref(null)

const getlist = () => {
    loading.value = true
    props.api.queryBizSysOrglist({
        pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum,
        page: queryParams.value.pageNum,
    }).then((res) => {
        mlist.value = res.Items || res.records || []
        total.value = res.Total ?? res.total ?? 0
        loading.value = false
    }).catch(() => { loading.value = false })
}

onMounted(() => getlist())
function openRow(row) { editingRow.value = row; diaEditOpened.value = true }
function closeRowEdit() { editingRow.value = {}; diaEditOpened.value = false }
function onDialogSave() { unref(unref(editLiveVCRef.value)?.vcauto)?.saveChange?.() }
function onBtnAddClick() { editingRow.value = { orderNo: 0 }; diaEditOpened.value = true }
function removeRow(row) {
    alertIsOK("确认删除？", (e) => {
        if (e === "yes") props.api.removeBizSysOrgByIds([row.id]).then(() => getlist())
    })
}
</script>

<style scoped>
.dialog-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }
.toolbar-btn-primary { --el-button-text-color: #fff; --el-button-bg-color: #449FFB; }
.toolbar-btn-add-yellow { --el-button-text-color: #fff; --el-button-bg-color: #FEB75B; }
</style>
