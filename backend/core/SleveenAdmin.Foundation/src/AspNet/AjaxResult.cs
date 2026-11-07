// 从 minehub HHNY.NET.Core.AjaxResult 抄入，用于返回 Newtonsoft JObject。
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SleveenAdmin.Foundation;

public class AjaxResult
{
    #region JSON的接口结果输出
    public static JObject wrapStatus(string status, Object data)
    {
        var res = new JObject();
        res["status"] = status;
        res["data"] = (JToken)data;
        return res;
    }
    public static JObject success(string msg, JObject dataObj)
    {
        var res = new JObject();

        res["status"] = "success";
        res["info"] = msg;

        if (dataObj == null) { dataObj = new JObject(); }
        res["data"] = dataObj;
        return res;
    }
    public static JObject success<T>(List<T> rows, int total)
    {
        var res = new JObject();

        res["status"] = "success";
        res["total"] = total;
        JArray ar = (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(rows))!;
        res["data"] = ar;
        return res;
    }

    public static JObject error(string msg, JObject dataObj)
    {
        var res = new JObject();

        res["status"] = "error";
        res["info"] = msg;

        if (dataObj == null) { dataObj = new JObject(); }
        res["data"] = dataObj;
        return res;
    }
    public static JObject error(string msg)
    {
        var res = new JObject();

        res["status"] = "error";
        res["info"] = msg;

        return res;
    }

    public static JObject success(string msg, DataTable dataObj)
    {
        var res = new JObject();

        res["status"] = "success";
        res["info"] = msg;
        JArray ar = (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataObj))!;
        res["data"] = ar;
        return res;
    }

    public static JObject success(int total, DataTable dataObj)
    {
        var res = new JObject();

        res["status"] = "success";
        res["total"] = total;
        JArray ar = (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataObj))!;
        res["data"] = ar;
        return res;
    }
    public static JObject success(int total, DataTable dataObj, Object dict)
    {
        var res = new JObject();

        res["status"] = "success";
        res["total"] = total;
        JArray ar = (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataObj))!;
        res["data"] = ar;
        res["dict"] = JObject.FromObject(dict);
        return res;
    }
    public static JObject success(int total, JArray arr)
    {
        var res = new JObject();

        res["status"] = "success";
        res["total"] = total;
        res["data"] = arr;
        return res;
    }

    public static JObject success(string msg, string dataObj)
    {
        var res = new JObject();

        res["status"] = "success";
        res["info"] = msg;
        res["data"] = dataObj;
        return res;
    }
    public static JObject success(DataTable dataObj)
    {
        var res = new JObject();

        res["status"] = "success";

        JArray ar = (JArray)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataObj))!;
        res["data"] = ar;
        return res;
    }

    public static JObject success(string dataObj)
    {
        var res = new JObject();

        res["status"] = "success";

        res["data"] = dataObj;
        return res;
    }
    public static JObject success(JArray dataObj)
    {
        var res = new JObject();

        res["status"] = "success";

        res["data"] = dataObj;
        return res;
    }

    public static JObject success(JObject dataObj)
    {
        var res = new JObject();

        res["status"] = "success";

        res["data"] = dataObj;
        return res;
    }

    public static JObject success(DataRow row)
    {
        var res = new JObject();

        res["status"] = "success";

        res["data"] = RowToJobj(row);
        return res;
    }
    public static JObject success(int cc)
    {
        var res = new JObject();

        res["status"] = "success";

        res["data"] = cc;
        return res;
    }
    public static JObject success<T>(IEnumerable<T> data)
    {
        var res = new JObject();

        res["status"] = "success";
        if (data is JToken token)
        {
            res["data"] = token;

        }
        else
        {
            var da = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
            if (da as JArray != null)
            {
                res["data"] = da as JArray;
            }
            else if (da as JObject != null)
            {
                res["data"] = da as JObject;
            }
            else
            {
                res["data"] = (JToken?)da;
            }
        }

        return res;
    }
    public static JObject success<T>(T data)
    {
        var res = new JObject();

        res["status"] = "success";
        if (data != null)
        {
            res["data"] = (JToken?)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
        }

        return res;
    }

    public static JObject success(Object data)
    {
        var res = new JObject();

        res["status"] = "success";
        if (data != null)
        {
            res["data"] = JObject.FromObject(data);
        }

        return res;
    }

    /// <summary>对齐 minehub Json.RowToJobj，避免再抄整份 Json 工具类。</summary>
    static JObject RowToJobj(DataRow? row)
    {
        var jObject = new JObject();
        if (row is null) return jObject;
        foreach (DataColumn column in row.Table.Columns)
        {
            var v = row[column];
            jObject.Add(column.ColumnName, v == DBNull.Value || v is null ? JValue.CreateNull() : JToken.FromObject(v));
        }
        return jObject;
    }

    /// <summary>用 Newtonsoft 写出，避免 System.Text.Json 把 JObject 序列化成空对象。</summary>
    public static ContentResult ToContent(JObject jobj) => new()
    {
        Content = jobj.ToString(Formatting.None),
        ContentType = "application/json"
    };
    #endregion
}
