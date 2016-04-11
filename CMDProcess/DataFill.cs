using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class DataFill
    {
        static public List<UserData> DataFillMethod(List<UserData> tempdata)
        {
            List<UserData> tempdataOrder = new List<UserData>();
            int userid = tempdata[0].userid;
            int intimeOrigin = tempdata.First().timeindex.Hour;
            int outtimeOrigin = tempdata.Last().timeindex.Hour;
            int index = outtimeOrigin - intimeOrigin;
            DateTime dt;
            //1、先处理头尾缺失数据。
            //intimeOrigin不是从23点开始的情况
            if (intimeOrigin>7&&intimeOrigin!=23)
            {
                //理论上来说不可能存在第一个点大于7，因为小于18个点的都已经剔除。

                int location = tempdata.Last().stationid;
                for (int j = 7; j > -1; j--)
                {
                    dt = new DateTime(2012, 3, 23, j, 30, 0);
                    tempdata.Add(new UserData(dt, userid, location));
                }
                dt = new DateTime(2012, 3, 22, 23, 30, 0);
                tempdata.Add(new UserData(dt, userid, location));
            }
            else if(intimeOrigin<7)
            {

                int location = tempdata.First().stationid;
                for(int j= intimeOrigin-1;j>-1;j--)
                {
                    dt = new DateTime(2012, 3, 23, j, 30, 0);
                    tempdata.Add(new UserData ( dt, userid, location));
                }
                dt = new DateTime(2012, 3, 22, 23, 30, 0);
                tempdata.Add(new UserData(dt, userid, location));
            }
            //排序
            tempdata = tempdata.OrderBy(s => s.timeindex).ToList();

            //outtimeOrigin不是在22点结束
            if (outtimeOrigin > 17)
            {

                int location = tempdata.Last().stationid;
                for (int j = outtimeOrigin+1; j <=22;j++)
                {
                    dt = new DateTime(2012, 3, 23, j, 30, 0);
                    tempdata.Add(new UserData(dt, userid, location));
                }

            }
            else if (outtimeOrigin < 17)
            {

                int location = tempdata.Last().stationid;
                for (int j = outtimeOrigin+ 1; j <=17; j++)
                {
                    dt = new DateTime(2012, 3, 23, j, 30, 0);
                    tempdata.Add(new UserData(dt, userid, location));
                }
                //17点后的数据缺失，认为其回家了。
                location = tempdata.First().stationid;
                for (int j = 18; j <= 22; j++)
                {
                    dt = new DateTime(2012, 3, 23, j, 30, 0);
                    tempdata.Add(new UserData(dt, userid, location));
                }

            }
            //排序
            tempdata =tempdata.OrderBy(s => s.timeindex).ToList();

            for (int i=1;i<tempdata.Count()-1;i++)
            {
                if(tempdata[i+1].timeindex.Hour-tempdata[i].timeindex.Hour>1)
                {
                    dt = new DateTime(2012, 3, 23, tempdata[i].timeindex.Hour+1, 30, 0);
                    int location = tempdata[i].stationid;
                    tempdata.Add(new UserData(dt, userid, location));
                }
            }

            //排序
            tempdata = tempdata.OrderBy(s => s.timeindex).ToList();

            //标准化数据，使每个用户为24个点，时间戳为每半整点。
            for (int i = 0; i < tempdata.Count() - 1; i++)
            {
                if (tempdata[i + 1].timeindex.Hour == tempdata[i].timeindex.Hour)
                {
                    tempdata.RemoveAt(i + 1);
                    dt = new DateTime(2012, 3, 23, tempdata[i].timeindex.Hour, 30, 0);
                    tempdata[i].timeindex = dt;
                }
            }
            //排序
            tempdata = tempdata.OrderBy(s => s.timeindex).ToList();

            if (tempdata.Count() == 24)
                return tempdata;
            else
            {
                Console.WriteLine("FillError");
                return null;
            }


        }
    }
}
