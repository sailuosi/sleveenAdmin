

export function formatDate(datee, fmt) {
    var res = datee;
    try {
        var da = datee;

        if (typeof datee == "string") {
            da = new Date(datee);
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


export function addWeek(datee, week) {
    var da = datee;
    if (typeof datee == "string") {
        da = new Date(datee);
    }
    var timestamp = da.getTime();

    var oneWeekAgoTimestamp = timestamp + week * 7 * 24 * 60 * 60 * 1000;

    return new Date(oneWeekAgoTimestamp);
}

export function addDay(datee, days) {
    var da = datee;
    if (typeof datee == "string") {
        da = new Date(datee);
    }
    var timestamp = da.getTime();

    var oneWeekAgoTimestamp = timestamp + days * 24 * 60 * 60 * 1000;

    return new Date(oneWeekAgoTimestamp);
}

export function getMonday(initdate){
    var datee;
    if(initdate == void 0 || initdate==null){
        datee= new Date();
    }
    else{
        datee= new Date(initdate);
    }

    let dayOfWeek = datee.getDay(); // 获取当前日期是星期几（0是周日，1是周一...）

    // 计算本周一的日期（如果今天是周日，则dayOfWeek为0，需要特殊处理）
    let diffToMonday = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;

    datee.setDate(datee.getDate() + diffToMonday);
    var monday= datee;
    let mondayStr = `${monday.getFullYear()}-${monday.getMonth() + 1}-${monday.getDate()}`;
    return new Date(mondayStr);
}

export function getSunday(initdate){
    var t=getMonday(initdate);
    return addDay(t,6);
}
// a > b时返回 true
export function compareDate(a,b){
    var dateA = a;
    if (typeof a == "string") {
        dateA = new Date(a);
    }
    var dateB = b;
    if (typeof b == "string") {
        dateB = new Date(b);
    }

    var av=dateA.getTime();
    var bv=dateB.getTime();
    if(av==bv){
        return 0;
    }
    if(av>bv){
        return 1;
    }
    return -1;
}

/**
 * 获取给定日期所在月份的总天数
 */
export function getMonthDays(datee) {
	let dateStr = formatDate(datee, "YYYY-MM");
	let year = dateStr.substring(0, 4) * 1;
	let month = dateStr.substring(5, 7) * 1;
	if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) {
		return 31;
	}
	if (month == 4 || month == 6 || month == 9 || month == 11) {
		return 30;
	}
	if (month == 2) {
		if (isLeapYear(year)) {
			return 29;
		}
		return 28;
	}
	return 0;
}

/**
 * 获取给定日期所在月份在日历中占几周（一行算一周）
 */
export function getMonthWeekRows(datee, firstDayOfWeek) {
	if (!firstDayOfWeek) {
		firstDayOfWeek = 1;//默认日历是按照周一开始的
	}
	
	let firstDayOfMonth = new Date(formatDate(datee, "YYYY-MM") + "-01").getDay();
	let firstDay = firstDayOfMonth - firstDayOfWeek;
	if (firstDay < 0) {
		firstDay += 7;
	}
	
	let monthdaysDiff = getMonthDays(datee) - 28;
	
	let weekDiff = firstDay + monthdaysDiff;
	if (weekDiff <= 0) {
		return 4;
	} else if (weekDiff <= 7) {
		return 5;
	} else {
		return 6;
	}
}

export function isLeapYear(year) {
	if (year % 400 == 0) {
		return true;
	}
	if (year % 100 == 0) {
		return false;
	}
	return year % 4 == 0;
}


export function getTimeDiff(startTime, endTime) {
    var start = new Date(startTime);
    var end = new Date(endTime);
   
    var diff = end.getTime() - start.getTime(); // 返回时间差的毫秒数
   
    // 转换为天、小时、分钟和秒
    var days = Math.floor(diff / (1000 * 60 * 60 * 24));
    var hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    var minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    var seconds = Math.floor((diff % (1000 * 60)) / 1000);
   
    return days + "天 " + hours + "小时 " + minutes + "分钟 " + seconds + "秒";
  }