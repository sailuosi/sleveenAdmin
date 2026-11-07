<template>
<div style="display:grid;" class="grist" :style="{'grid-template-columns':widthstr}">
    <div :class="['g-cell','row'+li.y1,'col'+li.x1]" v-for="(li,i) in rendlist"  :style="li.style"
        @click="oncellclick"
    >
        <slot :item="li" :name="li.field" :value="li.value">{{li.value}}</slot>
    </div>
</div>
</template>

<script>

import {styleStrToObj} from "@/utils/style.js"


function listToGrid(list,cols,spanEmpty){
    //如果需要将一个转换为grid模式进行绘制。需要把挨着的相同元素都合并为一个元素，并标注其“体积”，即行、列跨度。
    //此时的数据不再具备行结构，变成了单纯的一维数据，每个数据表示一个单元格。每个单元格记录自己的起始的行与列号。

    //先获取列信息,
    if(spanEmpty== void 0||spanEmpty==null){
        spanEmpty=false;
    }
    var res= [];
    cols.forEach((col,c)=>{
        res.push(
            {
                type:'head',
                value:col.caption,
                y1:1,//第0行
                y2:2,//第1边线
                x1:c+1,
                x2:c+2,
            }
        )


    })
    var startnum=2;
    //开始检查相同列值，执行合并录入
    cols.forEach((col,c)=>{
        //开始循环行数据。
        var preval=null;
        var preByVal=null;
        var samecount=0;
        var first=0;
        //是否合并的值依据，不设置时，使用列的自身值
        var mergeBy=col.mergeBy;
        if(mergeBy==void 0|| mergeBy==null){
            mergeBy = col.field;
        }
        // 检查分组配置，当存在分组时，合并的范围不能超过分组的边界。
        var groupFields=[];
        var groupVals=[];
        if(Array.isArray(col.group)){
            groupFields=col.group;
        }

        for(var j=0;j<list.length;j++){
            var row=list[j];
            var val=row[col.field];
            var byval=row[mergeBy]
            //不需要合并的列，每个值占据一个cell
            if(col.merge===false){
                //styleStrToObj
                res.push(
                    {
                        type:'cell',field:col.field,
                        column:col,
                        value:val,
                        row:row,
                        y1:startnum+j,//第0行
                        y2:startnum+j+1,//第1边线
                        x1:c+1,
                        x2:c+2,
                    }
                )
                continue ;
            }
            //行循环j 为第1行时，计算起始合并依据值。      
            if(j==0){
                preval=list[0][col.field];
                preByVal=list[0][mergeBy];
                if(groupFields.length>0){
                    //计算分组的值
                    groupFields.forEach((group,g)=>{
                        groupVals[g]=list[0][group];
                    })
                }
                samecount=1;
                first=0;
                continue;
            }
            
            

            var emptNo=spanEmpty==false && (byval==void 0||byval==null ||byval=='');
            //值相同
            var doMege=(!emptNo && preByVal==byval);
            if(doMege){
                //当值相同时，需要再依次检查所属分组，当分组值不同时，取消合并。
                if(groupFields.length>0){
                    //计算分组的值
                    for(var z=0;z<groupFields.length;z++){
                        var gname=groupFields[z];
                        var nowGroupVal=row[gname];
                        var preVal=groupVals[z];
                        if(preVal != nowGroupVal){
                            //分组的值发生变化，取消合并；
                            doMege=false;
                            break;
                        }
                    }
                }
            }

            if(doMege){
                //值相同
                samecount++;
            }else{
                //此时区域结束。存储区域信息，并重新初始化；
                res.push(
                    {
                        type:'cell',field:col.field,
                        column:col,
                        value:preval,
                        mergeValue:preByVal,
                        row:list[first],
                        y1:startnum+first,//第0行
                        y2:startnum+first+samecount,//第1边线
                        x1:1+c,
                        x2:1+c+1,
                    }
                )
                //初始化
                
                first=j;
                preval=val;
                preByVal=byval;
                if(groupFields.length>0){
                    //计算分组的值
                    groupFields.forEach((group,g)=>{
                        groupVals[g]=row[group];
                    })
                }

                samecount=1;
            }
            if(j==list.length-1){
                //如果是最后一个。
                res.push(
                    {
                        type:'cell',field:col.field,
                        column:col,
                        value:val,
                        row:list[j],
                        y1:startnum+first,//第0行
                        y2:startnum+first+samecount,//第1边线
                        x1:1+c,
                        x2:1+c+1,
                    }
                )
            }
            
        }

        
    })

    //进行样式整合

    

    var target= res.map((li)=>{
        var style=styleStrToObj(li.column?.style); 
        //{'grid-area':li.y1+'/'+li.x1+'/'+li.y2+'/'+li.x2  }
        style['grid-area']=li.y1+'/'+li.x1+'/'+li.y2+'/'+li.x2;
        li.style=style;
        if(!!li.column?.css){
            li.css=li.column.css;
        }
        return li;
    })

    return target;
}



export default {

/** 使用案例
    <gridlist :list="dlist" :cols="cols" >
        <template #op="p">
            <span class="fa fa-edit iconbtn" @click="editpi(p.item.row)"></span>
        </template>
        <template #G_Name="p">
            <el-link @click="opengset(p.item.row)">{{!p.value?'点击设置':p.value}}</el-link>
        </template>
    </gridlist>	

    cols:[//60px 100px 40px 150px minmax(400px,1fr) auto auto auto
        {field:"op",caption:"操作",merge:false,width:"60px",mergeBy:"",group:[]},
        {field:"PI_Type",caption:"类别",width:"100px"},
        {field:"PI_Num",caption:"序号",width:"40px"},
        {field:"PI_Content",caption:"主要内容",width:"150px"},
        {field:"PI_Specific",caption:"具体考核指标",width:"minmax(300px,1fr)"},
        {field:"PI_DutyOrg",caption:"责任部门",width:"auto"},
        {field:"G_Name",caption:"绑定群组",width:"auto"},
        {field:"PI_Note",caption:"上传资料标准",width:"minmax(auto,370px)"},
    ],
 */


    name: 'Gridlist',
    data(){
        return{
            showform:false,
        };
    },
    props:{
        list:{type:Array,default:function(){return [];}},
        cols:{type:Array,default:function(){return [];}},
        spanEmpty:{type:Boolean,default:false},
    },
    computed:{
        rendlist(){
            return listToGrid(this.list,this.cols,this.spanEmpty);
        },
        widthstr(){
            var widths=this.cols.map((col)=>{
                if(!!col.width){
                    return col.width;
                }
                return "auto";
            })
            return widths.join(" ");
        },
    },
    methods:{
        oncellclick(item){

            this.$emit("cellclick",item);
        },
    },
    mounted(){

    }
};
</script>

<style scoped>
        .g-cell{
            border-right:var(--g-cell-border,1px solid #eee);
            border-bottom:var(--g-cell-border,1px solid #eee);
            display: flex;
            align-items: center;
            padding:2px;
        }
        .g-cell.row1{
            border-top:var(--g-cell-border,1px solid #eee);
        }
        .g-cell.col1{
            border-left:var(--g-cell-border,1px solid #eee);
        }
        .g-cell:hover{
            outline:1px solid #E5B53C;
            background:var(--g-cell-bg-hover, #eef1f5);
        }
</style>