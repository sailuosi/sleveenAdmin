<template>
<div>
     相关人员：
    <el-checkbox-group v-model="checked">
        
        <el-checkbox label="role1man" >牵头人</el-checkbox>
        <el-checkbox label="role2man" >参与人</el-checkbox>
        <el-checkbox label="role3man" >责任领导</el-checkbox>
        <el-checkbox label="createman" >创建人</el-checkbox>
    </el-checkbox-group>
    责任部门：
    <el-checkbox-group v-model="checked">
        
        <el-checkbox label="org1Boss1" >行政领导</el-checkbox>
        <el-checkbox label="org1Boss2" >党务领导</el-checkbox>
        <el-checkbox label="org1Lv2Boss" >副职</el-checkbox>
        <el-checkbox label="org1MainPost" >主管人员</el-checkbox>
        <el-checkbox label="org1BelongMan" >本部门人员</el-checkbox>
        <el-checkbox label="org1BelongAdmin" >本部门管理员</el-checkbox>
        <el-checkbox label="org1DeptAdmin" >单位管理员</el-checkbox>
    </el-checkbox-group>
    参与部门：
    <el-checkbox-group v-model="checked">
        
        <el-checkbox label="org2Boss1" >行政领导</el-checkbox>
        <el-checkbox label="org2Boss2" >党务领导</el-checkbox>
        <el-checkbox label="org2Lv2Boss" >副职</el-checkbox>
        <el-checkbox label="org2MainPost" >主管人员</el-checkbox>
        <el-checkbox label="org2BelongMan" >本部门人员</el-checkbox>
        <el-checkbox label="org2BelongAdmin" >本部门管理员</el-checkbox>
        <el-checkbox label="org2DeptAdmin" >单位管理员</el-checkbox>
    </el-checkbox-group>
</div>
</template>

<script setup>
import { reactive, computed, nextTick, ref, onMounted } from 'vue';

const checked = defineModel()

</script>
