<template>
    <el-dialog append-to-body :modelValue="modelValue" :width="rendWidth" :class="'card-dia minied '+theme+(extraClass ? ' '+extraClass : '')" @update:modelValue="oninput" :top="alignCenter ? undefined : top"
      :align-center="alignCenter"
      :destroy-on-close="destroyOnClose"
      :close-on-press-escape="closeOnPressEscape"
      :close-on-click-modal="closeOnClickModal"
      :z-index="dialogZIndex"
    :style="diastyle">
      <mcardbox :position="position" :label="title" :height="rendHeight" :minHeight="minHeight" :maxHeight="maxHeight" useLabel
      :theme="theme"
      >
          <template #prelabel>
              <slot name="titlePre"></slot>
          </template>
        <template #label>
              <slot name="header-actions"></slot>
              <span class="close-cion" @click="onclose" title="点击关闭">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728="">
                      <path fill="currentColor" d="M764.288 214.592 512 466.88 259.712 214.592a31.936 31.936 0 0 0-45.12 45.12L466.752 512 214.528 764.224a31.936 31.936 0 1 0 45.12 45.184L512 557.184l252.288 252.288a31.936 31.936 0 0 0 45.12-45.12L557.12 512.064l252.288-252.352a31.936 31.936 0 1 0-45.12-45.184z"></path>
                  </svg>
              </span>
  
        </template>
        <template #default="p">
              <slot v-if="!bodyNotLive || modelValue" :width="p.width" :height="p.height"></slot>
        </template>
      </mcardbox>
    </el-dialog>
  </template>
  
<script setup>
import { computed } from 'vue';
import mcardbox from './mcardbox.vue';

defineOptions({ name: 'mdialog' });

const props = defineProps({
    modelValue: { type: Boolean, default: false },
    width: { type: String, default: null },
    minWidth: null,
    maxWidth: null,
    position: { type: String, default: 'top' },
    title: { type: [Array, String], default: '' },
    height: { type: String, default: null },
    minHeight: { type: String, default: null },
    maxHeight: { type: String, default: null },
    top:  { type: String, default: "60px" },
    /** 垂直居中（开启后忽略 top） */
    alignCenter: { type: Boolean, default: false },
    // big- 大弹窗  small--小弹窗 middle-中等
    size: { type: String, default: '' },
    destroyOnClose: { type: Boolean, default: false },
    // 是否允许按 ESC 关闭，默认关闭
    closeOnPressEscape: { type: Boolean, default: false },
    // 是否允许点击遮罩关闭，默认关闭
    closeOnClickModal: { type: Boolean, default: false },
    //子内容不保持
    bodyNotLive: { type: Boolean, default: false },
    theme: { type: String, default: 'fblue' },
    /** 额外挂到 el-dialog 根上的 class（append-to-body 场景可单独加样式） */
    extraClass: { type: String, default: '' },
    /** 嵌套高层 Teleport 编辑层时需显式抬高（如 5600） */
    zIndex: { type: [String, Number], default: null },
});

const emit = defineEmits(['update:modelValue', 'close']);

const dialogZIndex = computed(() => {
    if (props.zIndex == null || props.zIndex === '') return undefined;
    const n = Number(props.zIndex);
    return Number.isFinite(n) ? n : undefined;
});

const sizeMap = {
    big: '75vw',
    middle: '60vw',
    small: '40vw',
};
const heightMap = {
    big: '75vh',
    middle: '60vh',
    small: '40vh',
};

const diastyle = computed(() => {
    const tar = {};
    if (!!props.minWidth) {
        tar['min-width'] = props.minWidth;
    }
    if (!!props.maxWidth) {
        tar['max-width'] = props.maxWidth;
    }
    return tar;
});

const rendWidth = computed(() => {
    if (props.width != null) {
        return props.width;
    }
    if (props.size != '') {
        const t = sizeMap[props.size];
        if (!!t) {
            return t;
        }
    }
    return '';
});

const rendHeight = computed(() => {
    if (props.height != null) {
        return props.height;
    }
    if (props.size != '') {
        const t = heightMap[props.size];
        if (!!t) {
            return t;
        }
    }
    return '';
});

function oninput(e) {
    if (!e && props.modelValue) {
        emit('close');
    }
    emit('update:modelValue', e);
}

function onclose() {
    emit('close');
    emit('update:modelValue', false);
}
</script>
  <style lang="scss">
  .card-dia > .el-dialog__header {
      display: none;
  }
  div.el-dialog.card-dia.minied > div.el-dialog__body {
      padding: 0!important;
  }
  .card-dia.el-dialog.minied{
      background-color: transparent!important;
      box-shadow: none;
      padding: 0!important;
      border:none!important;
      outline: 1px solid #409eff;
  }
  .card-dia {
      .el-dialog__body {
          max-height: calc(100vh - 85px) !important;
          background-color: transparent!important;
          overflow-y: hidden;
          overflow-x: hidden;
          .cd-label {
              position: sticky;
          }
          .cardbox-pan {
              overflow: hidden;
          }
          .cd-box {
              max-height: calc(100vh - 125px) ;
              overflow: auto;
          }
      }
  }
  .card-dia.fgray .cd-box{
    max-height: calc(100vh - 185px) ;
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

  .card-dia.minied > :deep(.el-dialog__body) {
    padding-top: 0!important;
    padding-bottom: 0!important;
    padding-left: 0!important;
    padding-right: 0!important;
  }

  .fgray .close-cion{
    display: inline-flex!important;
    align-items: center!important;
    justify-content: center!important;
    z-index: 2!important;
    top: 50%!important;
    right: 18px!important;
    left: auto!important;
    width: 34px!important;
    height: 34px!important;
    padding: 0!important;
    border: 0!important;
    border-radius: 999px!important;
    background: #fff1f2!important;
    color: #dc2626!important;
    line-height: 1!important;
    box-shadow: 0 1px 2px #7f1d1d14!important;
    transform: translateY(-50%)!important;
    transition: background-color .2s ease,color .2s ease,box-shadow .2s ease!important;
  }
  .fgray .close-cion>svg{
    width: 14px;
  }
  </style>
