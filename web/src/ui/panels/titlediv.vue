<template>
<fieldset class="titlediv" style=" min-inline-size: auto;" :class="{'opened':opened}">
    <legend class="ttdheader" style="titleStyle">
        <span @click="togglecontent">
            <svg v-if="togglable" :style="{transform:opened?'rotate(0deg)':'rotate(180deg)'}" class="icon" style="transition:all .5s ease-out;width: 1em; height: 1em;vertical-align: middle;fill: currentColor;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1566">
                <path d="M512 830.4l402.4-401.9c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 887c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 466.4c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 830.4z" fill="#040000" p-id="1567"></path>
                <path d="M512 538.9L914.4 137c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 595.4c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 174.8c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 538.9z" fill="#040000" p-id="1568"></path>
            </svg>
            <slot name="title"><span v-html="title"></span></slot>
        </span>
        <slot name="tail"></slot>
    </legend>
    <transition name="tdfade">
        <div v-if="rended" :style="contStyle" style="transition:all .5s ease-out;transform-origin:50% 0% 0; overflow: hidden;" ref="content">
            <slot></slot>
        </div>
    </transition>
</fieldset>
</template>

<script>
export default {
    name: 'titlediv',

    props: {
        title: null,
        open: {
            type: Boolean,
            default: true
        },
        minHeight: {
            type: String,
            default: "0px"
        },
        height: {
            type: String,
            default: "auto"
        },
        titleStyle: null,
        togglable: {
            type: Boolean,
            default: true
        },
        //如果设置了懒加载，则初始必然不绘制。设置了open，则必然不是懒加载
        lazy: {
            type: Boolean,
            default: false
        },
        //状态输出与受控
        name: null,
        value: null,
    },
    data: function () {
        var opened = this.open;
        if (this.lazy) opened = false;
        return {
            opened: opened,
            rended: !this.lazy,
            myheight: (opened ? this.height : this.minHeight),
        };
    },
    computed: {
        contStyle() {
            var show = this.opened ? 1 : 0;
            var res = {
                height: this.myheight,
                opacity: show,
                transform: 'scale(1,' + show + ')',
                padding: this.opened ? '' : '0',
                margin: this.opened ? '' : '0'
            };
            return res;
        },
    },
    methods: {
        togglecontent() {
            if (!this.togglable) return;
            var vm = this;
            if (this.rended == false) {
                this.rended = true;
            }
            if (this.opened) {
                this.opened = false;
                this.myheight = this.minHeight;
            } else {
                vm.myheight = vm.height;
                this.opened = true;
            }
            var val = this.opened;
            if (!!this.name) {
                var i = -1;
                var tar = [];
                if (Array.isArray(this.value)) {
                    tar = this.value.concat();
                }
                i = tar.indexOf(this.name);
                if (val && i == -1) {
                    tar.push(this.name);
                }
                if (!val && i != -1) {
                    tar.splice(i, 1);
                }
                this.$emit('input', tar);
            } else {
                this.$emit('input', this.opened);
            }
            this.$emit('change', this.opened);
        },
    },
    watch: {
        value: {
            handler(newval, oldval) {
                if (newval == void 0 || newval == null) return;
                var needOpen = (typeof newval == 'boolean' && newval == true) || newval == this.name || (Array.isArray(newval) && newval.indexOf(this.name) != -1);
                if (needOpen != this.opened) {
                    this.togglecontent();
                }
            },
            immediate: true
        },
    },
};
</script>

<style scoped>
.ttdheader {
    width: auto;
    cursor: pointer;
}

.titlediv {
    border-radius: 5px;
    padding: 0.35em 0.75em;
    margin-top: 22px;
    margin-bottom: 18px;
    border: 1px solid #9A9A9A;
    transition: padding-bottom 0.3s ease;
    overflow: hidden;
}

.titlediv:not(.opened) {
    padding-bottom: 0.35em;
}

.titlediv.opened {
    padding-bottom: 0.75em;
}


</style>
