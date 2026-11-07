<template>
    <div  class="tags-pan" >
        <div class="tags">
            <el-tag v-for="(tag,t) in list" :key="t" class="stag" style="margin-right:6px;"  :closable="closable" 
                :effect="t==focusIndex?'dark':'light'"
                @click="onTagClick(tag,t)"
                @dblclick="onDBTagClick(tag,t)"
                @close="removingEmp(tag)">
                <slot :row="tag">{{tag[name]}}</slot>
            </el-tag>

            <el-popover  v-if="addable && action=='pop' && selectMan"  ref="popoverRef"
                :placement="placement"
                :title="title"
                :width="width" effect="light"
                :trigger="trigger" :show="show" @input="$emit('popchange')"
                content="">
                <template #default="p">
                    <lazydiv>
                        <slot name="addpop" :resize="upDatePosition"></slot>
                    </lazydiv>
                </template>
                <template #reference>
                    <el-tag  type="warning" class="addtag" style=""  title="点击添加">
                        <span class="add-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M480 480V128a32 32 0 0 1 64 0v352h352a32 32 0 1 1 0 64H544v352a32 32 0 1 1-64 0V544H128a32 32 0 0 1 0-64z"></path></svg>
                        </span>{{addlabel}}
                    </el-tag>
                </template>
            </el-popover >

            <el-tag v-else-if="!selectMan"  type="warning" class="addtag" style="" @click="onclickadd" title="点击添加">
                <span class="add-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M480 480V128a32 32 0 0 1 64 0v352h352a32 32 0 1 1 0 64H544v352a32 32 0 1 1-64 0V544H128a32 32 0 0 1 0-64z"></path></svg>
                        </span>{{addlabel}}
            </el-tag>

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
            <el-dialog :title="diatitle" :model-value="focused" width="75vw" append-to-body @close="closeFocus">
                <slot name="focus" :row="focusTag" :focused="focused"></slot>
            </el-dialog>
        </template>
        <template v-else>
            <div v-if="keep || focused">
                <slot name="focus" :row="focusTag" :focused="focused"></slot>
            </div>
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
        selectMan:{type:Boolean,default:true},
        closable:{type:Boolean,default:false},
        
        keep:{type:Boolean,default:false},//是否始终显示焦点面板
        addable:{type:Boolean,default:false},//是否显示虚拟的添加标签
        clearable:{type:Boolean,default:false},
        addlabel:{type:String,default:""},//添加
        action:{ type:String,default:"" },// 为pop时使用 pop
        //添加浮窗控制
        trigger:{ type:String,default:"click" },
        show:{ type:Boolean,default:false, },
        placement:{ type:String,default:"" },//bottom
        title:{ type:String,default:"" },
        
        width:{ type:String,default:"250px" },
        //详情弹窗控制
        position:{type:String,default:"bottom"},//内容窗体的显示位置。
        focusable:{type:Boolean,default:false},
        diatitle:{ type:String,default:"详情" },
        focusTrigger:{ type:String,default:"click" },
    },
    emits:["add","close"],
    data(){
        return {
            focusTag:{},
            focusIndex:-1,
            focused:false,
        };
    },
    computed:{
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
</style>