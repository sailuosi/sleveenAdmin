import emitter from '@/utils/mitt'
import {ref} from 'vue'
import {useUserInfo} from '@/stores/userInfo'
const useSate=useUserInfo()
const {userInfos:{account}}=useSate
export const useRemenberColumn= ()=>{

   return {
      headerDragend,
      setColumnWidth
   }
}
//拖动列回调
const headerDragend=(newWidth,oldWidth,column,... args)=> {
   const tableId=args[0]
   if(tableId){
      const key=`${account}_${tableId}`
      const item = localStorage.getItem(key);

      if(item){
         let obj =JSON.parse(item)
         obj[column.property]=newWidth
         localStorage.setItem(key,JSON.stringify(obj))
      }
      else{
         let obj={}
         obj[column.property]=newWidth
         localStorage.setItem(key,JSON.stringify(obj))
      }
   }
}
// 设置列宽
const setColumnWidth= (column, ... args) =>{
   const tableId=args[0]
   if (tableId){
      const key=`${account}_${tableId}`
      const item = localStorage.getItem(key);
      if(item){
         let obj =JSON.parse(item)
         Object.keys(obj).forEach(key=>{
            if (column.property==key){
               column.width=obj[key]
            }
         })
      }
   }

}
