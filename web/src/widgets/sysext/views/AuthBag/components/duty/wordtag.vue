<template>
<tagselect :listApi="loadList" refresh :rowApi="loadRow" :popWidth="250" pk="HH_WordBagOID" caption="Wb_Name" popTitle="选择词条集合"
    :modelValue="modelValue" @update:modelValue="$emit('update:modelValue',$event)" height="300px"
    :disabled="disabled" :placeholder="placeholder">
    <template #option="p">
        <span>{{p.row.Wb_Name}}</span>
    </template>
</tagselect>
</template>

<script setup>
import tagselect from "@/ui/input/tagselect.vue" 
import { selectHHWordBaglist ,getHHWordBagById} from "../../api/HHWordBagapi.js"

const props = defineProps({
    modelValue:null,
    vc:null,
    disabled:{ type:Boolean, default:false },
    placeholder:{ type:String, default:"请选择" },
})

function loadList(Wb_Name){
    return selectHHWordBaglist({
    	Wb_Name: Wb_Name
    }).then(res => {
        return res.data;
    });
}

function loadRow(oid){
    return getHHWordBagById(oid).then(res => {
        return res.data;
    });
}
</script>
