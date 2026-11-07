<template>
<tagselect :listApi="loadList" :rowApi="loadRow" refresh pk="HH_GoodsBagOID" caption="Gb_Name" popTitle="选择资源集合"
    :modelValue="modelValue" @update:modelValue="$emit('update:modelValue',$event)" height="300px"
    :disabled="disabled" :placeholder="placeholder">
    <template #option="p">
        <span>{{p.row.Gb_Name}}</span>
    </template>
</tagselect>
</template>

<script setup>
import tagselect from "@/ui/input/tagselect.vue" 
import { selectHHGoodsBaglist ,getHHGoodsBagById} from "../../api/goodsbagapi.js"

const props = defineProps({
    modelValue:null,
    vc:null,
    disabled:{ type:Boolean, default:false },
    placeholder:{ type:String, default:"请选择" },
})

function loadList(Gb_Name){
    return selectHHGoodsBaglist({
    	Gb_Name: Gb_Name
    }).then(res => {
        return res.data;
    });
}

function loadRow(oid){
    return getHHGoodsBagById(oid).then(res => {
        return res.data;
    });
}
</script>
