
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class HWIdentifiction
    {
        //Tuple<int, int,int> UserHWInfo;

        public HWIdentifiction()
        {
            //UserHWInfo = new Tuple<int, int,int>();
            //AllHomeWork = new HWPIdentify(); 
            // origindata2 = new DataDefinition2[2];
        }


        static public Tuple<int, int, int> HomeWorkIdentify(List<UserData> SpecialUser)
        {
            //List<DataDefinition2> SpecialUser = new List<DataDefinition2>();

            //判断时刻
            //HOME:0AM-7AM 7H
            //WORK:(9AM-5PM) 8H

            //位置排名
            Dictionary<int, int> TowerWorkRank = new Dictionary<int, int>();
            Dictionary<int, int> TowerHomeRank = new Dictionary<int, int>();
            foreach (UserData suser in SpecialUser)
            {
                //在家时间判断0-7AM
                if (suser.timeindex.Hour < 7)
                //if((suser.timeindex>=43&&suser.timeindex<=47)||(suser.timeindex>=0&&suser.timeindex<=11))
                {
                    //
                    if (!TowerHomeRank.ContainsKey(suser.cellid))
                    {
                        TowerHomeRank.Add(suser.cellid, 1);
                        continue;
                    }
                    else
                    {
                        TowerHomeRank[suser.cellid]++;
                        continue;
                    }
                }

                //工作时间判断
                if ((suser.timeindex.Hour >= 9 && suser.timeindex.Hour < 17))
                //if((suser.timeindex>=17&&suser.timeindex<=23)||(suser.timeindex>=29&&suser.timeindex<=33))
                {

                    if (!TowerWorkRank.ContainsKey(suser.cellid))
                    {
                        TowerWorkRank.Add(suser.cellid, 1);
                        continue;
                    }
                    else
                    {
                        TowerWorkRank[suser.cellid]++;
                        continue;
                    }
                }

            }

            //排序
            var DescendingsortedWork = TowerWorkRank.OrderByDescending(s => s.Value);
            var DescendingsortedHome = TowerHomeRank.OrderByDescending(s => s.Value);

            int Work = 0, Home = 0;
            if (DescendingsortedWork.Count() == 0)
                Work = 0;
            else
            {
                //某一地点在工作的比例要大于50%，否则判断为没有找到工作地点
                double perc = (double)DescendingsortedWork.ElementAt(0).Value / DescendingsortedWork.Sum(s => s.Value);
                if (perc > 0.5)
                {
                    Work = DescendingsortedWork.ElementAt(0).Key;
                    //AllHomeWork.SetTowerAttrib(Work, "work");
                }
                else
                    Work = 0;

            }

            if (DescendingsortedHome.Count() == 0)
                Home = 0;
            else
            {
                //某一地点在家的比例要大于50%，否则判断为没有找到家的地点
                double perc = (double)DescendingsortedHome.ElementAt(0).Value / DescendingsortedHome.Sum(s => s.Value);
                if (perc > 0.5)
                {
                    Home = DescendingsortedHome.ElementAt(0).Key;
                    //AllHomeWork.SetTowerAttrib(Home, "home");
                }
                else
                    Home = 0;
            }
            //将最大值存入(若没有，则为0,并返回
            return new Tuple<int, int, int>(SpecialUser.ElementAt(0).userid, Home, Work);
        }

        static public Tuple<int, int, int> HomeWorkIdentify(List<CellTra> CTResult, bool index)
        {
            //判断时刻
            //HOME:23PM-7AM 8H
            //WORK:(9AM-5PM) 8H
            Dictionary<int, double> WorkStationRank = new Dictionary<int, double>();
            Dictionary<int, double> HomeStationRank = new Dictionary<int, double>();

            DateTime t1 = new DateTime(2012, 3, 22, 23, 00, 00);
            DateTime t2 = new DateTime(2012, 3, 23, 00, 00, 00);
            DateTime t3 = new DateTime(2012, 3, 23, 7, 00, 00);

            //Home时间窗口重叠率，得到最大的
            foreach (CellTra tt in CTResult)
            {
                double RateofOverlap = 0;
                try
                {
                    //outtimeindex在2012，3，23 06:00:00之前
                    if ((tt.outtimeindex - t3).Ticks < 0)
                    {
                        //换算成小时数除以家的时间窗口7小时，得到重叠率
                        RateofOverlap = (tt.outtimeindex - tt.intimeindex).TotalHours / 7;
                        continue;
                    }
                    else
                    {
                        if ((tt.intimeindex - t3).Ticks <= 0)
                            RateofOverlap = (t3 - tt.intimeindex).TotalHours / 7;
                        continue;
                    }
                }
                finally
                {
                    if (!HomeStationRank.ContainsKey(tt.cellid))
                    {
                        HomeStationRank.Add(tt.cellid, RateofOverlap);
                    }
                    else
                    {
                        HomeStationRank[tt.cellid] += RateofOverlap; ;
                    }
                }
            }

            DateTime t4 = new DateTime(2012, 3, 23, 9, 00, 00);
            DateTime t7 = new DateTime(2012, 3, 23, 17, 00, 00);

            //Work时间窗口重叠率，得到最大的
            foreach (CellTra tt in CTResult)
            {
                double RateofOverlap = 0;
                try
                {
                    if ((tt.intimeindex - t4).Ticks < 0)
                    {

                        if ((tt.outtimeindex - t7).Ticks <= 0&&(tt.outtimeindex- t4).Ticks>=0)
                        {
                            RateofOverlap = ((tt.outtimeindex - t4).TotalHours) / 8;
                            continue;
                        }
                        else if ((tt.outtimeindex - t7).Ticks > 0)
                        {
                            RateofOverlap = 1.0;
                            continue;
                        }
                    }
                    else if ((tt.intimeindex - t4).Ticks > 0&&(tt.intimeindex- t7).Ticks<0)
                    {

                        if ((tt.outtimeindex - t7).Ticks <= 0)
                        {
                            //换算成小时数除以家的时间窗口8小时，得到重叠率
                            RateofOverlap = (tt.outtimeindex - tt.intimeindex).TotalHours / 8;
                            continue;
                        }
                        else if ((tt.outtimeindex - t7).Ticks > 0)
                        {
                            RateofOverlap = (t7 - tt.intimeindex).TotalHours / 8;
                            continue;
                        }
                    }
                }
                finally
                {
                    if (!WorkStationRank.ContainsKey(tt.cellid))
                    {
                        WorkStationRank.Add(tt.cellid, RateofOverlap);
                    }
                    else
                    {
                        WorkStationRank[tt.cellid] += RateofOverlap; ;
                    }
                }
            }

            //排序
            var DescendingsortedWork = WorkStationRank.OrderByDescending(s => s.Value);
            var DescendingsortedHome = HomeStationRank.OrderByDescending(s => s.Value);
            int Work = 0, Home = 0;

            //有大于2个唯一活动点的情况
            if (index == false)
            {

                if (DescendingsortedWork.Count() == 0)
                    Work = 0;
                else
                {
                    //某一地点在工作的比例要大于50%，否则判断为没有找到工作地点
                    double perc = (double)DescendingsortedWork.ElementAt(0).Value / DescendingsortedWork.Sum(s => s.Value);
                    if (perc > 0.5)
                    {
                        Work = DescendingsortedWork.ElementAt(0).Key;
                        //AllHomeWork.SetTowerAttrib(Work, "work");
                    }
                    else
                        Work = 0;

                }

                if (DescendingsortedHome.Count() == 0)
                    Home = 0;
                else
                {
                    //某一地点在家的比例要大于50%，否则判断为没有找到家的地点
                    double perc = (double)DescendingsortedHome.ElementAt(0).Value / DescendingsortedHome.Sum(s => s.Value);
                    if (perc > 0.5)
                    {
                        Home = DescendingsortedHome.ElementAt(0).Key;
                        //AllHomeWork.SetTowerAttrib(Home, "home");
                    }
                    else
                        Home = 0;
                }
            }
            //只有2个唯一活动点的情况。
            if (index == true)
            {
                Work = DescendingsortedWork.ElementAt(0).Key;
                Home = DescendingsortedHome.ElementAt(0).Key;
                //不允许home和work相同。
                //if (Work == Home)
                //{
                //    Console.WriteLine("homeworkdetectionError!");
                //    if (DescendingsortedWork.ElementAt(0).Value > DescendingsortedHome.ElementAt(0).Value)
                //        Home = DescendingsortedHome.ElementAt(1).Key;
                //}
            }


            return new Tuple<int, int, int>(CTResult[0].userid, Home, Work);
        }

        public class HWPIdentify : HWPIdenti
        {
            private List<UserHWP> Users;  //顶点数目
            private List<TowerAreaAttrib> TowerAreas;    //边的数目


            //构造器
            public HWPIdentify()
            {
                Users = new List<UserHWP>();
                TowerAreas = new List<TowerAreaAttrib>();
            }

            //获取索引为index的顶点的信息
            public UserHWP GetUser(int index)
            {
                return Users[index];
            }

            public TowerAreaAttrib GetTower(int index)
            {
                return TowerAreas[index];
            }

            //设置索引为index的顶点信息
            public void SetUser(UserHWP v)
            {
                Users.Add(v);
            }
            public int GetNumofUsers()
            {
                return Users.Count();
            }

            public int GetNumofTowers()
            {
                return TowerAreas.Count();
            }

            public void SetTowerAttrib(int TowerIndex, string attrib)
            {

                if (TowerAreas.Exists(x => x.TowerIndex == TowerIndex))
                {
                    int TIndex = TowerAreas.FindIndex(x => x.TowerIndex == TowerIndex);

                    //int FindIndex=TowerAreas.FindIndex(x => x.Attrib.ContainsKey(attrib));
                    if (TowerAreas[TIndex].Attrib.ContainsKey(attrib))
                    {

                        TowerAreas[TIndex].Attrib[attrib] = Convert.ToInt32(TowerAreas[TIndex].Attrib[attrib]) + 1;
                    }
                    else
                    {
                        TowerAreas[TIndex].Attrib.Add(attrib, 1);
                    }
                }
                else
                {
                    TowerAreaAttrib temp = new TowerAreaAttrib(TowerIndex);
                    temp.Attrib.Add(attrib, 1);
                    TowerAreas.Add(temp);
                }

            }


            public bool IsTowerHasAttrib(int TowerIndex, string attrib)
            {
                //遍历顶点数组
                foreach (TowerAreaAttrib nd in TowerAreas)
                {
                    if (nd.TowerIndex.Equals(TowerIndex))
                    {
                        if (nd.Attrib.ContainsKey(attrib))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

        }

        public class UserHWP
        {

            private string uid;
            private int homeIndex;
            private int workIndex;

            public UserHWP(string t1, int homeIndex, int workIndex)
            {
                uid = t1;
                this.homeIndex = homeIndex;
                this.workIndex = workIndex;
            }

            public string Uid
            {
                get { return uid; }
                set { uid = value; }
            }
            public int HomeIndex
            {
                get { return homeIndex; }
                set { homeIndex = value; }
            }
            public int WorkIndex
            {
                get { return workIndex; }
                set { workIndex = value; }
            }

        }

        public class TowerAreaAttrib
        {
            int towerIndex;
            IDictionary<string, Object> attrib = new Dictionary<string, object>();
            //IDictionary<string, Object> workAttrib = new Dictionary<string, object>();

            public TowerAreaAttrib(int towerIndex)
            {
                this.towerIndex = towerIndex;
            }

            public int TowerIndex
            {
                get { return towerIndex; }
                set { towerIndex = value; }
            }
            public IDictionary<string, Object> Attrib
            {
                get { return attrib; }
                set { attrib = value; }
            }

        }

        public interface HWPIdenti
        {

            void SetTowerAttrib(int TowerIndex, string attrib);
            //void SetWorkLoc(UserHWP user, TowerAreaAttrib tower);
            //void DelEdge(Node<T> v1, Node<T> v2);

            bool IsTowerHasAttrib(int TowerIndex, string attrib);
        }
    }
}
