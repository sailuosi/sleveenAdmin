import { Local } from './authlocal.ts';

import {getCodeValue} from "@s/api/system/dictapi.js"

const loadingCodes=[];
const codes={};

export function loadCode(code){
    // 存储字典信息到浏览器缓存
    return new Promise((onok,onerr)=>{
        var keys=Object.keys(codes);

        var toloadCodes=[];

        if(keys.indexOf(code)!=-1 ){
            //已存在 ，返回
            var tar=codes[code];
            onok(tar);
            return ;
        }

        //触发加载

        var cacheKey="dictList_"+code;
        var t=Local.get(cacheKey);
        if (Array.isArray(t)) {
            // 与 mdClassDict 共用 key：无 meta 或已过期则丢弃，避免改代码表后长期脏缓存
            var metaKey="dictListMeta_"+code;
            var meta=Local.get(metaKey);
            var ts=Number(meta && meta.ts);
            var ttl=30*1000;
            if(!Number.isFinite(ts) || Date.now()-ts>ttl){
                Local.remove(cacheKey);
                Local.remove(metaKey);
            }else{
                onok(t);
                return ;
            }
        }

        getCodeValue(code).then((res)=>{
            if(res.status=="success"){
                Local.set(cacheKey, res.data);
                Local.set("dictListMeta_"+code, { ts: Date.now() });
                onok(res.data);
                return ;
            }
            onerr(res);
            return ;
        }) 
    })
}