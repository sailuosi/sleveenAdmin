

export function fileSaveLocal(response){
    const data = response.data;
    const status = response.status || 200;
    const contentType = (response.headers['content-type'] || response.headers['Content-Type'] || '').toLowerCase();
    const mimeType = data instanceof Blob ? (data.type || '').toLowerCase() : '';

    // 服务端异常时可能返回 JSON 错误体（即使 responseType 为 blob）
    if (status >= 400 || contentType.includes('json') || mimeType.includes('json')) {
        parseExportErrorBlob(data).then((msg) => {
            if (typeof swal === 'function') {
                swal({ text: msg || '导出失败', icon: 'error', button: '关闭' });
            }
        });
        return;
    }
    if (data instanceof Blob && data.size < 4096) {
        parseExportErrorBlob(data).then((msg) => {
            if (msg) {
                if (typeof swal === 'function') {
                    swal({ text: msg, icon: 'error', button: '关闭' });
                }
                return;
            }
            downloadExportBlob(response, data);
        });
        return;
    }

    downloadExportBlob(response, data);
}

function parseExportErrorBlob(data) {
    return new Promise((resolve) => {
        if (!(data instanceof Blob)) {
            resolve('');
            return;
        }
        const reader = new FileReader();
        reader.onload = () => {
            const text = String(reader.result || '').trim();
            if (!text.startsWith('{')) {
                resolve('');
                return;
            }
            try {
                const json = JSON.parse(text);
                resolve(json.message || json.msg || '导出失败');
            } catch (e) {
                resolve('');
            }
        };
        reader.onerror = () => resolve('');
        reader.readAsText(data);
    });
}

function downloadExportBlob(response, data) {
    // 从响应头提取文件名
    const contentDisposition = response.headers['content-disposition'] || response.headers['Content-Disposition'];
    
    let filename = '导出数据.xlsx';
    if (contentDisposition) {
        // 优先提取 UTF-8 编码的文件名（filename*=UTF-8''xxx）
        let match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (match) {
            try {
                filename = decodeURIComponent(match[1]);
            } catch(e) {
                // 解码失败，使用默认文件名
            }
        } else {
            // 如果没有 UTF-8 编码，尝试提取普通文件名
            match = contentDisposition.match(/filename=["']?([^'";]+)["']?/i);
            if (match) {
                filename = match[1];
                try {
                    // 尝试解码（可能是 URL 编码）
                    filename = decodeURIComponent(filename);
                } catch(e) {
                    // 解码失败，使用原始值
                }
                // 清理可能的引号
                filename = filename.replace(/^["']|["']$/g, '');
            }
        }
    }

    // 创建下载链接
    const blob = data instanceof Blob ? data : new Blob([data]);
    const downloadUrl = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.download = filename;
    document.body.appendChild(link);
    link.click();

    // 清理资源
    setTimeout(() => {
        document.body.removeChild(link);
        window.URL.revokeObjectURL(downloadUrl);
    }, 100);
}

export function checkExportListPara(para,listvm){
    // 如果列表组件存在，提取列名映射（field -> caption）并转换为fields数组
    if(listvm){
        // 尝试多种方式访问 columnMap 和 columns（Vue 3 Options API 的 data 属性）
        let columnMap = listvm.columnMap || listvm.$data?.columnMap || listvm.setupState?.columnMap;
        let columns = listvm.columns || listvm.$data?.columns || listvm.setupState?.columns;
        
        const fields = [];
        
        // 方法1: 从 columnMap 获取列信息（Element Plus Table 的列信息）
        // columnMap 是在 getHeadClassName 方法中填充的，key 是 column.id，value 是 column 对象
        if(columnMap && typeof columnMap === 'object'){
            // 获取所有列，并按顺序排序
            const columnList = [];
            for(const columnId in columnMap){
                const column = columnMap[columnId];
                // Element Plus Table 的列对象可能使用 prop 而不是 property
                const fieldName = column.property || column.prop;
                const fieldLabel = column.label;
                
                if(column && fieldName && fieldLabel){
                    // 排除操作列和序号列
                    if(fieldName !== 'op' && fieldName !== undefined && fieldName !== ''){
                        columnList.push({
                            property: fieldName,
                            label: fieldLabel,
                            no: column.no || 0 // 用于排序
                        });
                    }
                }
            }
            // 按列序号排序
            columnList.sort((a, b) => (a.no || 0) - (b.no || 0));
            // 转换为 fields 数组
            columnList.forEach(col => {
                fields.push({
                    field: col.property,
                    caption: col.label
                });
            });
        }
        
        // 方法2: 如果 columnMap 没有数据，尝试从 columns 数组获取（叶子节点列）
        if(fields.length === 0 && columns && Array.isArray(columns)){
            // 递归获取所有叶子节点列
            function getLeafColumns(cols){
                const result = [];
                for(let i = 0; i < cols.length; i++){
                    const col = cols[i];
                    if(!col) continue; // 跳过稀疏数组中的空项
                    
                    if(col.children && col.children.length > 0){
                        result.push(...getLeafColumns(col.children));
                    } else {
                        const fieldName = col.property || col.prop;
                        const fieldLabel = col.label;
                        if(fieldName && fieldLabel){
                            if(fieldName !== 'op' && fieldName !== undefined && fieldName !== ''){
                                result.push({
                                    field: fieldName,
                                    caption: fieldLabel
                                });
                            }
                        }
                    }
                }
                return result;
            }
            const leafCols = getLeafColumns(columns);
            if(leafCols.length > 0){
                fields.push(...leafCols);
            }
        }
        
        // 方法3: 尝试从 DOM 获取列信息（通过表格表头）
        if(fields.length === 0 && listvm.$el){
            try {
                const table = listvm.$el.querySelector('table.el-table');
                if(table){
                    const headers = table.querySelectorAll('thead th');
                    headers.forEach((th, index) => {
                        const prop = th.getAttribute('data-property') || th.getAttribute('data-column-key');
                        const label = th.textContent?.trim();
                        if(prop && label && prop !== 'op' && prop !== 'selection' && prop !== 'index'){
                            fields.push({
                                field: prop,
                                caption: label
                            });
                        }
                    });
                }
            } catch(e) {
                // 从 DOM 获取失败，忽略错误
            }
        }
        
        // 方法4: 尝试调用 getLeafColumns 方法（如果存在）
        if(fields.length === 0 && typeof listvm.getLeafColumns === 'function'){
            try {
                const leafCols = listvm.getLeafColumns();
                if(leafCols && Array.isArray(leafCols)){
                    leafCols.forEach(col => {
                        const fieldName = col.property || col.prop;
                        const fieldLabel = col.label;
                        if(fieldName && fieldLabel && fieldName !== 'op'){
                            fields.push({
                                field: fieldName,
                                caption: fieldLabel
                            });
                        }
                    });
                }
            } catch(e) {
                // 调用 getLeafColumns 失败，忽略错误
            }
        }
        
        // 若调用方已指定 fields（如特种/安管格式导出），勿用列表全列覆盖
        if (fields.length > 0 && (!Array.isArray(para.fields) || para.fields.length === 0)) {
            para.fields = fields;
        }
    }
    return para;
}