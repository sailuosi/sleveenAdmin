/** 大卡片菜单分组（用于瀑布流列分配） */
export interface MegaMenuSectionLike {
  links: readonly unknown[];
  /** 是否显示分组标题，默认 true */
  isGroup?: boolean;
  groups?: readonly MegaMenuSectionLike[];
}

/** 根据分组数量决定列数 */
export function getMegaMenuColumnCount(sectionCount: number): number {
  if (sectionCount <= 1) return 1;
  if (sectionCount <= 2) return 1;
  if (sectionCount === 3) return 2;
  if (sectionCount >= 4) return 4;
  return 2;
}

/** 估算分组占用高度权重（链接数 + 标题 + 嵌套分组） */
function estimateSectionWeight(section: MegaMenuSectionLike): number {
  const hasTitle = section.isGroup !== false;
  const titleWeight = hasTitle ? 2.5 : 0;
  const groupTitleWeight = (section.groups?.length ?? 0) * 1.8;
  const linkCount = countMegaSectionLinks(section);
  return titleWeight + groupTitleWeight + linkCount * 1.15 + 0.8;
}

function countMegaSectionLinks(section: MegaMenuSectionLike): number {
  const nested = section.groups?.reduce((sum, group) => sum + countMegaSectionLinks(group), 0) ?? 0;
  return section.links.length + nested;
}

/**
 * 甬道式瀑布流分配：每个分组放入当前最短的列，避免 grid 行对齐产生空白
 */
export function distributeMegaMenuMasonry<T extends MegaMenuSectionLike>(
  sections: T[],
  columnCount?: number
): T[][] {
  if (!sections.length) return [];

  const count = columnCount ?? getMegaMenuColumnCount(sections.length);
  if (count <= 1) return [sections];

  const columns: T[][] = Array.from({ length: count }, () => []);
  const heights = Array<number>(count).fill(0);

  for (const section of sections) {
    let targetIdx = 0;
    for (let i = 1; i < count; i++) {
      if (heights[i] < heights[targetIdx]) targetIdx = i;
    }
    columns[targetIdx].push(section);
    heights[targetIdx] += estimateSectionWeight(section);
  }

  return columns.filter(column => column.length > 0);
}
