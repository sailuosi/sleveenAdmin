<template>
    <div class="smcell-i">
        <template v-if="editing && ctype !='checkbox'">
            <input class="simp" v-if="ctype=='date'" type="date" v-model="cellValue" v-focus @blur="sayFoucusOut" />
            <input class="simp" v-else-if="ctype=='number'" type="number" v-model="cellValue" v-focus @blur="sayFoucusOut" />
            <select v-else-if="ctype=='option'" v-model="cellValue" v-toclick @blur="sayFoucusOut">
                <option v-for="(opt,o) in col.options" :value="opt.value">{{ opt.label }}</option>
            </select>
            <div v-else-if="ctype=='txtAppend'" class="txtpand">
                <input type="text" v-model="cellValue" v-focus spellcheck="false" @blur="sayFoucusOut" />
                <slot name="txtAppend" :row="row" :col="col"></slot>
            </div>
            <input class="simp" v-else type="text" v-model="cellValue" v-focus spellcheck="false" @blur="sayFoucusOut" />
            <slot name="epend" :row="row" :col="col"></slot>
        </template>
        <template v-else>
            <input v-if="ctype=='checkbox'" type="checkbox" v-model="cellValue" @blur="sayFoucusOut" />
            <span v-else>{{ getCellCap() }}</span>            
        </template>
        <slot></slot>
    </div>
</template>

<script setup>
import { useSlots, useAttrs, provide, inject, computed } from 'vue'

const props = defineProps({
    row: null,
    col:null,
    editing:{type:Boolean,default:false},//是否处于编辑模式
})
const vFocus = {
  mounted: (el) => el.focus()
}
const vToclick = {
  mounted: (el) => el.click()
}
const emit = defineEmits(['change', 'blur'])

const ctype= computed(()=>{
    var col=props.col;
    return col.ctype; 
})

function sayFoucusOut(){
    emit('blur',props.row,props.col);
}

function getCodeCap(code, list) {
    if (Array.isArray(list)) {
        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            if (item.value == code) {
                return item.label;
            }
        }
    }
    return code;
}

const cellValue= computed({
    get(){
        return getCellVal(); 
    },
    set: (val)=>{
        emit('change',val); 
        props.row[props.col.field]    = val;
    }
})

function getCellVal() {
    var row=props.row;
    var col=props.col;
    var text = row[col.field];
    if (Array.isArray(col.options)) {
        if (typeof text == 'string' && text.indexOf(';') > 0) {
            var arr = text.split(';');
            return arr;
        }
        return text;
    }



    if (typeof text == 'undefined') {
        return "";
    } else {
        return text;
    }
}
function getCellCap() {
    var row=props.row;
    var col=props.col;
    var text = row[col.field];
    if (Array.isArray(col.options)) {
        if (typeof text == 'string' && text.indexOf(';') > 0) {
            var arr = text.split(';');
            var caps = arr.map((item) => { return getCodeCap(item, col.options); });
            return caps.join(',');
        }
        return getCodeCap(text, col.options);
    }



    if (typeof text == 'undefined') {
        return "";
    } else {
        return text;
    }
}
defineExpose({

})
</script>

<style scoped>
.smcell-i{
    width: 100%;
}
input.simp{
    spellcheck:false;
    width: -webkit-fill-available;
}
.txtpand{
    display: flex;
    flex-direction: row;
}
.txtpand>input{
    flex: 1;
    width: 0;
}
</style>