using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TemplateHandler.Parsers {
    abstract class ExcelHandler: OfficeHandler {

        protected List<ExcelWorkbook> wbs = null;

        public abstract override string parse(string path);

        private List<ExcelWorkbook> fillBlankData(int count) {
            List<ExcelWorkbook> blank = new List<ExcelWorkbook>();
            /*for(int i = 0; i < count; i++) {
                ExcelWorkbook wb = new ExcelWorkbook();

            }*/
            ExcelWorkbook wb = new ExcelWorkbook();
            wb.simpleValues["Vezetéknév"] = "NévV 1";
            wb.simpleValues["Keresztnév"] = "NévK 1";
            ExcelEnumeration enumer = new ExcelEnumeration();
            enumer.values = new string[] { "Nyelv1", "Nyelv2" };
            enumer.separator = ",";
            blank.Add(wb);
            wb.enumeratedValues["Nyelvek"] = enumer;
            wb = new ExcelWorkbook();
            wb.simpleValues["Vezetéknév"] = "NévV 2";
            wb.simpleValues["Keresztnév"] = "NévK 2";
            enumer = new ExcelEnumeration();
            enumer.values = new string[] { "Nyelv1", "Nyelv2" };
            enumer.separator = ";";
            wb.enumeratedValues["Nyelvek"] = enumer;
            blank.Add(wb);
            wb = new ExcelWorkbook();
            wb.simpleValues["Vezetéknév"] = "NévV 3";
            wb.simpleValues["Keresztnév"] = "NévK 3";
            enumer = new ExcelEnumeration();
            enumer.values = new string[] { "Nyelv1", "Nyelv2" };
            enumer.separator = "-";
            wb.enumeratedValues["Nyelvek"] = enumer;
            blank.Add(wb);
            return blank;
        }

        public override string execute(string path, string destination) {
            wbs = fillBlankData(3);
            if (wbs != null) {
                Excel.Application app = null;
                Excel.Workbook wb = null;
                try {
                    String[] paths = copyBaseFile(path, destination, wbs.Count);
                    app = new Excel.Application();
                    for (int i = 0; i < paths.Length; i++) {
                        wb = app.Workbooks.Open(paths[i]);
                        replaceSimpleValues(wb, wbs[i].simpleValues);
                        replaceEnumeratedValues(wb, wbs[i].enumeratedValues);
                        //replaceTableValues(wb, wbs[i].tables);
                        wb.Save();
                    }
                    app.Quit();
                    ExcelKiller.killProcess(app);
                    return null;
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    if (wb != null) {
                        wb.Save();
                    }
                    if (app != null) {
                        app.Quit();
                        ExcelKiller.killProcess(app);
                    }
                    return ex.Message;
                }
            } else {
                return "There are no workbooks";
            }
        }

        private void replaceSimpleValues(Excel.Workbook wb, Dictionary<String, String> simpleValues) {
            Excel.Worksheet sheet = (Excel.Worksheet)wb.ActiveSheet;
            Excel.Range range = sheet.Cells;
            foreach (KeyValuePair<String, String> item in simpleValues) {
                range.Replace(What: "<#<" + item.Key + ">#>", Replacement: item.Value, MatchCase:true);
            }
        }

        private void replaceEnumeratedValues(Excel.Workbook wb, Dictionary<String, ExcelEnumeration> enumeratedValues) {
            Excel.Worksheet sheet = (Excel.Worksheet)wb.ActiveSheet;
            Excel.Range range = sheet.Cells;
            foreach (KeyValuePair<String, ExcelEnumeration> item in enumeratedValues) {
                range.Replace(What: "<#<" + item.Key + ">#>", Replacement: item.Value.ToString(), MatchCase: true);
            }
        }

        /*private void replaceTableValues(Word.Document doc, List<WordTable> tables) {
            if ((tables.Count > 0) && (doc.Tables.Count > 0)) {
                List<Word.Table> wordTables = new List<Word.Table>();
                foreach (Word.Table table in doc.Tables) {
                    wordTables.Add(table);
                }
                foreach (WordTable item in tables) {
                    List<int> selectedTables = new List<int>();
                    for (int i = 0; i < wordTables.Count; i++) {
                        Word.Range searchRange = wordTables[i].Cell(1, 1).Range.Previous(Word.WdUnits.wdParagraph, 1);
                        if ((searchRange != null) && (searchRange.Text.IndexOf("<#<" + item.name + ">#>") != -1)) {
                            selectedTables.Add(i);
                        }
                    }
                    foreach (int i in selectedTables) {
                        Word.Table wordTable = wordTables[i];
                        Word.Range range = wordTables[i].Cell(1, 1).Range.Previous(Word.WdUnits.wdParagraph, 1);
                        range.Find.ClearFormatting();
                        range.Find.Execute(FindText: "<#<" + item.name + ">#>", ReplaceWith: "", Replace: Word.WdReplace.wdReplaceOne);
                        for (int l = 0; l < item.items.Length; l++) {
                            wordTable.Rows.Add();
                            int lastLine = wordTable.Rows.Count;
                            for (int j = 0; j < item.items[0].Length; j++) {
                                wordTable.Cell(lastLine, j + 1).Range.Text = item.items[l][j];
                            }
                        }
                    }
                }
            }
        }*/

        public override MemoryStream toStream() {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            if (wbs != null) {
                foreach (ExcelWorkbook wb in wbs) {
                    writer.WriteLine("Workbook:");
                    writer.WriteLine("\tSimple values:");
                    foreach (KeyValuePair<String, String> values in wb.simpleValues) {
                        writer.WriteLine("\t\t{0}: {1}", values.Key, values.Value);
                    }
                    writer.WriteLine("\tEnumerated values:");
                    foreach (KeyValuePair<String, ExcelEnumeration> item in wb.enumeratedValues) {
                        writer.WriteLine("\t\t" + item.Key + ": " + item.Value);
                    }
                    writer.WriteLine("\tTable values:");
                    foreach (ExcelTable item in wb.tables) {
                        writer.WriteLine("\t\t" + item.name + ":");
                        for (int i = 0; i < item.items.Length; i++) {
                            writer.Write("\t\t\t");
                            for (int j = 0; j < item.items[i].Length; j++) {
                                writer.Write(item.items[i][j] + ", ");
                            }
                            writer.WriteLine();
                        }
                    }
                }
            } else {
                writer.Write("There are no workbooks");
            }
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static class ExcelKiller {
            [DllImport("user32.dll")]
            static extern int GetWindowThreadProcessId(int hWnd, out int processId);

            static public void killProcess(Excel.Application app) {
                int id;
                GetWindowThreadProcessId(app.Hwnd, out id);
                Process.GetProcessById(id).Kill();
            }
        }

        protected class ExcelWorkbook {
            public Dictionary<String, String> simpleValues = new Dictionary<string, string>();
            public Dictionary<String, ExcelEnumeration> enumeratedValues = new Dictionary<string, ExcelEnumeration>();
            public List<ExcelTable> tables = new List<ExcelTable>();
        }

        protected class ExcelEnumeration {
            public String[] values { get; set; }
            public String separator { get; set; }
            public override string ToString() {
                return String.Join(separator + " ", values);
            }
        }

        protected class ExcelTable {
            public String name { get; set; }
            public String[][] items { get; set; }
        }
    }
}
