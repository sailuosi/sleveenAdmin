<template>
    <el-form-item :label="label" :prop="prop" :label-width="rendtype=='boolcheck'?'auto':labelWidth" class="fminput" :rules="myRules">
        <template #label>
            <span v-if="rendtype=='boolcheck'" class="boolcheck">
                <el-checkbox :modelValue="parsebool(modelValue)" @update:model-value="oninput" :label="label"  />
            </span>
            <span v-else>
                {{ label }}
            </span>
        </template>
        <template v-if="fake">
            <template v-if="dictype != ''">
                <dictag :options="renderlist" :value="inputval" :dict="dict"></dictag>
            </template>
            <div v-else class="fminput-fake">{{ inputval }}</div>
        </template>
        <template v-else-if="noedit">
            <dictag v-if="rendtype == 'dict'" :dictlist="renderlist" :value="inputval" :dict="dict"></dictag>
            <span v-else-if="rendtype == 'date' || rendmode == 'datepick'">{{ renddate }}</span>
            <span v-else>
                {{ inputval }}
            </span>
        </template>
        <template v-else>
            <el-input v-if="rendmode == 'input'" :type="type" :title="inputval" :modelValue="inputval" @update:model-value="oninput"  :class="['custom-input-style', { disabled: disabled }]" class="einput"
                :placeholder="renderholder" @keyup.enter="onenter" @change="onchange" :clearable="clearable"
                :style="{ width: valueWidth }" :disabled="disabled" :autosize="autosize" :rows="rows">
                <template #append v-if="!!append">
                    <slot name="append" > 
                        <el-button v-if="btn" class="btnsty"  @click="callParent">{{ append }}</el-button>
                        <span v-else>{{ append }}</span>
                    </slot>
                </template>
                <template #suffix>
                    <span v-if="!!suffix">{{suffix}}</span>
                </template>
            </el-input>
            <dicse v-else-if="dictype != ''" :modelValue="inputval" @update:model-value="oninput" :placeholder="renderholder"
                @keyup.enter="onenter" :disabled="disabled" :style="{ width: valueWidth }" @change="onchange"
                :type="dictype" :multi="multi" :seprator="seprator" :clearable="clearable" :dictlist="renderlist"
                :dict="dict" :class="['custom-input-style', { disabled: disabled }]"></dicse>

            <dateRangeInput v-else-if="isDateRange && sameMonthRange" v-model:start="rangeStart" v-model:end="rangeEnd"
                :disabled="disabled" :default-value="dateDefaultValue" :show-time="rendtype == 'datetimerange'"
                :disabled-date-fn="disabledDateFn" :style="{ width: valueWidth }" @change="onchange" />
            <el-date-picker v-else-if="rendmode == 'datepick'" :clearable="clearable" :disabled="disabled"
                :modelValue="inputval" @update:modelValue="oninput" :placeholder="renderholder" @change="onchange"
                :type="rendtype" :format="myvaluefmt" :value-format="pickerValueFormat" :disabled-date="disabledDateFn" :class="['custom-input-style', { disabled: disabled }]"
                :style="{ width: valueWidth }">
            </el-date-picker>
            <textarea v-else-if="rendtype == 'txtarea'" :value="inputval" @update:model-value="oninput($event.target.value)"
                :placeholder="renderholder" @keyup.enter="onenter" :clearable="clearable"
                :style="{ width: valueWidth ? valueWidth : '100%' }" :disabled="disabled" class="fmtarea" @change="onchange" :class="['custom-input-style', { disabled: disabled }]"/>
            <divinput v-else-if="rendtype == 'div'" :value="inputval" @update:model-value="oninput($event)" :placeholder="renderholder"
                @keyenter="onenter" :clearable="clearable" :disabled="disabled" @change="onchange" :class="['custom-input-style', { disabled: disabled }]">
            </divinput>
            <el-switch v-else-if="rendtype == 'boolean'||rendtype == 'bool'" ref="ed" :disabled="disabled" @change="onchange"
                :modelValue="parsebool(modelValue)" @update:model-value="oninput" @blur="onBlur">
            </el-switch>
            <el-switch v-else-if="rendtype == 'switch10'" ref="ed" :disabled="disabled" :active-value="on"
                :inactive-value="off" :modelValue="modelValue" @update:model-value="oninput" @blur="onBlur"
                @change="onchange">
            </el-switch>
            <el-input-number v-else-if="rendtype == 'float'" :modelValue="parsenum(modelValue)" @update:model-value="oninput"
                @blur="$emit('blur', $event)" @change="onchange" :precision="precision" :step="step" :max="max"
                :min="min" :class="['custom-input-style', { disabled: disabled }]"></el-input-number>
        </template>
        <slot></slot>
    </el-form-item>
