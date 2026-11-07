<template>
  <el-dialog append-to-body :modelValue="modelValue" :width="rendWidth" class="card-dia minied" @update:modelValue="oninput" :top="top" 
    :destroy-on-close="destroyOnClose"
  :style="diastyle">
    <cardbox :position="position" :label="label" :height="rendHeight" :minHeight="minHeight" :maxHeight="maxHeight" useLabel>
        <template #prelabel>
            <slot name="titlePre"></slot>
        </template>
      <template #label>

            <span class="close-cion" @click="onclose" title="点击关闭">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728="">
                    <path fill="currentColor" d="M764.288 214.592 512 466.88 259.712 214.592a31.936 31.936 0 0 0-45.12 45.12L466.752 512 214.528 764.224a31.936 31.936 0 1 0 45.12 45.184L512 557.184l252.288 252.288a31.936 31.936 0 0 0 45.12-45.12L557.12 512.064l252.288-252.352a31.936 31.936 0 1 0-45.12-45.184z"></path>
                </svg>
            </span>

      </template>
      <template #default="p">
            <slot v-if="!bodyNotLive || modelValue" :width="p.width" :height="p.height"></slot>
      </template>
    </cardbox>
  </el-dialog>
</template>

<script>
import cardbox from '../cardbox.vue';

export default {
	name: 'WebCardialog',
	components: {
		cardbox,
	},
	emits: ['update:modelValue','close'],
	props: {
		modelValue: { type: Boolean, default: false },
		width: { type: String,default:null, },
        minWidth:null,
        maxWidth:null,
		position: { type: String, default: 'top' },
		label: { type: [Array, String], default: '' },
		height: { type: String,default:null, },
        minHeight:{ type: String,default:null, },
        maxHeight:{ type: String,default:null, },
		top: null,
        // big- 大弹窗  small--小弹窗 middle-中等 
        size:{ type:String,default:"" },
        destroyOnClose:{ type: Boolean, default: false },
        //子内容不保持
        bodyNotLive:{ type: Boolean, default: false },
	},
	data() {
		return {
            sizeMap:{
                big:"75vw",
                middle:"60vw",
                small:"40vw",
            },
            heightMap:{
                big:"75vh",
                middle:"60vh",
                small:"40vh",
            },
        };
	},
    computed:{
        diastyle(){
            var tar={};
            if(!!this.minWidth){
                tar["min-width"]=this.minWidth;
            }
            if(!!this.maxWidth){
                tar["max-width"]=this.maxWidth;
            }
            return tar;
        },
        rendWidth(){
            if(this.width !=null ){
                return this.width;
            }
            if(this.size !=""){
                var t=this.sizeMap[this.size];
                if(!!t){
                    return t;
                }
            }
            return "";
        },
        rendHeight(){
            if(this.height !=null ){
                return this.height;
            }
            if(this.size !=""){
                var t=this.heightMap[this.size];
                if(!!t){
                    return t;
                }
            }
            return "";
        },
    },
	mounted() {},

	methods: {
		oninput(e) {
			this.$emit('update:modelValue', e);
		},
		onclose() {
			this.$emit('close');
			this.$emit('update:modelValue', false);
		},
	},
};
</script>
<style lang="scss">
.card-dia > .el-dialog__header {
	display: none;
}
div.el-dialog.card-dia.minied > div.el-dialog__body {
	padding: 0;
}
.card-dia {
	background: transparent;
	box-shadow: none;
}
.card-dia {
	.el-dialog__body {
		max-height: calc(100vh - 85px) !important;
		overflow-y: hidden;
		overflow-x: hidden;
		.cd-label {
			position: sticky;
		}
		.cardbox-pan {
			overflow: hidden;
		}
		.cd-box {
			max-height: calc(100vh - 125px) !important;
			overflow: auto;
		}
	}
}
</style>
<style scoped>
.close-cion {
	display: inline-block;
	width: 16px;
	height: 16px;
	position: absolute;
	right: 10px;
    cursor:pointer;
}
</style>
