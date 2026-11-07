<template>
    <div  class="tags-pan" >
        <div class="tags">
            <el-tag v-for="(tag,t) in list" :key="t" :class="{stag:true,'dragging':t==draggingIndex}" :style="tagStyle"  :closable="closable" 
                :effect="t==focusIndex?'dark':'light'"
                @click="onTagClick(tag,t)"
                @dblclick="onDBTagClick(tag,t)"
                @close="removingEmp(tag)"
                
                :draggable="draggable"
                @dragstart="ondragstart($event,tag,t)"
                @dragover.prevent="ondragOver($event,tag,t)" 
                @dragend="ondragEnd($event,tag,t)"
                @drop="onDrop($event,tag,t)"
                >
                <slot :row="tag">{{tag[name]}}</slot>
            </el-tag>

            <el-popover  v-if="addable && action=='pop'"  ref="popoverRef"
                :placement="placement"
                :title="title"
                :width="width" effect="light"
                :trigger="trigger" :show="show" @input="$emit('popchange')"
                content="">
                <template #default="p">
                    <lazydiv mode="open">
                        <slot name="addpop" :resize="upDatePosition"></slot>
                    </lazydiv>
                </template>
                <template #reference>
                    <el-tag  type="warning" class="addtag" style="" @click="onclickadd" title="点击添加">
                        <span class="add-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M480 480V128a32 32 0 0 1 64 0v352h352a32 32 0 1 1 0 64H544v352a32 32 0 1 1-64 0V544H128a32 32 0 0 1 0-64z"></path></svg>
                        </span>{{addlabel}}
                    </el-tag>
                </template>
            </el-popover >
           
            <el-tag v-else-if="addable"  type="warning" class="addtag" style="" @click="onclickadd" title="点击添加">
                <span class="add-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M480 480V128a32 32 0 0 1 64 0v352h352a32 32 0 1 1 0 64H544v352a32 32 0 1 1-64 0V544H128a32 32 0 0 1 0-64z"></path></svg>
                        </span>{{addlabel}}
            </el-tag>

            <el-tag v-if="addable && clearable && list.length>0"   type="danger" class="addtag" style="margin-left:5px" @click="onclickDelete" title="清空">
                <span class="add-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" ><path fill="currentColor" d="M352 192V95.936a32 32 0 0 1 32-32h256a32 32 0 0 1 32 32V192h256a32 32 0 1 1 0 64H96a32 32 0 0 1 0-64zm64 0h192v-64H416zM192 960a32 32 0 0 1-32-32V256h704v672a32 32 0 0 1-32 32zm224-192a32 32 0 0 0 32-32V416a32 32 0 0 0-64 0v320a32 32 0 0 0 32 32m192 0a32 32 0 0 0 32-32V416a32 32 0 0 0-64 0v320a32 32 0 0 0 32 32"></path></svg>
                        </span>
            </el-tag>
            <slot name="tail"></slot>
        </div>
        <template v-if="position=='dialog'">
            <el-dialog :title="diatitle" :model-value="focused" :width="width" append-to-body @close="closeFocus">
                <slot name="focus" :row="focusTag" :focused="focused"></slot>
            </el-dialog>
        </template>
        <template v-else>
            <div v-if="keep || focused">
                <slot name="focus" :row="focusTag" :focused="focused"></slot>
            </div>
        </template>
        <template v-if="action=='dialog'">
            <el-dialog :title="title" :model-value="showAdd" :width="width" append-to-body @close="showAdd=false">
                <slot name="addpop" ></slot>
            </el-dialog>
        </template>
    </div>
</template>

<script>

import lazydiv from "@/ui/panels/lazydiv.vue"


