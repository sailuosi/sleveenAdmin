<!--
 * @description: 数据权限中的表单项
 * @author: liyubao | xufuxing
 * @version: 1.0
 * @updateDate:2021-09-28 获取字段的方式调整为按代码生成器定义的表结构
-->

<template>
<div class="rule-group-box flex-row">
    <div class="rule-groups flex-item flex-column">
        <div class="rule-head flex-row">
            <el-input v-model="ruleForm.Text" style="width: 120px" placeholder="名称" />：
            <el-input v-model="ruleForm.key" style="width: 120px" placeholder="分组编码" />
            <div class="flex-item">
                <el-select class="sellect-operation"  v-model="ruleForm.Operation" style="width: 80px">
                    <el-option v-for="item in operations" :key="item.value" :label="item.label" :value="item.value"></el-option>
                </el-select>
            </div>
            <el-button  class="VM" type="danger" icon="el-icon-delete" v-if=" parentForm&&isDelete" @click="deleteGroupRule(parentForm)">删除分组</el-button>
            <el-button  class="VM" type="primary" icon="el-icon-plus" @click="addGroupRule(ruleForm)">添加分组</el-button>
        </div>
        <div class="rule-form flex-item">
            <template v-for="(ru, ruleIndex) in ruleForm.Filters" >
                <el-form class="rule-op">
                    <el-input v-model="ru.Text" style="width: 120px" placeholder="名称" />：
                    <el-form-item class="fmitem">
                        <el-autocomplete  v-model="ru.Key"
                            :fetch-suggestions="querySearch"   
                            class="inline-input w-50"     placeholder="Please Input"
                            @select="selectField"
                        >
                        </el-autocomplete>
                    </el-form-item>
                    <el-form-item class="fmitem contra">
                        <el-select  v-model="ru.Contrast">
                            <el-option v-for="item in contrasts" :key="item.value" :label="item.label" :value="item.value"></el-option>
                        </el-select>
                    </el-form-item>
                    <el-form-item class="fmitem opval" >
                        <el-autocomplete  v-model="ru.Value"
                            :fetch-suggestions="querySearchVal"   
                            class="inline-input w-50"     placeholder="Please Input"
                            @select="selectFieldVal($event,ru)"
                        >
                            <template #default="p">
                                <div class="value">{{ p.item.currentName }}</div>
                            </template>
                        </el-autocomplete>
                        <span class="moreicon" @click="handleSelectUser(ru, ruleIndex)"><el-icon><More /></el-icon></span>
                        
                    </el-form-item>
                    <el-button type="danger"  icon="el-icon-delete" v-if="ruleForm.Filters.length > 1" @click="deleteRule(ruleForm, ruleIndex)"></el-button>
                    <el-button type="primary"  icon="el-icon-plus" v-if="ruleIndex === ruleForm.Filters.length - 1" @click="addRule(ruleForm)"></el-button>
                </el-form>
            </template>
            <el-button type="primary"  icon="el-icon-plus" v-if="!ruleForm.Filters||ruleForm.Filters?.length==0" @click="addRule(ruleForm)"></el-button>
            <template v-if="ruleForm.Children && ruleForm.Children.length > 0">
                <rule-item :level="index" v-for="(child, index) in ruleForm.Children" :propertyDatas="propertyDatas" :key="index" :rule="child" :parentForm="ruleForm"></rule-item>
            </template>
        </div>
    </div>
    <el-dialog width="850px" title="设置过滤器" v-model="selectDialog">
        <el-form>

            <fminput v-model="focusFilter.Text" label="名称" ></fminput>
            <fminput v-model="focusFilter.Key" label="字段" ></fminput>
            <fminput v-model="focusFilter.Contrast" label="操作符" type="dict" :dictlist="contrasts"></fminput>
            <fminput v-model="focusFilter.Value" label="值" type="textarea"></fminput>
        </el-form>
        <div style="text-align:right;margin-top: 10px;">
            <el-button size="small" type="cancel" @click="selectDialog = false">取消</el-button>
            <el-button size="small" type="primary" @click="handleSaveUsers">确定</el-button>
        </div>
    </el-dialog>
</div>
</template>

