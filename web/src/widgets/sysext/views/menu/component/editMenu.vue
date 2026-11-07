<template>
	<div class="sys-menu-container">
		<el-dialog
			v-model="state.isShowDialog"
			class="sys-edit-menu-dialog"
			draggable
			:close-on-click-modal="false"
			width="700px"
			top="50px"
		>
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto" >
				<el-row :gutter="35">
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="上级菜单">
							<el-cascader
								:options="cascaderOptions"
								:props="cascaderProps"
								placeholder="请选择上级菜单"
								clearable
								filterable
								class="w100"
								v-model="state.ruleForm.Pid"
							>
								<template #default="{ node, data }">
									<span>{{ data.label }}</span>
									<span v-if="!node.isLeaf && data.children?.length"> ({{ data.children.length }}) </span>
								</template>
							</el-cascader>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="菜单类型" prop="Type" :rules="[{ required: true, message: '菜单类型不能为空', trigger: 'blur' }]">
							<el-radio-group v-model="state.ruleForm.Type">
								<el-radio :label="1" :value="1">目录</el-radio>
								<el-radio :label="2" :value="2">菜单</el-radio>
								<el-radio :label="3" :value="3">按钮</el-radio>
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="菜单名称" prop="Title" :rules="[{ required: true, message: '菜单名称不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.Title" placeholder="菜单名称" clearable />
						</el-form-item>
					</el-col>
					<template v-if="state.ruleForm.Type === 1 || state.ruleForm.Type === 2">
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="路由名称" prop="Name" :rules="[{ validator: requiredTrim('路由名称不能为空'), trigger: 'blur' }]">
								<el-input v-model="state.ruleForm.Name" placeholder="路由名称" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="路由路径" prop="Path" :rules="[{ validator: requiredTrim('路由路径不能为空'), trigger: 'blur' }]">
								<el-input v-model="state.ruleForm.Path" placeholder="路由路径" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="菜单编码">
								<el-input v-model="state.ruleForm.MenuCode" placeholder="菜单编码" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="组件路径">
								<el-input type="textarea" v-model="state.ruleForm.Component" placeholder="组件路径" clearable @change="checkWidget" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="菜单图标">
								<el-input v-model="state.ruleForm.Icon" placeholder="菜单图标" clearable @change="checkWidget" />
							</el-form-item>
						</el-col>


						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="菜单排序">
								<el-input-number v-model="state.ruleForm.OrderNo" placeholder="排序" class="w100" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<fminput label="菜单类型" v-model="state.ruleForm.MenuType" placeholder="菜单类型" type="dict" :dictlist="menuTypeList"  />
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="链接地址">
								<el-input v-model="state.ruleForm.OutLink" placeholder="外链/内嵌时链接地址" clearable type="textarea" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="是否隐藏">
								<el-radio-group v-model="state.ruleForm.IsHide">
									<el-radio :label="true" :value="true">隐藏</el-radio>
									<el-radio :label="false" :value="false">不隐藏</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="是否缓存">
								<el-radio-group v-model="state.ruleForm.IsKeepAlive">
									<el-radio :label="true" :value="true">缓存</el-radio>
									<el-radio :label="false" :value="false">不缓存</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="是否固定">
								<el-radio-group v-model="state.ruleForm.IsAffix">
									<el-radio :label="true" :value="true">固定</el-radio>
									<el-radio :label="false" :value="false">不固定</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="是否内嵌">
								<el-radio-group v-model="state.ruleForm.IsIframe">
									<el-radio :label="true" :value="true">内嵌</el-radio>
									<el-radio :label="false" :value="false">不内嵌</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>


					</template>


	
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="是否启用">
							<el-radio-group v-model="state.ruleForm.Status">
								<el-radio :label="1" :value="1">启用</el-radio>
								<el-radio :label="2" :value="2">不启用</el-radio>
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="是否隐藏">
								<el-radio-group v-model="state.ruleForm.IsDelete">
									<el-radio :label="true" :value="true">删除</el-radio>
									<el-radio :label="false" :value="false">不删除</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="是否APP端">
								<el-radio-group v-model="state.ruleForm.IsApp">
									<el-radio :label="true" :value="true">是</el-radio>
									<el-radio :label="false" :value="false">否</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="权限标识">
								<el-input v-model="state.ruleForm.Permission" placeholder="权限标识" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
							<el-form-item label="菜单排序">
								<el-input-number v-model="state.ruleForm.OrderNo" placeholder="排序" class="w100" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="重定向">
								<el-input v-model="state.ruleForm.Redirect" placeholder="重定向地址" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="主题">
								<el-input v-model="state.ruleForm.MenuTheme" placeholder="主题" clearable />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
							<el-form-item label="额外参数">
								<el-input v-model="state.ruleForm.ExtPara" placeholder="额外参数" clearable />
							</el-form-item>
						</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="备注">
							<el-input v-model="state.ruleForm.Remark" placeholder="请输入备注内容" clearable type="textarea" />
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysEditMenu">
import { computed, reactive, ref } from 'vue';

