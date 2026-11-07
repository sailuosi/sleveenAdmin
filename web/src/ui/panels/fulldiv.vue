<template>
    <div ref="sroot">
        <slot name="reference" v-if="!teleported" :toggle="toggle" >
            <span v-if="maxable" @click="openTomodal" class="el-button" style="margin-left: 15px;" title="最大化显示列表">
                    <el-icon ><FullScreen /></el-icon>             
            </span>
        </slot>        
        <teleport :to="modalRoot" :disabled="!teleported">
            <slot ></slot>
        </teleport>
        <modal :show="seemodal" @update:model-value="closeMoald" title="最大化" :width="width" :height="height">
            <template #header>
                <slot name="header">
                    <div >
                        {{title}}
                    </div>                    
                </slot>
            </template>            
            <div ref="groot" style="height:100%;"></div>
            <template #footer>
                <slot name="footer" v-if="teleported">
                    <div style="text-align:right;">
                        <span @click="closeMoald" class="el-button" style="margin-left: 15px;" title="还原列表显示">
                            <el-icon><Close /></el-icon>            
                        </span>                        
                    </div>
                </slot>
            </template>
        </modal>
    </div>
</template>
<script>


export default {
    name:"fulldiv",
    props:{
        maxable:{type:Boolean,default:true},//是否可以全屏。
        title:{type:String,default:"最大化"},//全屏的标题
        height:null,
        width:{type:String,default:"95vw"},
    },
    emits:["start","reset"],

    data(){
        return{
            //弹窗最大化相关功能
            seemodal:false,//是否显示弹窗
            teleported:false,//是否传送
            modalRoot:null,//传送时的根节点
        };
    },
    computed:{

    },
    created(){
  
    },
    methods:{
        //弹窗最大化的功能
        toggle(val){
            if(typeof val=="boolean"){
                if(val){
                    this.openTomodal();
                }else{
                    this.closeMoald();
                }
            }
        },
        openTomodal(){
            this.seemodal=true;
            this.$nextTick(()=>{
                this.openTele(); 
            });
        },
        closeMoald(){
            this.seemodal=false;
            this.teleported=false;
        },
        openTele(){
            var goot= this.$refs.groot;
            if(goot!=void 0){
                this.modalRoot=goot;
                this.teleported=true;
            }else{
                this.$nextTick(()=>{
                    this.openTele();
                }); 
            }
        },
    }

}
</script>

<script setup>
import modal from "@f/views/biz/components/vclist/modal.vue"

</script>
