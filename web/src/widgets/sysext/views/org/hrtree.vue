<template>
<el-tree ref="eltree" :data="rows" :empty-text="emptymsg" :props="treeKey" node-key="UCML_OrganizeOID" 
	:default-expanded-keys="expandedKeys" :default-checked-keys="checked" @node-click="onNodeClick" @check="orgcheck" 
	:show-checkbox="showCheck" :expand-on-click-node="false" :check-strictly="strict">
    <template #default="p">
        <span class="hr-tree-node" >
            <span :class="getPartyIcon(p)"></span>
            <span class="myonode">{{ p.node.label }}</span>
        </span>    
    </template>
</el-tree>
</template>

<script>
import { loadOrgTree } from "@s/api/system/mancommonapi.js"

export default {
    name: 'Hrtree',
    emits: ["nodeclick", "focuschange", "orgcheck"],
    data() {
        return {
            rows: [],
            orgData: {},
            focusRow: {},
            expandedKeys: [],
            emptymsg: "正在加载组织信息...",
            treeKey: {
                children: 'children',
                label: 'OrgName'
            }
        }
    },
    props: {
        checked: {
            type: Array,
        },
        showCheck: {
            type: Boolean,
            default: false
        },
        canfatherClick: {
            type: Boolean,
            default: true
        },
        strict: {
            type: Boolean,
            default: false
        },
        deptOID: {
            type: String,
            default: ""
        },
        //绑定的页面id
        bindPage: null,
        useAuth: {
        	type: Boolean,
        	default: false
        }
    },
    mounted() {
    	this.loadTreeData();
    },
    methods: {
    	loadTreeData() {
    		loadOrgTree({
                bindPage: this.bindPage,
                useAuth: this.useAuth
            }).then(res => {
            	if (Array.isArray(res.data)) {
            		res.data.forEach(data => {
            			data.children = [];
            			this.orgData[data.UCML_OrganizeOID.toUpperCase()] = data;
            		});
            		
            		this.rows = [];
            		res.data.forEach(data => {
            			if (data.ParentOID && this.orgData[data.ParentOID.toUpperCase()]) {
            				//子节点
            				this.orgData[data.ParentOID.toUpperCase()].children.push(data);
            			} else {
            				//没有父节点，则作为根节点
            				this.rows.push(data);
            			}
            		});
            		
            		if (this.deptOID) {
            			let key = this.deptOID.toUpperCase();
            			
            			while (this.orgData[key]) {
            				let data = this.orgData[key];
            				this.expandedKeys.push(key);
            				
            				if (data.ParentOID) {
            					key = data.ParentOID.toUpperCase();
            				} else {
            					break;
            				}
            			}
            		}
            		
            		this.scrollToNode();
            	}
            });
    	},
        scrollToNode() {
            this.$nextTick(() => {
                // 使用 ElementUI Tree 组件提供的 getNode 方法来获取节点对象
                if (this.$refs.eltree) {
                	const node = this.$refs.eltree.getNode(this.deptOID);
	                if (node) {
	                   var id = node.id;
	                    // 获取节点的 DOM 元素
	                    var nodeEl = this.$refs.eltree.$el.querySelector(`.el-tree-node[data-key="${this.deptOID}"]`);
	                  // var nodeEl = document.getElementById(id);
	                    if (nodeEl) {
	                        // 滚动到节点
	                        nodeEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
	                    }
	                }
                }
            });
        },
        onNodeClick(tar, node, me) {
            if (this.canfatherClick == false) {
            	return;
            }
            
            if (!this.focusRow || this.focusRow.UCML_OrganizeOID != tar.UCML_OrganizeOID) {
            	this.focusRow = tar;
            	this.$emit("focuschange", tar);
            }
            this.$emit("nodeclick", tar, node, me);
        },
        getPartyIcon(p) {
            return "partyname fa fa-sitemap partyic" + p.data.Varchar7 + " partlev" + p.data.treeDeep;
        },
        orgcheck(e, n) {
            this.$emit("orgcheck", e, n);
        }
    }
};
</script>

<style scoped>
.partyname {

    font-size: 14px;
}
.hr-tree-node{
    font-size:12px;
}
.is-current>div {
    /*border: 2px solid #185C37;*/
    border-radius: 5px;
    background-color: #CCE8CF;
    font-weight: bold;
}

.el-tree-node__content:hover {
    /*border: 2px solid #185C37;*/
    color: #185C37;
    font-weight: bold;
}

.partlev5 {
    color: #0D86D5;
}

.partlev10 {
    color: #53C1C1;
}

.partlev15 {
    color: #B19C4A;
}

.partlev20 {
    color: #5DB0E8;
}

.partlev25 {
    color: #00B474;
}

.partlev30 {
    color: #B19C4A;
}
</style>
