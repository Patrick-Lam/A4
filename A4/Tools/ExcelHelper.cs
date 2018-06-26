using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4.Tools
{
    public static class ExcelHelper
    {
        public static System.Data.DataTable ExcelToTable(string fileName)
        {
            //TODO:

            System.Data.DataTable table = new System.Data.DataTable();

            return table;
        }

        public static int DataTableToExcel(System.Data.DataTable data, string fileName, string sheetName, bool isColumnWritten)
        {
            //TODO:

            return -1;
        }

        public static List<MExcel> MuliExcelToTable(string fileName)
        {
            //TODO:

            List<MExcel> res = new List<MExcel>();

            return res;
        }

        public static void DataTableToExcel(List<MExcel> sheetList, string fileName, bool isColumnWritten)
        {
            //TODO:
        }

        public class MExcel
        {
            //TODO:
        }
    }
}
