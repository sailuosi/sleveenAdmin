import fs from 'fs';
import path from 'path';

const roots = [
  'src/layouts/modules/theme-drawer',
  'src/layouts/modules/global-search',
  'src/layouts/modules/global-breadcrumb',
  'src/layouts/modules/global-header',
  'src/layouts/modules/global-tab',
  'src/components/common',
  'src/components/custom',
  'src/components/advanced',
  'src/views/home/modules',
  'src/widgets/sysext/views/sys/notice'
];

const tagMap = {
  NBadge: 'ElBadge',
  NBreadcrumb: 'ElBreadcrumb',
  NBreadcrumbItem: 'ElBreadcrumbItem',
  NButton: 'ElButton',
  NCard: 'ElCard',
  NCheckbox: 'ElCheckbox',
  NColorPicker: 'ElColorPicker',
  NDivider: 'ElDivider',
  NDrawer: 'ElDrawer',
  NDrawerContent: 'div',
  NDropdown: 'ElDropdown',
  NEmpty: 'ElEmpty',
  NForm: 'ElForm',
  NFormItem: 'ElFormItem',
  NGradientText: 'span',
  NInput: 'ElInput',
  NInputGroup: 'div',
  NInputNumber: 'ElInputNumber',
  NModal: 'ElDialog',
  NPopover: 'ElPopover',
  NPopconfirm: 'ElPopconfirm',
  NScrollbar: 'ElScrollbar',
  NSelect: 'ElSelect',
  NSpace: 'div',
  NStatistic: 'ElStatistic',
  NSwitch: 'ElSwitch',
  NTab: 'ElTabPane',
  NTabs: 'ElTabs',
  NThing: 'div',
  NTooltip: 'ElTooltip',
  NList: 'div',
  NListItem: 'div',
  NGrid: 'div',
  NGi: 'div',
  NAlert: 'ElAlert'
};

function walk(dir, out = []) {
  if (!fs.existsSync(dir)) return out;
  for (const name of fs.readdirSync(dir)) {
    const p = path.join(dir, name);
    const st = fs.statSync(p);
    if (st.isDirectory()) walk(p, out);
    else if (name.endsWith('.vue') || name.endsWith('.ts')) out.push(p);
  }
  return out;
}

let count = 0;
for (const root of roots) {
  for (const file of walk(root)) {
    let text = fs.readFileSync(file, 'utf8');
    const orig = text;

    for (const [from, to] of Object.entries(tagMap)) {
      text = text.replace(new RegExp(`<${from}(?=[\\s/>])`, 'g'), `<${to}`);
      text = text.replace(new RegExp(`</${from}>`, 'g'), `</${to}>`);
    }

    text = text
      .replace(/\bv-model:value=/g, 'v-model=')
      .replace(/\bv-model:show=/g, 'v-model=')
      .replace(/\bpath="/g, 'prop="')
      .replace(/\bshow-password-on="click"/g, 'show-password')
      .replace(/\bquaternary\b/g, 'text')
      .replace(/\b:bordered="false"/g, 'shadow="never"')
      .replace(/\bsize="tiny"/g, 'size="small"')
      .replace(/from ['"]naive-ui['"]/g, "from 'element-plus'");

    if (text !== orig) {
      fs.writeFileSync(file, text);
      count += 1;
      console.log('updated', file);
    }
  }
}
console.log('total', count);
