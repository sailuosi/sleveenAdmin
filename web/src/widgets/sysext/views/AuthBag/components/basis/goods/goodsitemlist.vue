<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Gi_Name">
                    <el-input v-model="queryParams.Gi_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="编号" prop="Gi_Code">
                    <el-input v-model="queryParams.Gi_Code" placeholder="编号" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button  @click="resetQuery">重置</el-button>
                    <el-button  @click="openAdd">添加</el-button>
                    <el-dropdown @command="onRoleCommand">
                        <el-button>
                            角色设置<el-icon class="el-icon--right"><ArrowDown/></el-icon>
                        </el-button>
                        <template #dropdown>
                            <el-dropdown-menu>
                                <el-dropdown-item command="1">全部可用</el-dropdown-item>
                                <el-dropdown-item command="2">全部只读</el-dropdown-item>
                            </el-dropdown-menu>
                        </template>
                    </el-dropdown>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height">
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>
	            
	            <vccol field="Gi_Name" caption="名称"></vccol>
	            <vccol field="Gi_Code" caption="编号"></vccol>
	            <vccol field="Gi_Type" caption="类型"></vccol>
                <vccol field="Gi_Role" caption="角色" type="dict" dict="roleList"></vccol>
	            <vccol field="Gi_OID" caption="OID"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <goodsitemform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </goodsitemform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**  type="dict" dict="KB_HoGoodsItemType"
<vccol field="HH_GoodsItemOID" caption=""></vccol>
<vccol field="HH_GoodsBag_FK" caption="资源集合"></vccol>
 */


import { ArrowDown } from '@element-plus/icons-vue'
import { selectHHGoodsItemlist ,removeHHGoodsItem, batchSetHHGoodsItemRole} from "../../../api/goodsitemapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import goodsitemform from "./goodsitemform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,goodsitemform,ArrowDown
    },
    props:{
        bag:null,
    },
    emits:[],
    data(){
        return {
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
            roleList:[
                { label:"可用",value:"1"},
                { label:"只读",value:"2"},
            ],
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
				HH_GoodsBag_FK: this.bag.HH_GoodsBagOID
            },this.queryParams);
			return obj;
        },
    },
    watch:{
        "listpara":function(){
            this.getlist();
        }
    },
    mounted() {
        this.getlist();
    },
    methods:{
        getlist(){
           
            selectHHGoodsItemlist(this.listpara).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:30,
                pageNum:1,
            };
        },
        onclosefm(){
            this.diaRow=false;
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                HH_GoodsBag_FK:this.bag.HH_GoodsBagOID,
            };
            this.diaRow=true;
        },
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                removeHHGoodsItem(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
        onRoleCommand(role){
            const label = role === '1' ? '全部可用' : '全部只读';
            this.$confirm(`确定将当前资源集合下所有资源项设置为「${label}」吗？`, '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning',
            }).then(() => {
                batchSetHHGoodsItemRole({
                    HH_GoodsBag_FK: this.bag.HH_GoodsBagOID,
                    Gi_Role: role,
                }).then((res) => {
                    this.$message({ type: 'success', message: res.msg });
                    this.getlist();
                });
            });
        },
    }
}
</script>