</template>

<script setup>
import { computed, inject, onMounted, ref, watch, defineOptions, defineProps, defineEmits } from 'vue'
import { formatDate } from "../vclist/tool.js"
import { Search } from '@element-plus/icons-vue'
//子组件
import dicse from "./dicselect.vue"
import divinput from "./divinput.vue"
import dictag from "../dictag.vue"
import dateRangeInput from "./dateRangeInput.vue"


defineOptions({
    name: 'fminput',
})

const props = defineProps({
    label: { type: String, default: "" },
    append: { type: String, default: "" },
    suffix: { type: String, default: "" },
    fake: { type: Boolean, default: false },//是否假造输入框样式。ps:使用假造的输入框可以提高性能。
    prop: [String, Number, Array],
    placeholder: { type: String },
    showPlaceholder: { type: Boolean, default: true },
    modelValue: null,
    type: { type: String, default: "" },
    datefmt: { type: String, default: null },
    valuefmt: null,
    
    multi: { type: Boolean, default: false },
    seprator: { type: String, default: "," },
    //
    dict: { type: String, default: "" },//字典名，此时自动执行查询
    enumKey: { type: String, default: "" },//字典名，此时自动执行查询
    dictlist: { type: Array },
    disabled: { type: Boolean, default: false },
    noedit: { type: Boolean, default: false },
    clearable: { type: Boolean, default: false },
    labelWidth: { type: String },
    valueWidth: { type: String },
    height: { type: String, default: "auto" },
    autosize: { type: [Boolean, Object], default: false },
    //switch特殊属性
    on: { type: [String, Number], default: 1 },
    off: { type: [String, Number], default: 0 },
    btn: { type: Boolean, default: false }, //是否有按钮
    must: { type: Boolean, default: false }, //是否必填
    //数值控件专业
    max: null,
    min: null,
    step: null,
    precision: null,

    rows: null,
    sameMonthRange: { type: Boolean, default: false },
    dateDefaultValue: { type: Date, default: null },
    disabledDateFn: {
        type: Function,
        default: (time) => {
            return false;
        }
    }
});

const emit = defineEmits(['update:modelValue', 'change', 'appendClcik', 'keyenter', 'blur']);

const renderlist = ref([]);
const elForm = inject('elForm', null);

const myRules = computed(() => {
    const res = [];
    if (props.must) {
        const msg = props.label + "不能为空";
        res.push({
            required: true,
            message: msg,
            trigger: ['blur', 'change'],
        });
    }
    return res;
});

const rendval = computed(() => {
    if (props.modelValue != void 0 && props.modelValue != null) {
        return props.modelValue;
    }
    if (elForm != null) {
        const model = elForm.model;
        if (!!model) {
            return model[props.prop];
        }
    }
    return null;
});

const myvaluefmt = computed(() => {
    if (props.valuefmt != null) return props.valuefmt;
    if (rendtype.value == "date") {
        return "YYYY-MM-DD";
    }
    else if (rendtype.value == "datetime") {
        return "YYYY-MM-DD HH:mm:ss";
    }
    else if (rendtype.value == "year") {
        return "YYYY";
    }
    return null;
});

