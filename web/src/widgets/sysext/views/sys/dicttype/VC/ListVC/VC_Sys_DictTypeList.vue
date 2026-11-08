<template>
    <flexpan :flex="flex" :height="height">
        <template #header>
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 添加
                    </el-button>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :flex="flex" v-loading="loading" query :stripe="true">
                <vccol field="op" caption="操作" width="160px" align="center">
                    <template #default="cellp">
                        <el-link @click="openData(cellp.row)" style="margin-right: 12px;">字典项</el-link>
                        <el-link @click="openRow(cellp.row)" style="margin-right: 12px; color: #338fd1;">
                            <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                        </el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)">
                            <SvgIcon name="fa fa-trash-o" :size="18" />
                        </el-link>
                    </template>
                </vccol>
                <vccol field="code" caption="编码" width="160px"></vccol>
                <vccol field="name" caption="名称" min-width="160px"></vccol>
                <vccol field="remark" caption="备注" min-width="160px"></vccol>
            </vclist>
            <el-dialog v-model="diaEditOpened" width="520px" append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="dialog-header">
                        <span>编辑字典类型</span>
                        <el-button class="toolbar-btn-primary" @click="onDialogSave"><SvgIcon name="fa fa-save" /> 保存</el-button>
                    </div>
                </template>
                <livevc name="VC_Sys_DictTypeEdit" :row="editingRow" @close="closeRowEdit" @refresh="getlist"
                    :api="api" ref="editLiveVCRef" />
            </el-dialog>
            <el-drawer v-model="diaDataOpened" size="60%" :title="dataTitle">
                <VC_Sys_DictDataList v-if="diaDataOpened && activeTypeId"
                    :api="dataApi" :filters="{ typeId: activeTypeId }" :childed="true" :autoLoad="true" height="100%" :flex="true" />
            </el-drawer>
        </template>
    </flexpan>
</template>

<script setup>
import { ref, computed, onMounted, unref } from 'vue'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import livevc from "@/uiext/gene/live/livevc.vue"
import VC_Sys_DictDataList from "../../../dictdata/VC/ListVC/VC_Sys_DictDataList.vue"

const props = defineProps({
    page: null, api: null, dataApi: null, bus: null, height: null,
    hideToolbar: { type: Boolean, default: false }, autoLoad: { type: Boolean, default: true },
    position: null, flex: { type: Boolean, default: false }, pageSize: { type: Number, default: 30 },
})
const oidField = "id"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const diaEditOpened = ref(false)
const editingRow = ref({})
const editLiveVCRef = ref(null)
const diaDataOpened = ref(false)
const activeTypeId = ref(null)
const activeTypeName = ref('')

const dataTitle = computed(() => activeTypeName.value ? `字典项 — ${activeTypeName.value}` : '字典项')

const getlist = () => {
    loading.value = true
    props.api.queryBizSysDictTypelist({
        pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum,
        page: queryParams.value.pageNum,
    }).then((res) => {
        mlist.value = res.Items || res.records || []
        total.value = res.Total ?? res.total ?? 0
        loading.value = false
    }).catch(() => { loading.value = false })
}

function openData(row) {
    activeTypeId.value = row[oidField]
    activeTypeName.value = row.name || row.code
    diaDataOpened.value = true
}

const removeRow = (row) => {
    alertIsOK("确认删除？", (e) => {
        if (e === "yes") {
            props.api.removeBizSysDictTypeByIds([row[oidField]]).then(() => getlist())
        }
    })
}

onMounted(() => { if (props.autoLoad) getlist() })
function openRow(row) { editingRow.value = row; diaEditOpened.value = true }
function closeRowEdit() { editingRow.value = {}; diaEditOpened.value = false }
function onDialogSave() {
    unref(unref(editLiveVCRef.value)?.vcauto)?.saveChange?.()
}
function onBtnAddClick() { editingRow.value = {}; diaEditOpened.value = true }
</script>

<style scoped>
.dialog-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }
.toolbar-btn-primary { --el-button-text-color: #fff; --el-button-bg-color: #449FFB; --el-button-border-color: #449FFB; }
</style>
