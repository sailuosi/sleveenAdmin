<template>
<div class="portal-bar-pan" :class="'theme'+vc.theme">
    <div class="portal-bar" :style="titleStyle" >
        <slot></slot>
    </div>
    <div class="vc-tools" v-if="!!tools">
        <div class="portal-bar-more" v-if="tools.indexOf('2')!=-1" @click="moreClick(moreUrl)" :title="defaultMoreTitle">
            <a class="portal-bar-link" :style="{ color: color }"><i :class="moreIcon" :style="{ color: color }"></i> {{ moreTitle }}</a>
        </div>
        <div class="portal-bar-refresh" v-if="tools.indexOf('1')!=-1" @click="refreshClick()" :title="defaultRefreshTitle">
            <a class="portal-bar-link" :style="{ color: color }"><i :class="refreshIcon" :style="{ color: color }"></i> {{ refreshTitle }}</a>
        </div>
        <div class="portal-bar-add" v-if="tools.indexOf('3')!=-1" @click="addClick(addUrl)" :title="defaultAddTitle">
            <a class="portal-bar-link" :style="{ color: color }"><i :class="addIcon" :style="{ color: color }"></i> {{ addTitle }}</a>
        </div>    
    </div>
</div>

</template>

<script>
export default {
    name: "LionPortalBar",
    props: {
        vc:{type:Object,default:()=>{} },//组件对象
        cell:{type:Object,default:()=>{} },//组件对象
        
        background: {
            type: String,
            // ,
            // default: '#f4f6fc',
        },
        height: {
            type: Number,
            default: 40,
        },
        color: {
            type: String,
            default: "",
        },
        icon: {
            type: String,
            default: "el-icon-view",
        },
        url: {
            type: String,
            default: "#",
        },
        show: {
            type: Boolean,
            default: true,
        },
        addShow: {
            type: Boolean,
            default: false,
        },
        addTitle: {
            type: String,
            default: "",
        },
        addIcon: {
            type: String,
            default: "el-icon-plus",
        },
        addUrl: {
            type: String,
            default: "#",
        },
        moreShow: {
            type: Boolean,
            default: false,
        },
        moreTitle: {
            type: String,
            default: "",
        },
        moreIcon: {
            type: String,
            default: "el-icon-more",
        },
        moreUrl: {
            type: String,
            default: "#",
        },
        refreshShow: {
            type: Boolean,
            default: false,
        },
        refreshTitle: {
            type: String,
            default: "",
        },
        refreshIcon: {
            type: String,
            default: "el-icon-refresh",
        },
    },
    data() {
        return {
            option: {},
        };
    },
    computed: {
        defaultMoreTitle() {
            return "更多";
        },
        defaultRefreshTitle() {
            return "刷新";
        },
        titleStyle() {
            return {
                "min-height": this.height +"px"
            };
        },
        tools(){
            // 1--刷新  2--更多
            var tar= this.vc.tools;
            if(!!tar){
                return tar;
            }
            return "";
        },
    },
    methods: {
        titleClick(url) {
            if (this.titleClickShow) {
                this.$emit("titleClick", url);
            } else {
                if (url && url !== "#") {
                    // showOaWin(url, this.title);
                    window.open(url);
                }
            }
        },
        addClick(url) {
            if (this.addClickShow) {
                this.$emit("addClick", url);
            } else {
                if (url && url !== "#") {
                    window.open(url);
                }
            }
        },
        moreClick(url) {
            if (this.moreClickShow) {
                this.$emit("moreClick", url);
            } else {
                if (this.$parent.content.data.length === 1) {
                    if (url && url !== "#") {
                        window.open(url);
                    }
                } else {
                    let data = this.$parent.content.data;
                    let activeIndex = this.$parent.currentPortlet[0].index;
                    for (let i = 0; i < data.length; i++) {
                        if (activeIndex === i) {
                            if (data[i].moreClickShow) {
                                this.$emit("moreClick");
                            } else {
                                if (data[i].moreUrl && data[i].moreUrl !== "#") {
                                    window.open(data[i].moreUrl);
                                } else {
                                    if (url && url !== "#") {
                                        window.open(url);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        },
        refreshClick() {
            if (this.refreshClickShow) {
                this.$emit("refreshClick", true);
            } else {
                this.$emit("refreshClick", false);
            }
        },
    },
};
</script>

<style scoped>
.portal-bar-pan{
    display: flex;
    border-bottom: 1px solid #eee;
    margin-bottom: 5px;
}
.portal-bar-pan.theme1{
    border-bottom-color:#3385CC;
}

.portal-bar{
    flex:1;
    display: flex;
    align-items: center;
    padding-left: 10px;
}


.vc-tools{
    flex: 0 0 auto;
    display: flex;
    gap: 10px;
    align-items: center;
    padding-right: 10px;
}
.vc-tools>div{
    cursor:pointer;
}
</style>
