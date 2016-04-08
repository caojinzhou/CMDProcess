using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class TestHMM
    {
        StreamWriter swlog;
        StreamWriter sw;

        enum Activity { Shopping, Eating, Transportation, work, Social, home, study, Leisure };  // 隐藏状态（活动）
        string directoryPath = @"D:\\CHINAMOBILE\\HMMResult";//定义一个路径变量
        string directoryPath2 = @"D:\\CHINAMOBILE\\HMMLog";//定义一个路径变量
        HMMClass hmm;
        public TestHMM(int n,Double[,] A, Double[,] B, Double[,] PI)
        {

            hmm = new HMMClass(B.GetLength(0), B.GetLength(1));
            hmm.A = A;
            hmm.B = B;
            hmm.PI = PI;
            string filePath = "HMMResult_" + n + ".txt";//定义一个文件路径变量
            if (!Directory.Exists(directoryPath))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath);//创建一个路径的文件夹
            }
            //续写文件
            sw = new StreamWriter(Path.Combine(directoryPath, filePath));

            string filePath2 = "HMMResult_log_" + n + ".txt";//定义一个文件路径变量
            if (!Directory.Exists(directoryPath2))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath2);//创建一个路径的文件夹
            }
            //续写文件
            swlog = new StreamWriter(Path.Combine(directoryPath2, filePath2), true);
        }

        public void CheckViterbi(CellTra[] Obeservation,Tuple<int, int, int> userhwinfo,int n)
        {



            Console.WriteLine(Obeservation[0].userid);
            swlog.WriteLine(Obeservation[0].userid + "");
            swlog.WriteLine(Obeservation.Count() + "");


            // 找出最有可能的隐藏状态序列
            Double Probability;
            Double[,] DELTA;

            Console.WriteLine("------------维特比算法：双精度运算-----------------");
            Int32[] Q;
            if (userhwinfo.Item2 != 0 || userhwinfo.Item3 != 0)
                Q = hmm.Viterbi(Obeservation, out DELTA, out Probability, userhwinfo);
            else
                 Q = hmm.Viterbi(Obeservation, out DELTA, out Probability);

            swlog.WriteLine(Probability.ToString());

            Console.WriteLine("Probability =" + Probability.ToString("0.###E+0"));

            for (int t = 0; t < Q.Count(); t++)
            {
                sw.WriteLine(Obeservation[t].userid.ToString()+"\t"+((Activity)Q[t]).ToString() + "\t" + Obeservation[t].intimeindex.ToString() + "\t" + Obeservation[t].outtimeindex.ToString() + "\t" + Obeservation[t].NumST.ToString() + "\t" + Obeservation[t].stationid.ToString());
                Console.WriteLine(((Activity)Q[t]).ToString());
            }

            //for (int t = 0; t < DELTA.GetLength(0); t++)
            //{
                        //WriteLogData(DELTA[t,0].ToString());
                        //WriteLogData((Obeservation[t].timeindex / 3600).ToString());
                        //WriteLogData(Obeservation[t].towerindex.ToString());
                        //Console.WriteLine(((Activity)Q[t]).ToString());
            //}
            Console.WriteLine();
                    //Console.WriteLine("------------维特比算法：对数运算-----------------");
                    //Q = hmm.ViterbiLog(Obeservation, out Probability);
                    //Console.WriteLine("Probability =" + Probability.ToString("0.###E+0"));
                    //foreach (Int32 Value in Q)
                    //{
                    //    Console.WriteLine(((Weather)Value).ToString());
                    //}
        }

        public void Close()
        {
            sw.Flush();
            sw.Close();
            swlog.Flush();
            swlog.Close();
        }
    }
}
