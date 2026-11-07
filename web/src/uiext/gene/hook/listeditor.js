
export default function useListEditor() {
    var toUPdate= [];

    var toDelete = [];

    var toAdd = [];

    var rawOld = {};

    var keyField="";
    function init(list, PKName){
        keyField = PKName;

        rawOld=JSON.parse( JSON.stringify(list)) ; 
        
        toUPdate=[];
        toDelete=[];
        toAdd=[];
    }

    function compareRows(oldItems,newItems){
        var oldValueMap={};
        for(var i=0;i<oldItems.length;i++){
            var item = oldItems[i];
            oldValueMap[item[keyField]]=item;
        }
        if(newItems==void 0|| newItems.length==0){
            return []; 
        }
        var res=[];
        var keysFirst= Object.keys(newItems[0]||{});
        
        for(var i=0;i<newItems.length;i++){
            var item = newItems[i];
            var changeCount=0;
            var obj={};
            var pk=item[keyField];
            // 无主键（null / undefined / 空串）：必须整行作为新增提交。
            // 对象键 null 会变为 "null"，若与基线中某行误判相同会走进 diff 分支导致保存丢失。
            if (pk === undefined || pk === null || pk === "") {
                res.push(item);
                continue;
            }
            // 基线中不存在该主键：整行视为新增
            if (oldValueMap[pk] == void 0) {
                res.push(item);
                continue;
            }
            var oldItem = oldValueMap[pk];
            // 合并首行与当前行的字段名，避免仅 newItems[0] 缺少 PX_ExamInfo_FK 等键时，其它行变更无法检出
            var keySet = {};
            var a;
            for (a = 0; a < keysFirst.length; a++) {
                keySet[keysFirst[a]] = true;
            }
            var keysItem = Object.keys(item || {});
            for (a = 0; a < keysItem.length; a++) {
                keySet[keysItem[a]] = true;
            }
            var keys = Object.keys(keySet);
            for(var j=0;j<keys.length;j++){
                var key = keys[j];
                if(key==keyField){
                    continue;
                }
                if(key=="children" && Array.isArray(item[key])){
                    //树结构数据，递归比较
                    var childRes=compareRows(oldItem[key],item[key]);
                    if(childRes.length>0){
                        res=res.concat(childRes);
                        changeCount+=childRes.length;
                    }
                    continue;
                }
                if(item[key]!==oldItem[key]){
                    obj[key]=item[key];
                    changeCount++;
                    continue;
                } 
            } 
            if(changeCount>0){
                obj[keyField]=item[keyField];
                res.push(obj); 
            }
        }
        return res;

    }

    function compare(latlist,addedlist){
        toUPdate=compareRows(rawOld,latlist);
        return toUPdate;
        // if(latlist.length==0){
        //     return []; 
        // }
        // var res=[];
        // if(addedlist!=void 0){
        //     res=addedlist;
        // }
        // var keys= Object.keys(latlist[0]);
        // console.log(keys);
        
        // for(var i=0;i<latlist.length;i++){
        //     var item = latlist[i];
        //     var changeCount=0;
        //     var obj={};
        //     var pk=item[keyField];
        //     //无主键的为新增数据
        //     if(pk==void 0|| srcMap[pk]==void 0){
        //         res.push(item);
        //         continue;
        //     }
        //     var oldItem = srcMap[pk];
        //     for(var j=0;j<keys.length;j++){
        //         var key = keys[j];
        //         if(key==keyField){
        //             continue;
        //         }
        //         if(key=="children" && Array.isArray(item[key])){
        //             //树结构数据，递归比较
        //             var childRes=compare(item[key],oldItem[key]);
        //             if(childRes.length>0){
        //                 obj[key]=childRes;
        //                 changeCount++;
        //             }
        //         }
        //         if(item[key]!==oldItem[key]){
        //             obj[key]=item[key];
        //             changeCount++;
        //             continue;
        //         } 
        //     } 
        //     if(changeCount>0){
        //         obj[keyField]=item[keyField];
        //         res.push(obj); 
        //     }
        // }
        // return res;
    }

    function updateRow(row){
        if(!row || !row[keyField]){
            return;
        }
        if(!Array.isArray(rawOld)){
            return;
        }
        var pk = row[keyField];
        for(var i=0;i<rawOld.length;i++){
            if(rawOld[i][keyField]===pk){
                // 更新原始数据，只更新传入的字段
                for(var key in row){
                    if(key!==keyField){
                        rawOld[i][key]=row[key];
                    }
                }
                break;
            }
        }
    }

    return {
        init, compare, updateRow, toUPdate, toDelete, toAdd 
    }
}

