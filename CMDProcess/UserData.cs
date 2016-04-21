using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class UserData
    {
            public DateTime timeindex;
            public int userid;
            //public int stationid ;
        public int cellid;
            

            public UserData()
            {

            }

            public UserData(DateTime p1,int p2,int p3)
            {
                // TODO: Complete member initialization
                timeindex = p1;
                userid = p2;
                //stationid = p3;
            cellid = p3;
            }
    }

    class CellTra
    {
        public DateTime intimeindex;
        public DateTime outtimeindex;
        public int userid;
        public int cellid;
        public int NumST;


        public CellTra()
        {

        }

        public CellTra(DateTime p0,DateTime p1, int p2, int p3,int p5)
        {
            // TODO: Complete member initialization
            intimeindex = p0;
            outtimeindex = p1;
            userid = p2;
            cellid = p3;
            NumST = p5;
        }
        
    }

    class CellTra2
    {
        public DateTime intimeindex;
        public DateTime outtimeindex;
        public int userid;
        public Dictionary<int, int> cellNum;
        public List<UserData> DatainCluster;

        public CellTra2()
        {

        }

        public CellTra2(UserData data)
        {
            // TODO: Complete member initialization

            userid = data.userid;
            DatainCluster = new List<UserData>();
            cellNum = new Dictionary<int, int>();
            cellNum.Add(data.cellid, 1);
            DatainCluster.Add(data);
            intimeindex = data.timeindex;
            outtimeindex = data.timeindex;
        }

        public void PutinCluster(UserData data)
        {
            DatainCluster.Add(data);
            UpdateOuttimeindex();
            UpdateStationNum();
        }

        private void UpdateStationNum()
        {
            if(cellNum.ContainsKey(DatainCluster.Last().cellid))
            {
                cellNum[DatainCluster.Last().cellid]++;
            }
            else
            {
                cellNum.Add(DatainCluster.Last().cellid, 1);
            }
        }

        private  void UpdateOuttimeindex()
        {
            outtimeindex = DatainCluster.Last().timeindex;
        }
    }

    class CellTra3
    {
        public DateTime intimeindex;
        public DateTime outtimeindex;
        public int userid;
        public int cellNum;
        public int cellId;
        public List<UserData> DatainCluster;

        public CellTra3()
        {

        }

        public CellTra3(UserData data)
        {
            // TODO: Complete member initialization

            userid = data.userid;
            DatainCluster = new List<UserData>();
            cellNum = 0;
            cellNum++;
            cellId = data.cellid;
            DatainCluster.Add(data);
            intimeindex = data.timeindex;
            outtimeindex = data.timeindex;
        }

        public void PutinCluster(UserData data)
        {
            DatainCluster.Add(data);
            UpdateOuttimeindex();
            cellNum++;
        }

        private void UpdateStationNum()
        {

        }

        private void UpdateOuttimeindex()
        {
            outtimeindex = DatainCluster.Last().timeindex;
        }
    }
}
