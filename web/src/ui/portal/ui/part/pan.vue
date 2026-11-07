<template>
    <component :is="themePan" 
        :label="label" flex 
        :position="labelPose" useLabel 
        labelClass="portal-bar"
        :bodyStyle="bodyStyle" :labelStyle="labelStyle"
        :lazy="lazy"
        >
        <template #label>
            <slot name="label"></slot>
        </template>
        <template #default="p">
            <slot v-bind="p"></slot>
        </template>
    </component>
</template>

<script>
import labeldiv from "@/ui/panels/labeldiv.vue"
import cardbox from "@/ui/panels/cardbox.vue"

export default {
    name: 'WebPan',
    components: {

        labeldiv,
        cardbox
    },
    props:{
        theme:{ type:String,default:"" },
        position:{ type:String,default:"" },
        label:null,
        labelStyle:null,
        bodyStyle:null,
        lazy:{ type:Boolean,default:true },
    },
    data() {
        return {

        };
    },
    computed:{
        themePan(){
            var t="labeldiv";
            if(this.theme=="2"){
                t="cardbox"
            }
            return t;
        },
        labelPose(){
            var map={
                '0':'none',
                '1':'top',
                '2':'left',
                '3':'right',
                '4':'bottom'
            }
            var tar=map[this.position]
            if(tar != void 0){
                return tar;
            }
            return "top";
        },
    },
    mounted() {

    },

    methods: {

    },
};
</script>
