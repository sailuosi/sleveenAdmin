

import useListEditor from "./listeditor.js"

import { ref, computed, watch, onMounted,useAttrs } from 'vue';



export default function useBCList(){
    const loading = ref(false);
    const mlist = ref([]);
    const total = ref(0);
    const queryParams = ref({
        pageSize: props.pageSize,
        pageNum: 1,
    });

    const checked = ref([]);
    const focusRow = ref({});


    function loadData(para){

        loading.value = true;
        props.api.queryBCManContactlist(para).then((res) => {
            mlist.value = res.Items;
            total.value = res.Total;
            listHolder.init(res.Items, "UCML_CONTACTOID");
            if(props.bus){
                props.bus.setList(VCName,mlist);            
            }
            loading.value = false;
        })
        .catch((err) => {
            swal({ text: err.message, icon: "error", button: "关闭", });
            loading.value = false;
        });
    }


    return { loading, mlist, total, queryParams, checked, focusRow, loadData };
}