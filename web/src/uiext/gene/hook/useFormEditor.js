
export default function useFormEditor() {
    var toUPdate= [];

    var toDelete = [];

    var toAdd = [];

    var oldRow = {};

    var keyField="";
    function init(item, PKName){
        keyField = PKName;
        oldRow = {};
        // 新增/无主键时也必须重置 oldRow，否则 compare 仍与上一条表单的快照比较，导致「未变更」字段不进保存包
        if(item==void 0 || item==null){
            return;
        }
        if(item[PKName]==void 0 || item[PKName]==null){
            return;
        }
        oldRow=JSON.parse( JSON.stringify(item)) ; 
    }


    function compare(latRow){
        var keys= Object.keys(latRow);
        //console.log(keys);

        var changeCount=0;
        var obj={};
  
        for(var j=0;j<keys.length;j++){
            var key = keys[j];
            if(key==keyField){
                continue;
            }
            if(latRow[key]!==oldRow[key]){
                obj[key]=latRow[key];
                changeCount++;
                continue;
            } 
        } 
        if(changeCount>0){
            obj[keyField]=latRow[keyField];
            return obj;
        }
        
        return null;
    }

    return {
        init, compare
    }
}