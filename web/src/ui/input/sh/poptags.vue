<template>
    <div  class="tags-pan" >
        <div class="tags" :style="tagsContainerStyle">
            <el-tag v-for="(tag,t) in list" :key="t"
                :class="{ stag: true, dragging: t==draggingIndex }"
                :style="[tagStyle, tagLayoutStyle]"
                :closable="closable" 
                :effect="t==focusIndex?'dark':'light'"
                @click="onTagClick(tag,t)"
                @dblclick="onDBTagClick(tag,t)"
                @close="removingEmp(tag)"
                
                :draggable="draggable"
                @dragstart="ondragstart($event,tag,t)"
                @dragover.prevent="ondragOver($event,tag,t)" 
                @dragend="ondragEnd($event,tag,t)"
                @drop="onDrop($event,tag,t)"
                >
                <slot :row="tag">{{tag[name]}}</slot>
            </el-tag>

            <el-popover  v-if="addable && action=='pop'"  ref="popoverRef"
                :placement="placement"
                :title="title"
                :width="width" effect="light"
                :trigger="trigger" :show="show" @input="emit('popchange')"
                content="">
                <template #default="p">
                    <lazydiv>
                        <slot name="addpop" :resize="upDatePosition"></slot>
                    </lazydiv>
                </template>
                <template #reference>
                    <el-tag  type="warning" class="addtag" style="" @click="onclickadd" title="点击添加">
                        <span class="add-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M480 480V128a32 32 0 0 1 64 0v352h352a32 32 0 1 1 0 64H544v352a32 32 0 1 1-64 0V544H128a32 32 0 0 1 0-64z"></path></svg>
                        </span>{{addlabel}}
                    </el-tag>
                </template>
            </el-popover >
           
            <el-tag v-else-if="addable"  type="warning" class="addtag" style="" @click="onclickadd" title="点击添加">
                <span class="add-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M480 480V128a32 32 0 0 1 64 0v352h352a32 32 0 1 1 0 64H544v352a32 32 0 1 1-64 0V544H128a32 32 0 0 1 0-64z"></path></svg>
                        </span>{{addlabel}}
            </el-tag>

            <el-tag v-if="addable && clearable && list.length>0"   type="danger" class="addtag" style="margin-left:5px" @click="onclickDelete" title="清空">
                <span class="add-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" ><path fill="currentColor" d="M352 192V95.936a32 32 0 0 1 32-32h256a32 32 0 0 1 32 32V192h256a32 32 0 1 1 0 64H96a32 32 0 0 1 0-64zm64 0h192v-64H416zM192 960a32 32 0 0 1-32-32V256h704v672a32 32 0 0 1-32 32zm224-192a32 32 0 0 0 32-32V416a32 32 0 0 0-64 0v320a32 32 0 0 0 32 32m192 0a32 32 0 0 0 32-32V416a32 32 0 0 0-64 0v320a32 32 0 0 0 32 32"></path></svg>
                        </span>
            </el-tag>
            <slot name="tail"></slot>
        </div>
        <template v-if="position=='dialog'">
            <el-dialog :title="diatitle" :model-value="focused" :width="width" append-to-body @close="closeFocus">
                <slot name="focus" :row="focusTag" :focused="focused"></slot>
            </el-dialog>
        </template>
        <template v-else>
            <div v-if="keep || focused">
                <slot name="focus" :row="focusTag" :focused="focused"></slot>
            </div>
        </template>
        <template v-if="action=='dialog'">
            <el-dialog :title="title" :model-value="showAdd" :width="width" append-to-body @close="showAdd=false">
                <slot name="addpop" ></slot>
            </el-dialog>
        </template>
    </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import lazydiv from '@/ui/panels/lazydiv.vue'

defineOptions({
    name: 'tags',
})

function toCssSize(v) {
    if (v == null || v === '') return null
    return typeof v === 'number' ? `${v}px` : String(v)
}

const props = defineProps({
    list: { type: Array, default() { return [] } },
    name: { type: String, default: 'Name' }, // 显示在tag中的名称属性

    closable: { type: Boolean, default: false },
    tagStyle: { type: [String, Object], default: 'margin-right:6px;' },
    /** tag 最大宽度，如 160 / '160px'；配合 tagWrap 控制截断或换行 */
    tagWidth: { type: [String, Number], default: null },
    /** true：标签容器换行，且 tag 内文字可折行、高度随内容增高 */
    tagWrap: { type: Boolean, default: false },
    keep: { type: Boolean, default: false }, // 是否始终显示焦点面板
    addable: { type: Boolean, default: false }, // 是否显示虚拟的添加标签
    clearable: { type: Boolean, default: false },
    addlabel: { type: String, default: '' }, // 添加
    action: { type: String, default: '' }, // 为pop时使用 pop
    // 添加浮窗控制
    trigger: { type: String, default: 'click' },
    show: { type: Boolean, default: false },
    placement: { type: String, default: null }, // bottom
    title: { type: String, default: '' },

    width: { type: String, default: '250px' },
    // 详情弹窗控制
    position: { type: String, default: 'bottom' }, // 内容窗体的显示位置。
    focusable: { type: Boolean, default: false },
    diatitle: { type: String, default: '详情' },
    focusTrigger: { type: String, default: 'click' },
    // 拖动控制
    draggable: { type: Boolean, default: false },
    // 拖动分组，同一分组的元素可以相互拖入。
    group: { type: String, default: '' },
})

const emit = defineEmits(['add', 'close', 'select', 'sort', 'deleteall', 'update:list', 'popchange'])

const popoverRef = ref(null)

