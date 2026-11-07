<template>
<div v-bind="$attrs">
    <iframe class="d2-container-frame" :src="tarUrl" frameborder="0" />
</div>
</template>

<script>
export default {
    name: 'WebUrlapplet',
    props: {
        src: {
            type: String,
            required: false,
            default: ''
        }
    },
    data() {
        return {

        };
    },
    computed: {
        tarUrl() {
            //console.log(process);
            if (this.src.startsWith('http')) {
                return this.src;
            }
            if (!this.src) {
                return import.meta.env.VITE_APP_PUBLIC_PATH + "emply.html";
            }
            return import.meta.env.VITE_APP_PUBLIC_PATH +this.src;
        },
    },
    mounted() {

    },

    methods: {

    },
};
</script>
