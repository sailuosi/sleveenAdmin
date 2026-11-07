

// import invoke from '/@/utils/http/ucml-axios';

import { getTokenFull ,retryRequest,clearTokens,setToken} from "@/utils/authapi6.js"

import { loginToCCFlow } from "@s/api/ccflow/logincheck.js"
import { setToken as setFlowToken, setChannel,removeToken as removeCCToken } from "@f/common/utils/auth.js"

export function cleanAllToken(){
    clearTokens();
    removeCCToken();

}


export function getAPI6Token(){
    var tokenObj=getTokenFull();
    if (tokenObj.token) {
        return tokenObj.token;

    }
    return null;
}
function setAPI6Token(token,refreshToken) {
    setToken(token ,refreshToken);
}



// export function geneLoginAPI() {
//   return invoke( '/BPO_LoginAPI6/geneLoginAPI', {}, 'none');
// }


function loginToNet6Api(ticket,service){
    return service({
        url: '/api/sysAuth/uticket',
        method: 'post',
        headers:{
            isToken:false
        },
        useToken:false,
        data:{ticket: ticket}
    }).then((res)=>{
/**
        "accessToken": "eyJhbG",
        "refreshToken": "ey",
        "ccFlowToken": "",
        "channel": "1000006"
*/
        setAPI6Token(res.accessToken,res.refreshToken)
        return res;
    })
}
export function loginToNet6ApiBySSO(ticket,service){
    return service({
        url: '/api/sysAuth/ssoticketlogin',
        method: 'post',
        headers:{
            isToken:false
        },
        useToken:false,
        data:{token: ticket}
    }).then((res)=>{
/**
        "accessToken": "eyJhbG",
        "refreshToken": "ey",
        "ccFlowToken": "",
        "channel": "1000006"
*/
        //debugger;
        var t=res.AccessToken;
        if(!t){
            t=res.accessToken
        }
        setAPI6Token(t,res.refreshToken)
        return res;
    })
}
//使用账户密码进行登录
export function loginToNet6ByPwd(data,service){
    return service({
        url: '/api/sysAuth/login',
        method: 'post',
        headers:{
            isToken:false
        },
        useToken:false,
        data:data
    }).then((res)=>{
/**
        "accessToken": "eyJhbG",
        "refreshToken": "ey",
        "ccFlowToken": "",
        "channel": "1000006"
*/
        setAPI6Token(res.accessToken,res.refreshToken)
        return res;
    })
}



export function autoLoginAPI6(config,service,force){
    console.log("dg",service)
    return new Promise((onok,onno)=>{
        if(!!config && config.useToken==false){
            onok()
            return ;
        }
        if(force !== true){
            var token=getAPI6Token();
            if(!!token && token !="undefined"){
                onok()
                return ;
            }
        }

        //执锟斤拷锟皆讹拷锟斤拷录

        // geneLoginAPI().then((res)=>{
        //     var t=JSON.parse(res);
        //     console.log("UCMLtoken",t.data)
        //     return loginToNet6Api(t.data,service).then((t)=>{
        //         onok()
        //         return ;
        //     })
        // })
        // .catch((err)=>{
        //     onno(err);
        // })
    })
}



export function loginToFlow(){
    return loginToCCFlow().then((res)=>{
        console.log(res);
        setFlowToken(res.data.CCFlowToken)
        setChannel(res.data.channel)
    })
}