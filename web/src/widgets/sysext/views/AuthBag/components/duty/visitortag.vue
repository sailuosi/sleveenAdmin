<template>
<tagselect :listApi="loadList" :popWidth="250" :rowApi="loadRow" refresh pk="HH_VisitorBagOID" caption="Vb_Name" 
	popTitle="选择访客集合" :modelValue="modelValue" @update:modelValue="$emit('update:modelValue',$event)" height="300px"
	:disabled="disabled" :placeholder="placeholder">
    <template #option="p">
        <span>{{p.row.Vb_Name}}</span>
    </template>
</tagselect>
</template>

<script setup>
import tagselect from "@/ui/input/tagselect.vue" 
import { selectHHVisitorBaglist ,getHHVisitorBagById} from "../../api/HHVisitorBagapi.js"

const props = defineProps({
    modelValue:null,
    vc:null,
    disabled:{ type:Boolean, default:false },
    placeholder:{ type:String, default:"请选择" },
})

function loadList(Vb_Name){
    return selectHHVisitorBaglist({
    	Vb_Name: Vb_Name
    }).then(res => {
        return res.data;
    });
}

function loadRow(oid){
    return getHHVisitorBagById(oid).then(res => {
        return res.data;
    });
}
</script>
