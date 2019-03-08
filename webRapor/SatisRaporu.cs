﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
namespace webRapor
{
    public class SatisRaporu
    {
        SqlConnection con;
        SqlCommand cmd;
        DataTable rapor ;
        string constr = "Data Source=10.10.22.199;Initial Catalog=NORTHWND;User ID=test2;Password=test2";

        public SatisRaporu()
        {
            con = new SqlConnection(constr);
            rapor = new DataTable();
        }
        /// <summary>
        /// Verilen yil'a Göre Aylik Bazda Satis 
        /// Ve Ciro Rakamlarini raporlar.
        /// Ornek : AylikBazdaSatisRaporu(2015)
        /// </summary>
        /// <param name="yil">int tipinde yil bilgisi alir</param>
        /// <returns> DataTable Geri Döner  </returns>
        public DataTable AylikBazdaSatisRaporu(int yil)
        {

            rapor = getTable();
            setProduct();
            setSalesData(yil);
            if (rapor.Rows.Count == 0)
            {
                DataRow row = rapor.NewRow();
                row["ProductName"] = "Kayit Bekleme Gelmez";
            }
            return rapor;

        }
        private void setProduct()
        {
            

            cmd = new SqlCommand("Select * from Products Order by ProductName", con);
            cmd.CommandType = CommandType.Text;
            con.Open();
            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                DataRow drow = rapor.NewRow();

                drow["ProductName"] = rdr["ProductName"];
                rapor.Rows.Add(drow);

            }
            con.Close();

        }
        private void setSalesData(int yil)
        {
            DataTable satis = new DataTable();
            cmd = new SqlCommand("getProducts4Year", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@year", yil));
            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();
            satis.Load(rdr);
            con.Close();
            foreach (DataRow drow in rapor.Rows)
            {
                foreach (DataRow srow in satis.Rows)
                {
                    
                    if (drow["ProductName"].ToString() == srow["ProductName"].ToString())
                    {
                        string adetCol, ciroCol;
                        adetCol = "Adet_" + srow["ay"].ToString();
                        ciroCol = "Ciro_" + srow["ay"].ToString();

                        drow[adetCol] = srow["Adet"];
                        drow[ciroCol] = srow["Ciro"];
                    }
                }
            }



        }
        private DataTable getTable()
        {

            DataColumn col = new DataColumn("ProductName", typeof(string));
            col.Caption = "Urun Adi";
            rapor.Columns.Add(col);

            for (int i = 1; i <=12; i++)
            {
                col = new DataColumn("Adet_"+i.ToString(), typeof(int));
                col.Caption = "Adet";
                col.DefaultValue = 0;
                rapor.Columns.Add(col);

                col = new DataColumn("Ciro_" + i.ToString(), typeof(int));
                col.Caption = "Ciro";
                col.DefaultValue = 0;
                rapor.Columns.Add(col);

            }


            return rapor;
        }
    }

//    USE[NORTHWND]
//GO
///****** Object:  StoredProcedure [dbo].[getProducts4Year]    Script Date: 08.03.2019 14:31:47 ******/
//SET ANSI_NULLS ON
//GO
//SET QUOTED_IDENTIFIER ON
//GO
//--Select* from Products Order by ProductName
//ALTER Procedure[dbo].[getProducts4Year]
//    (@year as int)
//as
//Begin
//Select p.ProductName , month(o.OrderDate) as ay , sum(det.Quantity) as adet ,sum(det.Quantity* det.UnitPrice)  as Ciro
//from  orders o
//inner join[Order Details] det on det.OrderID = o.OrderID
//inner join Products p on det.ProductID = p.ProductID
//Where year(o.OrderDate) = @year
//Group by p.ProductName , month(o.OrderDate)
//Order by p.ProductName , month(o.OrderDate)
//End
}
