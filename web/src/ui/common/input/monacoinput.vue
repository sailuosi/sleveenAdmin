<template>

		<div ref="monacoEditorRef" @focusin="onFocusin" @focusout="onFocusOut"></div>

</template>

<script  setup name="sysEditPlugin">
import { reactive, ref,onMounted,watch } from 'vue';
import { ElMessage } from 'element-plus';
import * as monaco from 'monaco-editor';



const props = defineProps({
	title: String,
	language:{ type: String, default: 'csharp'  },
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const monacoEditorRef = ref();
const state = reactive({

	ruleForm: {} ,
});

window.MonacoEnvironment = {
  getWorkerUrl: (moduleId, label) => {
    return label === 'json' ? setWorkerUrl('json') : setWorkerUrl('editor') // label 可以为 javascript、css、html等，这里仅举例
  },
}

function setWorkerUrl(key) {
  const { NODE_ENV, BASE_URL } = process.env
  return NODE_ENV === 'development'
    ? `./${key}.worker.js` // 本地环境读取本地服务 worker 文件
    : `data:text/javascript;charset=utf-8,${encodeURIComponent(`
      importScripts('https:${BASE_URL}${key}.worker.js');`)}` // 发布环境通过 importScripts() 函数将外部脚本文件或库加载到 Worker 中
}

const focusing= ref(false);
function onFocusin(){
	console.log('onFocusin');
	focusing.value=true;
}

function onFocusOut(){
	console.log('onFocusOut');
	focusing.value=false;
}

const model = defineModel()
// 初始化monacoEditor对象
var monacoEditor = null;
const initMonacoEditor = () => {
	monacoEditor = monaco.editor.create(monacoEditorRef.value, {
		theme: 'vs-dark', // 主题 vs vs-dark hc-black
		value: model.value, // 默认显示的值
		language: props.language, // 语言,
		tabSize: 4, // 缩进
		formatOnPaste: true,
		wordWrap: 'on', //自动换行，注意大小写
		wrappingIndent: 'indent',
		folding: true, // 是否折叠
		foldingHighlight: true, // 折叠等高线
		foldingStrategy: 'indentation', // 折叠方式  auto | indentation
		showFoldingControls: 'always', // 是否一直显示折叠 always | mouSEOver
		disableLayerHinting: true, // 等宽优化
		emptySelectionClipboard: false, // 空选择剪切板
		selectionClipboard: false, // 选择剪切板
		automaticLayout: true, // 自动布局
		codeLens: false, // 代码镜头
		scrollBeyondLastLine: false, // 滚动完最后一行后再滚动一屏幕
		colorDecorators: true, // 颜色装饰器
		accessibilitySupport: 'auto', // 辅助功能支持  "auto" | "off" | "on"
		lineNumbers: 'on', // 行号 取值： "on" | "off" | "relative" | "interval" | function
		lineNumbersMinChars: 5, // 行号最小字符   number
		//enableSplitViewResizing: false,
		readOnly: false, //是否只读  取值 true | false
	});
	monacoEditor.onDidChangeModelContent((e) => {
		//console.log('当前内容:', editor.getValue());
		var val=monacoEditor.getValue();
		console.log('当前内容:',val);
		model.value=val;
		// 可在此触发Vue组件的data更新或emit事件
	});
};

watch(model, (val) => {
	if (monacoEditor == null) initMonacoEditor();
	else{
		//如果用户正在输入，不更新
		if(focusing.value==false){
			if(val==void 0||val==null){
				val="";
			}
			monacoEditor.setValue(val);
			return;
		}
	}
})

onMounted(() => {
    	// 延迟拿值防止取不到
	setTimeout(() => {
		if (monacoEditor == null) initMonacoEditor();
	}, 1);
})

function getValue(){
	return monacoEditor.getValue();
}
// 导出对象
defineExpose({ 
	getValue
 });
</script>
