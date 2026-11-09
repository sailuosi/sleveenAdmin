import type { Command, CommandContext } from './types.js';

interface StackEntry {
  command: Command;
  context: CommandContext;
}

/**
 * Linear undo/redo stack. All modeling mutations must go through execute().
 */
export class CommandStack {
  private stack: StackEntry[] = [];
  private index = -1;
  private listeners = new Set<() => void>();

  canUndo(): boolean {
    return this.index >= 0;
  }

  canRedo(): boolean {
    return this.index < this.stack.length - 1;
  }

  execute(command: Command, context: CommandContext): void {
    // Drop redo branch
    this.stack = this.stack.slice(0, this.index + 1);
    command.execute(context);
    this.stack.push({ command, context });
    this.index = this.stack.length - 1;
    this.emit();
  }

  undo(): void {
    if (!this.canUndo()) return;
    const entry = this.stack[this.index]!;
    entry.command.revert(entry.context);
    this.index -= 1;
    this.emit();
  }

  redo(): void {
    if (!this.canRedo()) return;
    this.index += 1;
    const entry = this.stack[this.index]!;
    entry.command.execute(entry.context);
    this.emit();
  }

  clear(): void {
    this.stack = [];
    this.index = -1;
    this.emit();
  }

  onChange(listener: () => void): () => void {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  private emit() {
    for (const listener of this.listeners) listener();
  }
}