/** 绑定值为字符串，避免 Date 序列化成 UTC 导致日期少一天 */
const pickerValueFormat = computed(() => {
    if (rendmode.value !== "datepick") return undefined;
    if (rendtype.value === "date") return "YYYY-MM-DD";
    if (rendtype.value === "datetime") return "YYYY-MM-DD HH:mm:ss";
    if (rendtype.value === "year") return "YYYY";
    return undefined;
});

//////为日期范围进行特殊处理，日期范围时，允许绑定开始和结束2个值。
const start= defineModel("start");
const end= defineModel("end");

const year= defineModel("year");
const month= defineModel("month");

const isDateRange = computed(() => rendtype.value == "daterange" || rendtype.value == "datetimerange");
const rangeStart = computed({
    get: () => rendtype.value == "datetimerange" ? start.value : rendval.value?.[0],
    set: (val) => {
        if(rendtype.value == "datetimerange"){
            start.value = val;
        }else{
            emit("update:modelValue", [val, rangeEnd.value]);
        }
    }
});
const rangeEnd = computed({
    get: () => rendtype.value == "datetimerange" ? end.value : rendval.value?.[1],
    set: (val) => {
        if(rendtype.value == "datetimerange"){
            end.value = val;
        }else{
            emit("update:modelValue", [rangeStart.value, val]);
        }
    }
});


const inputval = computed(() => {
    if(props.type == "datetimerange"){
        return [start.value, end.value];
    }
    if(rendtype.value == "month"){
        return new Date(year.value + "-" + month.value + "-01");
    }
    //因为字典全是字符串，这里进行一个转换。
    if (rendval.value == "1900-01-01T00:00:00") {
        return null;
    }
    if (rendmode.value == 'datepick') {
        if (rendval.value == null || rendval.value === "") return null;
        if (typeof rendval.value === "string") {
            const fmt = pickerValueFormat.value || myvaluefmt.value;
            return formatDate(rendval.value, fmt || "YYYY-MM-DD");
        }
        if (rendval.value instanceof Date) {
            const fmt = pickerValueFormat.value || myvaluefmt.value;
            return fmt ? formatDate(rendval.value, fmt) : rendval.value;
        }
        return rendval.value;
    }

    return rendval.value;
});

const renderholder = computed(() => {
    if (props.showPlaceholder === false) {
        return "";
    }

    if (!!props.placeholder && typeof props.placeholder == "string") {
        return props.placeholder;
    }
    return "请输入" + props.label + "...";
});

const rendtype = computed(() => {
    if (props.type != '') return props.type;
    if (Array.isArray(props.dictlist)) return "dict";
    return "text";
});

const dictype = computed(() => {
    if (rendtype.value == "dict") {
        return "select";
    }
    //select checkbox checkbtn radio
    if (/(select)|(checkbox)|(checkbtn)|(radio)|(radiobtn)/.test(rendtype.value)) {

        return rendtype.value
    }
    return "";
}); 

const rendmode = computed(() => {
    //year/month/date/dates/datetime/ week/datetimerange/daterange/ monthrange
    var datepicks = ["year", "month", "date", "dates", "datetime", " week", "datetimerange", "daterange", "monthrange"];
    if (datepicks.indexOf(rendtype.value) != -1) {
        return 'datepick';
    }
    var inputs = ["text", "number", "textarea"];
    if (inputs.indexOf(rendtype.value) != -1) {
        return 'input';
    }
    return rendtype.value;
});

const mydatefmt = computed(() => {
    if (props.datefmt != null) return props.datefmt;
    if (props.type == "date") {
        return "YYYY-MM-DD";
    }
    else if (props.type == "datetime") {
        return "YYYY-MM-DD hh:mm:ss";
    }
    return "YYYY-MM-DD";
});

const renddate = computed(() => {
    return formatDate(props.modelValue, mydatefmt.value);
});

onMounted(() => {

    if (Array.isArray(props.dictlist) && props.dictlist.length > 0) {
        renderlist.value = props.dictlist;
    } else {

    }
});

