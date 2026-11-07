<template>
    <div :class='mCss'>
        <template v-if="!isReadonly">
            <div ref="toolbar"></div>
            <div ref="editor" class="wang-inputpan" :style="panstyle"></div>
        </template>
        <div v-else v-html="rendervalue" :class="readonlyClass" :style="panstyle"></div>
    </div>
</template>
<script>

/* 创建 20190814  封装wangEditor组件
    tip:封装这个是因为常用的百度ueditor太大，而且时常有一些BUG例如多次使用的异常显示灯，而百度的轻量版是蛮好的，基于div的editable=true属性，但是却不能和UE同时使用，否则会有工具栏的显示异常。
    wangEditor的核心包仅65k，又无样式文件，是轻量级情况下非重度使用的首选。
* 更新20191030 优化缺省的textarea样式，增加默认高度随动
    关于高度随动的问题，设置height:auto/fit-content，即可，然后通过min-height来控制要求的高度
    上传图片目前使用的是 base64.
    [
    'head',  // 标题
    'bold',  // 粗体
    'fontSize',  // 字号
    'fontName',  // 字体
    'italic',  // 斜体
    'underline',  // 下划线
    'strikeThrough',  // 删除线
    'foreColor',  // 文字颜色
    'backColor',  // 背景颜色
    'link',  // 插入链接
    'list',  // 列表
    'justify',  // 对齐方式
    'quote',  // 引用
    'emoticon',  // 表情
    'image',  // 插入图片
    'table',  // 表格
    'video',  // 插入视频
    'code',  // 插入代码
    'undo',  // 撤销
    'redo'  // 重复
]  {{rendervalue}}

            <textarea :value="value" 
                @input="textareaClick"
                :style="ttStyle"
                class ="ttaree"
            ></textarea>


    const E = window.wangEditor

    // 切换语言
    const LANG = location.href.indexOf('lang=en') > 0 ? 'en' : 'zh-CN'
    E.i18nChangeLanguage(LANG)

    window.editor = E.createEditor({
      selector: '#editor-text-area',
      html: '<p><br></p>',
      config: {
        placeholder: 'Type here...',
        MENU_CONF: {
          uploadImage: {
            fieldName: 'your-fileName',
            base64LimitSize: 10 * 1024 * 1024 // 10M 以下插入 base64
          }
        },
        onChange(editor) {
          console.log(editor.getHtml())

          // 选中文字
          const selectionText = editor.getSelectionText()
          document.getElementById('selected-length').innerHTML = selectionText.length
          // 全部文字
          const text = editor.getText().replace(/\n|\r/mg, '')
          document.getElementById('total-length').innerHTML = text.length
        }
      }
    })

    window.toolbar = E.createToolbar({
      editor,
      selector: '#editor-toolbar',
      config: {}
    })
  
*/

import {loadJs,loadCss} from "../../../utils/loadJs.js"

