<template>
    <div :class="['slimtable',queryable?'queryable':'simnom']" :style="panstyle">
        <div class="hpan" :style="hpanstyle">
            <table :style="tbstyle" class="slimtb">
                <caption>
                    <slot name="caption"></slot>
                </caption>
                <colgroup>
                    <col style="width: 15px;" v-if="draggable &&dragby=='bar'" class="dragbby"></col>
                    <col v-if="checkable"></col>
                    <col v-if="rowNum"></col>
                    <col v-for="(li,i) in cols" 
                        :style="{width:li.width,'min-width':li.minWidth}"
                        class="tbccol"></col>            
                </colgroup>

                <thead @click="InputEnd">
                    <tr v-for="(ro,r) in rendhead" :key="r">
                        <th style="width: 15px;" v-if="draggable && dragby=='bar'" class="dragbby"></th>
                        <th v-if="r==0 && checkable"
                            :rowspan="rendhead.length"
                            class="tbheadth checkbox">
                            <span :class="allChecked?'allchecked':'unallcheck'">
                                <svg class="icon" style="width: 1em; height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1847">
                                <path d="M64.212 64.223h511.9v63.767h-511.9z" fill="#0f589e" p-id="1848"></path>
                                <path d="M64.212 127.99h63.813v639.812H64.212z" fill="#0f589e" p-id="1849"></path>
                                <path d="M119.333 704.223h648.816v63.579H119.333zM704.137 447.896h64.012v255.987h-64.012zM896.174 319.996h64.181v639.562h-64.181zM256.05 895.639h640.124v63.919H256.05z" fill="#0f589e" p-id="1850"></path>
                                <path v-if="allChecked" d="M319.472 256.53l255.73 255.73-63.54 63.541-255.731-255.73zM894.612 64.71l63.122 63.121L574.255 511.31l-63.122-63.122z" fill="#c92a52" p-id="1851"></path>
                                </svg>	
                            </span>
                        </th>
                        <th v-if="r==0 && rowNum" 
                            class="tbheadth rownum"
                            :rowspan="rendhead.length"
                        > 序号</th>
                        <th v-for="(li,i) in ro" 
                            :key="'th'+r+i" 
                            :rowspan="li.rowspan==void 0?1:li.rowspan"
                            :colspan="li.colspan==void 0?1:li.colspan"
                            scope="col" 
                            :style="{width:li.width,'min-width':li.minWidth}"
                            class="tbheadth stcol">
                            <template v-if="queryable">
                                <queryth :caption="li.caption" :column="li"
                                    @query="sayRunQuery" @queryinput="onQueryRowChange"
                                ></queryth>
                            </template>
                            <template v-else> 
                                {{li.caption}}
                            </template>
                        </th>
                    </tr>
                </thead>
                <tbody >
                    <tr v-for="(li,i) in list"
                        :draggable="draggable && editing==false && dragby=='row'"
                        @dragstart="ondragstart($event,li,i)"
                        @dragover.prevent="ondragOver($event,li,i)" 
                        @dragend="ondragEnd($event,li,i)"
                        @drop="onDrop($event,li,i)"
                        :class="[dragged==li?'dragging':'nmd']"
                    >
                        <td style="width: 15px;" v-if="draggable &&dragby=='bar'" class="dragbby"
                            :draggable="draggable && dragby=='bar'"
                            @dragstart="ondragstart($event,li,i)"
                            @dragend="ondragEnd($event,li,i)"
                            @drop="onDrop($event,li,i)"
                        >
                            <div class="dragbar">&nbsp</div>
                        </td>
                        <td v-if="rowNum" class="slimcell rnum">{{ (i+1) }}</td>
                        <template v-for="(col,c) in cols">
                            <td  class="slimcell" v-if="spanResult[i][c].rowspan>0" 
                                :rowspan="spanResult[i][c].rowspan"
                                :colspan="spanResult[i][c].colspan"
                                @click="onclickCell(li,col,i,c)"
                                :style="{'text-align':col.valign,width:col.width,'min-width':col.minWidth}"

                                :draggable="draggable && dragby==col.field"
                                @dragstart="ondragstart($event,li,i)"
                                @dragend="ondragEnd($event,li,i)"
                                @drop="onDrop($event,li,i)"
                                >
                                <template v-if="!!spanResult[i][c].slot">
                                    <slot :name="spanResult[i][c].slot" :field="col.field" :row="li" :index="i" :col="c">
                                        <slimcell :row="li" :col="col" :editing="(editRIndex==i && editCIndex==c)" 
                                            @blur="loseInput" @change="oncellchange($event,li,col)"
                                        ></slimcell>
                                    </slot>
                                </template>
                                <template v-else-if="col.ctype=='slot'">
                                    <slot :name="col.slot" :field="col.field" :row="li" :index="i" :col="c">
                                        <slimcell :row="li" :col="col" :editing="(editRIndex==i && editCIndex==c)" @blur="loseInput"
                                            @change="oncellchange($event,li,col)"
                                        ></slimcell>
                                    </slot>
                                </template>
                                <template v-else>
                                    <slimcell :row="li" :col="col" :editing="(editRIndex==i && editCIndex==c)" 
                                        @blur="loseInput" @change="oncellchange($event,li,col)">
                                        <template #epend="ep">
                                            <slot :name="col.key+ 'pand'" v-bind="ep"></slot>
                                        </template>
                                        <template #txtAppend="ep">
                                            <slot :name="col.appendSlot" v-bind="ep"></slot>
                                        </template>
                                    </slimcell>
                                </template>
                                <slot name="td" :row="li" :col="col" :editing="(editRIndex==i && editCIndex==c)"></slot>
                            </td>
                        </template>
                        <slot name="tr" :row="li"></slot>
                    </tr>
                </tbody>
                <tfoot>
                    <slot name="tfoot"></slot>
                    <el-pagination v-if="showPaged"
                        :current-page="para.pageNum" 
                        :page-size="para.pageSize"
                        :layout="layout" class="slimpage"
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
                </tfoot>
            </table>
        </div>

    </div>