<script>
/**
<el-icon><More /></el-icon>
                        <el-select  v-model="ru.Key" @change="handleChangeKey(ru)">
                            <el-option v-for="item in propertyData" :key="item.columnName" :label="item.comment" :value="item.columnName"></el-option>
                        </el-select>
                        <el-input :readonly="['CreateUserId', 'CreateUserName'].indexOf(ru.Key) >= 0 ? false : true"  style="text-overflow: ellipsis;padding-right: 30px;" v-model="ru.Text">
                            <template #append>
                                
                            </template>
                        </el-input>
                    <el-form-item v-else class="fmitem opval">
                        <el-popover ref="popover4" placement="bottom" width="250" trigger="focus">
                            <el-input slot="reference"  v-model="ru.Value"></el-input>
                            <div>
                                <el-tag  style="margin: 0 10px 10px 0;cursor: pointer;" @click="handleSelectCurrentName(item.columnName, ru, ruleIndex)" v-for="(item, index) in keys" :key="index">{{ item.currentName }}</el-tag>
                            </div>
                        </el-popover>
                    </el-form-item>
 */
//import { listToTreeSelect } from '@/utils'
//import * as login from '@/api/login'
//import * as users from '@/api/users'
//import * as roles from '@/api/roles'
import { More } from '@element-plus/icons-vue';
import fminput from "@/ui/common/input/fminput.vue"

const createFilter = (queryString) => {
  return (restaurant) => {
    return (
      restaurant.toLowerCase().indexOf(queryString.toLowerCase()) === 0
    )
  }
}

