<template>
    <div class="vchead" :class="{ 'vchead--hasquery': queryable,'noquery':!queryable, 'vchead--wrap': wraphead }">
        <div class="vccap">
            <slot>{{ cap }}</slot>
        </div>
        <template v-if="queryable">
            <div class="qhead qhead--underline" @click.stop @mousedown.stop @dblclick.stop @keydown.stop>
                <slot name="query" :searchtxt="searchtxt" :ftype="ftype" :options="options" :logic="logic" :logicList="logicList"  :searchVal="searchVal" :onput="onputTxt" :onchangeLogic="onChangeLogic">
                    <el-input v-if="ftype=='text'" v-model="searchtxt" class="shinput" @change="onchange"
                        @keydown.enter="onquery"
                        @clear="onquery"
                        @blur="checkSayQuery"
                        clearable :placeholder="holder" />
                    <div v-else-if="ftype=='number'||ftype=='money'" style="width: 100%;">
                        <el-input  v-model="searchtxt" class="shinput" 
                            clearable @change="onchange" @keydown.enter="onquery"
                            :placeholder="holder" type="number" />   
            
                    </div>
    
                    <el-select v-else-if="ftype=='dict'"
                        v-model="searchtxt" @change="onchange" @keydown.enter="onquery"
                        :placeholder="holder" multiple collapse-tags clearable
                        size="small" filterable
                        class="shinput"
                    >
                        <el-option v-for="item in options"
                            :key="item.value"
                            :label="item.label"
                            :value="item.value"
                        ></el-option>
                    </el-select>
                    <el-date-picker v-else-if="ftype=='date'||ftype=='datetime'" class="shinput"
                        v-model="searchtxt" @change="onchange" @keydown.enter="onquery"
                        type="daterange" style="width: 100%;"
                        range-separator="--"
                        start-placeholder="开始"
                        end-placeholder="结束"
                        size="small"
                        unlink-panels
                        format="YYYY-MM-DD"
                        value-format="YYYY-MM-DD"
                    />
                    <el-select v-else-if="ftype=='bool'"
                        v-model="searchtxt" @change="onchange" @keydown.enter="onquery"
                        :placeholder="holder" multiple collapse-tags clearable
                        size="small"
                        class="shinput"
                    >
                        <el-option :label="'是'" :value="true" ></el-option>
                        <el-option :label="'否'" :value="false" ></el-option>
                    </el-select>                
                </slot>
    
                <el-popover trigger="click" placement="bottom" :width="90">
                    <el-radio-group v-model="logic" size="small" @change="onchange">
                        <el-radio v-for="item in logicList" :key="item.value" :value="item.value">
                        {{ item.label }}
                        </el-radio>
                    </el-radio-group>
                    <template #reference>
                        <span class="q-op" @click.stop @mousedown.stop>
                    <svg class="icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="2890"><path d="M516.973714 950.857143a67.584 67.584 0 0 1-30.427428-7.606857l-111.616-53.248a64.073143 64.073143 0 0 1-38.034286-58.368V499.346286v-2.56L100.937143 179.712a65.609143 65.609143 0 0 1-5.046857-71.021714c10.166857-22.893714 32.914286-35.547429 58.368-35.547429h611.328c25.380571 0 48.274286 12.653714 58.368 35.547429 10.166857 22.820571 10.166857 48.201143-5.12 68.461714L582.948571 494.226286v388.169143c0 22.820571-12.653714 43.154286-30.427428 55.808a55.442286 55.442286 0 0 1-35.547429 12.653714zM154.185143 133.997714c-2.486857 0-5.046857 0-5.046857 2.56v5.12l235.958857 317.074286a70.217143 70.217143 0 0 1 12.653714 40.594286v332.288c0 2.56 0 5.12 2.56 5.12l114.102857 53.248h5.12c2.56 0 2.56-2.56 2.56-5.12V499.346286c0-15.213714 5.046857-27.940571 12.653715-40.594286l235.885714-317.147429c2.56-2.486857 2.56-5.046857 0-5.046857 0-2.56-2.486857-2.56-5.046857-2.56H154.331429z m740.790857 482.011429H717.385143a29.257143 29.257143 0 0 1-30.427429-30.427429 29.257143 29.257143 0 0 1 30.427429-30.427428h177.590857a29.257143 29.257143 0 0 1 30.427429 30.427428 29.257143 29.257143 0 0 1-30.427429 30.427429z m0 126.829714H717.385143a29.257143 29.257143 0 0 1-30.427429-30.427428 29.257143 29.257143 0 0 1 30.427429-30.427429h177.590857a29.257143 29.257143 0 0 1 30.427429 30.427429 29.257143 29.257143 0 0 1-30.427429 30.427428z m0 126.829714H717.385143a29.257143 29.257143 0 0 1-30.427429-30.427428 29.257143 29.257143 0 0 1 30.427429-30.427429h177.590857a29.257143 29.257143 0 0 1 30.427429 30.427429 29.257143 29.257143 0 0 1-30.427429 30.427428z" fill-opacity=".65" p-id="2891"></path><path d="M483.254857 949.76q16.822857 8.411429 33.718857 8.411429 22.162286 0 39.936-14.189715 33.353143-23.917714 33.353143-61.586285V496.64l234.422857-315.099429q24.868571-33.206857 5.924572-75.849142Q812.909714 65.828571 765.586286 65.828571H154.331429q-47.177143 0-64.950858 39.497143-22.528 40.96 5.851429 78.701715l234.422857 315.172571v332.434286q0 46.08 42.276572 64.950857l111.396571 53.174857z m33.718857 1.097143a55.442286 55.442286 0 0 0 35.547429-12.653714 69.705143 69.705143 0 0 0 30.427428-55.808V494.226286l235.885715-317.074286c15.286857-20.260571 15.286857-45.641143 5.12-68.461714-10.166857-22.893714-32.987429-35.547429-58.368-35.547429H154.331429c-25.380571 0-48.274286 12.653714-58.368 35.547429a65.609143 65.609143 0 0 0 5.12 70.948571l235.885714 317.147429v334.848c0 25.380571 15.213714 48.201143 38.034286 58.368l111.616 53.248c10.166857 5.12 20.333714 7.606857 30.427428 7.606857z m248.685715-816.859429c2.486857 0 4.973714 0 4.973714 2.56 2.56 0 2.56 2.56 0 5.12l-1.462857 2.048-0.146286 0.146286L534.674286 458.752a76.653714 76.653714 0 0 0-12.653715 40.594286v385.536c0 2.56 0 5.12-2.56 5.12h-5.12l-114.102857-53.321143c-2.56 0-2.56-2.486857-2.56-5.12V499.419429a70.217143 70.217143 0 0 0-12.653714-40.594286L149.138286 141.604571v-5.046857c0-2.56 2.56-2.56 5.12-2.56h611.328z m-607.597715 7.314286h603.721143L528.676571 454.656l-0.219428 0.292571q-13.677714 22.893714-13.677714 44.397715v382.683428l-109.714286-51.2V499.346286q0-26.112-14.116572-44.982857l-232.886857-313.051429z m559.323429 482.011429h177.590857q16.310857 0 27.062857-10.678858 10.678857-10.752 10.678857-27.062857 0-37.741714-37.741714-37.741714H717.385143q-37.741714 0-37.741714 37.741714t37.741714 37.741715z m0-7.314286h177.590857a29.257143 29.257143 0 0 0 30.427429-30.427429 29.257143 29.257143 0 0 0-30.427429-30.427428H717.385143a29.257143 29.257143 0 0 0-30.427429 30.427428 29.257143 29.257143 0 0 0 30.427429 30.427429z m0 134.144h177.590857q37.741714 0 37.741714-37.741714t-37.741714-37.741715H717.385143q-16.310857 0-27.062857 10.678857-10.678857 10.678857-10.678857 27.062858 0 37.741714 37.741714 37.741714z m0-7.314286h177.590857a29.257143 29.257143 0 0 0 30.427429-30.427428 29.257143 29.257143 0 0 0-30.427429-30.427429H717.385143a29.257143 29.257143 0 0 0-30.427429 30.427429 29.257143 29.257143 0 0 0 30.427429 30.427428z m0 134.144h177.590857q37.741714 0 37.741714-37.741714 0-16.310857-10.678857-27.062857-10.752-10.678857-27.062857-10.678857H717.385143q-16.310857 0-27.062857 10.678857-10.678857 10.752-10.678857 27.062857 0 37.741714 37.741714 37.741714z m0-7.314286h177.590857a29.257143 29.257143 0 0 0 30.427429-30.427428 29.257143 29.257143 0 0 0-30.427429-30.427429H717.385143a29.257143 29.257143 0 0 0-30.427429 30.427429 29.257143 29.257143 0 0 0 30.427429 30.427428z"  fill-opacity=".65" p-id="2892"></path></svg>
                        </span>
                    </template>
                </el-popover>
            </div>     
        </template>
    </div>
    </template>
    <script setup>
    import {
        ref,
        computed,
        watch,
        onMounted,
        inject,
        getCurrentInstance,
    } from 'vue'
    import { isDateQueryType, resolveListQueryOp, normalizeListDateRangeValue } from './tool.js'
    
    defineOptions({
        name: 'queryhead',
    })
    
    const vclist = inject('vclist')
    
    const props = defineProps({
        column: { default: undefined },
        param: { default: undefined },
        /** 是否有查询行 */
        queryable: { type: Boolean, default: false },
        cap: { type: String, default: '' },
        /** 是否事件停止冒泡 */
        wraphead: { type: Boolean, default: false },
        ctype: { default: undefined },
        width: { default: undefined },
        dict: { default: undefined },
        dictlist: { default: null },
        /** 是否可以显示全部的查询选项。 */
        custopts: { default: null },
        allopts: { type: Boolean, default: false },
        holder: { type: String, default: '筛选...' },
    })
    
    const emit = defineEmits(['queryinput', 'logicChange', 'query'])
    
    const instance = getCurrentInstance()
    
    const searchtxt = ref('')
    /** 是否存在了变化，没有执行查询 */
    const changeNotSay = ref(false)
    
    function onputTxt(value) {
        searchtxt.value = value
    }
    
    const logic = ref('')
    
    function onChangeLogic(value) {
        logic.value = value
    }
    
    const logicList = [
        { label: '默认', value: '' },
        { label: '包含', value: 'like' },
        { label: '以...开头', value: 'likeLeft' },
        { label: '等于', value: '=' },
        { label: '不等于', value: '<>' },
        { label: '大于', value: '>' },
        { label: '小于', value: '<' },
        { label: '大于等于', value: '>=' },
        { label: '小于等于', value: '<=' },
    ]
    
    
    const options = computed(() => props.dictlist)
    
    const ftype = computed(() => {
        if (!!props.ctype) {
            return props.ctype
        }
        return 'text'
    })
    
    const searchVal = computed(() => {
        try {
            return vclist.para[props.column.field]
        }
        catch (e) {
            return searchtxt.value
        }
    })
    
    
    function checkSayQuery() {
        if (changeNotSay.value) {
            onquery()
        }
    }
    
    function resolveQueryOp() {
        return resolveListQueryOp(ftype.value, logic.value)
    }

    function onchange() {
        var para = {
            value: searchtxt.value,
            op: resolveQueryOp(),
        }
        // 多选下清空时 el-select 会把值置为 []；统一转 null，避免把空条件拼进查询。
        if (Array.isArray(para.value)) {
            if (para.value.length === 0) {
                para.value = null
            } else if (isDateQueryType(ftype.value) && para.op === 'between') {
                para.value = normalizeListDateRangeValue(para.value)
            }
        }
        else if (para.value == '') {
            para.value = null
        }
        console.log('onchange', para)
        emit('queryinput', para)
        var logicEmit = '='
        if (props.ctype == 'DatePicker' || isDateQueryType(ftype.value)) {
            logicEmit = 'between'
        }
        else if (props.ctype == 'Input') {
            logicEmit = 'contain'
        }
        emit('logicChange', logicEmit)
        // 选择型的立即执行
        // 注意：列表查询列通常传入的是 ctype=dict/bool/datetime/date，
        // 这些类型在这里也必须立即触发查询，否则“筛选框不生效”。
        if (props.ctype == 'Select' || props.ctype == 'Checkbox' || props.ctype == 'DatePicker'
            || ftype.value == 'dict' || ftype.value == 'bool' || ftype.value == 'date' || ftype.value == 'datetime'
        ) {
            onquery()
        }
        else {
            changeNotSay.value = true
        }
    }
    
    function onquery() {
        setTimeout(() => {
            changeNotSay.value = false
            emit('query')
        }, 100)
    }
    
    
    
    onMounted(() => {
    })
    
    
    
    defineExpose({
        checkSayQuery,
        onchange,
        onquery,
    
    })
    //.queryEditImg background: url(../src/assets/images/wg_filter.gif);
    </script>
    <style scoped>
