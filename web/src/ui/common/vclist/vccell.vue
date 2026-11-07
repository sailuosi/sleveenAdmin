<template>
<span :class="{'overdot':!!maxHeight,'vcelval':true}">
    <template v-if="type=='tip'">
        <el-tooltip placement="top" trigger="hover">
            <template #content><div class="celltipval">{{rendVal}}</div></template>
            <div :style="css" v-text="rendVal"></div>
        </el-tooltip>
    </template>
    <template v-else-if="ctype == 'Boolean' || ctype == 'bool'">
        <el-switch :model-value="boolVal" disabled></el-switch>
    </template>
    <template v-else-if="ctype == 'dict'">
        <span v-if="plainDict" :title="rendVal">{{rendVal}}</span>
        <el-tag v-else-if="!!code?.theme && rendVal" :type="code?.theme" class="celltag" :title="rendVal">{{rendVal}}</el-tag>
        <span v-else>
            {{rendVal}}
        </span>
    </template>
    <span v-else :style="css" v-html="rendVal" :title="rendVal"></span>

</span>
</template>
<script>

import { formatDate,formatCurrency } from "./tool.js"

export default {
    name:"vccell",
    props: {
        type:null,
        ctype:null,
        baseVal:null,
        value:null,
        getCellStyle:null,
        maxHeight:null,
        dictlist:null,
        maxLine:null,
        fmt:null,//格式化参数
        /** 字典列仅显示纯文字，不使用标签样式 */
        plainDict:{ type:Boolean, default:false },
    },
    computed:{
    	boolVal() {
    		return this.value === true || this.value === "true" || this.value === 1 || this.value === "1";
    	},
        css(){
            var res={};
            if(!!this.maxHeight){
                res["max-height"]=this.maxHeight;
                res["text-overflow"]="ellipsis"; 
                res.overflow="hidden";
            }
            if(!!this.maxLine){
                if(this.maxLine*1==1){
                    res["text-overflow"]="ellipsis";
                    res["-o-text-overflow"]="ellipsis";
                    res["overflow"]="hidden";
                    res["white-space"]="nowrap";
                }
                else{
                    res["display"]="-webkit-box";
                    res["-webkit-line-clamp"]=this.maxLine;
                    res["-webkit-box-orient"]="vertical";
                }
            }
            return res;
        },
        dictMap(){
            var res={};
            if(Array.isArray(this.dictlist)){
                this.dictlist.forEach((dict)=>{
                    var keyFields = [dict?.value, dict?.key, dict?.Code, dict?.code, dict?.id, dict?.ID, dict?.Value,
                        dict?.label, dict?.text, dict?.Name, dict?.name, dict?.Caption, dict?.caption, dict?.Title, dict?.title, dict?.CodeName];
                    keyFields.forEach((k)=>{
                        this.normalizeCodeKeys(k).forEach((kk)=>{
                            res[kk]=dict;
                        });
                    });
                })
            }
            return res;
        },
        code(){
            try{
                var arr = this.extractRawValues(this.baseVal);
                if(arr.length>1){
                    var res=arr.map((item)=>{
                        return this.findDictByValue(item);
                    }).filter((item)=>item!=null && item!=undefined);
                    if(res.length>0){
                        return res;
                    }
                    return null;
                }
                var t= this.findDictByValue(this.baseVal);
                if(!!t){
                    if(!t.theme){
                        t.theme=null;
                    }
                    return t;
                }
                // 兼容组合码连写（如 HF），按字典项拆分后再映射标签
                var joined = this.findDictListByJoinedCode(this.baseVal);
                if(Array.isArray(joined) && joined.length>0){
                    return joined;
                }
            }
            catch(e){
                //return null;
            }

            // 无原始值时不应渲染空标签；有值但未匹配字典时仍用 info 标签展示原文
            if(this.extractRawValues(this.baseVal).length===0){
                return null;
            }
            return {
                theme:"info"
            };
        },
        rendVal(){
            var v=this.value;
            try{
                if(this.ctype=="datetime"){
                    var fmt= this.fmt;
                    if(!fmt){
                        fmt="YYYY-MM-DD hh:mm:ss";
                    }
                    return formatDate(v,fmt)
                }
                if(this.ctype=="date"){
                    var fmt= this.fmt;
                    if(!fmt){
                        fmt="YYYY-MM-DD";
                    }
                    return formatDate(v,fmt)
                }
                else if (this.ctype == "year") {
                    var fmt= this.fmt;
                    if(!fmt){
                        fmt="YYYY";
                    }
                    return formatDate(v,fmt)
                }
                else if (this.ctype == "money") {
                    return formatCurrency(v,this.fmt)
                }
                if(Array.isArray(this.code)){
                    return this.code.map((item)=>this.dictItemCaption(item)).filter(Boolean).join(',');
                }
                if(!!this.code){
                    const cap = this.dictItemCaption(this.code);
                    if(cap){
                        return cap;
                    }
                }
            }
            catch(e){

            }
            return this.normalizeDisplayText(v);
        },
    },
    methods:{
        dictItemCaption(item){
            if(item==null || typeof item!=="object"){
                return "";
            }
            return item.label || item.text || item.Name || item.name || item.Caption || item.caption || item.Title || item.title || "";
        },
        normalizeDisplayText(v){
            if(v===null || v===undefined){
                return "";
            }
            if(Array.isArray(v)){
                return v.map((item)=>this.normalizeDisplayText(item)).filter(Boolean).join("、");
            }
            var raw = String(v).trim();
            if(raw===""){
                return "";
            }
            // 优先解析标准 JSON 字符串/数组
            if((raw.startsWith("[") && raw.endsWith("]")) || (raw.startsWith("\"") && raw.endsWith("\""))){
                try{
                    var parsed = JSON.parse(raw);
                    return this.normalizeDisplayText(parsed);
                }catch(e){
                    // 非标准 JSON 时继续降级处理
                }
            }
            // 处理单引号数组/字符串，如 ['培训']、'2026年'
            if(raw.startsWith("[") && raw.endsWith("]")){
                var inner = raw.substring(1,raw.length-1).trim();
                if(inner===""){
                    return "";
                }
                return inner.split(",").map((item)=>{
                    var t = String(item).trim();
                    if((t.startsWith("\"") && t.endsWith("\"")) || (t.startsWith("'") && t.endsWith("'"))){
                        t = t.substring(1,t.length-1);
                    }
                    return t.trim();
                }).filter(Boolean).join("、");
            }
            if((raw.startsWith("\"") && raw.endsWith("\"")) || (raw.startsWith("'") && raw.endsWith("'"))){
                return raw.substring(1,raw.length-1).trim();
            }
            return raw;
        },
        extractRawValues(v){
            if(v===null || v===undefined){
                return [];
            }
            if(Array.isArray(v)){
                return v.map((item)=>String(item).trim()).filter(Boolean);
            }
            var raw = String(v).trim();
            if(raw===""){
                return [];
            }
            // JSON 数组字符串
            if(raw.startsWith("[") && raw.endsWith("]")){
                try{
                    var parsed = JSON.parse(raw);
                    if(Array.isArray(parsed)){
                        return parsed.map((item)=>String(item).trim()).filter(Boolean);
                    }
                }catch(e){
                    // 非标准 JSON
                }
                // 单引号/非标准数组兜底
                return raw.substring(1,raw.length-1).split(",").map((item)=>{
                    var t = String(item).trim();
                    if((t.startsWith("\"") && t.endsWith("\"")) || (t.startsWith("'") && t.endsWith("'"))){
                        t = t.substring(1,t.length-1);
                    }
                    return t.trim();
                }).filter(Boolean);
            }
            // 兼容多种分隔符：英文/中文逗号、分号、顿号、斜杠
            var hasMultiSep = /[,，;；、/]/.test(raw);
            if(hasMultiSep){
                return raw.split(/[,，;；、/]/).map((item)=>String(item).trim()).filter(Boolean);
            }
            return [raw];
        },
        normalizeCodeKeys(v){
            var vals = this.extractRawValues(v);
            var res = new Set();
            vals.forEach((val)=>{
                var raw = this.normalizeDisplayText(val);
                if(raw==="") return;
                res.add(raw);
                var num = Number(raw);
                if(!Number.isNaN(num)){
                    res.add(String(num));
                    if(Number.isInteger(num)){
                        res.add(String(parseInt(raw,10)));
                    }
                }
            });
            return Array.from(res);
        },
        findDictByValue(v){
            var keys = this.normalizeCodeKeys(v);
            for(var i=0;i<keys.length;i++){
                var hit = this.dictMap[keys[i]];
                if(hit){
                    return hit;
                }
            }
            return null;
        },
        findDictListByJoinedCode(v){
            if(v===null || v===undefined){
                return null;
            }
            var raw = String(v).trim();
            if(raw==="" || /[,，;；、/\[\]]/.test(raw)){
                return null;
            }
            if(!Array.isArray(this.dictlist) || this.dictlist.length===0){
                return null;
            }
            var chars = raw.split("");
            if(chars.length<2){
                return null;
            }
            var res = [];
            for(var i=0;i<chars.length;i++){
                var hit = this.findDictByValue(chars[i]);
                if(!hit){
                    return null;
                }
                if(!hit.theme){
                    hit.theme = "primary";
                }
                res.push(hit);
            }
            return res;
        },
    }
}
</script>
<style scoped>
.vcelval{
    /* 关键：空值时 span 内没有文本，默认行内元素可能宽度为 0
       导致 @click.native 事件“点不到”，无法进入编辑态 */
    display:inline-block;
    width:100%;
    min-width:1px;
    vertical-align:middle;
    box-sizing:border-box;
}