export default {
    name: 'ruleItem',
    components:{
        More,fminput
    },
    expose:["getValue"],
    props: {
        rule: Object,
        level: Number,
        parentForm: Object,
        isDelete: {
            type: Boolean,
            default: true
        },
        propertyDatas: Array
    },
    data() {
        return {
            contrasts: [{
                    label: '大于等于',
                    value: '>='
                },
                {
                    label: '小于等于',
                    value: '<='
                },
                {
                    label: '等于',
                    value: '=='
                },
                {
                    label: '不等于',
                    value: '!='
                },
                {
                    label: '包含',
                    value: 'contains'
                },
                {
                    label: '属于',
                    value: 'in'
                },
                {
                    label: '含有任意一个',
                    value: 'intersect'
                }
            ],
            operations: [{
                    label: '并且',
                    value: 'and'
                },
                {
                    label: '或者',
                    value: 'or'
                }
            ],
            keys: [{
                    comment: '{当前登录用户的角色}',
                    columnName: '{loginRole}',
                    currentName: '当前登录用户的角色'
                },
                {
                    comment: '{当前登录的用户}',
                    columnName: '{loginUser}',
                    currentName: '当前登录的用户'
                },
                {
                    comment: '{当前登录用户的部门}',
                    columnName: '{loginOrg}',
                    currentName: '当前登录用户的部门'
                }
            ],
            selectDialog: false,
            statusOptions: [{
                    key: true,
                    display_name: '停用'
                },
                {
                    key: false,
                    display_name: '正常'
                }
            ],
            userInfo: {
                listQuery: {
                    // 查询条件
                    page: 1,
                    limit: 15,
                    orgId: '',
                    key: undefined
                },
                selectIdsC: [],
                selectTextsC: []
            },
            roleInfo: {
                listQuery: {
                    page: 1,
                    limit: 15,
                    key: undefined
                }
            },
            comonInfo: {
                datas: [],
                total: 0,
                selectTexts: [],
                selectIds: [],
                loading: false
            },
            partDatas: [],
            orgsTree: [],
            ruleIndexInfo: {},
            loginKey: '',
            searchKey: '',
            ruleForm:{},
            testRule:{
                "Operation":"or",
                "Filters":[
                    {
                        "Key":"{loginRole}",
                        "Value":"09ee2ffa-7463-4938-ae0b-1cb4e80c7c13,77e6d0c3-f9e1-4933-92c3-c1c6eef75593",
                        "Contrast":"contains",
                        "names":"",
                        "Text":"管理员,神"
                    }
                ],
                "Children":[
                    {
                        "Operation":"and",
                        "Filters":[
                            {
                                "Key":"{loginRole}",
                                "Value":"0a7ebd0c-78d6-4fbc-8fbe-6fc25c3a932d",
                                "Contrast":"contains",
                                "Text":"测试"
                            },
                            {
                                "Key":"CreateUserId",
                                "Value":"{loginUser}",
                                "Contrast":"==",
                                "Text":""
                            }
                        ]
                    }
                ]
            },
            leftOpts:[],
            rightOpts:[
                {
                    comment: '{当前登录用户的角色}',
                    columnName: '{loginRole}',
                    currentName: '当前登录用户的角色'
                },
                {
                    comment: '{当前登录的用户}',
                    columnName: '{loginUser}',
                    currentName: '当前登录的用户'
                },
                {
                    comment: '{当前登录用户的部门}',
                    columnName: '{loginOrg}',
                    currentName: '当前登录用户的部门'
                }
            ],
            focusFilter:{},
        }
    },
    computed: {
        

        tableData() {
            if (this.loginKey === '{loginUser}') {
                return {
                    ...this.comonInfo,
                    ...this.userInfo
                }
            }
            return {
                ...this.comonInfo,
                ...this.roleInfo
            }
        },
        propertyData() {
            return this.propertyDatas && this.propertyDatas.concat(this.keys)
        },
        names() {
            let names = ''
            if (this.comonInfo.selectTexts.length > 0 || this.userInfo.selectTextsC.length > 0) {
                names = [...this.comonInfo.selectTexts, ...this.userInfo.selectTextsC].join(',')
            }
            return names
        }
    },
    watch: {
        "rule":function(){
            this.loadRule();
        }
    },
    mounted() {
        this.loadRule();
    },
    methods: {
        getValue(){
            return this.ruleForm;
        },
        loadRule(){
            if(!!this.rule){
                this.ruleForm=this.rule;
                return;
            }
            this.ruleForm=this.testRule;
        },
        //字段配置的搜索响应
        querySearch(queryString, cb){

            const results = queryString
                ? this.leftOpts.filter(createFilter(queryString))
                : this.leftOpts;
            cb(results)
        },
        selectField(item){
            console.log(item);
        },
        //字段值配置的响应。
        querySearchVal(queryString, cb){

            const results = queryString
                ? this.rightOpts.filter((li)=>{
                    if(li.currentName.indexOf(queryString)==-1){
                        return false;
                    }
                    return true;
                })
                : this.rightOpts;
            if(results.length==0){
                cb(this.rightOpts);
                return;
            }
            cb(results)
        },
        selectFieldVal(item,ru){
            console.log(item);
            ru.Text=item.currentName;
            ru.Value=item.columnName;
        },
        createKey(key) {
            return `${Math.random()}_${key}`
        },
        handleShowMore(key) {
            const keys = ['{loginUser}', '{loginRole}']
            if (keys.indexOf(key) > -1) {
                return true
            }
            return false
        },
        addGroupRule(item) {
            if(!Array.isArray(item.Children)){
                item.Children=[];
            }

            item.Children.push({
                Operation: 'or',
                Filters: [{
                    Key: '',
                    Value: '',
                    Contrast: '',
                    Text: ''
                }]
            })
        },
        deleteGroupRule(item) {
            item.Children.splice(this.level, 1)
            item.Children.length <= 0 && delete item['Children']
        },
        // key修改时
        handleChangeKey(item) {
            item.Value = ''
            item.Text = ''
            this.comonInfo.selectTexts = []
            this.comonInfo.selectIds = []
            this.userInfo.selectTextsC = []
            this.userInfo.selectIdsC = []
        },
        addRule(item) {
            if(!Array.isArray(item.Filters)){
                item.Filters=[];
            }
            item.Filters.push({
                Key: '',
                Value: '',
                Contrast: '',
                Text: ''
            })
        },
        deleteRule(item, index) {
            item.Filters.splice(index, 1)
        },
        // 通过部门获取用户
        handleNodeClick(data) {
            this.userInfo.listQuery.orgId = data.id
            this.userInfo.listQuery.page = 1
            this.groupData()
            this.getUserList()
        },
        // 搜索用户/角色
        handleSearchUser() {
            this.loadData()
        },
        // 获取全部用户
        getAllUsers() {
            this.userInfo.listQuery.orgId = ''
            this.userInfo.listQuery.page = 1
            this.getUserList()
        },
        // 分页查询用户/角色
        handlePageSearch(val) {
            this.loadData(val)
        },
        // 获取用户
        getUserList() {
            this.comonInfo.loading = true
            this.userInfo.listQuery.key = this.searchKey
            users.getList(this.userInfo.listQuery).then((response) => {
                this.comonInfo.datas = response.data
                this.comonInfo.total = response.count
                this.comonInfo.loading = false
                if (this.comonInfo.datas.length > 0) {
                    this.userInfo.selectTextsC = [...this.comonInfo.datas].filter((x) => this.comonInfo.selectTexts.indexOf(x.name) > -1).map((item) => item.name)
                    this.userInfo.selectIdsC = [...this.comonInfo.datas].filter((x) => this.comonInfo.selectIds.indexOf(x.id) > -1).map((item) => item.id)
                    if (this.comonInfo.selectTexts.length > 0) {
                        this.comonInfo.selectTexts = this.comonInfo.selectTexts.filter((item) => !this.comonInfo.datas.some((x) => x.name === item))
                    }
                    if (this.comonInfo.selectIds.length > 0) {
                        this.comonInfo.selectIds = this.comonInfo.selectIds.filter((item) => !this.comonInfo.datas.some((x) => x.id === item))
                    }
                }
                this.setSelectTable()
            })
        },
        groupData() {
            this.comonInfo.selectTexts = [...this.comonInfo.selectTexts, ...this.userInfo.selectTextsC]
            this.comonInfo.selectIds = [...this.comonInfo.selectIds, ...this.userInfo.selectIdsC]
        },
        // 获取部门信息
        getPartData() {
            login.getOrgs().then((response) => {
                this.partDatas = response.result.map(function (item) {
                    return {
                        id: item.id,
                        label: item.name,
                        parentId: item.parentId
                    }
                })
                var orgstmp = JSON.parse(JSON.stringify(this.partDatas))
                this.orgsTree = listToTreeSelect(orgstmp)
            })
        },
        // 弹出选择用户对话框
        handleSelectUser(item, index) {
            this.focusFilter=item;
            this.selectDialog = true
        },
        handleSelectCurrentName(key, rule) {
            this.selectDialog = false
            rule.Value = key
        },
        // 默认选中
        setSelectTable() {
            const selectIds = this.loginKey === '{loginUser}' ? this.userInfo.selectIdsC : this.comonInfo.selectIds
            this.$nextTick(() => {
                const rows = [...this.comonInfo.datas].filter((x) => [...selectIds].some((y) => y === x.id))
                rows.forEach((row) => {
                    this.$refs.multipleTable.toggleRowSelection(row)
                })
            })
        },

        // 加载数据
        loadData(page) {
            this.userInfo.listQuery.page = page || 1
            if (this.loginKey === '{loginUser}') {
                (!this.partDatas || this.partDatas.length <= 0) && this.getPartData()
                this.groupData()
                this.getUserList()
                return
            }
            this.getRoleList()
        },
        // 确认用户选择
        handleSaveUsers() {
            this.selectDialog = false
        },
        // 选择用户
        handleSelectionUser(val) {
            if (this.loginKey === '{loginUser}') {
                this.userInfo.selectTextsC = val.map((item) => item.name)
                this.userInfo.selectIdsC = val.map((item) => item.id)
                return
            }
            this.comonInfo.selectTexts = val.map((item) => item.name)
            this.comonInfo.selectIds = val.map((item) => item.id)
        },
        // 获取角色
        getRoleList() {
            this.comonInfo.loading = true
            this.roleInfo.listQuery.key = this.searchKey
            roles.getList(this.listQuery).then((response) => {
                this.comonInfo.datas = response.result
                this.comonInfo.loading = false
                this.setSelectTable()
            })
        }
    }
}
</script>

