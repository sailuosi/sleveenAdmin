import fs from 'fs';

const files = [
  'src/layouts/modules/theme-drawer/modules/layout/modules/content-settings.vue',
  'src/layouts/modules/theme-drawer/modules/layout/modules/tab-settings.vue',
  'src/layouts/modules/theme-drawer/modules/general/modules/watermark-settings.vue'
];

for (const file of files) {
  let text = fs.readFileSync(file, 'utf8');

  // Convert <ElSelect ... :options="EXPR" ... /> self-closing or with attrs
  text = text.replace(
    /<ElSelect\b([^>]*?):options="([^"]+)"([^>]*)\/>/g,
    (_m, pre, opts, post) => {
      return `<ElSelect${pre}${post}>\n        <ElOption v-for="opt in ${opts}" :key="opt.value" :label="opt.label" :value="opt.value" />\n      </ElSelect>`;
    }
  );

  text = text
    .replace(/:value="/g, ':model-value="')
    .replace(/@update:value="/g, '@update:model-value="');

  fs.writeFileSync(file, text);
  console.log('fixed', file);
}