import { apiSysMenuUpdatePost,apiSysMenuAddPost } from '../api/sysmenu.js';

import fminput from "@/ui/common/input/fminput.vue"

const props = defineProps({
	title: String,
	menuData: Array<SysMenu>,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} as UpdateMenuInput,
});

/** 接口 JSON 多为 camelCase（children/title/id），兼容 PascalCase */
function mapCascaderNode(m: any): { value: any; label: string; children?: any[] } {
	const kids = m?.Children ?? m?.children ?? [];
	return {
		value: m?.Id ?? m?.id ?? null,
		label: String(m?.Title ?? m?.title ?? ''),
		children: Array.isArray(kids) && kids.length ? kids.map(mapCascaderNode) : undefined
	};
}

const cascaderOptions = computed(() => [
	{ value: null, label: '顶级（无上级）' },
	...(Array.isArray(props.menuData) ? props.menuData.map(mapCascaderNode) : [])
]);

const cascaderProps = {
	checkStrictly: true,
	emitPath: false,
	value: 'value',
	label: 'label',
	children: 'children'
};

const requiredTrim =
	(message: string) =>
	(_rule: any, value: any, callback: (err?: Error) => void) => {
		const v = typeof value === 'string' ? value.trim() : value;
		if (v === undefined || v === null || v === '') return callback(new Error(message));
		return callback();
	};

const menuTypeList = ref([
	{
		label: 'Vue页面',
		value: '1',
	},
	{
		label: '内嵌页面',
		value: '2',
	},
	{
		label: '网页链接',
		value: '3',
	},
	{
		label: '无界Vue组件',
		value: '4',
	},
]);


// 打开弹窗（编辑时 row 带 Id；新增时 row 为空或默认值，不要 resetFields 否则会清空 Id 导致后端判为“菜单已存在”）
const openDialog = (row: any) => {
	state.ruleForm = row ? JSON.parse(JSON.stringify(row)) : ({} as UpdateMenuInput);
	if (state.ruleForm.IsApp == null) state.ruleForm.IsApp = false;
	state.isShowDialog = true;
	if (!row?.Id) ruleFormRef.value?.resetFields();
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 提交
const submit = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		if (state.ruleForm.Id != undefined && state.ruleForm.Id > 0) {
			await apiSysMenuUpdatePost(state.ruleForm);
		} else {
			await apiSysMenuAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

function checkWidget(e: string){
	console.log(e)
	if(e.startsWith("pcweb\\")){
		e = e.replaceAll("pcweb\\", "");
	}
	if(e.startsWith("web\\")){
		e = e.replaceAll("web\\", "");
	}
	if(e.startsWith("src\\")||e.startsWith("widgets")){
		var widgetName = e.replaceAll("\\", "/");


		//widgetName = widgetName.replaceAll("src/widgets/", "widgets/");
		//widgetName = widgetName.replaceAll("widgets/", "view.");
		//widgetName = widgetName.replaceAll("/", "_");
		//widgetName = widgetName.replaceAll("Page.vue", "");
		state.ruleForm.Component = widgetName;
		if(!state.ruleForm.Name){
			// 获取最后一个/的位置
			const lastSlashIndex = widgetName.lastIndexOf('/');
			
			// 获取文件名部分（包含.vue）
			const filename = lastSlashIndex >= 0 
				? widgetName.substring(lastSlashIndex + 1)
				: widgetName;
			
			// 移除.vue后缀并返回
			var vname= filename.replace(/\.vue$/, '');
			state.ruleForm.Name = vname;
			state.ruleForm.MenuCode=vname;
			state.ruleForm.Path="/"+ vname.replaceAll("_", '/');
		}
	}
}

// 导出对象
defineExpose({ openDialog });
</script>

<style scoped>
.mb20 {
	margin-bottom: 0;
}

:deep(.sys-edit-menu-dialog.el-dialog .el-dialog__body) {
	max-height: 70vh;
	overflow-y: auto;
	overflow-x: hidden;
}
</style>