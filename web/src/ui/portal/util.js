import { loadBiz, loadVC } from "@/uiext/gene/live/dynamicImport.js"

const portalVcList = import.meta.glob(["@/widgets/**/vc*.vue"]);



export function dynamicImportVC(dynamicViewsModules, component) {
    if(component == null || component === ''){
        return component;
    }
    // 配置若带 .vue 后缀则去掉，再与 glob 路径做 startsWith 匹配
    var path = String(component).replace(/\.vue$/i, '');
    if(path.startsWith("VC_")){
        return loadVC(path);
    }
    if(path.startsWith("Biz_") || path.startsWith("BPO_")){
        return loadBiz(path);
    }
	const keys = Object.keys(dynamicViewsModules);
	const matchKeys = keys.filter((key) => {
		var k = key.replace(/..\/views|../, '');
        if(key.substring(0,13)=='/src/widgets/'){
            k = key.replace("/src/widgets/", '')
        }
		return k.startsWith(`${path}`) || k.startsWith(`/${path}`);
	});
	if (matchKeys?.length === 1) {
		const matchKey = matchKeys[0];
		return dynamicViewsModules[matchKey];
	}
	if (matchKeys?.length > 1) {
		return false;
	}
}



export function ImportPaotalVC(path){
    return dynamicImportVC(portalVcList, path);
}


export function parUrlPara(txt){
    var res={};
    if(txt !=null && txt !=void 0 && typeof txt=="string"){
        var paraArr =txt.split('&');
        paraArr.forEach((item)=>{
            var itemArr = item.split('=');
            res[itemArr[0]] = itemArr[1];
        });
        
    }
    return res;
}