export default {
    name :'miWangE',
    data: function(){
        return{
            content:"",
            contentTxt:"",
            selected:"",
            emiting:false,
            focused:false,
            wangTar: null, // 编辑器实例对象
        }
    },
    props: {
        initvalue:String,
        modelValue: String,
        disabled:{type:Boolean,default:false},
        noedit:{type:Boolean,default:false},
        menus: {Type:Array,default(){
            return ['head','bold','fontSize','fontName','italic','justify','underline','foreColor','backColor','undo']; // 'image' 已移除
        }},
        toolbarKeys: { type: Array, default: null },
        height:{type:String,default:'200px'},//编辑器的高度
        autoGrow:{type:Boolean,default:true},//编辑器的高度自动随涨
        tbHide:{type:Boolean,default:false},
        //watchOut:{type:Boolean,default:false},
        onfocus:null,
        onblur:null,
        placeholder:{type:String,default:'请输入内容...'},//占位符文本
        //使用的版本号 ，默认为v3，v5虽好，但包大了十几倍。
        version:{type:String,default:"v5"},
        //是否使用全功能版本，基础120k,full 800k
        full:{type:Boolean,default:true},
        /** 传入则启用工具栏「上传图片」；签名为 (file) => Promise<string>，返回可插入编辑器的图片地址 */
        onImageUpload:{type:Function,default:null},
    },
    computed:{
        mCss(){
            return {
                "autohide":this.tbHide,
                "miwangee":true,
            }
        },
        panstyle(){
            return {
                height:this.height,
            };
        },
        isReadonly(){
            return this.disabled || this.noedit;
        },
        readonlyClass(){
            return this.noedit ? 'wang-noedit-content' : 'wang-disabled-content';
        },
        rendervalue(){
            if (this.modelValue == void 0 || this.modelValue === null || this.modelValue === '') {
                return "<p style='color: #909399; padding: 20px; text-align: center;'>暂无内容</p>";
            }
            
            // 如果是 JSON 格式的编辑器内部数据，返回空字符串
            if (typeof this.modelValue === 'string' && 
                (this.modelValue.trim().startsWith('{') || this.modelValue.trim().startsWith('['))) {
                try {
                    const jsonData = JSON.parse(this.modelValue);
                    if (jsonData.children) {
                        return "<p style='color: #909399; padding: 20px; text-align: center;'>暂无内容</p>"; // 编辑器内部格式，不显示
                    }
                } catch (e) {
                    // 不是有效的 JSON，继续处理
                }
            }
            
            return this.modelValue;
        },
        noHeart(){
            return window.wangEditor==void 0;
        }
    },
    watch: {
        modelValue: { handler: function(val,oldval){
                if(this.focused){
                    return ;
                }
                if(!this.noHeart &&val !== null  && !this.emiting){
                    //由于重置编辑器内容时，会改变用户输入光标，所以要检测是外界写入value时才更新。自动同步时，value和编辑器的值是相同的。
                    // var myvalue=this.editor.txt.html();
                    var myvalue=this.content;
                    if(val!=myvalue){
                        // 检查 wangTar 和 editor 是否存在
                        if(this.wangTar && this.wangTar.editor){
                            try {
                                // 清理和验证 HTML 后再设置
                                let cleanHtml = val || '';
                                if (typeof cleanHtml === 'string' && cleanHtml.trim() !== '') {
                                    // 如果是 JSON 格式，清空
                                    if (cleanHtml.trim().startsWith('{') || cleanHtml.trim().startsWith('[')) {
                                        try {
                                            const jsonData = JSON.parse(cleanHtml);
                                            if (jsonData.children) {
                                                cleanHtml = '<p><br></p>';
                                            }
                                        } catch (e) {
                                            cleanHtml = '<p><br></p>';
                                        }
                                    } else {
                                        cleanHtml = this.sanitizeHtml(cleanHtml);
                                    }
                                } else {
                                    cleanHtml = '<p><br></p>';
                                }
                                this.wangTar.editor.setHtml(cleanHtml);
                            } catch(e) {
                                console.warn("wanghtml: error setting html", e);
                            }
                        }
                    }
                }
            },
            //deep: true
        },
        disabled(){
            this.handleReadonlyChange();
        },
        noedit(){
            this.handleReadonlyChange();
        },
    },
    mounted(){
        this.init();
    },
    beforeUnmount: function(){
        this.destroyEditor();
    },
    destroyed: function(){
        // Vue 2 兼容性，Vue 3 使用 beforeUnmount
        this.destroyEditor();
    },
    methods:{
        sanitizeHtml(html){
            if (!html || typeof html !== 'string') {
                return '<p><br></p>';
            }
            
            // 移除危险内容
            let cleanHtml = html
                .replace(/<script[^>]*>.*?<\/script>/gi, '') // 移除脚本标签
                .replace(/<style[^>]*>.*?<\/style>/gi, '') // 移除样式标签
                .replace(/on\w+="[^"]*"/gi, '') // 移除事件处理器
                .replace(/javascript:/gi, '') // 移除 javascript: 协议
                .trim();
            
            // 确保 HTML 有基本结构
            if (!cleanHtml || cleanHtml === '') {
                return '<p><br></p>';
            }
            
            // 检查并修复不完整的表格结构（可能导致路径错误）
            // 如果包含 table 但没有完整的 tr/td 结构，移除 table
            if (/<table/i.test(cleanHtml)) {
                // 检查表格结构是否完整
                const tableMatch = cleanHtml.match(/<table[^>]*>[\s\S]*?<\/table>/gi);
                if (tableMatch) {
                    // 验证每个表格是否有完整的行和列
                    tableMatch.forEach(table => {
                        const trCount = (table.match(/<tr[^>]*>/gi) || []).length;
                        const tdCount = (table.match(/<td[^>]*>/gi) || []).length;
                        // 如果表格结构不完整，移除它
                        if (trCount === 0 || (trCount > 0 && tdCount === 0)) {
                            cleanHtml = cleanHtml.replace(table, '');
                        }
                    });
                } else {
                    // 如果 table 标签不匹配，移除所有 table 相关标签
                    cleanHtml = cleanHtml.replace(/<table[^>]*>.*?<\/table>/gi, '');
                    cleanHtml = cleanHtml.replace(/<table[^>]*>/gi, '');
                    cleanHtml = cleanHtml.replace(/<\/table>/gi, '');
                }
            }
            
            // 如果清理后为空，返回默认内容
            if (!cleanHtml || cleanHtml.trim() === '') {
                return '<p><br></p>';
            }
            
            // 如果 HTML 不包含任何有效的块级元素，包装在 p 标签中
            if (!/<(p|div|h[1-6]|ul|ol|li|blockquote|pre|table|tr|td|th)/i.test(cleanHtml)) {
                // 如果只是纯文本或内联元素，包装在 p 标签中
                cleanHtml = '<p>' + cleanHtml + '</p>';
            }
            
            return cleanHtml;
        },
        destroyEditor(){
            if(this.wangTar != null && this.wangTar.editor){
                try {
                    this.wangTar.editor.destroy();
                } catch(e) {
                    console.warn("wanghtml: error destroying editor", e);
                }
                this.wangTar = null;
            }
        },
        handleReadonlyChange(){
            if(this.isReadonly){
                this.destroyEditor();
            } else {
                this.init();
            }
        },
        init(){
            // 只读模式不初始化编辑器
            if(this.isReadonly){
                return;
            }
            
            // 如果已经存在编辑器实例，先销毁
            this.destroyEditor();
            
            this.checkEnv((e)=>{
                console.log("initWang")
                if(this.version=="v5"){
                    this.initV5();
                }
                else if(this.version=="v3"){
                    this.initV3();
                }  
            })
        },
        checkEnv(callback){
            // var p1= loadJs("/ccflow/sys/js/jquery-1.7.2.min.js")
            //     .then((e)=>loadJs("/ccflow/sys/js/jquery-ui/jquery-ui-1.9.2-min.js?2025"))
            //     .then((e)=>loadJs("/ccflow/sys/js/jquery.contextmenu.r2.js"))
            // ;
            //var p3= ;
            //var p2= loadJs("/ccflow/sys/js/jquery.contextmenu.r2.js");
            var  jsPath="";
            var cssPath="";
            if(this.version=="v5"){
                if(this.full){
                    jsPath="/ccflow/sys/js/wangeditor/v5.1.23/editor/index.js";
                    cssPath="/ccflow/sys/js/wangeditor/v5.1.23/editor/css/style.css"
                }
                else{
                    jsPath="/ccflow/sys/js/wangeditor/v5.1.23/core/index.js";
                    cssPath="/ccflow/sys/js/wangeditor/v5.1.23/core/css/style.css"
                }
            }
            else if(this.version=="v3"){
                jsPath="/ccflow/sys/js/wangeditor/v3/wangEditor.min.js";
                cssPath="/ccflow/sys/js/wangeditor/v3/wangEditor.min.css"
            }
            var pjs= loadJs(jsPath);
            var pcss=loadCss(cssPath)
            return Promise.all([pjs,pcss])
            .then((res)=>{
                callback();
            });
        },
        initV5(){
            const E = window.wangEditor
            var vm=this;
            
            // 重试函数，确保 ref 存在后再初始化
            var tryInit = function(retryCount) {
                retryCount = retryCount || 0;
                var maxRetries = 10;
                
                vm.$nextTick(() => {
                    var el=vm.$refs.editor;
                    var toolbarEl=vm.$refs.toolbar;
                    
                    // 检查必要的 ref 是否存在
                    if(!el || !toolbarEl){
                        if(retryCount < maxRetries){
                            // 如果 ref 不存在且未超过最大重试次数，延迟后重试
                            setTimeout(() => {
                                tryInit(retryCount + 1);
                            }, 100);
                        } else {
                            console.error("wanghtml: editor or toolbar ref not found after retries");
                        }
                        return;
                    }
                    
                    // 如果已经初始化过，先销毁
                    vm.destroyEditor();
                    
                    var menuConf = {};
                    if (typeof vm.onImageUpload === 'function') {
                        menuConf.uploadImage = {
                            customUpload(file, insertFn) {
                                Promise.resolve(vm.onImageUpload(file))
                                    .then((url) => {
                                        if (url) insertFn(url, file?.name || '', '');
                                    })
                                    .catch((err) => {
                                        console.warn('wanghtml: image upload failed', err);
                                    });
                            },
                        };
                    }
                    // 先创建编辑器，使用默认空内容，避免 HTML 格式问题
                    var editor = E.createEditor({
                        selector: el,
                        html: '<p><br></p>', // 始终使用默认空内容初始化
                        config: {
                            placeholder: vm.placeholder || '请输入内容...',
                            MENU_CONF: menuConf,
                            toolbarKeys: vm.toolbarKeys && vm.toolbarKeys.length ? vm.toolbarKeys : undefined,
                            onChange(editor) {
                                vm.content = editor.getHtml();
                                vm.contentTxt = editor.getText();
                                vm.selected = editor.getSelectionText();
                                vm.$emit("input", vm.content);
                                vm.$emit("update:modelValue", vm.content);
                            }
                        }
                    });

                    const toolbarConfig = {};
                    const defaultKeys = [
                        // 标题样式切换。若不需要可通过 toolbarKeys 覆盖去掉
                        'headerSelect',
                        // 基础文本格式：加粗、斜体、下划线、删除线
                        'bold',
                        'italic',
                        'underline',
                        'through',
                        // 文字颜色与背景色
                        'color',
                        'bgColor',
                        // 字号、字体、行高
                        'fontSize',
                        'fontFamily',
                        'lineHeight',
                        // 缩进与取消缩进
                        'indent',
                        'delIndent',
                        // 列表类：无序、有序、待办
                        'bulletedList',
                        'numberedList',
                        
                        // 对齐方式：左、右、中、两端
                        'justifyLeft',
                        'justifyRight',
                        'justifyCenter',
                        'justifyJustify',
                        // 链接、引用
                        'insertLink',
                        'blockquote',
                        // 表格
                        'insertTable',
                        // 图片：上传（需 onImageUpload）
                        ...(typeof vm.onImageUpload === 'function' ? ['uploadImage'] : []),
                        // 视频、代码块（已禁用）
                        // 'insertVideo',
                        
                        // 分割线
                        'divider',
                        // 撤销、重做
                        'undo',
                        'redo',
                        // 全屏
                        //'fullScreen'
                    ];
                    toolbarConfig.toolbarKeys = vm.toolbarKeys && vm.toolbarKeys.length ? vm.toolbarKeys : defaultKeys;
                    var toolbar = E.createToolbar({
                        editor: editor,
                        selector: toolbarEl,
                        config: toolbarConfig
                    });
                    
                    vm.wangTar = {
                        editor: editor,
                        toolbar: toolbar
                    };
                    
                    // 编辑器创建完成后，再设置 HTML 内容
                    // 使用 nextTick 确保编辑器完全初始化
                    vm.$nextTick(() => {
                        try {
                            let cleanHtml = vm.modelValue || '';
                            
                            // 如果数据是 JSON 格式（可能是编辑器内部格式），清空内容
                            if (typeof cleanHtml === 'string' && 
                                (cleanHtml.trim().startsWith('{') || cleanHtml.trim().startsWith('['))) {
                                try {
                                    const jsonData = JSON.parse(cleanHtml);
                                    if (jsonData.children) {
                                        cleanHtml = '<p><br></p>';
                                    }
                                } catch (e) {
                                    cleanHtml = '<p><br></p>';
                                }
                            } else if (typeof cleanHtml === 'string' && cleanHtml.trim() === '') {
                                cleanHtml = '<p><br></p>';
                            } else if (cleanHtml) {
                                // 验证和清理 HTML
                                cleanHtml = vm.sanitizeHtml(cleanHtml);
                            }
                            
                            // 只有当内容不为空且不是默认空段落时才设置
                            if (cleanHtml && cleanHtml !== '<p><br></p>' && cleanHtml.trim() !== '') {
                                editor.setHtml(cleanHtml);
                            }
                        } catch (e) {
                            console.warn("wanghtml: error setting initial html", e);
                            // 如果设置失败，保持默认空内容
                        }
                    });
                });
            };
            
            // 开始初始化
            tryInit();
        },
        initV3(){
            const me =this; 
            var E=window.wangEditor;
            if(E==void 0){
                console.log("请先引入核心库！");
            }else{
                var editor= new E(this.$refs.editor);
                //配置图片上传参数（已禁用）
                // editor.customConfig.uploadImgShowBase64 = true;
                //editor.customConfig.uploadImgServer = "../../OtherSource/controller.ashx?action=uploadimage";
                //editor.customConfig.uploadImgMaxSize = 30 * 1024 * 1024;
                //editor.customConfig.uploadImgMaxLength = 5;

                editor.customConfig.onchange = (val) => {
                    me.emiting=true;
                    me.content = val;
                    me.contentTxt=me.editor.txt.text();
                    me.$emit('change',{ value: val, text: me.contentTxt });                 
                    me.$emit('input',val);
                    me.$emit('update:modelValue',val); 
                    setTimeout(() => {
                        me.emiting=false;
                    }, 1000);  
                };     
                editor.customConfig.menus=me.menus;   
                editor.customConfig.onfocus=()=>{
                    me.focused=true;
                    me.$emit("onfocus",me);
                    if(typeof me.onfocus=='function'){
                        me.onfocus();
                    }
                };
                editor.customConfig.onblur=()=>{
                    me.focused=false;
                    me.$emit("onblur",me);
                    if(typeof me.onblur=='function'){
                        me.onblur();
                    }
                }; 
                editor.create();
                if(me.modelValue != void 0){
                    editor.txt.html(me.modelValue);                    
                }
                this.editor=editor;
                this.$nextTick(()=>{
                    //处理编辑器高度问题。
                    if(me.editor.$textContainerElem){
                        var x=me.editor.$textContainerElem;
                        if(me.autoGrow){
                            x[0].style.minHeight=me.height;
                            x[0].style.height="auto";
                        }else{
                            x[0].style.height=me.height;
                        }
                        //x[0].setAttribute("height",me.height);
                    }
                });
            }  
        }
    }
}
</script>
<style type="text/css" scoped>
.autohide .w-e-toolbar{
    display: none;
}
.autohide:focus-within .w-e-toolbar {
    display: block;
}
.ttaree{
    width:100%;
    border-radius: 5px;
}
div.miwangee{
    margin:5px;
}
div.wang-inputpan{
    border-radius: 4px;
    border: 1px solid #DCDFE6;
}
div.wang-disabled-content{
    border-radius: 4px;
    border: 1px solid #DCDFE6;
    padding: 12px;
    background-color: #f5f7fa;
    overflow: auto;
    min-height: 200px;
    background-color: #FFF;
}
div.wang-noedit-content{
    border-radius: 4px;
    border: 1px solid #DCDFE6;
    padding: 12px;
    overflow: auto;
    min-height: 200px;
    background-color: #FFF;
    line-height: 1.6;
    word-break: break-word;
}
</style>