const mlist = ref([])
const focusTag = ref({})
const focusIndex = ref(-1)
const focused = ref(false)
const draggingIndex = ref(-1)
const dragging = ref(false)
const draggingGroup = ref('')
const showAdd = ref(false)

const tagWidthCss = computed(() => toCssSize(props.tagWidth))

/** 标签容器：tagWrap 时通过 style 控制换行排布 */
const tagsContainerStyle = computed(() => {
    if (!props.tagWrap) return null
    return {
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'flex-start',
        gap: '4px 0',
    }
})

/** 单个 tag：tagWidth / tagWrap 均写入 style，不依赖额外 css class */
const tagLayoutStyle = computed(() => {
    const style = {}
    if (tagWidthCss.value) {
        style.maxWidth = tagWidthCss.value
    }
    if (props.tagWrap) {
        style.height = 'auto'
        style.whiteSpace = 'normal'
        style.wordBreak = 'break-word'
        style.overflow = 'visible'
        style.textOverflow = 'unset'
    } else if (tagWidthCss.value) {
        style.overflow = 'hidden'
        style.textOverflow = 'ellipsis'
        style.whiteSpace = 'nowrap'
    }
    return style
})

const groupName = computed(() => {
    if (!props.group || props.group == 'none') {
        return 'group' + (Math.random() * 100)
    }
    return props.group
})

onMounted(() => {
    mlist.value = props.list
})

watch(
    () => props.list,
    function () {
        // 拖动期间，不通知外界，不响应变化。
        if (dragging.value) {
            return
        }
        mlist.value = props.list
    },
)

function upDatePosition() {
    // 强制触发 resize事件，引起 popover 重新计算位置。
    // console.log(popoverRef.value)
    // 创建 resize 事件对象
    var event = new Event('resize')

    // 获取需要触发 resize 的元素（通常为 window）
    var element = document.defaultView || window

    // 调用 dispatchEvent() 方法来触发 resize 事件
    element.dispatchEvent(event)
    // nextTick(() => {
    //     popoverRef.value.updatePopper()
    // })
}

function onclickadd() {
    if (props.addable) {
        emit('add')
        if (props.action == 'dialog') {
            showAdd.value = true
        }
    }
}

function onclickDelete() {
    emit('deleteall')
}

function removingEmp(row) {
    emit('close', row)
}

function onTagClick(row, r) {
    if (props.focusTrigger == 'click') {
        focusTo(row, r)
    }
    emit('select', row)
}

function onDBTagClick(row, r) {
    if (props.focusTrigger == 'dblclick') {
        focusTo(row, r)
    }
}

function focusTo(row, r) {
    if (focused.value && r == focusIndex.value) {
        focused.value = false
        focusTag.value = {}
        focusIndex.value = -1
        return
    }
    focusTag.value = row
    focused.value = true
    focusIndex.value = r
}

function closeFocus() {
    focused.value = false
    focusTag.value = {}
    focusIndex.value = -1
}

/// 拖动控制
function ondragstart(e, row) {
    draggingGroup.value = groupName.value
    e.dataTransfer.effectAllowed = 'move' // none/move/link/copyMove/copyLink/linkMove/all
    e.dataTransfer.setData(
        'a',
        JSON.stringify({
            group: groupName.value,
            row: row,
        }),
    )
    // e.dataTransfer.setDragImage(image, xOffset, yOffset);
    // e.target.style.opacity=0;
    dragging.value = true
    console.log(e, groupName.value)
}

function ondragOver(e, row, r) {
    // const data = e.dataTransfer.getData("a");
    // console.log(e,data);
    if (draggingGroup.value != groupName.value) {
        e.dataTransfer.effectAllowed = 'none'
        return
    }
    e.dataTransfer.effectAllowed = 'move'
    changeOrder(r)
    draggingIndex.value = r
}

function ondragEnd(e, row, r) {}

function onDrop(event) {
    const data = event.dataTransfer.getData('text/plain')
    // event.target.textContent = data;
    event.preventDefault()
    draggingIndex.value = -1
    dragging.value = false
    draggingGroup.value = ''
    emit('sort', mlist.value)
    emit('update:list', mlist.value)
}

function changeOrder(newVal) {
    var oldVal = draggingIndex.value
    if (newVal == oldVal) {
        return
    }
    if (newVal != null) {
        if (oldVal != null && oldVal != -1) {
            // const myElement = refs['drag'+oldVal][0]
            // setTimeout(() =>{myElement.style.opacity = '1'},0)
            if (newVal > oldVal) {
                let f = props.list[oldVal]
                for (let i = oldVal; i < newVal; i++) {
                    props.list[i] = props.list[i + 1] // $set 触发视图更改
                }
                props.list[newVal] = f
            } else {
                let f = props.list[oldVal]
                for (let i = oldVal; i > newVal; i--) {
                    props.list[i] = props.list[i - 1] // $set 触发视图更改
                }
                props.list[newVal] = f
            }
            // const myElement1 = refs['drag'+newVal][0]
            // setTimeout(() =>{myElement1.style.opacity = '0'},0)
        }
    }
}
</script>

<style scoped>

.tags{

}
.addtag{
    border:2px dashed;
    cursor:pointer;
    border-width: thin;
    border-image: repeating-linear-gradient(135deg, #e6a23c, #e6a23c 5px, transparent 5px, transparent 10px) 1;
    min-width: 45px;
    text-align: center;
}
.add-icon{
    display:inline-block;
    height:12px;
    width:12px;
}

.dragging{
    color:#999;
    opacity:0.5;
}
</style>
