<template>
    <div class="fmdiv">
        <label>{{label}}</label>
        <div>
            <slot name="prefix"></slot>
            <slot name="value">{{value}}</slot>
            <slot></slot>
        </div>
    </div>
</template>

<script>
export default {
    name: 'fmdiv',
    props:{
        label:{ type:String,defualt:"" },
        value:{ type:String,defualt:"" },
    },
    data() {
        return {
            
        };
    },

    mounted() {
        
    },

    methods: {
        
    },
};
</script>

<style scoped>
.fmdiv{
    display:flex;
    line-height:1.7;
}

.fmdiv>label{
    flex:0 0 auto;
    font-size: 14px;
    color: #606266;
    padding: 0 12px 0 0;
}
.fmdiv>div{
    flex:1 1 auto;
}
</style>