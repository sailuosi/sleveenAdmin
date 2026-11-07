<template>
    <div v-if="!hideFm">
        <el-form class="markfm" v-loading="loading" :model="workInfo">
            <el-form-item label="流程名称">
                <template #label>
                    <div>
                        <span class="icon" style="margin-right:6px;">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" ><path fill="currentColor" d="M754.752 480H160a32 32 0 1 0 0 64h594.752L521.344 777.344a32 32 0 0 0 45.312 45.312l288-288a32 32 0 0 0 0-45.312l-288-288a32 32 0 1 0-45.312 45.312z"></path></svg>
                        </span>流程名称
                    </div>
                </template>
                <span style="font-size: 16px;font-weight: 600;color: #333;">{{currNode.FlowName}}</span>
            </el-form-item>
            <el-form-item label="当前节点">
                <template #label>
                    <div>
                        <span class="icon" style="margin-right:6px;">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" ><path fill="currentColor" d="M512 896a384 384 0 1 0 0-768 384 384 0 0 0 0 768m0 64a448 448 0 1 1 0-896 448 448 0 0 1 0 896"></path><path fill="currentColor" d="M512 96a32 32 0 0 1 32 32v192a32 32 0 0 1-64 0V128a32 32 0 0 1 32-32m0 576a32 32 0 0 1 32 32v192a32 32 0 1 1-64 0V704a32 32 0 0 1 32-32M96 512a32 32 0 0 1 32-32h192a32 32 0 0 1 0 64H128a32 32 0 0 1-32-32m576 0a32 32 0 0 1 32-32h192a32 32 0 1 1 0 64H704a32 32 0 0 1-32-32"></path></svg>
                        </span>当前节点
                    </div>
                </template>
                {{currNode.Name}}
            </el-form-item>

            <template v-if="sendBack.IsBackTracking==1 && backNormal==false">
                <el-form-item label="退回人">
                    <template #label><span class="fas fa-stamp" style="margin-right:6px;"></span>退回人</template>
                    <div >{{sendBack.ReturnerName}}</div>
                </el-form-item>
                <el-form-item label="退回信息">
                    <template #label><span class="fas fa-bullhorn" style="margin-right:6px;"></span>退回信息</template>
                    <div>{{sendBack.BeiZhu}}</div>
                </el-form-item>
                <el-form-item >
                    <div class="returned-tip">注意：退回人选择了退回后直接发给他，请您直接发送，流程将流转给退回人【{{sendBack.ReturnerName}}】</div>
                    <el-button type="warning" @click="execSend" :loading="sending">发送给退回人</el-button>
                    <el-link type="info" @click="backNormal=true" :disabled="sending" style="margin-left:22px;">正常发送?</el-link>
                </el-form-item>  
            </template>
            <template v-else>
                <el-form-item label="后继活动" v-if="!huiQianing">
                    <template #label>
                        <div>
                            <span class="icon" style="margin-right:6px;">
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" ><path fill="currentColor" d="m249.6 417.088 319.744 43.072 39.168 310.272L845.12 178.88 249.6 417.088zm-129.024 47.168a32 32 0 0 1-7.68-61.44l777.792-311.04a32 32 0 0 1 41.6 41.6l-310.336 775.68a32 32 0 0 1-61.44-7.808L512 516.992l-391.424-52.736z"></path></svg>
                            </span>后继活动
                        </div>
                    </template>
                    <el-radio-group v-model="focusNodeNo" @change="onNodeChange" :disabled="huiQianing" class="toselect">
                        <el-radio v-for="item in toNodes" :label="item.No" :value="item.No">
                            <span class="tonodename" v-if="item.OnOpt=='2'">{{item.ToOption}}</span>
                            <span class="tonodename" v-else-if="item.OnOpt=='3'">{{item.Des}}</span>
                            <span class="tonodename" v-else>{{item.Name}}</span>
                            <span v-if="!!item.Des" class="direct-tip"><i class="fas fa-info"></i>{{item.Des}}</span>
                        </el-radio>
                        <el-radio v-if="endable" label="EndFlow" value="EndFlow">结束</el-radio>
                    </el-radio-group>
                    <el-tag v-if="toNodes.length==0">结束</el-tag>
                </el-form-item>
                <el-form-item label="后续审批人" v-if="!huiQianing && nextConfig.node != void 0 && Array.isArray(nextConfig.teams) && nextConfig.teams.length>0">
                    {{nextCaption}}：
                        <span>
                            <el-tag v-for="(tea,t) in nextConfig.teams" style="margin-right:6px;">{{tea.FK_TeamText}}</el-tag>
                        </span>
                </el-form-item>
                <el-form-item label="办理模式"  v-if="!!curMode">
                    <template #label>
                        <div>
                            <span class="icon" style="margin-right:6px;">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" ><path fill="currentColor" d="M224 704h576V318.336L552.512 115.84a64 64 0 0 0-81.024 0L224 318.336zm0 64v128h576V768zM593.024 66.304l259.2 212.096A32 32 0 0 1 864 303.168V928a32 32 0 0 1-32 32H192a32 32 0 0 1-32-32V303.168a32 32 0 0 1 11.712-24.768l259.2-212.096a128 128 0 0 1 162.112 0"></path><path fill="currentColor" d="M512 448a64 64 0 1 0 0-128 64 64 0 0 0 0 128m0 64a128 128 0 1 1 0-256 128 128 0 0 1 0 256"></path></svg>
                            </span>办理模式
                        </div>
                    </template>
                    <el-popover placement="bottom" :title="curMode.caption" width="500" trigger="click"
                        :content="curMode.tip">
                        <template #reference>
                            <span>
                                <el-link >{{curMode.caption}}</el-link>
                                <i class="hink">{{curMode.hink}}</i>
                            </span>
                        </template>
                    </el-popover>
                </el-form-item>
                <template v-if="isHuiQian ||todoEmps.length>0">
                    <el-form-item label="在办人员">
                        <template #label><span class="fas fa-user-clock" style="margin-right:6px;"></span>在办人员</template>
                        <span>
                            <el-tag v-for="(man,m) in todoEmps" style="margin-right:6px;">{{man.name}}</el-tag>
                        </span>
                    </el-form-item>
                </template>

                <template v-if="!loading && showOpt ">
                    <OpenDialog :judgeOperation="opration" style="min-height:400px;width:100%;" :para="para"
                        :from="currNode" :to="focusNode"
                        :mainData="mainData"
                        :flow="flow" :work="work"
                    ></OpenDialog>
                </template>
                <el-form-item v-if="!appver&& !loading &&  !showOpt">
                    <el-button type="warning" @click="execSend" :loading="sending">执行发送</el-button>
                </el-form-item>           
            </template>
        </el-form>
        <div v-if="appver" class="app-bot-bar">
            <el-button v-if="!loading &&  !showOpt" type="warning" @click="execSend" :loading="sending">执行发送</el-button>
        </div>
    </div>
</template>

<script setup>

</script>
