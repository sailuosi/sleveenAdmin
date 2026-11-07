import {ref} from 'vue'

export const useTable= (api)=>{
  const tableData=ref([]);
  const totals=ref(0)
  const loading=ref(true)

  const refresh= (parma) => {
       api(parma).then(
          res => {
              const {data, total} = res
              totals.value = total
              tableData.value = data
              loading.value = false
          }
      )
  };
  const reset =(params)=>{
      refresh(params)
  }
  return [tableData,totals,loading,refresh,reset]
}
