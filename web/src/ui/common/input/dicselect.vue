<template>
<lazyspan @init="loadCodeValue">
    <el-select v-if="type=='select'" :modelValue="inputval" @update:modelValue="oninput" :placeholder="placeholder" :clearable="clearable" @change="onchange" :multiple="multi" :disabled="disabled">
        <el-option v-for="dict in mydicts" :key="dict.value" :label="dict.label" :value="dict.value"></el-option>
    </el-select>
    <el-checkbox-group v-else-if="type=='checkbox'" :disabled="disabled" :modelValue="inputval" @update:modelValue="oninput" :placeholder="placeholder" :clearable="clearable" @change="onchange">
        <el-checkbox :label="li.value" v-for="(li,l) in mydicts">{{li.label}}</el-checkbox>
    </el-checkbox-group>
    <el-checkbox-group v-else-if="type=='checkbtn'" :disabled="disabled" :modelValue="inputval" @update:modelValue="oninput" :placeholder="placeholder" :clearable="clearable" @change="onchange">
        <el-checkbox-button :label="li.value" v-for="(li,l) in mydicts">{{li.label}}</el-checkbox-button>
    </el-checkbox-group>
    <el-radio-group v-else-if="type=='radio'" :disabled="disabled" :modelValue="inputval" @update:modelValue="oninput" :placeholder="placeholder" :clearable="clearable" @change="onchange">
        <el-radio :label="li.value" :value="li.value" v-for="(li,l) in mydicts" :disabled="li.isReadOnly=='true'">{{li.label}}</el-radio>
    </el-radio-group>
    <el-radio-group v-else-if="type=='radiobtn'" :disabled="disabled" :modelValue="inputval" @update:modelValue="oninput" :placeholder="placeholder" :clearable="clearable" @change="onchange">
        <el-radio-button :label="li.value" :value="li.value" v-for="(li,l) in mydicts">{{li.label}}</el-radio-button>
    </el-radio-group>
</lazyspan>
</template>

<script>
import {getCodeValue} from "@/api/system/dictapi.js"
import {getEumValue} from "@/api/system/dictapi.js"

import lazyspan from "@/ui/panels/lazyspan.vue"

export default {
    name: "dicselect",
    components:{
        lazyspan
    },
    data() {
        return {
            renderlist: [],
        };
    },
    props: {
        type: {
            type: String,
            default: "select"
        }, // select checkbox checkbtn radio
        dict: {
            type: String
        },
        modelValue: [String, Number, Array],
        placeholder: {
            type: String,
            default: "请选择...",
        },
        // 
        dictlist: {
            type: Array,
        },
        clearable: {
            type: Boolean,
            default: false
        },

	    disabled:{type:Boolean,default:false},
        multi: {
            type: Boolean,
            default: false
        },
        seprator: {//多选时是否自动切割
            type: String,
            default: ""
        },
    },
    computed: {
        mydicts(){
            if(Array.isArray(this.dictlist) && this.dictlist.length>0){
                return this.dictlist;
            }
            return this.renderlist;
        },
        spliter(){
            if(!!this.seprator){
                return this.seprator;
            }
            return /[,;]/;
        },
        inputval() {
            //因为字典全是字符串，这里进行一个转换。
            var tar = this.modelValue;
            // 兼容字典项 value 类型和模型值类型不一致（number/string/boolean）的场景
            if (!this.multi && this.mydicts.length > 0 && tar != null) {
                const dictValType = typeof this.mydicts[0].value;
                // 常见是否字典使用 0/1，模型可能是 bool，先归一化
                if (typeof tar === "boolean") {
                    if (dictValType === "string") {
                        tar = tar ? "1" : "0";
                    } else if (dictValType === "number") {
                        tar = tar ? 1 : 0;
                    }
                }
                if (dictValType === "string" && typeof tar !== "string") {
                    tar = String(tar);
                } else if (dictValType === "number" && typeof tar === "string" && tar !== "") {
                    const numVal = Number(tar);
                    if (!Number.isNaN(numVal)) {
                        tar = numVal;
                    }
                }
            }
            // 纯数字代码与字典项 value 不一致时对齐（如库中 "01" 与字典 "1"，el-option 无法匹配会显示原始码）
            if (!this.multi && this.mydicts.length > 0 && tar != null && tar !== "") {
                const direct = this.mydicts.some(
                    (d) => d.value === tar || Object.is(d.value, tar)
                );
                if (!direct) {
                    const s = String(tar).trim();
                    if (/^\d+$/.test(s)) {
                        const n = Number(s);
                        const hit = this.mydicts.find((d) => {
                            const dv = d.value;
                            if (dv === n || Number(dv) === n) return true;
                            if (typeof dv === "string" && /^\d+$/.test(dv.trim()) && Number(dv) === n) {
                                return true;
                            }
                            return false;
                        });
                        if (hit) {
                            tar = hit.value;
                        }
                    }
                }
            }
            if (this.multi) {
                if (Array.isArray(tar)) {
                    return tar;
                }
                if (tar == null || tar == void 0 || tar == "") {
                    return [];
                }
                if ( typeof this.spliter.test=="function" && this.spliter.test(tar)) {
                    return tar.split(this.spliter)
                }
                else if(typeof this.spliter=="string"){
                    return tar.split(this.spliter)
                }
                return [tar];
            }
            return tar;
        },
    },
    created() {

        if (this.dict == void 0) {
            this.renderlist = this.dictlist;
        } else {

        }  
    },
    watch: {
        dictlist: function (val) {
            this.renderlist = val;
        }
    },
    emits: ["update:modelValue", "input", "change"],
    methods: {
        oninput(e) {
            var val = e;
            if (this.multi && !!this.seprator && Array.isArray(val)) {
                val = val.join(this.seprator)
            }
            this.$emit("update:modelValue", val);
            this.$emit("input", val);
        },
        onchange(e) {
            var val = e;
            if (this.multi && !!this.seprator && Array.isArray(val)) {
                val = val.join(this.seprator)
            }
            this.$emit("change", val);
        },
        loadCodeValue(){
            
            if(this.dictype =='') return;
            if( !!this.dict ){
                //检查是否有缓存
                if(window.codeMap==void 0){
                    window.codeMap={};
                }
                var codemap=window.codeMap;
                if(Array.isArray(codemap[this.dict]) && codemap[this.dict].length>0){
                    this.renderlist=codemap[this.dict];
                    return ;
                }

                // 门户代码表已从 ZH_ 迁移到 HH_：主码表无数据时互为回退
                var altDict = null;
                if (this.dict.indexOf("ZH_") === 0) {
                    altDict = "HH_" + this.dict.slice(3);
                } else if (this.dict.indexOf("HH_") === 0) {
                    altDict = "ZH_" + this.dict.slice(3);
                }

                getCodeValue(this.dict).then((res)=>{
                    var list = res && res.data;
                    if (Array.isArray(list) && list.length > 0) {
                        this.renderlist = list;
                        window.codeMap[this.dict] = list;
                        return;
                    }
                    if (!altDict) {
                        this.renderlist = Array.isArray(list) ? list : [];
                        return;
                    }
                    getCodeValue(altDict).then((res2)=>{
                        var list2 = res2 && res2.data;
                        this.renderlist = Array.isArray(list2) ? list2 : [];
                        if (this.renderlist.length > 0) {
                            window.codeMap[this.dict] = this.renderlist;
                            window.codeMap[altDict] = this.renderlist;
                        }
                    });
                })
            }
            if(!!this.enumKey){
                getEumValue(this.enumKey).then((res)=>{
                    this.renderlist=res.data;
                })
            }
            
        },
    }
};
</script>
