export function patchTreeData(list){
    var res=[];
    if(Array.isArray(list)){
        list.forEach((item) => {
            //res.push(item);
            var row=item.Record;
            row.children=[];
            if(Array.isArray(item.Children)){
                row.children=patchTreeData(item.Children);
            }
            res.push(row);
        });
    }
    return res;
}

/** 扁平列表(含 parentFK) 转为 patchTreeData 需要的 { Record, Children } 结构 */
export function flatToTreeNodes(items, idKey, parentKey, labelKey) {
    idKey = idKey || 'PX_View_TrainYearSumoryOID';
    parentKey = parentKey || 'parentFK';
    labelKey = labelKey || 'TS_KMName';
    if (!Array.isArray(items) || items.length === 0) return [];
    const map = {};
    items.forEach((it) => {
        const rec = { ...it, Caption: it[labelKey] || '' };
        map[it[idKey]] = { Record: rec, Children: [] };
    });
    const roots = [];
    items.forEach((it) => {
        const node = map[it[idKey]];
        const pid = it[parentKey];
        if (pid == null || pid === '' || !map[pid]) {
            roots.push(node);
        } else {
            if (!map[pid].Children) map[pid].Children = [];
            map[pid].Children.push(node);
        }
    });
    return roots;
}
