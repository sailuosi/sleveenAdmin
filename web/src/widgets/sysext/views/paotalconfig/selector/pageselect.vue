<template>
    <el-checkbox-group :model-value="checkIds" @update:modelValue="onput" class="deptmans" :max="myMax" :min="min">
        <div>
            <el-input v-model="searchValue" placeholder="请搜索姓名..." :style="{ width: '270px' }"></el-input>
            <el-button-group>
                <slot name="btn"></slot>
                <el-button @click="pre">上一页</el-button>
                <el-button @click="next">下一页</el-button>
            </el-button-group>
            <slot></slot>
        </div>

        <el-checkbox class="checkbox-line" v-for="(man, m) in filteredList" :label="man.sppId">
            <span>{{ m+1 }}</span>
            <span class="el-icon-s-custom" style="margin-left:16px;margin-right:4px;"></span>
            <span>{{ man.PP_Title }}</span>
            <span class="el-icon-s-home" style="margin-left:16px;margin-right:4px;"></span>
            <span>{{ man.PP_Dept }}/{{ man.PP_OrgName }}</span>
        </el-checkbox>

    </el-checkbox-group>
</template>

<script>

import { fetchSysPortPagePage } from "@s/api/paotou/paotalapi.js"


export default {
    name: 'pageselect',
    props: {
        deptCode: null,
        modelValue: null,
        max:null,
        min:null,
    },
    emits: ["change", "update:modelValue","load"],
    data() {
        return {
            mlist: [],
            checkIds: [],
            map: {},
            pageNum: 1,
            searchValue: '',
        };
    },
    computed: {
        listpara() {
            return {
                pageSize: 20,
                pageNum: this.pageNum,
                deptCode: this.deptCode
            };
        },
        filteredList() {
            return this.mlist.filter(man => {
                return man.PP_Title.toLowerCase().includes(this.searchValue.toLowerCase());
            });
        },
        myMax(){
            if(this.max==1){
                return void 0;
            }
            return this.max;
        },
    },
    mounted() {
        this.getList();
    },
    watch: {
        "listpara": function () {
            this.getList();
        }
    },
    methods: {
        pre() {
            if (this.pageNum > 1) {
                this.pageNum--;
            }
        },
        next() {
            this.pageNum++;
        },
        getList() {

            fetchSysPortPagePage(this.listpara).then((res) => {
                this.mlist = res.data;
                var obj = {};
                res.data.forEach((li => {
                    obj[li.sppId] = li;
                }))
                this.map = obj;
                this.$emit("load");
            })
        },
        getChecklist(e) {
            if(this.max==1){
                if(e.length>1){
                    //获取
                    e.forEach((li)=>{
                        if(this.checkIds.indexOf(li)==-1){
                            this.checkIds=[li];
                            return [this.map[li]];
                        }
                    })
                    return ;
                }
                else if(e.length==1){
                    this.checkIds=[e[0]];
                    return [this.map[e[0]]];
                }
                else{
                    this.checkIds=[];
                    return [];
                }
            }
            this.checkIds = e;
            return e.map((oid) => this.map[oid]);
        },
        onput(e) {
            
            var list = this.getChecklist(e);
            this.$emit("update:modelValue", list, e);
            this.$emit("change",list,e);
            
        },
    },
};
</script>

<style scoped>
.deptmans {
    display: grid;
    /* //grid-template-columns:1fr 1fr; */
}

.checkbox-line {
    border-bottom: 1px solid #ccc;
    padding-top: 10px;
    padding-bottom: 10px;
}
</style>
