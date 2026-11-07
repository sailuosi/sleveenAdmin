<template>
<div>
        <template v-if="mode=='3'">
            <el-carousel ref="carousel" :direction="content.carouselDirection" :type="content.carouselType" :interval="content.carouselInterval || 3000" @change="handleCarouselChange">
                <el-carousel-item v-for="(vc,v) in vclist" :key="v">
                    <slot :vc="vc" :index="v" :width="width" :height="height"></slot>
                </el-carousel-item>
            </el-carousel>
        </template>
        <el-tabs v-else-if="mode=='1'" v-model="activeTab" :class="mytabcss" :tab-position="position" :type="tabType"  :stretch="stretch">
            <el-tab-pane v-for="(vc,v) in vclist" :label="vc.title" :name="'vc'+v" :lazy="true" :key="v">
                <template #label>
                    <span :class="['vctitle',virtical?'ver':'hzo']">
                        <i :class="vc.icon"></i> 
                        <template v-if="virtical">
                            <span v-for="(wo,w) in getwords(vc.title)">{{wo}}</span>
                        </template>
                        <template v-else>
                            {{vc.title}}
                        </template>
                    </span>
                </template>
                <slot :vc="vc" :index="v" :width="virtical?width:(width-60)" :height="virtical?(height-60):height"></slot>
            </el-tab-pane>
        </el-tabs>
        <cardtab v-else-if="mode=='4'" :tabs="vclist" labelKey="title" :position="position">
            <template #default="p">
                <slot :index="p.index" :vc="p.row.data" :width="virtical?width:(width-60)" :height="virtical?(height-60):height"></slot>
            </template>
        </cardtab>
        <div v-else class="lion-empty" style="margin: 20px auto; width: 30%;">
            <template v-for="(vc,v) in vclist" :key="v">
                <slot :vc="vc" :index="v" :width="width" :height="height"></slot>
            </template>
        </div>
</div>
</template>

<script>

import cardtab from "@/ui/panels/cardtab/cardtab.vue"

export default {
    name: 'WebManyplay',
    components:{
        cardtab
    },
    props:{
        mode:{ type:String,default:"" },
        stretch:{ type:Boolean,default:false },
        vclist:{ type:Array,default:function(){ return []; } },
        tabType:null,
        width:null,
        height:null,
        position:null,
        content: {
        	type: Object,
        	default: {}
        }
    },
    data() {
        return {
            activeTab: "vc0",
        };
    },
    computed:{
        virtical(){
            var t=this.mode;
            if(t=="left"||t=="right"){
                return false;
            }
            return true;
        },
        mytabcss(){
            //content.isTab != false || content.isCarousel == false? 'portal-content-tab' : 'portal-content-tab portal-content-notab'
            var res=["vc-tab","applet-tab"];
            //res.push("thee"+this.thee);
            //res.push("pos-"+this.applet.titleAlign);
            return res;
        },
    },
    mounted() {

    },

    methods: {
        getwords(txt){
            if(!!txt && typeof txt=="string"){
                return txt.split("");
            }
            return [];
        },
    },
};
</script>
