﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class Program
    {

        static void Main(string[] args)
        {
            int Distance = 400;
            double Timespan = 1.0;

            string ResultdirectoryPath = @"D:\\201512_CMProcess\\CMDProcessResult_version2";

            if (!Directory.Exists(ResultdirectoryPath))//如果路径不存在
            {
                Directory.CreateDirectory(ResultdirectoryPath);//创建一个路径的文件夹
            }

            string ResultdirectoryPath2 = @"D:\\201512_CMProcess\\CMDProcessResult2_version2";

            if (!Directory.Exists(ResultdirectoryPath2))//如果路径不存在
            {
                Directory.CreateDirectory(ResultdirectoryPath2);//创建一个路径的文件夹
            }

            //结果输出路径
            string directoryPath_STInfo = ResultdirectoryPath + "\\STDataResult_" + Distance + "_" + Timespan + "Hour";//定义一个路径变量
            string directoryPath_HWInfo = ResultdirectoryPath + "\\HWInfoResult_0.5";//定义一个路径变量
            string directoryPath_STNumStatistic = ResultdirectoryPath2 + "\\STNumStatistic";//定义一个路径变量
            string directoryPath_STNumInfo = directoryPath_STNumStatistic + "\\STNumDataResult_" + Distance + "_" + Timespan + "Hour";//定义一个路径变量
            string directoryPath_HWStatist = ResultdirectoryPath2 + "\\HWStatistResult";

            if (!Directory.Exists(directoryPath_STInfo))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath_STInfo);//创建一个路径的文件夹
            }

            if (!Directory.Exists(directoryPath_STNumInfo))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath_STNumInfo);//创建一个路径的文件夹
            }

            if (!Directory.Exists(directoryPath_HWInfo))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath_HWInfo);//创建一个路径的文件夹
            }

            if (!Directory.Exists(directoryPath_HWStatist))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath_HWStatist);//创建一个路径的文件夹
            }

            if (!Directory.Exists(directoryPath_STNumStatistic))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath_STNumStatistic);//创建一个路径的文件夹
            }

            StreamWriter swST;
            StreamWriter swHW;
            StreamWriter swSTNum;
            StreamWriter swHWStat = new StreamWriter(Path.Combine(directoryPath_HWStatist, "HWStatisTotalUser_0.5.txt"), false);
            StreamWriter swSTRatio = new StreamWriter(Path.Combine(directoryPath_STNumStatistic, "STRatioTotalUser_" + Distance + "_" + Timespan + "Hour.txt"), false);
            StreamWriter swlog = new StreamWriter(ResultdirectoryPath2 + "\\log_" + Distance + "_" + Timespan + "Hour.txt");

           
            //统计结果
            Dictionary<int, int> DiffSTPointByUser = new Dictionary<int, int>();
            Dictionary<int, int> STPointCountByUser = new Dictionary<int, int>();  
            Dictionary<int, List<int>> SpatialUserInfo = new Dictionary<int, List<int>>();
            Dictionary<int, int> STPointTemporal = new Dictionary<int, int>();
            for(int i=0;i<24;i++)
            {
                STPointTemporal.Add(i, 0);
            }

            //输入轨迹存储
            Dictionary<int, List<UserData>> tempdata = new Dictionary<int, List<UserData>>();
            Dictionary<int, string[]> StationInfo= new Dictionary<int, string[]>();

            //基站id数据
            try
            {

                using (StreamReader sr = new StreamReader("D:\\201512_CMProcess\\stationIdInfo5934.txt"))
                {

                    String line;
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] strArr = line.Split('\t');
                        String[] LatLon = new string[] { strArr[1], strArr[2] };

                        StationInfo.Add(Convert.ToInt32(strArr[0]), LatLon);
                        i++;
                    }
                    Console.WriteLine(i + "  station id has been read");

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            //分163个文件读
            DirectoryInfo dir = new System.IO.DirectoryInfo("D:\\201512_CMProcess\\DataStatisticalResult");

            //停留点数据
            List<CellTra> SpecialSTUser = new List<CellTra>();
            StopIdentification2 STPointInstance = new StopIdentification2();
            int n = 0;
            List<Tuple<int, int, int>> userhwinfo1 = new List<Tuple<int, int, int>>();

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (fi.Extension == ".txt")
                {

                    StreamReader sr = new StreamReader(fi.FullName);
                    String line;
                    int m=0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //文件顺序：timeindex, userid, stationid，已排序
                        string[] strArr = line.Split('\t');
                        DateTime dt = DateTime.ParseExact(strArr[0], "yyyy/M/dd H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        int userid=Convert.ToInt32(strArr[1]);
                        int stationid=Convert.ToInt32(strArr[2]);
                        //添加到hash表中，key为用户id
                        if (tempdata.ContainsKey(userid))
                        {
                            tempdata[userid].Add(new UserData(dt,userid,stationid));
                        }
                        else
                        {
                            tempdata.Add(Convert.ToInt32(strArr[1]), new List<UserData>() {new UserData(dt, userid, stationid) });
                        }
                        m++;
                    }


                    //输出停留点、停留点个数、家工作信息
                    string filePath = "STTraByUser_" + n + ".txt";
                    string filePath_HW = "HWInfoByUser_" + n + ".txt";

                    swST = new StreamWriter(Path.Combine(directoryPath_STInfo, filePath), false);
                    swSTNum = new StreamWriter(Path.Combine(directoryPath_STNumInfo, filePath), false);
                    swHW = new StreamWriter(Path.Combine(directoryPath_HWInfo, filePath_HW), false);


                    //得到某一文件原始数据序列，100000用户为一个文件,一个tt为一个用户数据

                    //停留点数据合并统计
                    
                    int NumofCandiateSTPoint = 0;
                    int NumofClusters = 0;
                    int NumofClustersRemoved = 0;
                    int numofSTUser = 0;
                    List<UserData> UserData = new List<UserData>();

                    foreach (var tt in tempdata)
                    {
                        //T1:先判断原始轨迹点的个数。
                        int CountOrigin = tt.Value.Count();
                        if (CountOrigin < 18)
                            return;

                        //T2：直接进行信号补齐！！！表示有完整24小时数据，但是点数不一定24个。
                        UserData = DataFill.DataFillMethod(tt.Value);

                        //T3：计算停留点
                        SpecialSTUser = STPointInstance.ClusterAnalysis(UserData, Distance, Timespan, StationInfo);
                        //T4：根据停留点个数
                        if (SpecialSTUser.Count() == 0)
                        {
                            continue;
                        }
                        else if(SpecialSTUser.Count()==1)
                        {
                            userhwinfo1.Add(new Tuple<int, int, int>(SpecialSTUser[0].userid,SpecialSTUser[0].stationid,0));
                        }
                        else if(SpecialSTUser.Count()==2)
                        {

                        }
                        else if(SpecialSTUser.Count()>2)
                        {

                        }

                        //输出停留点
                        foreach (CellTra de in SpecialSTUser)
                            swST.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", de.userid, de.stationid, StationInfo[de.stationid][0], StationInfo[de.stationid][1], de.intimeindex, de.outtimeindex, de.NumST);

                        NumofCandiateSTPoint += STPointInstance.NumofCandiateSTPoint;
                        NumofClusters += STPointInstance.NumofClusters;
                        NumofClustersRemoved += STPointInstance.NumofClustersRemoved;
                        
                        //输出每个用户不同轨迹点个数，作为验证参数之用。
                        swSTNum.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", tt.Key, tt.Value.Count(),STPointInstance.NumofCandiateSTPoint,STPointInstance.NumofClusters,STPointInstance.NumofClustersRemoved);

                        var HWInfo = HWIdentifiction.HomeWorkIdentify(tt.Value);
                        //Home- Work 识别,使用两种方法探测1、原始序列；2、停留点序列
                        userhwinfo1.Add(HWInfo);

                       // Tuple<int, int, int> userhwinfo2 = HWIdentifiction.HomeWorkIdentify(SpecialSTUser);
                        //userid，home，work.若为0，则为空
                        swHW.WriteLine("{0}\t{1}\t{2}", HWInfo.Item1, HWInfo.Item2, HWInfo.Item3);
                        numofSTUser++;

                        //查找到每个用户出现的不同地点的个数
                        for (int q = 0; q < SpecialSTUser.Count(); q++)
                        {
                            //前一晚23点的问题。
                            if (SpecialSTUser[q].intimeindex.Hour == 23)
                            {
                                STPointTemporal[23]++;
                                for (int mq = 0; mq <= SpecialSTUser[q].outtimeindex.Hour; mq++)
                                {
                                    STPointTemporal[mq]++;
                                }
                            }
                            else
                            {
                                for (int mq = SpecialSTUser[q].intimeindex.Hour; mq <= SpecialSTUser[q].outtimeindex.Hour; mq++)
                                {
                                    STPointTemporal[mq]++;
                                }
                            }

                            //地点数据
                            if (q==0)
                                SpatialUserInfo.Add(SpecialSTUser[0].userid, new List<int>() { SpecialSTUser[q].stationid });
                            else
                                SpatialUserInfo[SpecialSTUser[0].userid].Add(SpecialSTUser[q].stationid);
                        }

                        DiffSTPointByUser.Add(SpecialSTUser[0].userid, SpatialUserInfo[SpecialSTUser[0].userid].Distinct().Count());
                        STPointCountByUser.Add(SpecialSTUser[0].userid, STPointInstance.NumofClustersRemoved);

                        SpatialUserInfo.Clear();
                        SpecialSTUser.Clear();
                    }



                    //输出家和工作信息
                    int UserCount = tempdata.Keys.Count();

                    ////统计该文件所有的家和工作个数
                    int Both = 0;
                    int OnlyH = 0;
                    int OnlyW = 0;
                    int Neither = 0;
                    int BothSame = 0;

                    foreach (var tt in userhwinfo1)
                    {
                        if (tt.Item2 != 0 && tt.Item3 != 0)
                        {
                            Both++;
                            if (tt.Item2 == tt.Item3)
                                BothSame++;
                        }
                        else if (tt.Item2 == 0 || tt.Item3 == 0)
                        {
                            if (tt.Item2 != 0 && tt.Item3 == 0)
                            {
                                OnlyW++;
                            }
                            else if (tt.Item3 != 0 && tt.Item2 == 0)
                            {
                                OnlyH++;
                            }
                            else if (tt.Item2 == 0 && tt.Item3 == 0)
                            {
                                Neither++;
                            }
                        }
                    }

                    swHWStat.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", n, userhwinfo1.Count(), Both, BothSame, OnlyH, OnlyW, Neither);


                    swlog.WriteLine(n + "\t" + m+"\t"+ UserCount+"\t"+ numofSTUser);
                    swSTRatio.WriteLine(n + "\t"+ numofSTUser + "\t" + m + "\t" + NumofCandiateSTPoint + "\t" + NumofClusters + "\t" + NumofClustersRemoved);
                    Console.WriteLine("文件序号：" + n + "数据条数：" + m);
                    Console.WriteLine("该文件总用户数：" + UserCount);
                    Console.WriteLine("有停留点用户数：" + numofSTUser);

                    tempdata.Clear();
                    swST.Flush();
                    swST.Close();
                    swHW.Flush();
                    swHW.Close();

                    swSTNum.Flush();
                    swSTNum.Close();

                    n++;//文件循环163
                    
                    sr.Close();
                    sr.Dispose();
                    userhwinfo1.Clear();
                }
            }

            StreamWriter swtti = new StreamWriter(Path.Combine(directoryPath_STNumStatistic, "STPointByUser.txt"), false);
            StreamWriter swtti2 = new StreamWriter(Path.Combine(directoryPath_STNumStatistic, "DistinctSTPointByUser.txt"), false);
            StreamWriter swcountDiffLoc = new StreamWriter(Path.Combine(directoryPath_STNumStatistic, "DiffSTPointCountInfo.txt"), false);
            StreamWriter swcountLoc = new StreamWriter(Path.Combine(directoryPath_STNumStatistic, "TotalSTPointCountInfo.txt"), false);
            StreamWriter swSTTime = new StreamWriter(Path.Combine(directoryPath_STNumStatistic, "STTemporalTotalUser_" + Distance + "_" + Timespan + "Hour.txt"), false);

            Dictionary<int, int> DiffLocInfo = new Dictionary<int, int>();
            Dictionary<int, int> LocInfo = new Dictionary<int, int>();

            //分别输出每个用户的数据列表,统计概率再输出到一个表
            foreach(var st in STPointCountByUser)
            {
                //字段：userid，停留点个数
                swtti.WriteLine("{0}\t{1}", st.Key,st.Value);
                if (LocInfo.ContainsKey(st.Value))
                {
                    LocInfo[st.Value]++;
                }
                else
                {
                    LocInfo.Add(st.Value, 1);
                }

            }

            foreach(var dst in DiffSTPointByUser)
            {

                //字段：userid，distinct停留点个数
                swtti2.WriteLine("{0}\t{1}",dst. Key, dst.Value);
                if (DiffLocInfo.ContainsKey(dst.Value))
                {
                    DiffLocInfo[dst.Value]++;
                }
                else
                {
                    DiffLocInfo.Add(dst.Value, 1);
                }

            }

            foreach (var mm in LocInfo)
            {
                swcountLoc.WriteLine("{0}\t{1}", mm.Key, mm.Value);
            }

            foreach (var mm in DiffLocInfo)
            {
                swcountDiffLoc.WriteLine("{0}\t{1}", mm.Key, mm.Value);
            }

            foreach(var mm in STPointTemporal)
            {
                swSTTime.WriteLine("{0}\t{1}",mm.Key,mm.Value);
            }

            swtti2.Flush();
            swtti2.Close();
            swtti.Flush();
            swtti.Close();
            swcountLoc.Flush();
            swcountLoc.Close();
            swcountDiffLoc.Flush();
            swcountDiffLoc.Close();

            swSTTime.Flush();
            swSTTime.Close();

            swlog.Flush();
            swlog.Close();
            swHWStat.Flush();
            swHWStat.Close();
            swSTRatio.Flush();
            swSTRatio.Close();
        }
    }
}