</template>

<script setup>
/**
 * 功能定位：
 * 1. 轻量 -- 不提供复杂的table特性如固定表头、分页等
 * 2. 编辑 -- 支持编辑，能够在单元格内快速进行修改；
 */


import { useSlots, useAttrs, provide, inject,computed,ref } from 'vue'
import slimcell from "./slimtbpart/slimcell.vue"
import queryth from "./slimtbpart/queryth.vue"



const props = defineProps({
    list:{type:Array,default:function(){return [];}},
    cols:{type:Array,default:function(){return [];}},
    theads:{type:Array,default:function(){return [];}},
    rowNum:{type:Boolean,default:false},
    checkable:{type:Boolean,default:false},
    editable:{type:Boolean,default:false},
    //单元格的合并计算方法，返回一个对象，包含rowspan和colspan属性
    spaning:{type:Function,default:null},
    // 编辑模式：1-点击单元格编辑；2-双击单元格编辑
    clickToEdit:{type:String,default:"1"},
    //样式控制
    tblayout:{type:String,default:"auto"},
    width:{type:String,default:"100%"},
    height:null,
    //拖动控制
    draggable:{ type:Boolean,default:false },
    //拖动分组，同一分组的元素可以相互拖入。
    group:{ type:String,default:"" },
    //拖动的触发目标： row --整行； bar -- 拖动条； col-- 某个列可拖动
    dragby:{ type:String,default:"bar" },
    //是否有查询行
    queryable:{ type: Boolean, default: false },
    //是否有分页栏
    paged:{ type: Boolean, default: false },
    para:null,
    total:null,
    layout: { type: String, default: "total,sizes, prev, pager, next, jumper,slot"},
    pageSizes: {    type: Array,    default() {      return [10, 20, 30, 50,100, 200, 300, 500]    }  },
})

const emit = defineEmits(['change', 'delete',"cellchange","queryinput","logicChange","query"])

const count = defineModel("count", { type: Number, default: 0 })

provide('path', '/project/')
const path = inject('path')

defineOptions({
    inheritAttrs: false,
    customOptions: {
        /* ... */
    }
})

const slots = useSlots()
const attrs = useAttrs()

