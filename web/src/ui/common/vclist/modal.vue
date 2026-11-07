<template>
  <Transition name="modal">
    <teleport to="body">
        <div v-show="show" class="modal-mask" :style="{'z-index':getMaxZIndex()}">
        <div class="modal-wrapper">
            <div class="modal-container" :style="{ width:width,height:height}">
            <div class="modal-header">
                <slot name="header">{{title}}</slot>
            </div>
            <div class="modal-body">
                <slot ></slot>
            </div>
            <div class="modal-footer">
                <slot name="footer">
                </slot>
            </div>
            </div>
        </div>
        </div>        
    </teleport>
  </Transition>
</template>

<script>

import teleport from "../teleport.vue"
export default {
    components:{
        teleport
    },
  props: {
    show: Boolean,
    title:{type:String,default:"提示"},
    width:{type:[String],default:"50vw"},
    height:{type:[String],default:"95vh"},
  },
  methods:{
    getMaxZIndex() {
        var eles = document.querySelectorAll("body>div");
        var maxZindex = 1100;//因左侧菜单栏的定位为1000,这里要比他高
        for (var i = 0; i < eles.length; i++) {
            var heindex= Math.max(maxZindex, eles[i].style.zIndex || 0);
            //最后一个数字超大的是参数，豁免他的值
            if(heindex<100000 && heindex>maxZindex){
                maxZindex=heindex;
            }
        }
        return maxZindex+1;
    },
  },
}
</script>



<style>
.modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.5);
  display: table;
  transition: opacity 0.3s ease;
}

.modal-wrapper {
  display: table-cell;
  vertical-align: middle;
}

.modal-container {
    max-height: 100vh;
    display: flex;
    flex-direction: column;
  margin: 0px auto;
  padding: 20px 30px;
  background-color: #fff;
  border-radius: 2px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.33);
  transition: all 0.3s ease;
}

.modal-header h3 {
  margin-top: 0;
  color: #42b983;
}

.modal-body {
  margin: 20px 0;
  flex:1;
  overflow: auto;
}

.modal-default-button {
  float: right;
}

/*
 * 对于 transition="modal" 的元素来说
 * 当通过 Vue.js 切换它们的可见性时
 * 以下样式会被自动应用。
 *
 * 你可以简单地通过编辑这些样式
 * 来体验该模态框的过渡效果。
 */

.modal-enter-from {
  opacity: 0;
}

.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  -webkit-transform: scale(1.1);
  transform: scale(1.1);
}
</style>