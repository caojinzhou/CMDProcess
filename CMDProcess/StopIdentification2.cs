
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class StopIdentification2
    {
        public int NumofCandiateSTPoint;
        public int NumofClusters;
        public int NumofClustersRemoved;
        private double Timespan = 0;
        private int DIST = 0;
        private Dictionary<int, string[]> Stationinfo = new Dictionary<int, string[]>();


        public List<CellTra> ClusterAnalysis(List<UserData> SpecialUser, int distance, double timespan, Dictionary<int, string[]> StationInfo)
        {
            List<CellTra> STResult2 = new List<CellTra>();
            Timespan = timespan;
            DIST = distance;
            Stationinfo = StationInfo;
            int n = SpecialUser.Count();
            int CurrentIndex = -1;

            List<CellTra2> CandiCluster = new List<CellTra2>();
            CellTra2 CurrentCluster = new CellTra2();

            //判断停留点集是否为空
            if (CandiCluster.Count() == 0)
            {
                CandiCluster.Add(new CellTra2(SpecialUser[0]));
                CurrentCluster = CandiCluster.Last();
                CurrentIndex = 0;
            }

            for (int i = 1; i < n; i++)
            {
                if (DistanceCalculate(SpecialUser[i], CurrentCluster) < DIST)
                {
                    CandiCluster[CurrentIndex].PutinCluster(SpecialUser[i]);
                    CurrentCluster = CandiCluster[CurrentIndex];
                }
                else
                {
                    CurrentCluster = null;
                    CurrentIndex = -1;

                    for (int p = 0; p < CandiCluster.Count(); p++)
                    {
                        if (DistanceCalculate(SpecialUser.ElementAt(i), CandiCluster[p]) < DIST)
                        {
                            //增加判断时间，跟当前要加入的cluster的outtimeindex比较，如大于一小时，则不加入。
                            if ((SpecialUser[i].timeindex - CandiCluster[p].outtimeindex).TotalHours <= 1)
                            {
                                CandiCluster[p].PutinCluster(SpecialUser[i]);
                                CurrentCluster = CandiCluster[p];
                                CurrentIndex = p;
                                break;
                            }
                        }
                    }
                    if (CurrentIndex == -1)
                    {
                        CandiCluster.Add(new CellTra2(SpecialUser[i]));
                        CurrentCluster = CandiCluster.Last();
                        CurrentIndex = CandiCluster.Count() - 1;
                    }
                }
            }

            NumofClusters = CandiCluster.Count();
            //通过时间计算,移除outtime与intime时间差小于1小时的候选点。小于1小时即表示有2个点存在。
            CandiCluster.RemoveAll((T => (T.outtimeindex - T.intimeindex).TotalHours < Timespan));
            NumofClustersRemoved = CandiCluster.Count();
            NumofCandiateSTPoint = 0;
            
            //更改结果类
            foreach (var tt in CandiCluster)
            {
                STResult2.Add(new CellTra(tt.intimeindex, tt.outtimeindex, tt.userid, tt.stationNum.ElementAt(0).Key, tt.DatainCluster.Count()));
            }

            return STResult2;
        }

        private double DistanceCalculate(UserData currentPoint, CellTra2 currentS)
        {
            double Distance = 0;
            for (int i = 0; i < currentS.DatainCluster.Count(); i++)
            {
                double RealDistance;
                if (currentPoint.stationid == currentS.DatainCluster[i].stationid)
                {
                    RealDistance = 0;
                    continue;
                }
                else
                {
                    double lon1 = Convert.ToDouble(Stationinfo[currentPoint.stationid][1]);
                    double lat1 = Convert.ToDouble(Stationinfo[currentPoint.stationid][0]);
                    double lon2 = Convert.ToDouble(Stationinfo[currentS.DatainCluster[i].stationid][1]);
                    double lat2 = Convert.ToDouble(Stationinfo[currentS.DatainCluster[i].stationid][0]);
                    RealDistance = Discalculate.LonLatTransform(lon1, lat1, lon2, lat2);
                }

                double RealTimeHour = (currentPoint.timeindex - currentS.DatainCluster[i].timeindex).TotalHours;
                double UpperDistance = 40 * RealTimeHour * 1000;//换算成米

                if (RealDistance < UpperDistance)
                    Distance += RealDistance;
                else
                    Distance += UpperDistance;
            }
            return Distance /= currentS.DatainCluster.Count();
        }

        //时间扩充函数
        private List<CellTra2> STMethod1(List<CellTra2> CTResult)
        {
            //List<CellTra> STResult = new List<CellTra>();

            //得到Cell Trajectory
            for (int p = 0; p < CTResult.Count(); p++)
            {
                CTResult[p].stationNum.OrderByDescending(s => s.Value);

                double lon1 = Convert.ToDouble(Stationinfo[CTResult[p].stationNum.ElementAt(0).Key][1]);
                double lat1 = Convert.ToDouble(Stationinfo[CTResult[p].stationNum.ElementAt(0).Key][0]);

                if (p != 0)
                {

                    double lon2 = Convert.ToDouble(Stationinfo[CTResult[p - 1].stationNum.ElementAt(0).Key][1]);
                    double lat2 = Convert.ToDouble(Stationinfo[CTResult[p - 1].stationNum.ElementAt(0).Key][0]);
                    double RealDistance = Discalculate.LonLatTransform(lon1, lat1, lon2, lat2);
                    double time = RealDistance / 40/ 1000*3600000;//单位：毫秒

                    double milliseconds = (Datetomilliseconds(CTResult[p].intimeindex) + Datetomilliseconds(CTResult[p - 1].outtimeindex) + time)/2;
                    CTResult[p].intimeindex = ConvertIntDateTime(milliseconds);
                }
                else if(p!= CTResult.Count()-1)
                {
                    CTResult[p + 1].stationNum.OrderByDescending(s => s.Value);
                    double lon3 = Convert.ToDouble(Stationinfo[CTResult[p + 1].stationNum.ElementAt(0).Key][1]);
                    double lat3 = Convert.ToDouble(Stationinfo[CTResult[p + 1].stationNum.ElementAt(0).Key][0]);

                    double RealDistance2 = Discalculate.LonLatTransform(lon1, lat1, lon3, lat3);
                    double time2 = RealDistance2 /40 / 1000 *3600000;//单位：毫秒

                    double milliseconds2 = (Datetomilliseconds(CTResult[p].outtimeindex) + Datetomilliseconds(CTResult[p + 1].intimeindex) + time2) / 2;
                    CTResult[p].outtimeindex = ConvertIntDateTime(milliseconds2);
                }
            }

            return CTResult;
        }


        /// <summary>datetime转换毫秒</summary>
        /// <param name="DateTime1"></param>
        /// <returns>毫秒数</returns>
        private double Datetomilliseconds(DateTime DateTime1)
        {
            DateTime oldTime = new DateTime(1970, 1, 1,0,0,0);
            TimeSpan span = DateTime1.Subtract(oldTime);

            double milliSecondsTime = span.TotalMilliseconds;
            return milliSecondsTime;
        }

        /// <summary>
        /// 将Unix时间戳转换为DateTime类型时间
        /// </summary>
        /// <param name="d">double 型数字</param>
        /// <returns>DateTime</returns>
        private static System.DateTime ConvertIntDateTime(double d)
        {
            System.DateTime time = System.DateTime.MinValue;
            System.DateTime startTime = new System.DateTime(1970, 1, 1,0,0,0);
            time = startTime.AddMilliseconds(d);
            return time;
        }
    }
}
