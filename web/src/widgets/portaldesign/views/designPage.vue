<template>
	<PortalDesign :name="code" :isCopy="_isCopy" embedded @finishCopy="finishCopy" @back="handleBack"/>
</template>

<script>
import PortalDesign from "@p/components/index.vue";
import { NextLoading } from '@/utils/loading.js';

export default {
	name: 'portal-design-index',
	components: {
		PortalDesign
	},
	data() {
		return {
			_name: "qmindextest",
			_isCopy: false
		}
	},
	props: {
		name: {
			type: String,
			default: ""
		},
		isCopy: {
			type: String,
			default: ""
		}
	},
    computed:{
        code(){
            var code="qmindextest";
            if(!!this.$route.query.name){
                code=this.$route.query.name;
                return code;
            }
            if(!!this.$route.params.name){
                code=this.$route.params.name;
                return code;
            }
            return code;
        },
    },
	created() {
		if (this.$route.query) {//有参数时以参数为准
			var name = this.$route.query.name;
			if (!!name) {
				this._name = name;
			}
			var isCopy = this.$route.query.isCopy;
        	if (isCopy === true || isCopy === "true") {
        		this._isCopy = true;
        	}
        	if (isCopy === false || isCopy === "false") {
        		this._isCopy = false;
        	}
		} else {
			if (this.name) {
				this._name = this.name;
			}
			
			if (this.isCopy === true || this.isCopy === "true") {
        		this._isCopy = true;
        	}
        	if (this.isCopy === false || this.isCopy === "false") {
        		this._isCopy = false;
        	}
		}
		NextLoading.done();
	},
	methods: {
		finishCopy(name) {
			this._name = name;
			this._isCopy = false;
		},
		handleBack() {
			this.$router.back(-1);
		}
	}
}
</script>