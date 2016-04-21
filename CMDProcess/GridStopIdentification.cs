using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class GridStopIdentification
    {

        public int NumofCandiateSTPoint;
        public int NumofClusters;
        public int NumofClustersRemoved;
        private double Timespan = 0;
        private int DIST = 0;
        //private Dictionary<int, string[]> Stationinfo = new Dictionary<int, string[]>();


        public List<CellTra> ClusterAnalysis(List<UserData> SpecialUser, int distance, double timespan)
        {
            List<CellTra> STResult2 = new List<CellTra>();
            Timespan = timespan;
            DIST = distance;
            //Stationinfo = StationInfo;
            int n = SpecialUser.Count();
            int CurrentIndex = -1;

            List<CellTra3> CandiCluster = new List<CellTra3>();
            CellTra3 CurrentCluster = new CellTra3();

            //判断停留点集是否为空
            if (CandiCluster.Count() == 0)
            {
                CandiCluster.Add(new CellTra3(SpecialUser[0]));
                CurrentCluster = CandiCluster.Last();
                CurrentIndex = 0;
            }

            for (int i = 1; i < n; i++)
            {
                if (SpecialUser[i].cellid==CurrentCluster.cellId)
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
                        if (SpecialUser.ElementAt(i).cellid== CandiCluster[p].cellId)
                        {
                            //增加判断时间，跟当前要加入的cluster的outtimeindex比较，如大于一小时，则不加入。
                            if ((SpecialUser[i].timeindex - CandiCluster[p].outtimeindex).TotalHours <= Timespan)
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
                        CandiCluster.Add(new CellTra3(SpecialUser[i]));
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
                STResult2.Add(new CellTra(tt.intimeindex, tt.outtimeindex, tt.userid, tt.cellNum, tt.DatainCluster.Count()));
            }

            return STResult2;
        }

        /// <summary>datetime转换毫秒</summary>
        /// <param name="DateTime1"></param>
        /// <returns>毫秒数</returns>
        private double Datetomilliseconds(DateTime DateTime1)
        {
            DateTime oldTime = new DateTime(1970, 1, 1, 0, 0, 0);
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
            System.DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0);
            time = startTime.AddMilliseconds(d);
            return time;
        }
    }
}