export default {
    name:"tags",
    components:{
        lazydiv
    },
    props:{
        list:{type:Array,default:function(){ return []; }},
        name:{type:String,default:"Name"},//显示在tag中的名称属性

        closable:{type:Boolean,default:false},
        tagStyle:{ type:String,default:"margin-right:6px;" },
        keep:{type:Boolean,default:false},//是否始终显示焦点面板
        addable:{type:Boolean,default:false},//是否显示虚拟的添加标签
        clearable:{type:Boolean,default:false},
        addlabel:{type:String,default:""},//添加
        action:{ type:String,default:"" },// 为pop时使用 pop
        //添加浮窗控制
        trigger:{ type:String,default:"click" },
        show:{ type:Boolean,default:false, },
        placement:{ type:String,default:null },//bottom
        title:{ type:String,default:"" },
        
        width:{ type:String,default:"250px" },
        //详情弹窗控制
        position:{type:String,default:"bottom"},//内容窗体的显示位置。
        focusable:{type:Boolean,default:false},
        diatitle:{ type:String,default:"详情" },
        focusTrigger:{ type:String,default:"click" },
        //拖动控制
        draggable:{ type:Boolean,default:false },
        //拖动分组，同一分组的元素可以相互拖入。
        group:{ type:String,default:"" },
    },
    emits:["add","close","select","sort"],
    data(){
        return {
            mlist:[],
            focusTag:{},
            focusIndex:-1,
            focused:false,
            draggingIndex:-1,
            dragging:false,
            draggingGroup:"",
            showAdd:false,
        };
    },
    computed:{
        groupName(){
            if(!this.group||this.group=="none"){
                return "group"+ (Math.random()*100);
            }
            return this.group;
        },
    },
    mounted() {
        this.mlist=this.list;
    },
    watch:{
        "list":function(){
            //拖动期间，不通知外界，不响应变化。
            if(this.dragging){
                return ;
            }
            this.mlist=this.list;
        }
    },
    methods:{
        upDatePosition() {
            //强制触发 resize事件，引起 popover 重新计算位置。
            //console.log(this.$refs.popoverRef)
            // 创建 resize 事件对象
            var event = new Event('resize');
            
            // 获取需要触发 resize 的元素（通常为 window）
            var element = document.defaultView || window;
            
            // 调用 dispatchEvent() 方法来触发 resize 事件
            element.dispatchEvent(event);
            // this.$nextTick(() => {
            //     this.$refs.popoverRef.updatePopper()
            // })
        },
        onclickadd(){
            if(this.addable){
                this.$emit("add");
                if(this.action=="dialog"){
                    this.showAdd=true; 
                }
            }
        },
        onclickDelete(){
            this.$emit("deleteall");
        },
        removingEmp(row){
            this.$emit("close",row);
        },
        onTagClick(row,r){
            if(this.focusTrigger=="click"){
                this.focusTo(row,r);
            }
            this.$emit("select",row)
        },
        onDBTagClick(row,r){
            if(this.focusTrigger=="dblclick"){
                this.focusTo(row,r);
            }
        },
        focusTo(row,r){
            
            if( this.focused && r == this.focusIndex ){
                this.focused=false;
                this.focusTag={};
                this.focusIndex=-1;
                return ;
            }
            this.focusTag=row;
            this.focused=true;
            this.focusIndex=r;
        },
        closeFocus(){
            this.focused=false;
            this.focusTag={};
            this.focusIndex=-1;
        },
        ///拖动控制
        ondragstart(e,row){
            this.draggingGroup=this.groupName;
            e.dataTransfer.effectAllowed = "move";//none/move/link/copyMove/copyLink/linkMove/all
            e.dataTransfer.setData("a",JSON.stringify({
                group:this.groupName,
                row:row,
            }) );
            //e.dataTransfer.setDragImage(image, xOffset, yOffset);
            //e.target.style.opacity=0;
            this.dragging=true;
            console.log(e,this.groupName)
        },
        ondragOver(e,row,r){
            //const data = e.dataTransfer.getData("a");
            //console.log(e,data);
            if(this.draggingGroup != this.groupName){
                e.dataTransfer.effectAllowed = "none"
                return ;
            }
            e.dataTransfer.effectAllowed = "move";
            this.changeOrder(r)
            this.draggingIndex=r;
        },
        ondragEnd(e,row,r){
            
        },
        onDrop(event) {
            const data = event.dataTransfer.getData("text/plain");
            //event.target.textContent = data;
            event.preventDefault();
            this.draggingIndex=-1;
            this.dragging=false;
            this.draggingGroup="";
            this.$emit("sort",this.mlist);
            this.$emit("update:list",this.mlist)
        },
        changeOrder(newVal){
            var oldVal=this.draggingIndex;
            if(newVal==oldVal){
                return ;
            }
            if(newVal!=null){
                if(oldVal!=null && oldVal!=-1){
                    //const myElement = this.$refs['drag'+oldVal][0]
                    //setTimeout(() =>{myElement.style.opacity = '1'},0)
                    if(newVal>oldVal){
                        let f=this.list[oldVal]
                        for(let i=oldVal;i<newVal;i++){
                            this.list[i]= this.list[i+1] // $set 触发视图更改
                        }
                        this.list[newVal]=f;
                    }else{
                        let f=this.list[oldVal]
                        for(let i=oldVal;i>newVal;i--){
                            this.list[i]= this.list[i-1] // $set 触发视图更改
                        }
                        this.list[newVal]= f;
                    }
                    //const myElement1 = this.$refs['drag'+newVal][0]
                    //setTimeout(() =>{myElement1.style.opacity = '0'},0)
                }
            }
        },
    }
}
</script>

<style scoped>

.tags{
    margin-bottom:5px;
}
.addtag{
    border:2px dashed;
    cursor:pointer;
    border-width: thin;
    border-image: repeating-linear-gradient(135deg, #e6a23c, #e6a23c 5px, transparent 5px, transparent 10px) 1;
    min-width: 45px;
    text-align: center;
}
.add-icon{
    display:inline-block;
    height:12px;
    width:12px;
}

.dragging{
    color:#999;
    opacity:0.5;
}
</style>