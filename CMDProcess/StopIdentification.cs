
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class StopIdentification
    {
        public int NumofCandiateSTPoint;
        public int NumofClusters;
        public int NumofClustersRemoved;
        private double Timespan = 0;
        private int DIST=0;
        private Dictionary<int, string[]> Stationinfo = new Dictionary<int, string[]>();


        public List<CellTra> ClusterAnalysis(List<UserData> SpecialUser,int distance,double timespan, Dictionary<int, string[]> StationInfo)
        {
            List<CellTra> STResult2 = new List<CellTra>();
            Timespan = timespan;
            DIST = distance;
            Stationinfo = StationInfo;
            int m = 0;
            int n = SpecialUser.Count();
            List<CellTra> CTResult = new List<CellTra>();

            //判断停留点集是否为空
            if (CTResult.Count() == 0)
            {
                CellTra currentST = new CellTra(SpecialUser[m].timeindex, SpecialUser[m].timeindex, SpecialUser[m].userid, SpecialUser[m].stationid, 1);
                CTResult.Add(currentST);
            }

            for (int i = 1; i < n; i++)
            {

                    if (SpecialUser.ElementAt(i).stationid != CTResult.ElementAt(CTResult.Count - 1).stationid)
                    {
                        CellTra currentST = new CellTra(SpecialUser[i].timeindex, SpecialUser[i].timeindex, SpecialUser[i].userid, SpecialUser[i].stationid, 1);
                        CTResult.Add(currentST);
                    }
                    else
                    {
                    //修改停留点参数
                    //聚类点时间 -只需要更新离开时间
                        CTResult[CTResult.Count - 1].outtimeindex = SpecialUser[i].timeindex;
                        CTResult[CTResult.Count - 1].NumST++;
                }
            }
            NumofCandiateSTPoint = CTResult.Count();

            STResult2 = STMethod1(CTResult);

            return STResult2;
        }

        private List<CellTra> STMethod1(List<CellTra> CTResult)
        {
            List<CellTra> STResult = new List<CellTra>();

            //得到Cell Trajectory
            for (int p = 0; p < CTResult.Count(); p++)
            {
                //停留聚类为0
                if (STResult.Count() == 0)
                {
                    STResult.Add(CTResult[p]);
                }
                else
                {
                    //比较该点与最近停留点的距离
                    double lon1 = Convert.ToDouble(Stationinfo[CTResult[p].stationid][1]);
                    double lat1 = Convert.ToDouble(Stationinfo[CTResult[p].stationid][0]);
                    double lon2 = Convert.ToDouble(Stationinfo[STResult[STResult.Count() - 1].stationid][1]);
                    double lat2 = Convert.ToDouble(Stationinfo[STResult[STResult.Count() - 1].stationid][0]);
                    if (Discalculate.LonLatTransform(lon1, lat1, lon2, lat2) < DIST)
                    {
                        STResult[STResult.Count() - 1].outtimeindex = CTResult[p].outtimeindex;
                        if(CTResult[p].NumST>STResult[STResult.Count()-1].NumST)
                            STResult[STResult.Count() - 1].stationid = CTResult[p].stationid;
                        STResult[STResult.Count() - 1].NumST += CTResult[p].NumST;
                    }
                    else
                    {
                        STResult.Add(CTResult[p]);
                    }
                }
            }

            NumofClusters = STResult.Count();

            //通过时间计算,移除outtime与intime时间差小于两小时的候选点

            STResult.RemoveAll((T=>(T.outtimeindex-T.intimeindex).TotalHours<Timespan));

            NumofClustersRemoved = STResult.Count();

            return STResult;
        }
    }
}
