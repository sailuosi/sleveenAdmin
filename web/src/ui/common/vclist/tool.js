

/**
 * 通用js方法封装处理
 * Copyright (c) 2019 ruoyi
 */


// 日期格式化
export function parseTime(time, pattern) {
  if (arguments.length === 0 || !time) {
    return null
  }
  const format = pattern || '{y}-{m}-{d} {h}:{i}:{s}'
  let date
  if (typeof time === 'object') {
    date = time
  } else {
    if ((typeof time === 'string') && (/^[0-9]+$/.test(time))) {
      time = parseInt(time)
    } else if (typeof time === 'string') {
      time = time.replace(new RegExp(/-/gm), '/').replace('T', ' ').replace(new RegExp(/\.[\d]{3}/gm), '');
    }
    if ((typeof time === 'number') && (time.toString().length === 10)) {
      time = time * 1000
    }
    date = new Date(time)
  }
  const formatObj = {
    y: date.getFullYear(),
    m: date.getMonth() + 1,
    d: date.getDate(),
    h: date.getHours(),
    i: date.getMinutes(),
    s: date.getSeconds(),
    a: date.getDay()
  }
  const time_str = format.replace(/{(y|m|d|h|i|s|a)+}/g, (result, key) => {
    let value = formatObj[key]
    // Note: getDay() returns 0 on Sunday
    if (key === 'a') { return ['日', '一', '二', '三', '四', '五', '六'][value] }
    if (result.length > 0 && value < 10) {
      value = '0' + value
    }
    return value || 0
  })
  return time_str
}
//日期格式第2版本。
export function formatDate(datee, fmt) {
  var res = datee;
  try {
    var da = datee;

    if (typeof datee == "string") {
      da = new Date(datee);
    }
    if (datee != void 0 && typeof datee.toDate == "function") {
        da = datee.toDate();
    }
    if (da instanceof Date && !isNaN(da.getTime())) {
      var o = {
        "M+": da.getMonth() + 1,                 //月份
        "D+": da.getDate(),//日
        "H+": da.getHours(),
        "h+": da.getHours(),                   //小时
        "m+": da.getMinutes(),                 //分
        "s+": da.getSeconds(),                 //秒
        "q+": Math.floor((da.getMonth() + 3) / 3), //季度
        "S": da.getMilliseconds()             //毫秒
      };
      if (/(Y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (da.getFullYear() + "").substr(4 - RegExp.$1.length));
      for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
          fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
      res = fmt;
    }

  }
  catch (e) {
    return datee;
  }
  return res;
}
// 表单重置
export function resetForm(refName) {
  if (this.$refs[refName]) {
    this.$refs[refName].resetFields();
  }
}

// 添加日期范围
export function addDateRange(params, dateRange, propName) {
  let search = params;
  search.params = typeof (search.params) === 'object' && search.params !== null && !Array.isArray(search.params) ? search.params : {};
  dateRange = Array.isArray(dateRange) ? dateRange : [];
  if (typeof (propName) === 'undefined') {
    search.params['beginTime'] = dateRange[0];
    search.params['endTime'] = dateRange[1];
  } else {
    search.params['begin' + propName] = dateRange[0];
    search.params['end' + propName] = dateRange[1];
  }
  return search;
}

// 回显数据字典
export function selectDictLabel(datas, value) {
  var actions = [];
  Object.keys(datas).some((key) => {
    if (datas[key].value == ('' + value)) {
      actions.push(datas[key].label);
      return true;
    }
  })
  return actions.join('');
}

// 回显数据字典（字符串数组）
export function selectDictLabels(datas, value, separator) {
  if(value === undefined) {
    return "";
  }
  var actions = [];
  var currentSeparator = undefined === separator ? "," : separator;
  var temp = value.split(currentSeparator);
  Object.keys(value.split(currentSeparator)).some((val) => {
    Object.keys(datas).some((key) => {
      if (datas[key].value == ('' + temp[val])) {
        actions.push(datas[key].label + currentSeparator);
      }
    })
  })
  return actions.join('').substring(0, actions.join('').length - 1);
}

// 字符串格式化(%s )
export function sprintf(str) {
  var args = arguments, flag = true, i = 1;
  str = str.replace(/%s/g, function () {
    var arg = args[i++];
    if (typeof arg === 'undefined') {
      flag = false;
      return '';
    }
    return arg;
  });
  return flag ? str : '';
}

// 转换字符串，undefined,null等转化为""
export function parseStrEmpty(str) {
  if (!str || str == "undefined" || str == "null") {
    return "";
  }
  return str;
}

// 数据合并
export function mergeRecursive(source, target) {
  for (var p in target) {
    try {
      if (target[p].constructor == Object) {
        source[p] = mergeRecursive(source[p], target[p]);
      } else {
        source[p] = target[p];
      }
    } catch (e) {
      source[p] = target[p];
    }
  }
  return source;
};

/**
 * 构造树型结构数据
 * @param {*} data 数据源
 * @param {*} id id字段 默认 'id'
 * @param {*} parentId 父节点字段 默认 'parentId'
 * @param {*} children 孩子节点字段 默认 'children'
 */
export function handleTree(data, id, parentId, children) {
  if (data === undefined) {
    console.warn(`no data can be handle `);
    return [];
  }
  let config = {
    id: id || 'id',
    parentId: parentId || 'parentId',
    childrenList: children || 'children'
  };

  var childrenListMap = {};
  var nodeIds = {};
  var tree = [];

  for (let d of data) {
    let parentId = d[config.parentId];
    if (childrenListMap[parentId] == null) {
      childrenListMap[parentId] = [];
    }
    nodeIds[d[config.id]] = d;
    childrenListMap[parentId].push(d);
  }

  for (let d of data) {
    let parentId = d[config.parentId];
    if (nodeIds[parentId] == null) {
      tree.push(d);
    }
  }

  for (let t of tree) {
    adaptToChildrenList(t);
  }

  function adaptToChildrenList(o) {
    if (childrenListMap[o[config.id]]) {
      o[config.childrenList] = childrenListMap[o[config.id]];
    }
    if (o[config.childrenList]) {
      for (let c of o[config.childrenList]) {
        adaptToChildrenList(c);
      }
    }
  }
  return tree;
}

/**
* 参数处理
* @param {*} params  参数
*/
export function tansParams(params) {
  let result = ''
  for (const propName of Object.keys(params)) {
    const value = params[propName];
    var part = encodeURIComponent(propName) + "=";
    if (value !== null && typeof (value) !== "undefined") {
      if (typeof value === 'object') {
        for (const key of Object.keys(value)) {
          if (value[key] !== null && typeof (value[key]) !== 'undefined') {
            let params = propName + '[' + key + ']';
            var subPart = encodeURIComponent(params) + "=";
            result += subPart + encodeURIComponent(value[key]) + "&";
          }
        }
      } else {
        result += part + encodeURIComponent(value) + "&";
      }
    }
  }
  return result
}


// 返回项目路径
export function getNormalPath(p) {
  if (p.length === 0 || !p || p == 'undefined') {
    return p
  };
  let res = p.replace('//', '/')
  if (res[res.length - 1] === '/') {
    return res.slice(0, res.length - 1)
  }
  return res;
}

// 验证是否为blob格式
export async function blobValidate(data) {
  try {
    const text = await data.text();
    JSON.parse(text);
    return false;
  } catch (error) {
    return true;
  }
}

// 回显分类字典
export function selectCategoryName(datas, code) {
  var actions = [];
  Object.keys(datas).some((key) => {
    if (datas[key].ccode == ('' + code)) {
      actions.push(datas[key].name);
      return true;
    }
  })
  return actions.join('');
}

// 独立单位代码映射单位名
export function independentCodeFormat(row, deptList) {
  var dept = _.find(deptList, function (o) {
    return o.deptCode === row.independentCode;
  });
  if (dept) {
    return dept.deptName;
  }
}

//查询是否拥有权限

export function hasPermis(value,permissions){
    const all_permission = "*:*:*";


    if (!!value) {

        const hasPermissions = permissions.some(permission => {
            return all_permission === permission || value==permission;
        })
        return hasPermissions;
    } else {
        //错误的权限字符信息，直接为无权限。
        return false;
    }

}

function makeparam(data) {
  let url = ''
  for (var k in data) {
    let value = data[k] !== undefined ? data[k] : ''
    url += '&' + k + '=' + encodeURIComponent(value)
  }
  //去除第一个&
  return url ? url.substring(1) : ''
}

export function makeurl(url,data){
  url += (url.indexOf('?') < 0 ? '?' : '&') + makeparam(data)
  return url;
}


/**
 * 金额数字转千分符字符串
 * @param {Number|String} num - 输入的数字或数字字符串
 * @param {Number} decimalPlaces - 小数位长度，默认为2
 * @returns {String} 格式化后的千分符字符串
 */
export function formatCurrency(num, decimalPlaces) {
  // 默认小数位为2
  decimalPlaces = (typeof decimalPlaces !== 'undefined') ? decimalPlaces : 2;
  
  // 参数校验
  if (num === null || num === undefined || num === '') {
      return '';
  }
  
  // 转换为字符串并去除空格
  var numStr = String(num).replace(/\s/g, '');
  
  // 验证是否为有效数字
  if (!/^[-+]?\d*\.?\d+$/.test(numStr)) {
      return '';
  }
  
  // 转换为数字
  var number = parseFloat(numStr);
  
  // 处理NaN情况
  if (isNaN(number)) {
      return '';
  }
  
  // 保留指定小数位
  var fixedNum = number.toFixed(decimalPlaces);
  
  // 分离整数和小数部分
  var parts = fixedNum.split('.');
  var integerPart = parts[0];
  var decimalPart = parts.length > 1 ? parts[1] : '';
  
  // 处理负数
  var isNegative = integerPart.charAt(0) === '-';
  if (isNegative) {
      integerPart = integerPart.substring(1);
  }
  
  // 添加千分符
  var formattedInteger = '';
  var len = integerPart.length;
  
  for (var i = 0; i < len; i++) {
      formattedInteger += integerPart.charAt(i);
      // 从右往左每三位添加千分符
      if ((len - i - 1) % 3 === 0 && i !== len - 1) {
          formattedInteger += ',';
      }
  }
  
  // 组合结果
  var result = formattedInteger;
  if (decimalPart) {
      result += '.' + decimalPart;
  }
  
  // 添加负号
  if (isNegative) {
      result = '-' + result;
  }
  
  return result;
}

export function isDateQueryType(ftype) {
  return ftype === 'date' || ftype === 'datetime'
}

export function resolveListQueryOp(ftype, logic) {
  if (logic != null && logic !== '') {
    return logic
  }
  if (ftype === 'dict' || ftype === 'bool') {
    return '='
  }
  if (isDateQueryType(ftype)) {
    return 'between'
  }
  return 'like'
}

/** 列头日期区间补全到当天起止，避免结束日 00:00:00 漏掉当天数据 */
export function normalizeListDateRangeValue(value) {
  if (!Array.isArray(value) || value.length < 2) {
    return value
  }
  const start = String(value[0] || '').trim()
  const end = String(value[1] || '').trim()
  if (!start || !end) {
    return value
  }
  const startDay = start.slice(0, 10)
  const endDay = end.slice(0, 10)
  if (!/^\d{4}-\d{2}-\d{2}$/.test(startDay) || !/^\d{4}-\d{2}-\d{2}$/.test(endDay)) {
    return value
  }
  return [startDay + ' 00:00:00', endDay + ' 23:59:59']
}