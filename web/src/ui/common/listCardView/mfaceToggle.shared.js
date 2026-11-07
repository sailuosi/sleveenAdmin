import { computed, nextTick, unref } from "vue";

/** vclist face：表格 */
export const VIEW_TABLE = "table";
/** vclist face：卡片 */
export const VIEW_CARD = "card";

/** 卡片模式时工具栏容器 class（右对齐、透明底） */
export const CARD_TOOLBAR_CLASS = "vc-top-toolbar--card-actions";

export function isCardView(face) {
    return unref(face) === VIEW_CARD;
}

export function isTableView(face) {
    return unref(face) === VIEW_TABLE;
}

/** 在卡片 / 表格之间切换，返回切换后的 face */
export function toggleView(mface) {
    const next = isCardView(mface) ? VIEW_TABLE : VIEW_CARD;
    mface.value = next;
    return next;
}

/**
 * 切换视图后的通用回调（切到卡片时 scrollToTop）
 * @param {{ cardViewRef?: import('vue').Ref, onToggled?: (face: string) => void }} options
 */
export function useOnViewToggled({ cardViewRef, onToggled } = {}) {
    function handleViewToggled(face) {
        onToggled?.(face);
        if (!isCardView(face)) {
            return;
        }
        nextTick(() => {
            cardViewRef?.value?.scrollToTop?.();
        });
    }
    return { onViewToggled: handleViewToggled };
}

/**
 * 卡片模式下 Teleport 工具栏到卡片页头
 */
export function useCardToolbarTeleport({ mface, cardViewRef, position }) {
    const toolbarTeleportTo = computed(() => {
        if (isCardView(mface)) {
            return cardViewRef.value?.toolbarAnchor ?? null;
        }
        return unref(position) ?? null;
    });
    const toolbarTeleportDisabled = computed(() => {
        if (isCardView(mface)) {
            return !cardViewRef.value?.toolbarAnchor;
        }
        return !unref(position);
    });
    return { toolbarTeleportTo, toolbarTeleportDisabled };
}