const showPaged= computed(()=>{
    return props.paged || props.total>0;
})
/**** 布局样式控制*********************  */
const tbstyle= computed(()=>{
    var obj={
        'table-layout':props.tblayout,
    };
    //{'table-layout':tblayout,width:width}

    return obj; 
})
const panstyle= computed(()=>{
    var obj={};
    if(props.width){
        var w=props.width;
        if(isNaN(w)){
            obj.width=w;
        }
        else{
            obj.width=w+"px";
        }
    }
    return obj;
})
const hpanstyle= computed(()=>{
    var obj={};
    if(props.height){
        var h=props.height;
        if(isNaN(h)){
            obj.height=h;
        }
        else{
            obj.height=(h-1)+"px";
        }
    } 
    return obj;
})
/****   核心绘制数据体        ********** */
const rendhead=computed(()=>{
    var arr=[];
    if(Array.isArray(props.theads) && props.theads.length>0){
        return props.theads;
    }else{
        arr.push(props.cols);
    }
    return arr;
})

const spanResult= computed(()=>{
    var arr=[];
    var obj={
        rowspan:1,
        colspan:1,
    }; 
    if(typeof props.spaning=='function'){
        for(var i=0;i<props.list.length;i++){
            var row=props.list[i];
            var colarr=[];
            for(var j=0;j<props.cols.length;j++){
                var col=props.cols[j];
                obj=props.spaning(row,col,i,j);
                if(!!obj){
                    colarr.push(obj);
                }else{
                    colarr.push({
                        rowspan:1,
                        colspan:1,
                    });
                }
            }
            arr.push(colarr);
        }
    }
    else{
        for(var i=0;i<props.list.length;i++){
            var colarr=[];
            for(var j=0;j<props.cols.length;j++){
                colarr.push(obj);
            }
            arr.push(colarr);
        } 
    }
    return arr;
})


/************编辑控制功能********** */
const editRIndex= ref(-1);
const editCIndex= ref(-1);
function onclickCell(row,col,i,c){
    emit("rowclick", row, col, i,c);
    emit("update:focusRow", row, col, i,c);
    if(props.editable==false){return;} 
    if(props.clickToEdit=="1"){
        if(editRIndex.value==i && editCIndex.value==c){return;}
        editRIndex.value=i;
        editCIndex.value=c;
    }
}

function loseInput(){
    //editRIndex.value=-1;
    //editCIndex.value=-1;
}
function InputEnd(){
    editRIndex.value=-1;
    editCIndex.value=-1;
}

const editing= computed(()=>{
    if(props.editable==false){return false;}
    return (editRIndex.value!=-1 && editCIndex.value!=-1);
})

function oncellchange(e,row,col){
    emit("cellchange",e,row,col)
}
/**************拖动功能****************** */

// 按住的延迟判定逻辑
const pressTimer= ref(null);
const pressed=ref(false);

function onMouseDown(e,row,col,i,c){
    // 设置1秒后触发的定时器
    pressTimer.value = setTimeout(() => {
        console.log('长按1秒触发！');
        pressed.value=true;
        // 这里执行长按后的操作
    }, 1000);
}
function onMouseUp(e,row,col,i,c){
    // 清除定时器
    clearTimeout(pressTimer.value);
    pressed.value=false; 
}

const draggingIndex = ref(-1);
const dragging= ref(false);
const dragged= ref(null);
const draggingGroup = ref("");
const groupName=computed(()=>{
    if(!props.group||props.group=="none"){
        return "group"+ (Math.random()*100);
    }
    return props.group;
});
        ///拖动控制
