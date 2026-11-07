<template>
	<div class="sys-notice-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="900px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="标题" prop="title" :rules="[{ required: true, message: '标题不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.title" placeholder="标题" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="类型" prop="type" :rules="[{ required: true, message: '类型不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.type" placeholder="类型" filterable allow-create default-first-option style="width: 100%">
								<el-option label="通知" :value="1" />
								<el-option label="公告" :value="2" />
                                <el-option label="消息" :value="3" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="通知方式" prop="type" :rules="[{ required: true, message: '类型不能为空', trigger: 'blur' }]">
							<dicselect  v-model="state.ruleForm.msgWay"
								:dictlist="MsgWays" multi seprator=";"
							>
							</dicselect>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="提示类型" prop="MsgType" >
							<el-input v-model="state.ruleForm.msgType" placeholder="提示类型'success' | 'warning' | 'info' | 'error' | ''" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="提示位置" prop="MsgPosition" >
							<el-input v-model="state.ruleForm.msgPosition" placeholder="提示位置'top-right' | 'top-left' | 'bottom-right' | 'bottom-left'" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="提示时长" prop="MsgDuration" >
							<el-input v-model="state.ruleForm.msgDuration" placeholder="提示时长4500" type="number" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="通知人员">
							<tags :list="noticeMan" name="ManName" class="tagsinfo" :closable="!disabled" @close="removeDutyMan" :addable="!disabled" action="pop" width="430px" :placement="placement">
							<template #addpop="p">
								<manseach ref="comselect" v-model="addingEmp" multiple class="mansh">
									<span class="dialog-footer">
										<el-button type="primary" @click="doDutyManAdd">确 定</el-button>
									</span>
								</manseach>
							</template>
						</tags>
						</el-form-item>
					</el-col>

					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="内容" prop="content" :rules="[{ required: true, message: '内容不能为空', trigger: 'blur' }]">
							<textarea
								height="500" simple
								v-model="state.ruleForm.content"
								/>
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

<script  setup name="sysNoticeEdit">
import { reactive, ref,onMounted ,watch} from 'vue';
//import Editor from '/@/components/editor/index.vue';

import dicselect from "@/ui/common/input/dicselect.vue"
import tags from "@/ui/common/tags.vue"
import manseach from "@s/views/man/manseach.vue"
//import { getAPI } from '@s/utils/axios-utils';
//import { SysNoticeApi } from '@s/api-services/api';
//import { UpdateNoticeInput } from '@s/api-services/models';
import { apiSysNoticeAddPost,apiSysNoticeUpdatePost,loadNoticeMan,addNoticeMan,removeNoticeMan } from "@s/api/system/sysnoticeapi.js"


const props = defineProps({
	title: String,
	adding: { type: Boolean, default: false },
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} 
});

// 打开弹窗
const openDialog = (row) => {
	state.ruleForm = JSON.parse(JSON.stringify(row));
	state.isShowDialog = true;
	ruleFormRef.value?.resetFields();
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

const MsgWays= ref([
	{value:"1",label:"系统内置"},
	{value:"2",label:"手机APP消息"},
	{value:"3",label:"集成平台消息"},
	{value:"4",label:"集成平台待办"}]);

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 提交
const submit = () => {
	ruleFormRef.value.validate(async (valid) => {
		if (!valid) return;
		if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
			await apiSysNoticeUpdatePost(state.ruleForm);
		} else {
			await apiSysNoticeAddPost(state.ruleForm);
		}
		closeDialog();
	});
};


const noticeMan= ref([]);
const doLoadNoticeMan = async () => {
	let res = await loadNoticeMan({
		id: state.ruleForm.id
	});
	noticeMan.value = res.data?? [];
};
const addingEmp = ref([]);
onMounted(() => {
	doLoadNoticeMan();
});
watch(() => state.ruleForm.id, (newVal, oldVal) => {
	doLoadNoticeMan();	
})

function removeDutyMan(row){
	if(props.adding){
		let index = this.dutyMans.findIndex(item => item === row);
		if(index !== -1){
			this.dutyMans.splice(index, 1);
		}
		return ;
	}
	var para={
		id:state.ruleForm.id,
		acc:row.AccountNo,
	}

	removeNoticeMan(para).then((res) => {
		doLoadNoticeMan();
	})

	

}

function doDutyManAdd(){
	//过滤重复添加的人员。
	var addedID=noticeMan.value.map((li)=>li.AccountNo);
	var para=[];
	var count = 0;
	addingEmp.value.forEach((man)=>{
		if(addedID.indexOf(man.CON_EMP_NUM)!=-1){
			count++;
			return;
		}
		para.push({
			NoticeId:state.ruleForm.id,
			ReadStatus:0,
			ManName:man.ManName,
			AccountNo:man.CON_EMP_NUM,
			UserId:man.oid,
		});
	})
	if(props.adding){
		noticeMan.value=this.dutyMans.concat(para);
		return ;
	}
	addNoticeMan(para).then((res)=>{
		//this.seeAddMan=false;
		addingEmp.value=[];
		doLoadNoticeMan();

		//this.$emit("saved");
	})
}

// 导出对象
defineExpose({ openDialog });
</script>
