<template>
    <div class="lr-pan mr" left="1" right="4" >
        <div :style="leftcss" class="pan-left verflex cpan">
            <div class="l-head">
                <span v-if="!!labelL" class="panlabel">{{labelL}}</span><slot name="lhead"></slot>
            </div>
            <div class="l-body">
                <slot name="lbody"></slot>
            </div>
        </div>
        <div :style="midcss" class="pan-right verflex cpan" >
            <div class="m-head">
                <span v-if="!!labelM" class="panlabel">{{labelM}}</span>
                <slot name="mhead"></slot>
            </div>
            <div class="m-body">
                <slot name="mbody"></slot>
            </div>
        </div>
        <div :style="rightcss" class="pan-right verflex cpan">
            <div class="r-head">
                <span v-if="!!labelR" class="panlabel">{{labelR}}</span>
                <slot name="rhead"></slot>
            </div>
            <div class="r-body">
                <slot name="rbody"></slot>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'Leftright',
    props:{
        left:{ type:String,default:"2 0 auto" },
        mid:{ type:String,default:"4 0 auto" },
        right:{ type:String,default:"2 0 auto" },
        labelM:null,
        labelL:null,
        labelR:null
    },
    data() {
        return {
        	
        };
    },
    computed:{
        leftcss(){
            return {
                flex:this.left,
            }
        },
        midcss(){
            return {
                flex:this.mid,
            }
        },
        rightcss(){
            return {
                flex:this.right,
            }
        },
    }
};
</script>

<style scoped>
div.lr-pan.mr{
    display:flex;
    height: 100%;
    flex-direction: row;
}
.cpan{
    box-shadow: 0 2px 12px 0 rgb(0 0 0 / 10%);
    border-radius: 10px;
    border: 1px solid #ebeef5;
    background-color: #fff;
    overflow: hidden;
    color: #303133;
    transition: .3s;
}
.pan-left{
    height:calc(100% - 12px);
    width:0;
}
.pan-right{
    height:calc(100% - 12px);
    width:0;
    margin-left:10px;
    /* margin-right:5px; */
}

.verflex{
    display:flex;
    height: calc(100% - 12px);
    flex-direction: column;
}

.l-head{
    flex: 0 0 auto;
    padding:5px 10px;
    border-bottom: 1px solid #25a8f3;
    color:#333;
    height:40px;
    display: flex;
    align-items: center;
}
.r-head{
    flex: 0 0 auto;
    padding:5px 10px;
    border-bottom: 1px solid #25a8f3;
    color:#333;
    height:40px;
    display: flex;
    align-items: center;
}
.l-body{
    flex:1 0 auto;
    height:0;
    overflow:auto;
}
.m-head{
    flex: 0 0 auto;
    padding:5px 10px;
    border-bottom: 1px solid #25a8f3;
    color:#333;
    height:40px;
    display: flex;
    align-items: center;
}
.m-body{
    flex:1 0 auto;
    height:0;
    overflow:auto;
}
.r-body{
    flex:1 0 auto;
    height:0;
    overflow:auto;
}

.panlabel{
    flex:0 0 auto;
}
</style>