watch(() => props.dictlist, (val) => {
    renderlist.value = val;
});

const oninput = (e) => {
    if(props.type == "datetimerange"){
        start.value = e[0];
        end.value = e[1];
    }
    else{
        emit("update:modelValue", e);
    }
    if(rendtype.value == "month"){
        //如果是月份，支持年、月2个参数的绑定
        if(e!=null && e!=undefined){
            year.value = e.getFullYear();
            month.value = e.getMonth() + 1;
        }
    }
};
const onenter = (e) => {
    emit("keyenter");
};
const onchange = (e) => {
    emit("change", e);
};
const onBlur = (e) => {
    emit('blur', e);
}
const parsebool = (val) => {
    if (val == "true" || val == "True" || val == true || val == "1" || val == 1) return true;
    return false;
};
const parsenum = (val) => {
    return 1 * val;
};
const callParent = () => {
    emit('appendClcik');
};
</script>
<style scoped>
.boolcheck{
    margin-left:16px;
}


.fminput-fake {
    border-color: #e4e7ed;
    border: 1px solid #e4e7ed;
    line-height: 26px;
    padding: 3px 15px;
    margin: 3px 0;
    border-radius: 4px;
    min-height: 26px;
    word-break: break-all;
}

.fminput {
    display: flex;
    align-items: center;
}

.fminput>div.el-form-item__content {
    flex: 1;
    margin-left: 0 !important;
}



button.el-button.btnsty {
    background-color: rgb(39, 86, 156);
    color:white;
    border: 0px;
}
button.el-button.btnsty:hover {
    background-color: rgb(111, 131, 161);
    color:white; 
    border: 0px;
}
textarea.fmtarea {
    height: v-bind(height);
    outline: none;
    padding: 3px 15px;
    line-height: 1.5;
    border-radius: 3px;
    border-color: #E4E7ED;
    background-color: #FFFFFF;
    font-family: inherit;
}

textarea.fmtarea:focus {
    border: 1px solid #409EFF;

}

div.fmtdiva {
    height: v-bind(height);
    padding: 5px 15px;
    line-height: 1.5;
    border-radius: 3px;
    border-color: #E4E7ED;
    background-color: #FFFFFF;
    border: 1px solid rgb(168, 171, 178);
}

div.fmtdiva:focus {
    border: 1px solid #409EFF;

}


.custom-input-style :deep(.el-input__wrapper) {
    box-shadow:  0 0 0 1px #d0d2d7;
    border: 0px dashed #dcdfe6;
}
.custom-input-style :deep(.el-select__wrapper) {
    box-shadow:  0 0 0 1px #d0d2d7;
    border: 0px dashed #dcdfe6; 
}
.custom-input-style :deep(.el-textarea__inner) {
    box-shadow:  0 0 0 1px #d0d2d7;
    border: 0px dashed #dcdfe6;
}


.custom-input-style.disabled :deep(.el-input__wrapper) {
    box-shadow:  0 0 0 0px rgb(164, 164, 164); 
    border: 1px dashed #dcdfe6;
}

.custom-input-style.disabled :deep(.el-select__wrapper) {
    box-shadow:  0 0 0 0px rgb(121, 121, 121);
    border: 1px dashed #dcdfe6;
}
.custom-input-style.disabled :deep(.el-textarea__inner) {
    box-shadow:  0 0 0 0px rgb(164, 164, 164);
    border: 1px dashed #dcdfe6;
}


.custom-input-style :deep(.el-input__wrapper.is-focus) {
    box-shadow:  0 0 0 1px #409eff;
    border: 0px dashed #dcdfe6;
}
.custom-input-style :deep(.el-select__wrapper.is-focus) {
    box-shadow:  0 0 0 1px #409eff;
    border: 0px dashed #dcdfe6; 
}
.custom-input-style :deep(.el-textarea__inner.is-focus) {
    box-shadow:  0 0 0 1px #409eff;
    border: 0px dashed #dcdfe6;
}
</style>
