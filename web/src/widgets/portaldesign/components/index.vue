<template>
	<lion-portal-design v-loading="loading"
		v-if="show"
		:name="name"
		:isCopy="isCopy"
		:embedded="embedded"
		:portlets="portlets"
		:portal="portal"
		:applets="applets"
		:selectedApplet="selectedApplet"
		:deleteAppletIds="deleteAppletIds"
		@back="$emit('back')"
		@updateSelectd="updateSelectd"
		@deleteApplet="deleteApplet"
		@clearApplet="clearApplet"
		@submit="handleSubmit"
		@regain="handleRegain">
	</lion-portal-design>
</template>

<script>
import { loadConfigApplets, getPortConfig, saveApplets } from "@p/api/design";
import LionPortalDesign from "@p/components/design.vue";
import { useRoute } from 'vue-router';
import mittBus from '@/utils/mitt.js';

const route = useRoute();

export default {
	name: 'PortalDesign',
	components: {
		LionPortalDesign
	},
	data() {
		return {
			portlets: [],
			portal: {},
			applets: [],
			selectedApplet: {},
			deleteAppletIds: [],
			show: false,
			loading: false
		}
	},
	props: {
		name: {
			type: String,
			default: ''
		},
		isCopy: {
			type: Boolean,
			default: false
		},
		embedded: {
			type: Boolean,
			default: false
		}
	},
	emits: ['finishCopy', 'back'],
	mounted() {
		this.loadPortlets();
		this.loadConfig();
		this.show = true;
	},
	methods: {
		async loadPortlets() {
			this.portlets = [];
			try {
				const res = await loadConfigApplets({ type: "1" });
				const list = res?.data ?? res;
				this.portlets = Array.isArray(list) ? list : [];
			} catch (e) {
				console.warn('loadConfigApplets failed', e);
				this.portlets = [];
			}
		},
		async loadConfig() {
			this.loading = true;
			
			if (!this.name) {
				return;
			}
			const res = await getPortConfig(this.name);
			
			if(res.status == "success") {
				const portal = res.data.portal || {};
				if (this.isCopy === true) {
					portal.P_Key = "";
					portal.oid = "new";
				}
				const col = Number(portal.colnum);
				portal.colnum = Number.isFinite(col) && col > 0 ? col : 24;
				const row = Number(portal.rowheight);
				portal.rowheight = Number.isFinite(row) && row > 0 ? row : 30;
				this.portal = portal;
				
				var applets = Array.isArray(res.data.cells) ? res.data.cells : [];
				var vclist = res.data.applets;
				if(!Array.isArray(vclist)) {
					vclist = [];
				}
				if (this.isCopy === true) {
					vclist.forEach(vc => {
						vc.oid = "new";
					});
				}
				const num = (v, d) => {
					const n = Number(v);
					return Number.isFinite(n) ? n : d;
				};
				applets.forEach((applet, idx) => {
					if (this.isCopy === true) {
						applet.oid = "new";
					}
					applet.x = num(applet.x, 0);
					applet.y = num(applet.y, 0);
					applet.w = Math.max(1, num(applet.w, 1));
					applet.h = Math.max(1, num(applet.h, 1));
					applet.i = num(applet.i, idx + 1);
					applet.mode = applet.mode || "1";
					
					applet.vclist = vclist.filter((app) => {
						return app.link == applet.ckey;
					});
				});
				
				this.applets = applets;
				
				this.deleteAppletIds = [];
				this.selectedApplet = {}
			}
			
			this.loading = false;
		},
		updateSelectd(applet) {
			this.selectedApplet = applet;
		},
		deleteApplet(applet, index) {
			this.deleteAppletIds.push(applet.oid);
			this.applets.splice(index, 1);
		},
		clearApplet() {
			while (this.applets.length > 0) {
				let applet = this.applets.shift();
				this.deleteAppletIds.push(applet.oid);
			}
		},
		handleRegain() {
			this.loadConfig();
		},
		handleSubmit() {
			this.loading = true;
			
			saveApplets({
				portal: this.portal,
				applets: this.applets,
				delIds: this.deleteAppletIds.join(",")
			}).then((e) => {
				this.$message.success(e.data);
				if (this.isCopy) {
					var pKey = this.portal.P_Key;
					this.$emit('finishCopy', pKey);
					let that = this;
					var timerId = setInterval(function() {
						if (pKey == that.name && !that.isCopy) {
							that.loadConfig();
							clearInterval(timerId);
						}
					}, 500);
				} else {
					this.loadConfig();
				}
			});
		}
	}
}
</script>