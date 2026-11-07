<script>
export default {
    props: {
        to: {      type: String,    },
        disabled: {      type: Boolean,      default: true    }
    },
    //functional: true,
    render: function (createElement, context) {
        // 完全透传任何 attribute、事件监听器、子节点等。
        //return createElement('button', context.data, context.children)
        var res= this.$slots.default;
        if(!!res && res.length>1){
            return res[0]
        }
        return res;
    },
    watch: {
        disabled() {
            if(!this.disabled) { // 把当前的DOM元素 向目标元素传送过去
                document.querySelector(this.to).appendChild(this.$el)
            }else { // 调用父组件的方法， 再将当前的DOM元素还原的原来的位置
                this.parent.toSourceDom(this.$el)
            }
        }
    },
    mounted(){
        if(!this.disabled){
            document.querySelector(this.to).appendChild(this.$el)
        }
    },
  /**
   *  render方法中，如果有变量得使用{}
   *  this.$scopedSlots.default() 表示预留一个插槽
   */
}
</script>
<style scoped>
.Teleport {
  width: 100%;
  height: 100%;
}
</style>