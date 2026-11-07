<template>
<div class="seeditpa" style="display: flex;">
    <slot name="label"></slot>
    <label v-if="label.length>0" v-html="label" style="flex:0 0 auto;" class="selabel"></label>
    <el-input ref="ed" :style="{width:width,flex:1}" v-if="isbaseType" :type="type" :modelValue="decodeVal" :placeholder="placeholder==''?'请输入内容':placeholder" @input="oninput" @blur="$emit('blur',$event)" :disabled="disabled" :autofocus="autofocus" @keyup.enter="$emit('keyenter')" :autosize="autosize" @change="onchange"
    ></el-input>
    <textarea ref="ed" class="setextarea" :style="{width:width,flex:1,height:height}" v-else-if="type=='txtarea'" type="textarea" :value="decodeVal" :placeholder="placeholder==''?'请输入内容':placeholder" 
        @change="ontxtareaInput" @blur="$emit('blur',$event)" @keyup.enter="$emit('keyenter')"  :disabled="disabled"></textarea>
    <el-date-picker ref="ed" v-else-if="isDateType(type)" :type="type" :format="showdatepic" :value-format="myvaluefmt"
        @keyup.enter="$emit('keyenter')" :style="{width:width}"
        :modelValue="decodeVal"  @input="oninput" :disabled="disabled"
        :placeholder="placeholder==''?'请选择日期':placeholder" @blur="$emit('blur',$event)" @change="onchange"></el-date-picker>
    <el-switch v-else-if="type=='boolean' || type=='bool'" ref="ed" :disabled="disabled"
        :modelValue="parsrbool(modelValue)" @input="oninput" @update:modelValue="oninput" @blur="$emit('blur',$event)" @change="onchange">
    </el-switch>
    <el-select v-else-if="type=='dict'" ref="ed" :model-value="decodeVal"
        clearable
        @update:model-value="oninput"
        @blur="$emit('blur',$event)" @change="onchange"
        :placeholder="placeholder==''?'请选择':placeholder" :disabled="disabled" :multiple="multiple" :multipleLimit="multipleLimit" :style="{width:width,flex:1,height:height}">
        <el-option
            v-for="(item,i) in codeValue"
            :key="i"
            :label="item.label"
            :value="item.value">
        </el-option>
    </el-select>
    <el-checkbox-group v-else-if="type=='checkbox'" ref="ed" :modelValue="decodeVal" @input="oninput" @blur="$emit('blur',$event)" @change="onchange"
    >
        <el-checkbox :label="item.value" v-for="(item,i) in codeValue" :disabled="item.disabled" :key="i">{{item.label}}</el-checkbox>
    </el-checkbox-group>
    <el-radio-group v-else-if="type=='radio'" ref="ed" :modelValue="decodeVal" @input="oninput" @blur="$emit('blur',$event)" @change="onchange"
    >
        <el-radio :label="item.value" :value="item.value" v-for="(item,i) in codeValue" :disabled="item.disabled" :key="i">{{item.label}}</el-radio>
    </el-radio-group>
    <uehtml v-else-if="type=='html'" ref="ed" :modelValue="decodeVal" @input="oninput" :disabled="disabled"
        :height="height" :placeholder="placeholder==''?'请输入内容':placeholder" style="flex:1;">
    </uehtml>
    <el-input-number ref="ed" :style="{width:width,flex:1}" v-else-if="type=='fullnum'" :modelValue="decodeVal" :placeholder="placeholder==''?'请输入内容':placeholder" @input="oninput" @blur="$emit('blur',$event)" @change="onchange" :disabled="disabled" @keyup.enter="$emit('keyenter')" :max="max" :min="min" :step="step" :precision="precision" :controls-position="controlsPosition" :step-strictly="strictly"></el-input-number>
    <divinput v-else-if="type=='div'" :modelValue="decodeVal" @input="oninput" :placeholder="placeholder==''?'请输入内容':placeholder" @keyup.enter="onenter" :disabled="disabled"   @blur="$emit('blur',$event)" @change="onchange">
    </divinput>
    <slot v-else name="editor" :modelValue="decodeVal" :oninput="oninput" :disabled="disabled"
        :height="height" :placeholder="placeholder==''?'请输入内容':placeholder"></slot>
    <slot></slot>
