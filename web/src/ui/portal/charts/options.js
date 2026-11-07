
function Option(){
    this.base= new Object();

    this.title= new Object();
    //系列的配置
    this.series= new Array();

    this.color= new Array();
    //系列的数据
    this.seryData= new Object();
    
    this.tooltip={};
    this.legend= new Object();
    //x轴的配置
    this.xAxis= new Array();
    //y轴的配置
    this.yAxis= new Array();

    this.xAxisData= new Object();
}
Option.prototype.base= new Object();

Option.prototype.title= new Object();
//系列的配置
Option.prototype.series= new Array();

Option.prototype.color= new Array();
//系列的数据
Option.prototype.seryData= new Object();

Option.prototype.tooltip={};
Option.prototype.legend= new Object();
//x轴的配置
Option.prototype.xAxis= new Array();
//y轴的配置
Option.prototype.yAxis= new Array();

Option.prototype.xAxisData= new Object();

Option.prototype.toPlain=function(){
    var res={};
    res.series=this.series.map((sery,s)=>{
        var tar={};
        if(sery instanceof Sery){
            tar= sery.toPlain();
        }
        else{
            tar= Object.assign({},sery);
        }

        var data=this.seryData[s];
        if(Array.isArray(data)){
            tar.data=data;
        }
        return tar;
    })

    res.title = Object.assign({},this.title);
    res.tooltip = Object.assign({},this.tooltip);
    res.legend = Object.assign({},this.legend);
    if(this.xAxis.length>0){

        res.xAxis=this.xAxis.map((x,i)=>{
            
            var tar= Object.assign({},x);

            var xdata=this.xAxisData[i]

            if(Array.isArray(xdata) && xdata.length>0){
                tar.data=xdata;
            } 
            return tar;    
        });
   
    }

    if(this.yAxis.length>0){
        res.yAxis=this.yAxis;
    }

    if(Array.isArray(this.color)&& this.color.length>0){
        res.color= this.color.concat();
    }
    return res;
}


/// 链式方法组

Option.prototype.setSery=function(index,object){

    this.series[index]=object;
    return this;
}
Option.prototype.setSeryData=function(index,list){

    this.seryData[index]=list;
    return this;
}
Option.prototype.setBase=function(val){

    this.base=val;
    return this;
}

Option.prototype.setTitle=function(val){

    this.title=val;
    return this;
}

Option.prototype.setTooltip=function(val){

    this.tooltip=val;
    return this;
}
Option.prototype.setLegend=function(val){

    this.legend=val;
    return this;
}

Option.prototype.setXAxis=function(index,val){
    this.xAxis[index]=val;
    return this;
}

Option.prototype.setYAxis=function(index,val){
    this.yAxis[index]=val;
    return this;
}
Option.prototype.setXAxisData=function(index,val){
    this.xAxisData[index]=val;
    return this;
}

Option.prototype.setColor=function(val){
    this.color=val;
    return this;
}
//// x轴对象

function XAxis(type){
    this.type=type;


}




//// 系列对象
function Sery(type){
    this.type=type;

    this.base=new Object();
    this.emphasis= new Object();
}

Sery.prototype.base=new Object();
Sery.prototype.id=null;
Sery.prototype.name=null;
Sery.prototype.colorBy="data";
Sery.prototype.legendHoverLink=true;
Sery.prototype.emphasis= new Object();

Sery.prototype.toPlain=function(){
    var res= {
        type:this.type,
        id:this.id,
        name:this.name,
        colorBy:this.colorBy,
        legendHoverLink:this.legendHoverLink,
    }

    return Object.assign(res,this.base);
}
Sery.prototype.setBase=function(val){
    this.base=val;
    return this;
}

Sery.prototype.setName=function(val){
    this.name=val;
    return this;
}
Sery.prototype.setEmphasis=function(val){
    this.emphasis=val;
    return this;
}
Sery.prototype.setColorBy=function(val){
    this.colorBy=val;
    return this;
}

//////饼图
function PieSery(){
    
    var superToPlain=this.toPlain;
    this.toPlain=function(){
        var obj=superToPlain.call(this);
        
        //var obj={};
        obj.radius=this.radius;
        //console.log(obj)
        return obj;
    }
}

PieSery.prototype=new Sery("pie");
PieSery.prototype.radius=[0,'75%'];
PieSery.prototype.setRadius=function(val){
    this.radius=val;
    return this;
}

////// 柱状图
function BarSery(){
    
    var superToPlain=this.toPlain;
    this.toPlain=function(){
        var obj=superToPlain.call(this);
        
        //var obj={};
        obj.stack=this.stack;

        return obj;
    }
}

BarSery.prototype=new Sery("bar");
BarSery.prototype.stack="";



BarSery.prototype.setStack=function(val){
    this.stack=val;
    return this;
}






export { Option, PieSery,BarSery }
