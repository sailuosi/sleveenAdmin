

//按帧率渲染
export function useDefer(maxFrameCount=100) {
        let frameCount = 0;
        //
        const refreshCount=()=>{
          requestAnimationFrame(()=>{
            frameCount++;
            //console.log("当前帧率:",frameCount)
            if(frameCount<maxFrameCount){
              refreshCount();
            }
          })
        }
         refreshCount();
       //多少帧率渲染
       const defer=(n)=>{return frameCount>=n;}

       return defer;
}
