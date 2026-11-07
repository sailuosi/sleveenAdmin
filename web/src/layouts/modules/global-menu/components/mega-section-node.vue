<script setup lang="ts">
import type { MegaSection } from '../utils/megaMenuSections';
import type { MenuWithMeta } from '../utils/megaMenuSections';
import { canNavigateMenu } from '../utils/megaMenuSections';

defineOptions({
  name: 'MegaSectionNode'
});

interface Props {
  section: MegaSection;
  /** 是否为嵌套子分组（缩小标题样式） */
  nested?: boolean;
  /** 是否允许二级标题跳转（由一级模块白名单控制） */
  allowTitleNavigate?: boolean;
  isLinkActive: (menu: App.Global.Menu) => boolean;
  onLinkClick: (menu: MenuWithMeta) => void;
}

const props = withDefaults(defineProps<Props>(), {
  nested: false,
  allowTitleNavigate: false
});

function canClickTitle() {
  return props.allowTitleNavigate && canNavigateMenu(props.section.titleMenu);
}

function onTitleClick() {
  if (!canClickTitle()) return;
  const menu = props.section.titleMenu as MenuWithMeta | undefined;
  if (!menu) return;
  props.onLinkClick(menu);
}
</script>

<template>
  <div class="mega-section" :class="{ 'mega-section--nested': nested }">
    <div
      v-if="section.isGroup && section.label"
      class="mega-section-title"
      :class="{
        'mega-section-title--nested': nested,
        'mega-section-title--clickable': canClickTitle()
      }"
      :title="canClickTitle() ? `进入${section.label}` : undefined"
      @click.stop="onTitleClick"
    >
      {{ section.label }}
    </div>

    <div v-if="section.links.length" class="mega-links">
      <div
        v-for="link in section.links"
        :key="link.key"
        class="mega-link"
        :class="{
          'mega-link--active': isLinkActive(link),
          'mega-link--disabled': (link as MenuWithMeta).disabled
        }"
        @click.stop="onLinkClick(link as MenuWithMeta)"
      >
        <span class="link-dot" />
        {{ link.label }}
      </div>
    </div>

    <MegaSectionNode
      v-for="group in section.groups"
      :key="group.label + group.links.map(item => item.key).join('-')"
      :section="group"
      nested
      :allow-title-navigate="allowTitleNavigate"
      :is-link-active="isLinkActive"
      :on-link-click="onLinkClick"
    />
  </div>
</template>

<style scoped>
.mega-section--nested {
  margin-top: 4px;
}

.mega-section-title {
  color: #1e3a5f;
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 6px;
  padding: 0 4px 7px;
  border-bottom: 1px solid rgba(232, 240, 248, 0.8);
  letter-spacing: 0.02em;
}

.mega-section-title--nested {
  font-size: 13px;
  font-weight: 600;
  color: #516377;
  margin-bottom: 4px;
  padding: 4px 4px 4px;
  border-bottom: none;
}

.mega-section-title--clickable {
  cursor: pointer;
  transition: color 0.2s ease;
}

.mega-section-title--clickable:hover {
  color: #2f89f5;
}

.mega-links {
  display: flex;
  flex-direction: column;
  gap: 1px;
  padding: 0 4px;
}

.mega-link {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 5px 8px;
  color: #3d526b;
  font-size: 15px;
  border-radius: 6px;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.2s;
}

.mega-link:hover,
.mega-link--active {
  background: rgba(47, 137, 245, 0.07);
  color: #2f89f5;
}

.mega-link--disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.link-dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: #c5d7e8;
  flex-shrink: 0;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}

.mega-link:hover .link-dot,
.mega-link--active .link-dot {
  background: #2f89f5;
  transform: scale(1.35);
}
</style>
