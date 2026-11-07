<template>
    <div>
        <template  v-for="(item, index) in menus">
            <template v-if="Array.isArray(item.children) && item.children.length>0">
                <el-sub-menu :index="item.key?item.key:'m-' + index" :key="index"  class="sedmenu">
                    <template slot="title">
                        <i :class="item.icon" style="color:#fff;margin-right:6px;" class="side-menu-icon"></i>
                        <span slot="title">{{ item.label }}</span>
                    </template>  
                    <navmenu :menus="item.children" @nav="navto"></navmenu>
                </el-sub-menu>    
            </template>
            <template v-else>
                <el-menu-item class="lemenu"
                    :index="item.key?item.key:index + 1 + '-' "
                    @click="navto(item)"
                >
                    <i :class="item.icon" class="side-menu-icon"></i>
                    {{ item.label }}
                    <label class="pull-right side-ladge" :class="item.class" v-if="item.count">{{item.count}}</label>                        
                </el-menu-item> 
            </template>        
        </template>
    </div>
</template>

<script>


export default {
    name: 'navmenu',
    props: {
        menus: { type:Array,default:function(){ return []; } }
    },
    data(){
        return {
        };
    },
    methods:{
        navto( item) {
            this.$emit("nav",item);
        },
    },
}
</script>

<style scoped>
:deep(.sedmenu>.el-sub-menu__title) {
    border-bottom: 2px solid #efefef;
    font-weight: 700;
}
.lemenu{
    height: auto;
    line-height: 1.5;
    padding: 6px 40px;
    border-bottom: 1px solid #efefef;
    font-weight: 700;
    color: #666;
}
</style>
