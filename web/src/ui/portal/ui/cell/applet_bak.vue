<template>
	<div class="applet-parent">

        <transition :name="setTransitionName" mode="out-in">
            <keep-alive :include="getKeepAliveNames">
                <component :is="rendTarget" :key="state.refreshRouterViewKey" class="w100" v-show="!isIframePage" 
                    :cell="cell" 
                    :vc="vc"
                    :root="root"
                    :paotalConfig="paotalConfig"
                    :para="rendPara"
                    :shareState="shareState"
                    @setState="$emit('setState',$event)"
                />
            </keep-alive>
        </transition>

		<transition :name="setTransitionName" mode="out-in">
			<Iframes class="w100" v-show="isIframePage" :refreshKey="state.iframeRefreshKey" :name="setTransitionName" :list="state.iframeList" />
		</transition>
	</div>
</template>

<script setup  name="applet">
import { defineAsyncComponent, computed, reactive, onBeforeMount, onUnmounted, nextTick, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useKeepALiveNames } from '/@/stores/keepAliveNames';
import { useThemeConfig } from '/@/stores/themeConfig';
import { Session } from '/@/utils/storage';
import mittBus from '@f/common/utils/mitt.js';

const props = defineProps({
	// 刷新 iframe
	name: {        default:null	},
    framed:{ type:Boolean,default:false },
    animation: { type: String, default: 'fadeLeft' },
    cell:null,
    vc:null,
    root:null,
    paotalConfig:null,
    para:null,
    shareState:null,
    width:null,
    height:null,
});
// 引入组件
const Iframes = defineAsyncComponent(() => import('/@/layout/routerView/iframes.vue'));

// 定义变量内容
const route = useRoute();
const router = useRouter();
const storesKeepAliveNames = useKeepALiveNames();
const storesThemeConfig = useThemeConfig();
const { keepAliveNames, cachedViews } = storeToRefs(storesKeepAliveNames);
const { themeConfig } = storeToRefs(storesThemeConfig);
const state = reactive({
	refreshRouterViewKey: '', // 非 iframe tagsview 右键菜单刷新时
	iframeRefreshKey: '', // iframe tagsview 右键菜单刷新时
	keepAliveNameList: [],
	iframeList: [],
});

// 设置主界面切换动画
const setTransitionName = computed(() => {
	return props.animation;
});
 
const rendTarget= computed(() => {
	return defineAsyncComponent( props.name);
});
const rendPara= computed(() => {
	var para = props.para;
    var res={};
    if(para !=null && para !=void 0 && typeof para=="object"){
        res=Object.assign(res,para);
    }
    //解析参数
    if(!!props.vc.para){
        var urlPara=parUrlPara(props.vc.para);
        res=Object.assign(res,urlPara);
    }
    return res;
});

function parUrlPara(txt){
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

// 获取组件缓存列表(name值)
const getKeepAliveNames = computed(() => {
	return themeConfig.value.isTagsview ? cachedViews.value : state.keepAliveNameList;
});
// 设置 iframe 显示/隐藏
const isIframePage = computed(() => {
	return props.framed;
});


// 页面加载前，处理缓存，页面刷新时路由缓存处理
onBeforeMount(() => {
	state.keepAliveNameList = keepAliveNames.value;
	mittBus.on('onAppletRefresh', (fullPath) => {
		state.keepAliveNameList = keepAliveNames.value.filter((name) => route.name !== name);
		state.refreshRouterViewKey = '';
		state.iframeRefreshKey = '';
		nextTick(() => {
			state.refreshRouterViewKey = fullPath;
			state.iframeRefreshKey = fullPath;
			state.keepAliveNameList = keepAliveNames.value;
		});
	});
});
// 页面加载时
onMounted(() => {

	// https://gitee.com/lyt-top/vue-next-admin/issues/I58U75
	// https://gitee.com/lyt-top/vue-next-admin/issues/I59RXK
	// https://gitee.com/lyt-top/vue-next-admin/pulls/40
	nextTick(() => {
		setTimeout(() => {
			if (themeConfig.value.isCacheTagsView) {
				let tagsViewArr = Session.get('tagsViewList') || [];
				cachedViews.value = tagsViewArr.filter((item) => item.meta?.isKeepAlive).map((item) => item.name );
			}
		}, 0);
	});
});
// 页面卸载时
onUnmounted(() => {
	mittBus.off('onAppletRefresh', () => {});
});
// 监听路由变化，防止 tagsView 多标签时，切换动画消失
// https://toscode.gitee.com/lyt-top/vue-next-admin/pulls/38/files
// watch(
// 	() => route.fullPath,
// 	() => {
// 		state.refreshRouterViewKey = decodeURI(route.fullPath);
// 	},
// 	{
// 		immediate: true,
// 	}
// );
</script>
