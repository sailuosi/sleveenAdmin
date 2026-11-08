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
                :flex="flex" v-loading="loading" query :maxLine="1"
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                :orderCol="true" :stripe="true">
                <vccol field="op" caption="操作" width="100px" type="text" disabled align="center" headerAlign="center">
                    <template #default="cellp">
                        <el-link @click="openRow(cellp.row)" style="margin-right: 20px; color: #338fd1;">
                            <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                        </el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)">
                            <SvgIcon name="fa fa-trash-o" :size="18" />
                        </el-link>
                    </template>
                </vccol>
                <vccol field="account" caption="账号" width="140px" type="text"></vccol>
                <vccol field="realName" caption="姓名" width="120px" type="text"></vccol>
                <vccol field="status" caption="状态" width="80px" type="text"></vccol>
                <vccol field="orgId" caption="组织" min-width="120px" type="text"></vccol>
                <vccol field="tenantId" caption="租户" min-width="120px" type="text"></vccol>
                <vccol field="createTime" caption="创建时间" width="168px" type="datetime"></vccol>
            </vclist>
            <el-dialog v-model="diaEditOpened" width="560px" append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="dialog-header">
                        <span>编辑用户</span>
                        <el-button class="toolbar-action-btn toolbar-btn-primary" @click="onDialogSave">
                            <SvgIcon name="fa fa-save" /> 保存
                        </el-button>
                    </div>
                </template>
                <livevc name="VC_Sys_UserEdit" :row="editingRow" @close="closeRowEdit" @refresh="getlist"
                    :api="api" :bus="bus" :page="page" ref="editLiveVCRef" />
            </el-dialog>
        </template>
    </flexpan>
</template>

<script setup>
import { ref, computed, onMounted, unref } from 'vue'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import livevc from "@/uiext/gene/live/livevc.vue"
import useListEditor from "@/uiext/gene/hook/listeditor.js"

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, flex: { type: Boolean, default: false },
    pageSize: { type: Number, default: 30 },
})
const listHolder = useListEditor()
const VCName = "VC_Sys_UserList"
const oidField = "id"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const queryRowCondtions = ref([])
const diaEditOpened = ref(false)
const editingRow = ref({})
const editLiveVCRef = ref(null)

const listpara = computed(() => ({
    pageSize: queryParams.value.pageSize,
    pageNum: queryParams.value.pageNum,
    page: queryParams.value.pageNum,
    keyword: queryParams.value.keyword,
}))

const getlist = () => {
    loading.value = true
    props.api.queryBizSysUserlist(listpara.value).then((res) => {
        mlist.value = res.Items || res.records || res.Records || []
        total.value = res.Total ?? res.total ?? 0
        listHolder.init(mlist.value, oidField)
        loading.value = false
    }).catch((err) => {
        swal({ text: err.message, icon: "error", button: "关闭" })
        loading.value = false
    })
}

const removeRow = (row) => {
    alertIsOK("确认删除该用户？", (e) => {
        if (e === "yes") {
            props.api.removeBizSysUserByIds([row[oidField]]).then(() => {
                getlist()
                swal({ text: "删除成功", icon: "success", button: "关闭" })
            })
        }
    }, "删除用户")
}

onMounted(() => {
    if (props.autoLoad) getlist()
})

function openRow(row) { editingRow.value = row; diaEditOpened.value = true }
function closeRowEdit() { editingRow.value = {}; diaEditOpened.value = false }
function onDialogSave() {
    const inner = unref(unref(editLiveVCRef.value)?.vcauto)
    inner?.saveChange?.()
}
function onBtnAddClick() {
    editingRow.value = { status: 1 }
    diaEditOpened.value = true
}
function setFocusRow() {}
</script>

<style lang="css" scoped>
.vc-top-toolbar .toolbar-action-btn, .dialog-header .toolbar-action-btn {
    height: 32px; padding: 0 12px; border-radius: 3px; font-size: 14px; font-weight: 500;
}
.toolbar-btn-add-yellow {
    --el-button-text-color: #fff; --el-button-bg-color: #FEB75B; --el-button-border-color: #FEB75B;
}
.toolbar-btn-primary {
    --el-button-text-color: #fff; --el-button-bg-color: #449FFB; --el-button-border-color: #449FFB;
}
.dialog-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }
</style>
