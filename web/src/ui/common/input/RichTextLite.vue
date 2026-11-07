<template>
    <div :class="mCss">
        <div v-if="!disabled" :id="toolbarId" class="w-e-toolbar"></div>
        <div v-if="!disabled" :id="editorId" class="wang-inputpan" :style="editorStyle"></div>
        <div v-if="disabled" v-html="rendervalue"></div>
    </div>
</template>

<script setup>
import { ref, shallowRef, computed, watch, nextTick, onBeforeUnmount, onMounted } from 'vue';
import '@wangeditor/editor/dist/css/style.css';
import { uploadWangImage, unwrapWangImageResult, resolveWangImageUrl } from './wangImageApi.js';

let WEditor = null;
let WToolbar = null;

const DEFAULT_MENUS = [
    'headerSelect',
    'bold',
    'fontSize',
    'fontFamily',
    'italic',
    'underline',
    'strikeThrough',
    'color',
    'bgColor',
    'list',
    'todo',
    'justify',
    'quote',
    'insertLink',
    'uploadImage',
    'codeBlock',
    'divider',
    'undo',
    'redo',
    'fullScreen',
];

const props = defineProps({
    initvalue: { type: String, default: '' },
    modelValue: { type: String, default: '' },
    disabled: { type: Boolean, default: false },
    menus: {
        type: Array,
        default: null,
    },
    /** 富文本插图 FTP 前缀路径，默认 /wang → wang/yyyy/MM/... */
    pathPrefix: { type: String, default: '/wang' },
    /** 自定义上传；(file, insertFn) => Promise。未传则走独立 wangImage 接口 */
    uploadImageFn: { type: Function, default: null },
    height: { type: String, default: '200px' },
    autoGrow: { type: Boolean, default: true },
    tbHide: { type: Boolean, default: false },
    onfocus: { type: Function, default: null },
    onblur: { type: Function, default: null },
    version: { type: String, default: 'v5' },
    full: { type: Boolean, default: true },
    placeholder: { type: String, default: '请输入内容...' },
    editorMode: { type: String, default: 'default' },
    toolbarMode: { type: String, default: 'default' },
});

const emit = defineEmits(['update:modelValue', 'input', 'change', 'onfocus', 'onblur']);

const editorRef = shallowRef();
const innerHtml = ref(props.modelValue || props.initvalue || '');
const emiting = ref(false);
const focused = ref(false);
const uid = Math.random().toString(36).slice(2, 10);
const editorId = `rtl-editor-${uid}`;
const toolbarId = `rtl-toolbar-${uid}`;

const mCss = computed(() => ({
    miwangee: true,
    autohide: props.tbHide,
}));

const editorStyle = computed(() => {
    if (props.autoGrow) {
        return {
            minHeight: props.height,
            height: 'auto',
        };
    }
    return {
        height: props.height,
    };
});

const toolbarKeys = computed(() => {
    const keys = Array.isArray(props.menus) && props.menus.length ? [...props.menus] : [...DEFAULT_MENUS];
    if (!keys.includes('uploadImage')) {
        const linkIdx = keys.indexOf('insertLink');
        if (linkIdx >= 0) keys.splice(linkIdx + 1, 0, 'uploadImage');
        else keys.push('uploadImage');
    }
    return keys;
});

const toolbarConfig = computed(() => ({
    toolbarKeys: toolbarKeys.value,
}));

async function defaultUploadImage(file, insertFn) {
    const res = await uploadWangImage(file, props.pathPrefix);
    const fileOut = unwrapWangImageResult(res);
    const url = resolveWangImageUrl(fileOut);
    if (!url) {
        throw new Error('上传成功但未返回图片地址');
    }
    insertFn(url, file?.name || 'image', url);
}

function buildUploadImageConf() {
    return {
        base64LimitSize: 0,
        customUpload: async (file, insertFn) => {
            try {
                if (typeof props.uploadImageFn === 'function') {
                    await props.uploadImageFn(file, insertFn);
                } else {
                    await defaultUploadImage(file, insertFn);
                }
            } catch (err) {
                console.error('RichTextLite uploadImage failed', err);
                if (typeof swal === 'function') {
                    swal({
                        text: err?.message || '图片上传失败',
                        icon: 'error',
                        button: '确定',
                    });
                }
                throw err;
            }
        },
    };
}

const editorConfig = computed(() => ({
    placeholder: props.placeholder,
    readOnly: props.disabled,
    MENU_CONF: {
        uploadImage: buildUploadImageConf(),
    },
}));

const rendervalue = computed(() => {
    const value = props.modelValue ?? props.initvalue ?? '';
    if (!value) return '';
    if (typeof value === 'string' && (value.trim().startsWith('{') || value.trim().startsWith('['))) {
        try {
            const json = JSON.parse(value);
            if (json.children) {
                return '';
            }
        } catch (e) {
            return '';
        }
    }
    return value;
});

const handleCreated = (editor) => {
    editorRef.value = editor;
    if (props.disabled) {
        editor.disable();
    } else {
        editor.enable();
    }
    nextTick(() => {
        const content = props.modelValue || props.initvalue || '<p><br></p>';
        editor.setHtml(content);
    });
};

