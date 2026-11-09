import type { Command, CommandContext, DiagramSnapshot, Point } from '../types.js';
import {
  buildCut,
  buildDuplicate,
  buildPaste,
  copySelection,
  getClipboard,
  setClipboard,
  type ClipboardPayload,
} from '../clipboard.js';

export function createCopyCommand(): Command {
  return {
    name: 'elements.copy',
    execute(ctx: CommandContext) {
      copySelection(ctx.store.getState());
    },
    revert() {},
  };
}

export function createCutCommand(): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;
  let previousClipboard: ClipboardPayload | null = null;
  let cutClipboard: ClipboardPayload | null = null;

  return {
    name: 'elements.cut',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        previousClipboard = getClipboard();
        const built = buildCut(before);
        cutClipboard = built.clipboard;
        setClipboard(cutClipboard);
        after = structuredClone(built.state);
      } else {
        setClipboard(cutClipboard);
      }
      ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
      setClipboard(previousClipboard);
    },
  };
}

export function createPasteCommand(offset: Point = { x: 40, y: 40 }): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'elements.paste',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const clip = getClipboard();
        if (!clip) return;
        after = structuredClone(buildPaste(before, clip, offset).state);
      }
      if (after) ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}

export function createDuplicateCommand(offset: Point = { x: 40, y: 40 }): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'elements.duplicate',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const next = buildDuplicate(before, offset);
        if (!next) return;
        after = structuredClone(next);
      }
      if (after) ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
