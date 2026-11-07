<template>
    <div class='Ue_module'>
        <textarea :id='myid'></textarea>  
    </div>
</template>
<script>

export default {
    setup() {
        
    },
    data: function () {
        //编辑器必须有唯一id。这里采用外界传入法+内部生成法。 
        var xx = this.id;
        if (!!xx) {
            var id0 = 'ue' + xx;
        } else {
            var chars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];
            var res = "";
            for (var i = 0; i < 8; i++) {
                var id = Math.ceil(Math.random() * 35);
                res += chars[id];
            }
            var id0 = 'ue' + res;
        }
        var height = 280;
        if (this.height != "") {
            height = this.height;
        }
        var path = "";
        if (window.UCMLLocalResourcePath != void 0) {
            path = window.UCMLLocalResourcePath + "upload/";
        }
        return {
            myid: id0,
            editor: null,
            myvalue: null,
            defaultconfig: {
                initialFrameWidth: '800px',
                initialFrameHeight: height,
                catcherUrlPrefix: path,
                fileManagerUrlPrefix: path,
                fileUrlPrefix: path,
                imageManagerUrlPrefix: path,
                imageUrlPrefix: path,
                scrawlUrlPrefix: path,
                snapscreenUrlPrefix: path,
                videoUrlPrefix: path,
                zIndex:"auto",
            }
        }
    },
    props: {
        initvalue: String,
        value: { type: String, default: "" },
        config: { type: Object, default: function () { return {}; } },
        id: null,
        height: { type: String, default: "" },
        disabled: { type: Boolean, default: false },
        placeholder: { type: String, default: "请输入内容" },
    },
    watch: {
        value: {
            handler: function (val, oldval) {
                if (val == null || val == void 0) {
                    val = "";
                }
                //由于重置编辑器内容时，会改变用户输入光标，所以要检测是外界写入value时才更新。自动同步时，value和编辑器的值是相同的。
                var myvalue = this.editor.getContent();
                if (val != myvalue) {
                    this.editor.setContent(val);
                    this.setPlacehold(val);
                }
            },
            deep: true
        }
    },
    computed: {
        configInfo() {
            return Object.assign(this.defaultconfig, this.config)
        },
    },
    mounted: function () {
        //当id动态绑定时，插入必须延迟到UI下一次绘制。也可采用传入法，但是父组件需要定义多个id对应多个UE实例   
        const me = this;
        this.$nextTick(function () {
            //加载默认配置

            var defig = me.configInfo;

            me.editor = UE.getEditor(me.myid, defig);

            me.editor.ready(function () {
                //如果传入了初始值，就使用它
                //console.log(me.editor);
                if (!!me.value) {
                    me.editor.setContent(me.value);
                }
                me.setPlacehold(me.value);
                //注册控件内容变化事件，并传值给父组件。
                me.editor.addListener("contentChange", function () {
                    const wordCount = me.editor.getContentLength(true);
                    const content = me.editor.getContent();
                    const plainTxt = me.editor.getPlainTxt();
                    me.myvalue = content;
                    me.$emit('UEinput', { wordCount: wordCount, plainTxt: plainTxt });
                    me.$emit('input', content);
                    me.setPlacehold(content);
                }.bind(this));
                if (me.disabled) {
                    me.editor.disable();
                }

                me.$emit('ready', me.editor);
            }.bind(this));
            //console.log(me.editor);
        });

    },
    methods: {
        setPlacehold(val) {
            if (val == null || val == void 0 || val == "") {
                this.showplacehold();
            } else {
                this.hidePlacehold();
            }
        },
        showplacehold() {
            //edui1_iframeholder
            var tar = this.editor.iframe.parentNode;
            var bg = tar.lastChild;
            if (bg.tagName != "DIV") {
                tar.style.position = "relative";
                var bgDiv = document.createElement("DIV");
                bgDiv.style.position = "absolute";
                bgDiv.style.color = "#9CA0A6";
                bgDiv.style.top = 0;
                bgDiv.innerHTML = this.placeholder;
                tar.appendChild(bgDiv);
            }
            else {
                bg.style.display = "block";
                bg.innerHTML = this.placeholder;
            }
        },
        hidePlacehold() {
            var tar = this.editor.iframe.parentNode;
            var bg = tar.lastChild;
            if (bg.tagName == "DIV") {
                bg.style.display = "none";
            }
        },
    },
    destroyed: function () {
        this.editor.destroy();
    }
}
</script>
