<template>
    <div v-html="value" :style="css">
        
    </div>
</template>

<script>

const syncVar = (node, binding, vnode) => {
  if (binding.arg) {
    binding.value[binding.arg] = node.offsetWidth < node.scrollWidth
  } else {
    vnode.context[binding.expression] = node.offsetWidth < node.scrollWidth
  }
}

export default {
    name: 'sizediv',
    props:{
        value:null,
        size:null,
        maxHeight:null,
    },
    data() {
        return {
            
        };
    },
  directives:{
    trunced:{
      inserted: function(el, binding, vnode) {
        const node = binding.modifiers.parent ? el.parentNode : el
        syncVar(node, binding, vnode);

        const ro = new ResizeObserver((entries, observer) => {
          entries[0].target && syncVar(node, binding, vnode)
        });
        ro.observe(node);
      },
      componentUpdated: function(el, binding, vnode) {
        const node = binding.modifiers.parent ? el.parentNode : el
        syncVar(node, binding, vnode);
      }
    },
  },
    computed:{
        css() {
            var res = {};
            if (!!this.maxHeight) {
                res["max-height"] = this.maxHeight;
            }
            if(this.size==1 ||this.size=="1"){
                res["text-overflow"] = "ellipsis";
                res.overflow = "hidden";  
            }
            else if( !isNaN(this.size) && this.size*1>1 ){
                res["display"]= "-webkit-box";
                res["-webkit-box-orient"]= "vertical";
                res["overflow"]= "hidden";
                res["-webkit-line-clamp"]= this.size;
                res["text-overflow"]= "ellipsis";
            }
            return res;
        },
    },
    mounted() {
        
    },

    methods: {
        
    },
};
</script>

<style scoped>

</style>