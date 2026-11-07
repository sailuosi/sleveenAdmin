

export function setCondition(conditions, row) {
    var has=false;
    var arr=conditions.value;
    for (var i = 0; i < arr.length; i++) {
        var c = arr[i];
        if (c.field == row.field) {
            arr[i] = row;
            has = true;
            break;
        }
    }
    if (!has) {
        arr.push(row);
    }
    conditions.value=arr.concat();
}