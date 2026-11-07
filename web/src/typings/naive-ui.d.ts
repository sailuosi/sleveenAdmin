/** Local stubs formerly backed by naive-ui; kept for unused Soybean table helpers. */
declare namespace NaiveUI {
  type ThemeColor = 'default' | 'error' | 'primary' | 'info' | 'success' | 'warning';
  type Align = 'stretch' | 'baseline' | 'start' | 'end' | 'center' | 'flex-end' | 'flex-start';

  type DataTableBaseColumn<T> = {
    key?: keyof T | (string & {});
    title?: string;
    width?: number | string;
    minWidth?: number | string;
    type?: string;
    fixed?: TableColumnFixed;
  };

  type DataTableExpandColumn<T> = DataTableBaseColumn<T> & { type: 'expand' };
  type DataTableSelectionColumn<T> = DataTableBaseColumn<T> & { type: 'selection' };
  type TableColumnGroup<T> = DataTableBaseColumn<T> & { children?: TableColumn<T>[] };
  type TableColumnCheck = import('@sa/hooks').TableColumnCheck;
  type TableColumnFixed = import('@sa/hooks').TableColumnCheck['fixed'];

  type SetTableColumnKey<C, T> = Omit<C, 'key'> & { key: keyof T | (string & {}) };

  type TableColumnWithKey<T> = SetTableColumnKey<DataTableBaseColumn<T>, T> | SetTableColumnKey<TableColumnGroup<T>, T>;

  type TableColumn<T> = TableColumnWithKey<T> | DataTableSelectionColumn<T> | DataTableExpandColumn<T>;

  type TableOperateType = 'add' | 'edit';
}
