

import { ref} from 'vue';

export const listener= ref({});

var opt=null;

export function initListen(els,config, onchange){
    opt=config;
    //console.log("init窗口监听",els);
    //
    if(opt.resize){

        var resize = new ResizeObserver(onchange);
        if(Array.isArray(els)){
            els.forEach((e)=>{
                resize.observe(e,{attributes:false,attributeFilter:['style'],attributeOldValue:true});
            })
        }
        else{
            resize.observe(els,{attributes:true,attributeFilter:['style'],attributeOldValue:true});        
        }      
        listener.value.resize =resize;  
    }

    if(opt.intersection){
        var opts={
            rootMargin:"5px",
            threshold:[0.1,0.1],
        };
        if(opt.margin !=void 0){
            opts.rootMargin=opt.margin;
        }
        var iobserver = new IntersectionObserver(onchange, opts);
        if(Array.isArray(els)){
            els.forEach((e)=>{
                iobserver.observe(e,{attributes:true,attributeFilter:['style'],attributeOldValue:true});
            })
        }
        else{
            iobserver.observe(els,{attributes:true,attributeFilter:['style'],attributeOldValue:true});        
        }    
        listener.value.intersection =iobserver;  
    }

 

    if(opt.mutation){
        var config = { attributes: true, childList: true, subtree: true };
        if(opt.mutationOpt !=void 0){
            config= Object.assign({},config,opt.mutationOpt)
        }
        let MutationObserver = window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver;
        const mobserver = new MutationObserver(onchange);

        if(Array.isArray(els)){
            els.forEach((e)=>{
                mobserver.observe(e,config);
            })
        }
        else{
            mobserver.observe(els,config);        
        }    
        listener.value.mutation =mobserver;  
    }

}

export function stopListen(){
    var resizer=listener.value.resize;
    if (!! resizer) {
        //console.log("destroy窗口监听");
        resizer.disconnect()
        if(typeof resizer.takeRecords=="function"){
            resizer.takeRecords()
        }
        resizer = null
    }
    var intersection=listener.value.intersection;
    if (!! intersection) {
        //console.log("destroy窗口监听");
        intersection.disconnect()
        if(typeof intersection.takeRecords=="function"){
            intersection.takeRecords()
        }
        intersection = null
    }
    var mutation=listener.value.mutation;
    if (!! mutation) {
        //console.log("destroy窗口监听");
        mutation.disconnect()
        if(typeof mutation.takeRecords=="function"){
            mutation.takeRecords()
        }
        mutation = null
    }

}