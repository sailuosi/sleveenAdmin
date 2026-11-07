const ucmlvclist= import.meta.glob(["@/widgets/**/VC/**/VC_*.vue"]);

const ucmlbpolist= import.meta.glob(["@/widgets/**/BPO_*.vue"]);

function buildMap(src){
    console.log(src);
    const keys = Object.keys(src);
    var vcbody={};
    var vctool={};
    keys.forEach((li)=>{
        var idx=li.lastIndexOf("/");
        var path= li.substring(0,idx)+"/toolbar.vue";
        var name=li.substring(idx+1);
        if(name.endsWith(".vue")){
            name=name.substring(0,name.length-4);
        }
        vcbody[name]= src[li];
        // if(!!ucmlvctoolist[path]){
        //     vctool[name] = ucmlvctoolist[path];
        // }
        // else{
        //     vctool[name] = path;
        // }
        
    })
    return {
        body:vcbody,
        tool:vctool
    };
}

const ucmlVCMap=buildMap(ucmlvclist);

const ucmlVCBody=ucmlVCMap.body;
const ucmlVCTool=ucmlVCMap.tool;



export function loadUcmlVC(component) {
    
    if(!!ucmlVCBody[component]){
        return ucmlVCBody[component]
    }

    return component;
}

export function loadUcmlVCTool(component) {
    if(!!ucmlVCTool[component]){
        return ucmlVCTool[component]
    }

    return component;
}
 


function buildBPOMap(src){
    //console.log(ucmlvctoolist);
    const keys = Object.keys(src);
    var vcbody={};

    keys.forEach((li)=>{
        var idx=li.lastIndexOf("/");

        var name=li.substring(idx+1);
        if(name.endsWith(".vue")){
            name=name.substring(0,name.length-4);
        }
        vcbody[name]= src[li];

        
    })

    return vcbody;

}

const ucmlBPOMap=buildBPOMap(ucmlbpolist);


export function loadUcmlBPO(component) {
    if(!!ucmlBPOMap[component]){
        return ucmlBPOMap[component]
    }

    return component;
}