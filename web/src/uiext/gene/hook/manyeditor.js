import useListEditor from "./listeditor.js"
import useFormEditor from "./useFormEditor.js"


export  function useManyEditor() {

    var formMap= {};
    var listMap = {};

    function initRow( key, row, PKName){
        var holder= useFormEditor();
        holder.init(row, PKName);
        formMap[key]=holder;
    }

    function initList(key, list, PKName){
        var holder= useListEditor();
        holder.init(list, PKName);
        listMap[key]=holder; 
    }

    function compareRow(key,row){
        var holder= formMap[key];
        //不存在，说明全为新的数据，直接返回
        if(holder==void 0){
            return row;
        }
        return holder.compare(row);
    }

    function compareList(key,list){
        var holder= listMap[key];
        //不存在，说明全为新的数据，直接返回
        if(holder==void 0){
            return list;
        }
        return holder.compare(list); 
    }

    return {
        initRow, initList, compareRow, compareList 
    }
}