/**
* Copyright (C) 2016 Open University (http://www.ou.nl/)
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*         http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.IO;

namespace StealthAssessment
{
    class Excel
    {
        //Declaring Variables.
        Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
        Workbook wb = null;
        Worksheet ws = null;
        Range range = null;
        string path = Path.Combine(System.IO.Directory.GetParent(Environment.CurrentDirectory).ToString() + "\\ConfigFiles\\");

        //Constructor.
        public Excel()
        {
            xlApp.Visible = false;
            xlApp.ScreenUpdating = false;
            xlApp.DisplayAlerts = false;

            if (xlApp == null)
            {
                Console.WriteLine("EXCEL could not be started. Check that your office installation and project references are correct.");
            }

            return;
        }

        //Destructor.
        ~Excel()
        {
            return;
        }

        public void ExcelCreate(string filename)
        {
            wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            ws = (Worksheet)wb.Worksheets[1];

            if (ws == null)
            {
                Console.WriteLine("Worksheet could not be created. Check that your office installation and project references are correct.");
            }

            ws.Name = filename;

            return;
        }

        public void ExcelAddSheets(string filename)
        {
            ws = wb.Sheets.Add();
            ws.Name = filename;
        }

        public void ExcelOpen(string filename)
        {
            wb = xlApp.Workbooks.Open(Path.Combine(@path + filename), CorruptLoad: true);
        }

        public void ExcelSave(string filename)
        {
            if (!System.IO.File.Exists(Path.Combine(@path + filename)))
            {
                wb.SaveAs(Path.Combine(@path + filename));
            }
            else
            {
                System.IO.File.Replace(Path.Combine(@path + filename), Path.Combine(@path + filename), Path.Combine(@path + filename + ".bac"));
            }
        }

        public void ExcelQuit()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(range);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ws);

            wb.Close(true);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(wb);

            xlApp.Quit();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(xlApp);
        }

        public void SelectWorksheet(string sheetname)
        {
            ws = (Worksheet)this.xlApp.Worksheets[sheetname];
            ws.Select(true);
        }

        private void SetAppInactive()
        {
            try
            {
                // Turn App interactiveness off
                this.xlApp.Interactive = false;
            }

            catch
            {

            }
        }

        public string[][] LoadData()
        {
            //Declaration of variables
            string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            var Conversion = new Exceptions();
            bool repeat = true;     //This variable checks if the user should re-enter input.

            while (this.xlApp.Interactive == true)
            {
                // If Excel is currently busy, try until go thru
                SetAppInactive();
            }

            try
            {
                ws = (Worksheet)this.xlApp.ActiveWorkbook.ActiveSheet;
                range = ws.UsedRange;
                range.Select();

                //Clean empty cells from buffer
                ws.Columns.ClearFormats();
                ws.Rows.ClearFormats();

                int overhead = 1; //(Optional manually set overhead)
                int rows_no = range.Rows.Count - overhead;
                int col_no = range.Columns.Count;

                string[][] Data = new string[col_no][]; //Stores all the data from the game log file

                for (int x = 0; x < col_no; x++)
                {
                    Data[x] = new string[rows_no];

                    for (int y = 0; y < rows_no; y++)
                    {
                        repeat = true;
                        Range r = ws.get_Range(alphabet[x] + (y + 1));

                        if (r == null)
                        {
                            Console.WriteLine("Could not get a range. Check to be sure you have the correct versions of the office DLLs.");
                        }

                        if (r.Value2 != null)
                        {
                            r.Select();
                            
                            if (r.Value2 is double)
                            {
                                while (repeat)
                                {
                                    repeat = Conversion.DoubleToString(r.Value2);
                                }

                                Data[x][y] = Convert.ToString(r.Value2);
                            }

                            else
                                Data[x][y] = r.Value2;
                        }
                    }
                }

                return Data;
            }

            finally
            {
                // Turn App interactiveness on
                this.xlApp.Interactive = true;
            }
           

        }

        public string[] ExtractMetrics(string filename)
        {
            ExcelOpen(filename); //(Optional to select file in the future)

            string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            ws = (Worksheet)this.xlApp.ActiveWorkbook.ActiveSheet;
            range = ws.UsedRange;
            range.Select();

            //Clean empty cells from buffer
            ws.Columns.ClearFormats();
            ws.Rows.ClearFormats();

            int col_no = range.Columns.Count;
            string[] Metrics = new string[col_no];

            for (int i = 0; i < col_no; i++)
            {
                Range r = ws.get_Range(alphabet[i] + 1);
                if (r == null)
                {
                    Console.WriteLine("Could not get a range. Check to be sure you have the correct versions of the office DLLs.");
                }

                if (r.Value2 != null)
                {
                    r.Select();
                    Metrics[i] = r.Value2;
                }
            }

            ExcelSave(filename);
            ExcelQuit();

            return Metrics;
        }

    }
}