</div>
</template>
<style scoped>
.seeditpa {
    width: 100%;
}
</style>
<script>
import uehtml from "./uehtml.vue"
import divinput from "./divinput.vue"
/**
 * 增加lazy模式，lazy下，自行管理响应变化值，向外输出，而不直接依赖。
 */
export default {
    setup() {

    },
    name:"seditor",
    components:{uehtml,divinput},
    emits:["change","input","blur","keyenter"],
    props: {
        modelValue: null,
        placeholder: { type: String, default: "" },
        label: { type: String, default: "" },
        //text/datetime/codetable/textarea/html
        type: { type: String, default: "text" },
        //日期控件特有
        //日期类型 year/month/date/dates/ week/datetime/datetimerange/ daterange/monthrange
        showFormat: { type: String, },
        valueFormat: { type: String, },
        //代码表特有
        dict: null,
        dictlist:null,
        //如果是，自动调取，否则,需要手动传入代码值
        autoGetDict: { type: Boolean, default: true },

        disabled: { type: Boolean, default: false },

        inline: { type: Boolean, default: true },
        width: { type: String, default: "auto" },
        valueWidth:{type:String},
        //高度只对textarea/html有效
        height: { type: String, default: "" },
        //只有代码表可用的多选和多选限制
        multiple: { type: Boolean, default: false },
        multipleLimit: { type: Number, default: 0 },
        //是否自动聚焦
        autofocus: { type: Boolean, default: false },
        autosize: { type: [Boolean,Object], default: false },
        clearable:{ type: Boolean, default: false },
        lazy:{type:Boolean,default:false},
        //fullnum 计数器特有属性
        max:{type:Number},
        min:{type:Number},
        step:{type:Number},
        precision:{type:Number},
        controlsPosition:null,
        strictly:{type:Boolean,default:false},
        /** 多选字典值拼接分隔符（存库为字符串，如 P;F） */
        seprator: { type: String, default: ";" },
    },
    data() {
        var cv = [];
        if (this.autoGetDict == false && this.dictlist != null) {
            cv = this.dictlist;
        }
        return {
            codeValue: cv,
            valtemp: null,
        };
    },
    watch:{
        dictlist:function(e){
            this.codeValue=e;
        },
        modelValue:function(e){
            if(this.valtemp!=e){
                this.valtemp=e;
            }
        },
    },
    mounted() {
        this.loadCodeValue();
    },
    computed: {
        isbaseType() {
            return this.type == "text" || this.type == "number"|| this.type == "textarea";
        },
        decodeVal() {
            if(this.type=="textarea"  && (this.modelValue==null||this.modelValue==void 0)){
                return "";
            }
            const raw = !this.lazy ? this.modelValue : (this.valtemp == null ? (this.valtemp = this.modelValue) : this.valtemp);
            if (this.isMultiDictValue()) {
                return this.splitMultiValue(raw);
            }
            if(!this.lazy){
                return this.modelValue;
            }
            if (this.valtemp==null){
                this.valtemp = this.modelValue;
            }
            if (this.type == "number") {
                //因数值内内容，存在输入“抖动”的问题，输出值被自动格式化，加了.00。导致用户修改困难，这里做此修改。

                if (this.modelValue * 1 != this.valtemp * 1) {
                    this.valtemp = this.modelValue;
                }
                return this.valtemp;
            }
            if( this.valtemp != this.modelValue ){
                this.valtemp = this.modelValue
            }
            if(this.lazy){
                return this.valtemp;
            }
            return this.modelValue;
        },
        showdatepic() {
            //将格式化转换为Element控件要求的格式
            if (typeof this.showFormat != "string") return null;
            var fmt = this.showFormat.replace(/Y/g, "y");
            fmt = fmt.replace(/D/g, "d");
            return fmt;
        },
        myvaluefmt(){
            if(this.valueFormat!=null) return this.valueFormat;
            if(this.type=="date"){
                return "YYYY-MM-DD HH:mm:ss";
            }
            else if(this.type=="datetime"){
                return "YYYY-MM-DD HH:mm:ss";
            }
            else if(this.type=="year"){
                return "YYYY";
            }
            return null; 
        },
    },
    methods: {
        isMultiDictValue() {
            return (this.type === "dict" && this.multiple) || this.type === "checkbox";
        },
        splitMultiValue(val) {
            if (Array.isArray(val)) {
                return val;
            }
            if (val == null || val === "") {
                return [];
            }
            const sep = this.seprator || ";";
            if (typeof sep === "string" && sep.length > 0) {
                return String(val).split(sep).map((s) => s.trim()).filter(Boolean);
            }
            return String(val).split(/[,;]/).map((s) => s.trim()).filter(Boolean);
        },
        joinMultiValue(val) {
            if (!Array.isArray(val)) {
                return val == null ? "" : val;
            }
            const sep = this.seprator || ";";
            return val.map((s) => String(s).trim()).filter(Boolean).join(sep);
        },
        focus() {
            if (!!this.$refs.ed && typeof this.$refs.ed.focus == "function") {
                this.$refs.ed.focus();
            }
        },
        isDateType(type) {
            var arr = ["year", "month", "date", "dates", " week", "datetime", "datetimerange", "daterange", "monthrange"]
            return arr.indexOf(type) != -1;
        },
        onenter(e){
            this.$emit('keyenter');
        },
        oninput(e) {
            if (e === undefined) {
                e = null;
            }
            let out = e;
            if (this.isMultiDictValue() && Array.isArray(e)) {
                out = this.joinMultiValue(e);
            }
            this.valtemp = out;
            this.$emit("input", out);
            this.$emit("update:model-value", out);
        },
        onchange(e){
            let out = e;
            if (this.isMultiDictValue() && Array.isArray(e)) {
                out = this.joinMultiValue(e);
            }
            this.$emit("change", out);
            this.$emit("update:model-value", out);
        },

        parsrbool(val) {
            if (val == "true" || val == "True" || val == true) return true;
            return false;
        },
        formatDate(datee, fmt) {
            return window.formatDate(datee, fmt);
        },
        wrapUrl(file) {
            return UCMLLocalResourcePath + "OtherSource/UCMLFileDownloadNew.aspx?path=" + file.FA_Path + "&name=" + file.FA_Name + "&action=UploadToWeb";
        },
        loadCodeValue() {
            if(Array.isArray(this.dictlist)){
                this.codeValue=this.dictlist;
                return ;
            }
            if (this.type == "dict" && this.autoGetDict) {
                var res= this.getDictRefs( this.dict);
                this.codeValue=res[this.dict];
            }
        },
        ontxtareaInput(e) {
            this.$emit("update:modelValue", e.target.value);
        },
    },
}
</script>
<style >
    .selabel{
        font-size:14px;color:#666;
    }
    .setextarea{
        border:1px solid #999ca3;
        outline:none;
        border-radius: 3px;
        background-color: #fff;
        padding: 5px 15px;
        line-height: 1.5;
    }
div.fmtdiva{
    height:v-bind(height);
    padding:5px 15px;
    line-height:1.5;
    border-radius:3px;
    border-color:#E4E7ED;
    background-color: #FFFFFF;
    border:1px solid rgb(168,171,178);
    text-align: left;
    white-space: pre-wrap;
}
div.fmtdiva:focus{
  border: 1px solid #409EFF;
  
}
</style>
