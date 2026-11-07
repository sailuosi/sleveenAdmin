<template>
	<div :class="['fakeinput', inline ? 'inline' : 'mult']">
		<div class="val">
			<slot>
				<el-popover placement="right-end"  :width="200" trigger="hover" :content=modelValue>
					<template #reference>
						<span v-html="modelValue"></span>
					</template>
				</el-popover>
			</slot>
		</div>
		<div class="append">
			<slot name="append"> </slot>
		</div>
	</div>
</template>

<script>
export default {
	name: 'fakeinput',
	props: {
		modelValue: null,
		inline: { type: Boolean, default: true },
	},
	data() {
		return {};
	},

	mounted() {},

	methods: {},
};
</script>

<style scoped>
.fakeinput {
	display: flex;
	flex-direction: row;
	align-items: center;
	margin: 3px 0;
}
.fakeinput > .val {
	flex: 1;
	height: 100%;
	border-color: #e4e7ed;
	border: 1px solid #e4e7ed;
	line-height: 26px;
	padding: 2px 15px;
	border-top-left-radius: 4px;
	border-bottom-left-radius: 4px;
	min-height: 26px;
	word-break: break-all;
	background-color: #fff;
}
.fakeinput > .append {
	flex: 0 0 auto;
	height: 100%;
}

.fakeinput.inline {
	height: 32px;
}
.fakeinput > .append {
	border-color: #e4e7ed;
	border: 1px solid #e4e7ed;
	border-top-right-radius: 4px;
	border-bottom-right-radius: 4px;
}

.fakeinput > .append > :deep(.el-button) {
	vertical-align: baseline;
}
</style>
