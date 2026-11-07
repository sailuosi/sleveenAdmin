import { Local, Session } from '@/utils/storage';


function autopology(err){
    let back= new Promise(function(resolve){
        console.log("重试请求");
        resolve();
    })
    
    return back.then(function(){
        setTimeout(() => {
            return service(err.config)
        }, 300);
    })        
    

}


/**
 * 解密 JWT token 的信息
 * @param token jwt token 字符串
 * @returns <any>object
 */
export function decryptJWT(token) {
	token = token.replace(/_/g, '/').replace(/-/g, '+');
	var json = decodeURIComponent(escape(window.atob(token.split('.')[1])));
	return JSON.parse(json);
}

/**
 * 将 JWT 时间戳转换成 Date
 * @description 主要针对 `exp`，`iat`，`nbf`
 * @param timestamp 时间戳
 * @returns Date 对象
 */
export function getJWTDate(timestamp) {
	return new Date(timestamp * 1000);
}


const MAX_RETRY_COUNT = 3;
const DELAY_TIME = 2000;

let currentCount = 1;

export async function retryRequest(err) {
    if (err.config.retry === false) throw err;
    if (err.message === "canceled") throw err; // 过滤手动取消请求的情况

    const config = err.config;

    if (MAX_RETRY_COUNT >= currentCount && config) {
        console.log(`重试${currentCount}次...`);
        currentCount++;
        await sleep(DELAY_TIME);
        const res = await service(config);
        return Promise.resolve(res);
    }
    console.log("重试次数已用完...");
    currentCount = 0;
    throw err;
}

 function sleep(delayTime) {
    return new Promise(resolve => setTimeout(resolve, delayTime));
}

// token 键定义

const accessTokenKey =  import.meta.env.VITE_APP_TokenKey;
const refreshAccessTokenKey = `x-${accessTokenKey}`;

// 获取 token
export const getToken = () => {
	return Local.get(accessTokenKey);
};

export function setToken(token,freshToken){
    Local.set(accessTokenKey,token);
    Local.set(refreshAccessTokenKey,freshToken);
}



// 清除 token
export const clearTokens = () => {
	Local.remove(accessTokenKey);
	Local.remove(refreshAccessTokenKey);
	Session.clear();
};


// 清除 token
export const clearAccessTokens = () => {
	Local.remove(accessTokenKey);
	Local.remove(refreshAccessTokenKey);

	// 清除其他
	Session.clear();

	// 刷新浏览器
	window.location.reload();
};

export function getTokenFull(){
    //console.log('ddd')
    var obj={};
    const accessToken = Local.get(accessTokenKey);
    if (accessToken) {
        // 将 token 添加到请求报文头中
        obj.token = `Bearer ${accessToken}`;

        // 判断 accessToken 是否过期
        const jwt = decryptJWT(accessToken);
        const exp = getJWTDate(jwt.exp );

        // token 已经过期
        if (new Date() >= exp) {
            // 获取刷新 token
            const refreshAccessToken = Local.get(refreshAccessTokenKey);

            // 携带刷新 token
            if (refreshAccessToken) {
                obj.refresh = `Bearer ${refreshAccessToken}`;
            }
        }

    }
    return obj;
}