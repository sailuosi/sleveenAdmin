
function getPublicPath(){
    if(window.CCliveResourcePath != void 0){
        return window.CCliveResourcePath;
    }
    var path=import.meta.env.VITE_APP_PUBLIC_PATH;
    if(!!path && path != void 0){
        return path;
    }
    if(window.UCMLLocalResourcePath != void 0){
        return window.UCMLLocalResourcePath;
    }
    return "";
}

export function loadJs(src,isok) {
    return new Promise((resolve,reject)=>{
        if(isok===true){
            resolve();
            return ;
        }
        if(/http/.test(src)==false){
            if(/^[/]/.test(src)){
                src=src.substring(1);
            }
            src= getPublicPath()+src;
        }
        let script = document.createElement('script');
        script.type = "text/javascript";
        script.src=  src;
        document.body.appendChild(script);

        script.onload = ()=>{
            resolve();
        }
        script.onerror = ()=>{
            reject();
        }
    })
}


export function loadCss(src,isok) {
    return new Promise((resolve,reject)=>{
        if(isok===true){
            resolve();
            return ;
        }
        let link = document.createElement('link');
        if(/http/.test(src)==false){
            src= getPublicPath()+src;
        }
        link.href = src;
        link.rel = 'stylesheet';
        link.type = 'text/css';
        document.body.appendChild(link);

        link.onload = ()=>{
            resolve();
        }
        link.onerror = ()=>{
            reject();
        }
    })
}

export function openLink(url,opennew){
    let link = document.createElement('a');
    link.href = url;
    if(opennew){
        link.target = "_blank";
    }
    // link.onclick=function(){
    //     if(opennew){
    //         window.open(url, '_blank');
    //     }
    //     else{
    //         window.open(url);
    //     }
    // }
    document.body.appendChild(link);
    var evt = document.createEvent("MouseEvents"); 
       evt.initEvent("click",true,true); 
       link.dispatchEvent(evt); 
    document.body.removeChild(link);
    //link.click();
}
/* css: function(path){
 if(!path || path.length === 0){
    throw new Error('argument "path" is required !');
    }
    var head = document.getElementsByTagName('head')[0];
    var link = document.createElement('link');
    link.href = path;
    link.rel = 'stylesheet';
    link.type = 'text/css';
    head.appendChild(link);
    },
    */

    export function parUrlPara(txt){
        var res={};
        try{
            if(txt !=null && txt !=void 0 && typeof txt=="string"){
                var paraArr =txt.split('&');
                paraArr.forEach((item)=>{
                    var itemArr = item.split('=');
                    res[itemArr[0]] = itemArr[1];
                });
                
            }
        }
        catch(e){}

        return res;
    }