.celltipval{
    max-width: 60vw;
}

.celltag{
    border:none;
    --el-tag-text-color: #449FFB;
    --el-tag-border-color: #449FFB;
    --el-tag-bg-color: rgba(68, 159, 251, 0.08);
    display: inline-flex;
    align-items: center;
    justify-content: center;
    height: 24px;
    line-height: 24px;
    padding-top: 0;
    padding-bottom: 0;
    max-width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    vertical-align: middle;
}

/* 默认 .celltag 强制了蓝色变量，会盖住 el-tag 的 type；danger 等需单独恢复主题色 */
.celltag.el-tag--danger {
    --el-tag-text-color: var(--el-color-danger, #f56c6c);
    --el-tag-border-color: var(--el-color-danger, #f56c6c);
    --el-tag-bg-color: var(--el-color-danger-light-9, rgba(245, 108, 108, 0.12));
}
.celltag.el-tag--success {
    --el-tag-text-color: var(--el-color-success, #67c23a);
    --el-tag-border-color: var(--el-color-success, #67c23a);
    --el-tag-bg-color: var(--el-color-success-light-9, rgba(103, 194, 58, 0.12));
}
.celltag.el-tag--warning {
    --el-tag-text-color: var(--el-color-warning, #e6a23c);
    --el-tag-border-color: var(--el-color-warning, #e6a23c);
    --el-tag-bg-color: var(--el-color-warning-light-9, rgba(230, 162, 60, 0.12));
}
.celltag.el-tag--info {
    --el-tag-text-color: var(--el-color-info, #909399);
    --el-tag-border-color: var(--el-color-info, #909399);
    --el-tag-bg-color: var(--el-color-info-light-9, rgba(144, 147, 153, 0.12));
}

.celltag :deep(.el-tag__content){
    display: inline-flex;
    align-items: center;
    line-height: 1;
}
</style>