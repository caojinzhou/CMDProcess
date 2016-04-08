using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDProcess
{
    class Discalculate
    {

	  struct Result_Point
	  {
		  int flag;     //指示标志
		  string guidname;
		  double x1;
		  double y1;    //标准POI数据
		  double x2;
		  double y2;    //变换前签到数据
		  double x3;
		  double y3;   //变换后签到数据
		  double dis;
		  double afterdis;
	  };
	  double dis_ave;
	  double dis_var;
	  double dis_afterave;
	  double dis_aftervar;

	  //Result_Point resultpoint;


        /*Bool CoordinateTransform(double a[],double b[], int countpoint,Point*data)
        {
	        resultpoint=new Result_Point[countpoint];
	        Result_Point resultpoint[countpoint];
	        int i;
	        for(i=0;i<countpoint;i++)
	        {
		        resultpoint[i].flag=data[i].flag;
		        resultpoint[i].guidname=data[i].guidname;
		        resultpoint[i].x1=data[i].x2;
		        resultpoint[i].y1=data[i].y2;
		        resultpoint[i].x2=data[i].x1;
		        resultpoint[i].y2=data[i].y1;
		        resultpoint[i].x3=a[0]+a[1]*data[i].x1+a[2]*data[i].y1;
		        resultpoint[i].y3=b[0]+b[1]*data[i].x1+b[2]*data[i].y1;
		        resultpoint[i].dis=LonLatTransform(resultpoint[i].x1,resultpoint[i].y1,resultpoint[i].x2,resultpoint[i].y2);
		        resultpoint[i].afterdis=LonLatTransform(resultpoint[i].x1,resultpoint[i].y1,resultpoint[i].x3,resultpoint[i].y3);
	        }
	        return true;
        }*/

        static public double LonLatTransform(double x1,double y1,double x2,double y2) //参数：点1经度，点1纬度，点2经度，点2纬度
        {
	        const double Rc=6378137;  // 赤道半径
	        const double PI=3.14159265;
	        double rady1 = y1*PI/180;
	        double rady2 = y2*PI/180;
	        double a = rady1 - rady2;
	        double b = x1*PI/180 - x2*PI/180;
            double s=2*Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a/2),2)+Math.Cos(rady1)*Math.Cos(rady2)*Math.Pow(Math.Sin(b/2),2)));
	        //double s = 2 * asin(sqrt(pow(sin(a/2),2) +cos(rady1)*cos(rady2)*pow(sin(b/2),2)));
	        s = s * Rc;
	        return s;
            //单位：米
        }
    }
}