<style scoped >

.fmitem{
    display: inline-block;
    margin-bottom: 0;
    margin-right: 5px;
}
.contra{
    width:110px;
}
.opval{
    width:260px;
}
.moreicon{
    cursor:pointer;
}

.rule-group-box {
  border-radius: 5px;
  overflow: hidden;
  margin-bottom: 10px;

}
.add-rule-group {
    text-align: center;
    border: 1px solid #e4e4e4;
    border-radius: 0px 5px 5px 0;
    font-size: 60px;
    font-weight: bold;
    padding: 0 30px;
    border-left: 0;
    cursor: pointer;
    color: #999999;
}
.add-rule-group i:hover {
        color: #4a4b4b;
}
.rule-group-box .rule-groups .rule-head {
    padding: 5px 10px;
    background: #e4e4e4;
}
.rule-group-box .rule-groups .rule-form {
    padding-bottom: 0;
    border: 1px solid #e4e4e4;
    border-radius: 0 0 0 5px;
    border-top: 0;
    padding-left: 10px;
    padding-right: 10px;
}

.flex-item {
    flex: 1;
    flex-basis: 0%;
    overflow: auto;
}
.flex-row {
    flex-direction: row;
}
.flex-column, .flex-row {
    display: flex;
}
.flex-column {
    height: 100%;
    flex-direction: column;
    overflow: hidden;
}
.rule-op{
    margin:10px 0;
}
</style>
