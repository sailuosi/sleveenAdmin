<template>
<el-pagination class="vcpager"
      :background="background"
      :current-page="curPage" :small="small"
      :page-size="pageSize"
      :layout="layout"
      :page-sizes="pageSizes"
      :total="total"
      @update:page-size="handleSizeChange"
      @update:current-page="handleCurrentChange"
    >
    <template #default>
        <span @click="refresh" class="el-button" style="margin-left: 15px;display: inline-flex;align-items: center;">
            <svg class="icon" style="width: 1em; height: 1em;vertical-align: top;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="2499"><path d="M928 416l-256-6.4 114.688-114.112A347.776 347.776 0 0 0 512 160a352 352 0 1 0 311.872 512H896a416 416 0 1 1-62.016-423.232L923.072 160z" fill="#333333" p-id="2500"></path></svg>                
        </span>
        <slot></slot>        
    </template>
</el-pagination>
</template>
<script>

import { scrollTo } from '@/ui/func/scroll-to.js'
export default {
    name:"vcpager",
    props: {
        vc:null,
        total: { type: Number, default: 0 },
        para: null,
        layout: { type: String, default: "total,sizes, prev, pager, next, jumper,slot"},
        background: {    type: Boolean,    default: true  },
        autoScroll: {    type: Boolean,    default: true  },
        pageSizes: {    type: Array,    default() {      return [10, 20, 30, 50,100, 200, 300, 500]    }  },
        small:{type:Boolean,default:false},
    },
    data: function () {
        return {
        };
    },
    computed:{
        curPage:{
            get() {
                return this.para.pageNum;
            },
        },
        pageSize:{
            get() {
                return this.para.pageSize
            },
        },
    },
    methods: {
        handlePageChange(e) {
            this.vc.$emit("query",e);
            //this.BC.GotoPage(e);
            //this.$message('正在加载数据...');
        },
        refresh() {
            this.vc.$emit("query");
            //this.vc.$emit("refresh");
            //this.BC.Refresh();
            //this.$message('正在刷新数据...');
        },
        handleSizeChange(val) {
                var p=this.para;
                p.pageSize=val;
                this.vc.$emit('update:para', p)
            this.vc.$emit('query', { pageSize: this.pageSize, pageNum: this.curPage })
            if (this.autoScroll) {
                scrollTo(0, 800)
            }
        },
        handleCurrentChange(val) {
            var p=this.para;
            p.pageNum=val;
            this.vc.$emit('update:para', p)
            this.vc.$emit('query', { pageSize: this.pageSize, pageNum: this.curPage })
            if (this.autoScroll) {
                scrollTo(0, 800)
            }
        }
    },
}
</script>
<style >
.el-pagination.vcpager{
    justify-content: right;
    text-align: right;
}
</style>
