// 通用函数

import { ElMessage } from 'element-plus';
import { formatDate } from './formatTime';
import { useI18n } from 'vue-i18n';
function toClipboard(text) {
	// 检查浏览器是否支持 Clipboard API
	if (navigator.clipboard && window.isSecureContext) {
	  // 使用 Clipboard API
	  navigator.clipboard.writeText(text).then(() => {
		console.log('文本已复制到剪贴板');
	  }).catch(err => {
		console.error('复制失败: ', err);
	  });
	} else {
	  // 降级处理：使用 document.execCommand
	  const textArea = document.createElement('textarea');
	  textArea.value = text;
	  textArea.style.position = 'fixed';
	  textArea.style.left = '-999999px';
	  textArea.style.top = '-999999px';
	  document.body.appendChild(textArea);
	  textArea.select();
	  try {
		const successful = document.execCommand('copy');
		if (successful) {
		  console.log('文本已复制到剪贴板');
		} else {
		  console.error('复制失败');
		}
	  } catch (err) {
		console.error('复制出错: ', err);
	  }
	  document.body.removeChild(textArea);
	}
  }
export default function () {
	const { t } = useI18n();


	// 百分比格式化
	const percentFormat = (row: EmptyArrayType, column: number, cellValue: string) => {
		return cellValue ? `${cellValue}%` : '-';
	};
	// 列表日期时间格式化
	const dateFormatYMD = (row: EmptyArrayType, column: number, cellValue: string) => {
		if (!cellValue) return '-';
		return formatDate(new Date(cellValue), 'YYYY-mm-dd');
	};
	// 列表日期时间格式化
	const dateFormatYMDHMS = (row: EmptyArrayType, column: number, cellValue: string) => {
		if (!cellValue) return '-';
		return formatDate(new Date(cellValue), 'YYYY-mm-dd HH:MM:SS');
	};
	// 列表日期时间格式化
	const dateFormatHMS = (row: EmptyArrayType, column: number, cellValue: string) => {
		if (!cellValue) return '-';
		let time = 0;
		if (typeof row === 'number') time = row;
		if (typeof cellValue === 'number') time = cellValue;
		return formatDate(new Date(time * 1000), 'HH:MM:SS');
	};
	// 小数格式化
	const scaleFormat = (value: string = '0', scale: number = 4) => {
		return Number.parseFloat(value).toFixed(scale);
	};
	// 小数格式化
	const scale2Format = (value: string = '0') => {
		return Number.parseFloat(value).toFixed(2);
	};
	// 点击复制文本
	const copyText = (text: string) => {
		return new Promise((resolve, reject) => {
			try {
				//复制
				toClipboard(text);
				//下面可以设置复制成功的提示框等操作
				ElMessage.success(t('message.layout.copyTextSuccess'));
				resolve(text);
			} catch (e) {
				//复制失败
				ElMessage.error(t('message.layout.copyTextError'));
				reject(e);
			}
		});
	};
	// 去掉Html标签(取前面5个字符)
	const removeHtmlSub = (value: string) => {
		var str = value.replace(/<[^>]+>/g, '');
		if (str.length > 50) return str.substring(0, 50) + '......';
		else return str;
	};
	// 去掉Html标签
	const removeHtml = (value: string) => {
		return value.replace(/<[^>]+>/g, '');
	};
	// 获取枚举描述
	const getEnumDesc = (key: any, lstEnum: any) => {
		return lstEnum.find((x: any) => x.value == key)?.describe;
	};
	return {
		percentFormat,
		dateFormatYMD,
		dateFormatYMDHMS,
		dateFormatHMS,
		scaleFormat,
		scale2Format,
		copyText,
		removeHtmlSub,
		removeHtml,
		getEnumDesc,
	};
}