function     ondragstart(e,row){
    draggingGroup.value=groupName.value;
    e.dataTransfer.effectAllowed = "move";//none/move/link/copyMove/copyLink/linkMove/all
    e.dataTransfer.setData("a",JSON.stringify({
        group:groupName.value,
        row:row,
    }) );
    //e.dataTransfer.setDragImage(image, xOffset, yOffset);
    //e.target.style.opacity=0;
    dragging.value=true;
    dragged.value=row;
    //console.log(e,this.groupName)
}
function ondragOver(e,row,r){
    //const data = e.dataTransfer.getData("a");
    //console.log(e,data);
    if(draggingGroup.value != groupName.value){
        e.dataTransfer.effectAllowed = "none"
        return ;
    }
    e.dataTransfer.effectAllowed = "move";
    changeOrder(r)
    draggingIndex.value=r;
}
function ondragEnd(e,row,r){
    
}
function onDrop(event) {
    const data = event.dataTransfer.getData("text/plain");
    //event.target.textContent = data;
    event.preventDefault();
    draggingIndex.value=-1;
    dragging.value=false;
    draggingGroup.value="";
    emit("sort",props.mlist);
    emit("update:list",props.mlist)
    pressed.value=false;
    dragged.value=null;
}
function changeOrder(newVal){
    var oldVal=draggingIndex.value;
    if(newVal==oldVal){
        return ;
    }
    if(newVal!=null){
        if(oldVal!=null && oldVal!=-1){
            //const myElement = this.$refs['drag'+oldVal][0]
            //setTimeout(() =>{myElement.style.opacity = '1'},0)
            if(newVal>oldVal){
                let f=props.list[oldVal]
                for(let i=oldVal;i<newVal;i++){
                    props.list[i]= props.list[i+1] // $set 触发视图更改
                }
                props.list[newVal]=f;
            }else{
                let f=props.list[oldVal]
                for(let i=oldVal;i>newVal;i--){
                    props.list[i]= props.list[i-1] // $set 触发视图更改
                }
                props.list[newVal]= f;
            }
            //const myElement1 = this.$refs['drag'+newVal][0]
            //setTimeout(() =>{myElement1.style.opacity = '0'},0)
        }
    }
}


/***************************查询行功能部分 */
const queryData=ref({});
function setQuery(id,info) {
    console.log("setQuery",id,info);
    queryData.value[id] = info;
    sayQueryChange();
}
function sayQueryChange(){
    var nowCondtions=[];

    var keys=Object.keys(queryData.value);
    keys.forEach(field=>{
        var has=false;
        var info= queryData.value[field];
        for(var i=0;i<nowCondtions.length;i++){
            var item=nowCondtions[i];
            if(item.field==field){
                item.op= info.op;
                item.value= info.value;
                has=true;
                break;
            }
        }
        if(!has){
            nowCondtions.push(info);
        }
    });
    emit("update:queryConditions",nowCondtions);    
}
function onQueryRowChange(e){
    console.log("onquerychange",e);
    setQuery(e.field,e);
    sayRunQuery(e);
}
function sayRunQuery(){  
    emit("query");
}
/***************************** */

function handleSizeChange(val) {
    var p=props.para;
    p.pageSize=val;
    emit('update:para', p)
    emit('query', p)
}
function    handleCurrentChange(val) {
    var p=props.para;
    p.pageNum=val;
    emit('update:para', p)
    emit('query', p)
}


defineExpose({

})

/** 水平滚动条 控制，让它始终位于 表体下方。
 条件： 传入表体高度
 */

</script>

<style scoped>
.slimtable{
    overflow-x: auto;
}
.hpan{
    overflow-y:auto;
}
.slimtb{
    width:100%;
    border-collapse:collapse;
    border:1px solid #ddd;
}
th {
  position: sticky;
  top: 0px;
  
  vertical-align: bottom;
}
.tbheadth{
    border:1px solid #ddd;
    padding:5px 0;
    background-color: #fafafa;
}
.slimcell{
    border:1px solid #ddd;
    padding:5px;
}
.tbheadth.rownum{
    width:50px;
}

tbody > tr:nth-of-type(even) {
  background-color: rgb(237 238 242);
}

td.rnum{
    max-width: 90px;
}

.dragbar{
    width:10px;
    height: 100%;
    cursor:move;
}
tr.dragging{
    border:2px #1e88e5 solid;
}

th.dragbby{

}

.slimtable.queryable>.hpan>table>thead>tr>th.tbheadth{
    padding-bottom: 0;
    padding-top: 0;
}

.slimpage{
    float: none;;
}

.slimtable tfoot{
    position: sticky;
    bottom: 0;
    background: #fff;
}
</style>