.vchead {
    --vc-cap-h: 36px;
    --vc-qhead-h: 26px;
    display: flex;
    flex-direction: column;
    width: 100%;
    min-height: 100%;
}
.vccap {
    box-sizing: border-box;
    height: var(--vc-cap-h);
    min-height: var(--vc-cap-h);
    padding: 0 10px;
    display: flex;
    align-items: center;
    line-height: 1.3;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    cursor: pointer;
}
.vchead--wrap .vccap {
    white-space: normal;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    line-clamp: 2;
}
.vchead.noquery .vccap {
    flex: 1;
    height: auto;
    min-height: var(--vc-cap-h);
}
.qhead {
  display: flex;
  align-items: center;
  background: #fff;
  box-sizing: border-box;
  height: var(--vc-qhead-h);
  min-height: var(--vc-qhead-h);
  padding: 0 4px;
  overflow: hidden;
  cursor: default;
}
.queryEditImg {
    cursor: pointer;
    width: 13px;
    height: 14px;
    display: block;
    
}
.vcqyrwse span.el-input__prefix{
    left:5px;
}
.vcqyrwse .el-input__inner{
    width:20px;
    padding: 0 !important;
}
.vcthead>.cell{
    display:flex;
    align-items:center;
}
.shinput{
    width: 100%;
    height:26px;
}
.shinput:deep(.el-input__wrapper),
.shinput:deep(.el-select__wrapper) {
    min-height: 26px;
    height: 26px;
    box-sizing: border-box;
    background: #fff;
    border: none;
    box-shadow: none;
}
.shinput:deep(.el-range-editor.el-input__wrapper) {
    min-height: 26px;
    height: 26px;
    padding-top: 0;
    padding-bottom: 0;
    background: #fff;
    border: none;
    box-shadow: none;
}
.shinput:deep(.el-range-input) {
    height: 24px;
    line-height: 24px;
    font-size: 12px;
}
.shinput:deep(.el-range-separator) {
    padding: 0 2px;
    font-size: 12px;
}
.shinput:deep(.el-input__inner) {
    height: 24px;
    line-height: 24px;
}
.shinput:deep(>div){
  box-shadow: none;
}
.shinput{
  box-shadow: none;
}
.qhead:deep(>div){
  box-shadow: none; 
}
.q-op{
  color:#999;
  flex: 0 0 22px;
  width: 22px;
  height: 22px;
  margin-left: 2px;
  border-left: 1px solid #d8dee8;
  border-radius: 3px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.q-op:hover{
  color:#333;
  background: #eef3f8;
  cursor: pointer;
}
    
    </style>
