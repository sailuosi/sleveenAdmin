<template>
  <div>
    <component :is="header"></component>
    <component v-for="(tp,index) in slotList" :is="tp()"></component>
<!--    <component :is="renderMain()"></component>-->
    <component :is="Math.random() > 0.5 ? renderFooter() : null"></component>
  </div>
</template>

<script lang="jsx" setup>
import { renderSlot,defineComponent,useSlots } from "vue";
const props=defineProps({
  name:{
    type:String,
    default:''
  }
})
const slot=useSlots();

console.log("slot===",slot)


let slotList=[]
for (let slotKey in slot) {
  console.log("slotKey===",slotKey)
  slotList.push(() => <div style={{ width, fontSize: "15px" }} class={["bf-color"]}>{slot[slotKey]?.()}</div>)
}

const width = "100px";

const header = <header>this is header=={props.name}</header>;
const renderMain = () => <el-button onClick={confirm}>确定</el-button>;

const renderFooter = () => <footer>this is footer</footer>;
const confirm = () => {
  // 确认提交
  alert("ttt")
}

</script>
<style scoped>

.bf-color {
  background-color: rebeccapurple !important;
  width: 200px;
}

</style>