const handleChange = (editor) => {
    if (emiting.value) return;
    const html = editor.getHtml();
    innerHtml.value = html;
    emit('input', html);
    emit('update:modelValue', html);
    emit('change', { value: html, text: editor.getText() });
};

const handleFocus = () => {
    focused.value = true;
    emit('onfocus');
    if (typeof props.onfocus === 'function') {
        props.onfocus();
    }
};

const handleBlur = () => {
    focused.value = false;
    emit('onblur');
    if (typeof props.onblur === 'function') {
        props.onblur();
    }
};

watch(
    () => props.modelValue,
    (val) => {
        if (focused.value) {
            return;
        }
        const safeVal = val ?? '';
        if (safeVal === innerHtml.value) {
            return;
        }
        innerHtml.value = safeVal;
        if (editorRef.value) {
            emiting.value = true;
            editorRef.value.setHtml(safeVal || '<p><br></p>');
            nextTick(() => {
                emiting.value = false;
            });
        }
    }
);

watch(
    () => props.disabled,
    (val) => {
        if (!editorRef.value) return;
        if (val) {
            editorRef.value.disable();
        } else {
            editorRef.value.enable();
        }
    }
);

onBeforeUnmount(() => {
    destroyEditor();
});

const destroyEditor = () => {
    if(checkTickor!=null){
        clearTimeout(checkTickor)
    }
    if (editorRef.value) {
        try {
            editorRef.value.destroy();
        } catch (err) {
            console.warn('RichTextLite destroy error', err);
        }
        editorRef.value = null;
    }
};

const ensureModulesLoaded = async () => {
    if (WEditor && WToolbar) return;
    const editorMod = await import('@wangeditor/editor');
    if (editorMod.createEditor) {
        WEditor = editorMod.createEditor;
        WToolbar = editorMod.createToolbar;
    } else if (editorMod.default) {
        const { createEditor, createToolbar } = editorMod.default;
        WEditor = createEditor;
        WToolbar = createToolbar;
    } else {
        const { createEditor, createToolbar } = editorMod;
        WEditor = createEditor;
        WToolbar = createToolbar;
    }
};

var checkTickor=null;

const initEditor = async () => {
    if (props.disabled) {
        return;
    }
    await ensureModulesLoaded();
    const editorContainer = document.getElementById(editorId);
    const toolbarContainer = document.getElementById(toolbarId);
    if (!editorContainer || !toolbarContainer) {
        console.warn('RichTextLite: container not ready, retrying...');
        checkTickor=setTimeout(() => {
            initEditor();
        }, 100);
        return;
    }

    console.log('RichTextLite initEditor', editorId, toolbarId);

    destroyEditor();

    let cleanHtml = props.modelValue || props.initvalue || '';
    if (typeof cleanHtml === 'string' && (cleanHtml.trim().startsWith('{') || cleanHtml.trim().startsWith('['))) {
        cleanHtml = '<p><br></p>';
    }
    if (!cleanHtml) {
        cleanHtml = '<p><br></p>';
    }

    const editor = WEditor({
        selector: `#${editorId}`,
        html: cleanHtml,
        config: {
            placeholder: props.placeholder,
            MENU_CONF: {
                uploadImage: buildUploadImageConf(),
            },
            onChange: (ed) => {
                handleChangeLegacy(ed);
            },
            onFocus: () => handleFocus(),
            onBlur: () => handleBlur(),
        },
        mode: props.editorMode,
    });

    WToolbar({
        editor,
        selector: `#${toolbarId}`,
        mode: props.toolbarMode,
        config: {
            toolbarKeys: toolbarKeys.value,
        },
    });

    editorRef.value = editor;

    if (props.disabled) {
        editor.disable();
    } else {
        editor.enable();
    }
};

const handleChangeLegacy = (editor) => {
    if (emiting.value) return;
    const html = editor.getHtml();
    innerHtml.value = html;
    emit('input', html);
    emit('update:modelValue', html);
    emit('change', { value: html, text: editor.getText() });
};

watch(
    () => props.modelValue,
    (val) => {
        if (focused.value) {
            return;
        }
        const safeVal = val ?? '';
        if (safeVal === innerHtml.value) {
            return;
        }
        innerHtml.value = safeVal;
        if (editorRef.value) {
            emiting.value = true;
            editorRef.value.setHtml(safeVal || '<p><br></p>');
            nextTick(() => {
                emiting.value = false;
            });
        } else {
            initEditor();
        }
    }
);

watch(
    () => props.disabled,
    (val) => {
        if (!editorRef.value) {
            if (!val) {
                nextTick(() => {
                    initEditor();
                });
            }
            return;
        }
        if (val) {
            editorRef.value.disable();
        } else {
            editorRef.value.enable();
        }
    }
);

nextTick(() => {
    initEditor();
});

onMounted(() => {
    nextTick(() => {
        initEditor();
    });
});
</script>

<style scoped>
.miwangee {
    margin: 5px;
}
.w-e-toolbar {
    border: 1px solid #dcdfe6;
    border-bottom: none;
    border-radius: 4px 4px 0 0;
}
.wang-inputpan {
    border: 1px solid #dcdfe6;
    border-radius: 0 0 4px 4px;
    background-color: #fff;
}
.autohide .w-e-toolbar {
    display: none;
}
.autohide:focus-within .w-e-toolbar {
    display: block;
}
</style>

