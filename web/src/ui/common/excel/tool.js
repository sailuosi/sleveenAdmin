var colCode=["A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"];

function getCodeByIndex(c){
    //拼接表格坐标AA12
    var colNum="";
    if(c<colCode.length){
        colNum=colCode[c];
    }else{
        //不考虑超过2个字母编码的数据。
        var num1=Math.floor(c/colCode.length);
        var num2=c%colCode.length;
        colNum=colCode[num1]+colCode[num2];
    }    
}

function stringTonum(a) {
    if(!a) return null;
    if(typeof a !="string"){
        a=a+'';
    }
    var str = a.toLowerCase().split("");
    var num = 0;
    var al = str.length;
    var getCharNumber = function (charx) {
        return charx.charCodeAt() - 96;
    };
    var numout = 0;
    var charnum = 0;
    for (var i = 0; i < al; i++) {
        charnum = getCharNumber(str[i]);
        numout += charnum * Math.pow(26, al - i - 1);
    };
    return numout;
}

function numToString(numm) {
    var stringArray = [];
    var numToStringAction = function (nnum) {
        var num = nnum - 1;
        var a = parseInt(num / 26);
        var b = num % 26;
        stringArray.push(String.fromCharCode(64 + parseInt(b + 1)));
        if (a > 0) {
            numToStringAction(a);
        }
    }
    numToStringAction(numm);
    return stringArray.reverse().join("");
}

function getCodeRow(value){
    var num=value.replace(/[a-zA-Z]*/,"");
    return num*1;
}

function getCodeColumn(value){
    var num=value.replace(/[0-9]/g,"");
    return stringTonum(num);
}

function applyStyle(baseStyle,sheet){
    var values=Object.values(sheet)
    values.forEach((val)=>{
        if(val.v!=void 0){
            val.s=baseStyle;
        }
    })
    return sheet;
}
function applyStyleByArea(baseStyle,area,sheet){
    //A1:L6这种格式
    var arearr=area.split(":");
    var start=arearr[0];
    var end=arearr[1];
    var startRow=getCodeRow(start);
    var endRow=getCodeRow(end);
    var startColumn=getCodeColumn(start);
    var endColumn=getCodeColumn(end);

    //循环区域，设置样式
    for(var i=startColumn;i<=endColumn;i++){
        //行循环
        var columncode=numToString(i);
        for(var j=startRow;j<=endRow;j++){
            var areaCode=columncode+j;
            if(sheet[areaCode]==void 0){
                sheet[areaCode]={
                    s:baseStyle,
                    v:"",t:"s"
                }
                continue;
            }
            else{
                //z类型会被直接跳过输出，无法打上样式。
                if(sheet[areaCode].t=='z'){
                    sheet[areaCode].t='s'
                }
                sheet[areaCode].s=Object.assign({},sheet[areaCode].s,baseStyle);
            }
        }
    }

    return sheet;
}

function addRangeBorder(range,ws){
    let arr = ["A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"];

    range.forEach(item=>{
      let startRowNumber = Number(item.s.c),
              endRowNumber = Number(item.e.c);

      for(let i = startRowNumber +1;i<= endRowNumber;i++){
        ws[arr[i]+(Number(item.e.r)+1)]= {s:{border:{top:{style:'thin'}, left:{style:'thin'},bottom:{style:'thin'},right:{style:'thin'}}}};
      }
      //手动补上标题栏的高度
    })

    return ws;
}

// 将一个sheet转成最终的excel文件的blob对象，然后利用URL.createObjectURL下载
export function sheetToBlob(sheet, sheetName) {
    var baseColStyle={
        font: { name: '宋体',sz: 14,bold: false,  },
        alignment: { horizontal: "center", vertical: "center", wrapText: true },
        border:{
            top: { style: "thin" },
            bottom: { style: "thin"  },
            left: { style: "thin", color: { auto: 1 } },
            right: { style: "thin", color: { auto: 1 } }
        }
    };
    applyStyleByArea(baseColStyle,sheet["!ref"],sheet);

    sheetName = sheetName || 'sheet1';
    var workbook = {
        SheetNames: [sheetName],
        Sheets: {}
    };
    workbook.Sheets[sheetName] = sheet; // 生成excel的配置项

    var wopts = {
        bookType: 'xlsx', // 要生成的文件类型
        bookSST: false, // 是否生成Shared String Table，官方解释是，如果开启生成速度会下降，但在低版本IOS设备上有更好的兼容性
        type: 'binary'
    };
    //var wbout = XLSX.write(workbook, wopts);
    var wbout = XLSX.writeFile(workbook, "列表导出结果.xlsx", wopts);
    var blob = new Blob([s2ab(wbout)], {
        type: "application/octet-stream"
    }); // 字符串转ArrayBuffer
    function s2ab(s) {
        var buf = new ArrayBuffer(s.length);
        var view = new Uint8Array(buf);
        for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
        return buf;
    }
    return blob;
}





export function openDownloadDialog(url, saveName) {
    if (typeof url == 'object' && url instanceof Blob) {
        url = URL.createObjectURL(url); // 创建blob地址
    }
    var aLink = document.createElement('a');
    aLink.href = url;
    aLink.download = saveName || ''; // HTML5新增的属性，指定保存文件名，可以不要后缀，注意，file:///模式下不会生效
    var event;
    if (window.MouseEvent) event = new MouseEvent('click');
    else {
        event = document.createEvent('MouseEvents');
        event.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
    }
    aLink.dispatchEvent(event);
}

export function writeSheetColumn(sheet,rows,column){
    //

    return sheet;
}


export function writeSheetColumnsConfig(sheet,rows,columns,config){
    
    //从配置中解析出数据体开始行，标题行开始行、结束行
    var thStart=config.thStart||1;
    var thEnd=config.thEnd||1;

    var bodyStart=config.bodyStart||2;

    var startColumn= config.startColumn||1;
    
    //先列循环，再行循环
    for(var i=0;i<config.columns.length;i++){
        var column=config.columns[i];
        var caption=column.column||"";
        var field=column.field||column.property;
        var no=column.no;
        
        if(typeof no=="string"){
            if(no=="auto"){
                //此时，必须传入field属性，将根据它从当前table的列集合中，获取列位置。
                for(var c=0;c<columns.length;c++){
                    if(columns[c].property==field){
                        no=columns[c].no;
                        break;
                    }
                }
            }else{
                no=stringTonum(no);
            }
        }

        if(no==void 0){
            startColumn++;
            no=startColumn;
        }
        var colCode=numToString(no);

        //绘制标题
        sheet[colCode + thStart]=caption;
        //合并标题
        var mergeObj={//坐标从0开始
            s:{r:thStart-1,c:no-1},//合并开始坐标start
            e:{r:thEnd-1,c:no-1}//合并结束坐标end
        };
        sheet["!merges"].push(mergeObj);

        //绘制数据体部分


        for(var r=0;r<rows.length;r++){
            var row=rows[r];
            var rowindex=bodyStart+r;
            var cellkey=colCode+rowindex;
            
            var value="";
            if(!!field){
                value=row[field];
            }
            else if(typeof column.getValue=="function"){
                value= column.getValue(row,column);
            }
            sheet[cellkey]={
                t:"s",
                v:value
            }
        }
    }
    return sheet;
}