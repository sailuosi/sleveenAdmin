<template>
<div>
    <div class="sh-bar">
        <el-input v-model="txt" placeholder="输入搜索关键字" @update:modelValue="load" />
        <slot></slot>
    </div>
    <div v-if="bodyType=='ul'" class="selectoptions" :style="bdstyle" v-loading="loading">
        <div v-for="(man,m) in options" @click="doselect(man)" :class="['searchli',checkedIds.indexOf(man[rowKey])==-1?'uncheck':'checked']">
            <slot name="option" :row="man" :index="m"></slot>
        </div>
        <el-empty description="无数据" v-if="!options || options.length==0" >
            <el-link @click="load">刷新</el-link>
        </el-empty>
    </div>
    <table border="1" v-if="bodyType=='table'" class="selectoptions" :style="bdstyle" v-loading="loading">
        <tr>
            <slot name="th"></slot>
        </tr>
        <tr v-for="(man,m) in options" @click="doselect(man)" :class="['searchli',checkedIds.indexOf(man[rowKey])==-1?'uncheck':'checked']">
            <slot name="tr" :row="man" :index="m"></slot>
        </tr>
    </table>
    <el-pagination v-if="page"  background layout="prev, pager, next" :total="total" @current-change="handlePageChange" />
</div>
</template>

<script>
/** v-loading="loading"
 * 任意选组件，UI绘制方式为直接绘制，无弹窗。
 * 任意人员选择器
 */
import { styleStrToObj } from "@/utils/style.js"

export default {
    name:"inputsearch",
    props:{
        multiple:{ type:Boolean,default:false, },
        onload:null,
        rowKey:{type:String,default:""},
        initLoad:{ type:Boolean,default:false, },
        height:null,
        orgoid:null,
        page:{ type:Boolean,default:false, },
        total:null,
        bodyStyle:{ type:String,default:"", },
        //下拉框类型，ul,table
        bodyType:{ type:String,default:"ul", },
    },
    emits:["change","input","update:modelValue"],
    data(){
        return{
            txt:"",
            options: [],
            checked:[],
            checkedIds:[],
            selectId:null,
            selectRow:null,
            map:{},
            currentPage:1,
            loading :true
        };
    },
    computed:{
        appver(){
            return false;
        },
        mystyle(){
            return {
                width:this.appver==false?'auto':"300px",
            }
        },
        bdstyle(){
            var t=styleStrToObj(this.bodyStyle);
            if(!!this.height){
                t.height=this.height;
            }
            return t;
        }
    },
    watch: {
        "orgoid":function(newVal){
            this.load();
        }
    },
    mounted(){
        if(this.initLoad){
            this.load();
        }
        else{
            this.loading=false;
        }
    },
    methods:{
        onchange(e){
            if(this.multiple){
                var val=[];
                if(Array.isArray(this.checked)){
                    val=this.checked.map((o)=>{
                        return this.map[o];
                    })
                }

                this.$emit("input",this.checked,this.checkedIds);
                this.$emit("update:modelValue",this.checked,this.checkedIds);
                this.$emit("change",this.checked,this.checkedIds);
            }
            else{
                this.$emit("input",this.selectRow,this.selectId);
                this.$emit("update:modelValue",this.selectRow,this.selectId);
                this.$emit("change",this.selectId,this.selectRow,this.selectId);
            }
        },
        doselect(row){
            var oid=row[this.rowKey];
            if(this.multiple){
                if(this.checkedIds.indexOf(oid)==-1){
                    this.checkedIds.push(oid);
                    this.checked.push(row);
                }
                else{
                    var index = this.checkedIds.indexOf(oid);
                    this.checkedIds.splice(index, 1);
                    this.checked.splice(index, 1);
                }
            }
            else{
                this.selectId=oid;
                this.selectRow=row;
                this.checkedIds=[oid];
            }

            this.onchange(row);
        },
        handlePageChange(newPage) {
           
            this.currentPage = newPage; // 更新当前页码
            this.changeSelectRestFul(this.txt); // 重新加载数据
        },
        load(){
            //console.log("load",this.txt);
            this.changeSelectRestFul(this.txt);
        },
        changeSelectRestFul(inputKey) {
            var vm=this;
            vm.loading=true;
            this.onload(inputKey,this.currentPage).then((data)=>{
                vm.loading=false;
                vm.options = data;
                data.forEach((man)=>{
                    vm.map[man[vm.rowKey]]=man;
                });
            })
            .catch((e)=>{
                vm.loading=false;
            });
        }
    }
}
</script>
<style scoped lang="scss">
.searchli{
    padding: 5px;
    border-bottom: 1px solid #eee;
    cursor: pointer;
    border-radius:5px;
}
.searchli:hover{
    background-color:#efefef;
}
.searchli.checked{
    background-color:#C2DEF1;
}
.sh-bar{
  display:flex;
  position: sticky;
  top: 0px;
}
:deep(.el-input__wrapper) {
  margin-right: 5px !important;
}
.selectoptions{
    height:var(--inputsearchsl-height);
    overflow:auto;
}

</style>
