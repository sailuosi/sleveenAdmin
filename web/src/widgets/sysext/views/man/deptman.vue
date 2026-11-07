<template>
    <div>
        <div>
            <el-input v-model="searchValue" placeholder="请搜索姓名..." :style="{ width: '100px',marginRight:'10px' }"></el-input>
            <el-button-group>
                <slot name="btn"></slot>
                <el-button @click="pre">上一页</el-button>
                <el-button @click="next">下一页</el-button>
            </el-button-group>
            <slot></slot>
        </div>
        <el-checkbox v-model="checkAll" @change="handleCheckAllChange">
            全选
        </el-checkbox>
        <el-checkbox-group :model-value="checkIds" @update:modelValue="onput" class="deptmans">
            
            
            <el-checkbox class="checkbox-line" v-for="(man, m) in filteredList" :label="man.oid" :key="m">
                <span>{{ m+1 }}</span>
                <span class="el-icon-s-custom" style="margin-left:5px;margin-right:3px;"></span>
                <span>{{ man.ManName }}</span>
                <span class="el-icon-s-home" style="margin-left:10px;margin-right:3px;"></span>
                <span>{{ man.OrgName }}/{{ man.divName }}</span>
            </el-checkbox>

        </el-checkbox-group>
    </div>
    
</template>

<script>
import {
    loadDeptMan
} from "@s/api/system/mancommonapi.js"

export default {
    name: 'WebDeptman',
    props: {
        deptCode: null,
        modelValue: null,
    },
    emits: ["change", "update:modelValue","load"],
    data() {
        return {
            checkAll: false,
            isIndeterminate: false, // 添加这个属性
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
                pageSize: 10,
                pageNum: this.pageNum,
                deptCode: this.deptCode
            };
        },
        filteredList() {
            return this.mlist.filter(man => {
                return man.ManName.toLowerCase().includes(this.searchValue.toLowerCase());
            });
        }
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
        handleCheckAllChange(value) {
            // 更新全选状态
            this.checkAll = value;
            // 更新半选状态
            this.isIndeterminate = false;
            // 根据全选状态来设置 checkIds
            this.checkIds = value ? this.filteredList.map(item => item.oid) : [];
            // 确保触发更新外部绑定的 modelValue
            this.$emit('update:modelValue', this.getChecklist(this.checkIds), this.checkIds);
        },
        onput(newCheckIds) {
            // 更新全选状态
            this.checkAll = newCheckIds.length === this.filteredList.length;
            // 更新半选状态
            this.isIndeterminate = newCheckIds.length > 0 && newCheckIds.length < this.filteredList.length;
            // 更新内部的 checkIds
            this.checkIds = newCheckIds;
            // 确保触发更新外部绑定的 modelValue
            this.$emit('update:modelValue', this.getChecklist(this.checkIds), this.checkIds);
        },
        pre() {
            if (this.pageNum > 1) {
                this.pageNum--;
            }
        },
        next() {
            this.pageNum++;
        },
        getList() {

            loadDeptMan(this.listpara).then((res) => {
                this.mlist = res.data;
                var obj = {};
                res.data.forEach((li => {
                    obj[li.oid] = li;
                }))
                this.map = obj;
                this.$emit("load");
            })
        },
        getChecklist(e) {
            return e.map((oid) => this.map[oid]);
        },
        /*onput(e) {
            //console.log(e);
            var list = this.getChecklist(e);
            this.$emit("update:modelValue", list, e);
            this.checkIds = e;
        },*/
    },
};
</script>

<style scoped>
.deptmans {
    display: grid;
    /* //grid-template-columns:1fr 1fr; */
    position: sticky;
    top:0px;
}

.checkbox-line {
    border-bottom: 1px solid #ccc;
    padding-top: 10px;
    padding-bottom: 10px;
    box-sizing: border-box;
    overflow: hidden;
    white-space: nowrap;

}

.el-checkbox{
  margin-right: 0px;
}
</style>
