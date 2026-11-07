

export function styleStrToObj(value){
    //将标准的样式字符串，转换为 vue的对象语法。

    if(value==null || value== void 0 || value==""){
        return {};
    }
    value=value.replace(/\n/g,"");
    var arr=value.split(";");

    var res={};
    arr.forEach((ar)=>{
        if(ar.indexOf(":") != -1) {
            var ops=ar.split(":");
            if(ops.length>1){
                res[ops[0]]=ops[1];
            }
        }
    })
    return res;
}

export function objToStyleStr(value){
    //将标准的样式字符串，转换为 vue的对象语法。

    if(value==null || value== void 0 || value==""){
        return "";
    }

    var res= [];

    var keys=Object.keys(value);

    keys.forEach((key)=>{
        var val=value[key];
        if(val === "" || val === void 0){
            return ;
        }
        var str=key+":"+val;
        res.push(str);
    })
    var tar= res.join(";");
    if(res.length>0){
        tar = tar +";";
    }
    return tar;
}