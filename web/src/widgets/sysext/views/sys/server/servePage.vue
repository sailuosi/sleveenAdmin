<template>
	<div class="sys-server-container">
		<el-row :gutter="8">
			<el-col :md="12" :sm="24">
				<el-card shadow="hover" header="系统信息">
					<table class="sysInfo_table">
						<tr>
							<td class="sysInfo_td">主机名称：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.HostName }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">操作系统：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.SystemOs }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">系统架构：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.OsArchitecture }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">CPU核数：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.ProcessorCount }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">运行时长：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.SysRunTime }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">外网地址：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.RemoteIp }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">内网地址：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.LocalIp }}</td>
						</tr>
						<tr>
							<td class="sysInfo_td">运行框架：</td>
							<td class="sysInfo_td">{{ state.machineBaseInfo.FrameworkDescription }}</td>
						</tr>
					</table>
				</el-card>
			</el-col>
			<el-col :md="12" :sm="24">
				<el-card shadow="hover" header="使用信息">
					<el-row>
						<el-col :xs="12" :sm="12" :md="12" :lg="12" :xl="12" style="text-align: center">
							<el-progress type="dashboard"
								:percentage="parseInt(state.machineUseInfo.RamRate == undefined ? 0 : state.machineUseInfo.RamRate.substr(0, state.machineUseInfo.RamRate.length - 1))"
								:color="'var(--el-color-primary)'">
								<template #default>
									<span>{{ state.machineUseInfo.RamRate }}<br /></span>
									<span style="font-size: 10px">
										已用:{{ state.machineUseInfo.UsedRam }}<br />
										剩余:{{ state.machineUseInfo.FreeRam }}<br />
										内存使用率
									</span>
								</template>
							</el-progress>
						</el-col>
						<el-col :xs="12" :sm="12" :md="12" :lg="12" :xl="12" style="text-align: center">
							<el-progress type="dashboard"
								:percentage="parseInt(state.machineUseInfo.CpuRate == undefined ? 0 : state.machineUseInfo.CpuRate.substr(0, state.machineUseInfo.CpuRate.length - 1))"
								:color="'var(--el-color-primary)'">
								<template #default>
									<span>{{ state.machineUseInfo.CpuRate }}<br /></span>
									<span style="font-size: 10px"> CPU使用率 </span>
								</template>
							</el-progress>
						</el-col>
					</el-row>

					<el-row>
						<table class="sysInfo_table">
							<tr>
								<td class="sysInfo_td">启动时间：</td>
								<td class="sysInfo_td">{{ state.machineUseInfo.StartTime }}</td>
							</tr>
							<tr>
								<td class="sysInfo_td">运行时长：</td>
								<td class="sysInfo_td">{{ state.machineUseInfo.RunTime }}</td>
							</tr>
							<tr>
								<td class="sysInfo_td">网站目录：</td>
								<td class="sysInfo_td">{{ state.machineBaseInfo.Wwwroot }}</td>
							</tr>
							<tr>
								<td class="sysInfo_td">开发环境：</td>
								<td class="sysInfo_td">{{ state.machineBaseInfo.Environment }}</td>
							</tr>
							<tr>
								<td class="sysInfo_td">环境变量：</td>
								<td class="sysInfo_td">{{ state.machineBaseInfo.Stage }}</td>
							</tr>
						</table>
					</el-row>
				</el-card>
			</el-col>
		</el-row>

		<el-row :gutter="8">
			<el-col :md="24" :sm="24">
				<el-card shadow="hover" header="程序集信息" style="margin-top: 8px; --el-card-padding: 10px">
					<div v-for="d in state.assemblyInfo" :key="d.name"
						style="display: inline-block; margin: 4px;text-align: left;">
						<el-tag round>
							<div style="display: inline-flex">
								<div style="">{{ d.Name }}</div>
								<div style="color: black;  font-size: 9px; margin-left: 3px">v{{ d.Version }}</div>
							</div>
						</el-tag>
					</div>
				</el-card>
			</el-col>
		</el-row>

		<el-row :gutter="8">
			<el-col :md="24" :sm="24">
				<el-card shadow="hover" header="磁盘信息" style="margin-top: 8px">
					<el-row>
						<el-col :span="4" :xs="24 / state.machineDiskInfo.length * 2"
							:sm="24 / state.machineDiskInfo.length" :md="24 / state.machineDiskInfo.length"
							:lg="24 / state.machineDiskInfo.length" :xl="24 / state.machineDiskInfo.length"
							v-for="d in state.machineDiskInfo" :key="d.DiskName" style="text-align: center">
							<el-progress type="circle" :percentage="d.AvailablePercent" :color="'var(--el-color-primary)'">
								<template #default>
									<span>{{ d.AvailablePercent }}%<br /></span>
									<span style="font-size: 10px">
										已用:{{ d.Used }}GB<br />
										剩余:{{ d.AvailableFreeSpace }}GB<br />
										{{ d.DiskName }}
									</span>
								</template>
							</el-progress>
						</el-col>
					</el-row>
				</el-card>
			</el-col>
		</el-row>
	</div>
</template>

<script lang="ts" setup name="sysServer">
import { onActivated, onDeactivated, onMounted, reactive } from 'vue';

import { apiSysServerServerBaseGet,apiSysServerServerUsedGet,apiSysServerServerDiskGet,apiSysServerAssemblyListGet } from '@s/api/system/sysserveapi.js';

 
const state = reactive({
	machineBaseInfo: [] as any,
	machineUseInfo: [] as any,
	machineDiskInfo: [] as any,
	assemblyInfo: [] as any,
	timer: null as any,
});

onMounted(async () => {
	loadMachineBaseInfo();
	loadMachineUseInfo();
	loadMachineDiskInfo();
	loadAssemblyInfo();
});

// 服务器配置信息
const loadMachineBaseInfo = async () => {
	var res = await apiSysServerServerBaseGet();
	state.machineBaseInfo = res;
};

// 服务器内存信息
const loadMachineUseInfo = async () => {
	var res = await apiSysServerServerUsedGet();
	state.machineUseInfo = res;
};

// 服务器磁盘信息
const loadMachineDiskInfo = async () => {
	var res = await apiSysServerServerDiskGet();
	state.machineDiskInfo = res;
};

// 框架程序集信息
const loadAssemblyInfo = async () => {
	var res = await apiSysServerAssemblyListGet();
	state.assemblyInfo = res;
};

// 实时刷新内存
const refreshData = () => {
	loadMachineUseInfo();
};

onActivated(() => {
	state.timer = setInterval(() => {
		refreshData();
	}, 10000);
});

onDeactivated(() => {
	clearInterval(state.timer);
});
</script>

<style lang="scss" scoped>
.sysInfo_table {
	width: 100%;
	min-height: 40px;
	line-height: 40px;
	text-align: center;
}

.sysInfo_td {
	border-bottom: 1px solid #e8e8e8;
}
</style>
