<template>
    <el-table-column
        :width="width"
        :min-width="minWidth"
        :fixed="fixed"
        :align="myAlign"
        :header-align="headerAlign"
        label-class-name="vccolgroup vcthead nqy"
    >
        <template #header>
            <div class="vchead noquery vchead--group">
                <div class="vccap">{{ cap }}</div>
            </div>
        </template>
        <slot></slot>
    </el-table-column>
</template>
<script>
/**
 * 列表多级表头分组（可选）。仅在使用本组件的 vclist 上出现合并表头，不影响其它列表。
 * 用法：<vccolgroup caption="分组名"><vccol .../><vccol .../></vccolgroup>
 */
export default {
    name: "vccolgroup",
    props: {
        caption: { type: String, default: "" },
        label: { type: String, default: "" },
        align: { type: String, default: "" },
        headerAlign: { type: String, default: "center" },
        width: { type: [String, Number], default: null },
        minWidth: { type: [String, Number], default: null },
        fixed: { type: [String, Boolean], default: null },
    },
    inject: ["vclist"],
    computed: {
        cap() {
            if (this.caption !== "") return this.caption;
            if (this.label !== "") return this.label;
            return "";
        },
        myAlign() {
            if (this.align !== "") return this.align;
            if (this.vclist && this.vclist.align) return this.vclist.align;
            return "center";
        },
    }
};
</script>
