using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OfficeHandlerService.Office {
    abstract class ExcelHandler: OfficeHandler {

        protected List<ExcelWorkbook> wbs = null;

        public abstract override string parse(string path);

        public override string execute(string path, string destination) {
            if (wbs != null) {
                Excel.Application app = null;
                Excel.Workbook wb = null;
                try {
                    string error = null;
                    String[] paths = copyBaseFile(path, destination, wbs.Count, out error);
                    if (path != null) {
                        Dictionary<int, List<KeyValuePair<int[], string>>> diffInTemplate = new Dictionary<int, List<KeyValuePair<int[], string>>>();
                        app = new Excel.Application();
                        for (int i = 0; i < paths.Length; i++) {
                            wb = app.Workbooks.Open(paths[i]);
                            wbs[i].unParsedSimpleValues = replaceSimpleValues(wb, wbs[i].simpleValues, wbs[i].unParsedSimpleValues);
                            wbs[i].unParsedEnumeratedValues = replaceEnumeratedValues(wb, wbs[i].enumeratedValues, wbs[i].unParsedEnumeratedValues);
                            wbs[i].unParsedtables = replaceTableValues(wb, wbs[i].tables, wbs[i].unParsedtables);
                            List<KeyValuePair<int[], string>> diff = new List<KeyValuePair<int[], string>>();
                            Excel.Worksheet sheet = (Excel.Worksheet)wb.ActiveSheet;
                            Excel.Range range = sheet.Cells;
                            Excel.Range found = range.Find(What: "<#<");
                            while (found != null) {
                                int[] pos = new int[2];
                                pos[0] = found.Row;
                                pos[1] = found.Column;
                                string cell = found.Text;
                                int start = cell.IndexOf("<#<");
                                while (start > -1) {
                                    int end = cell.IndexOf(">#>", start);
                                    diff.Add(new KeyValuePair<int[], string>(pos, cell.Substring(start + 3, end - start - 3)));
                                    start = cell.IndexOf("<#<", end);
                                }
                                sheet.Cells[found.Row,found.Column] = cell.Replace("<#<", "!#!");
                                found = range.Find(What: "<#<");
                            }
                            range.Replace(What: "!#!", Replacement: "<#<");
                            diffInTemplate[i] = diff;
                            wb.Save();
                        }
                        writeDifferencesToFile(destination, diffInTemplate);
                        app.Quit();
                        try {
                            ExcelKiller.killProcess(app);
                        } catch (Exception exc) {
                            return "[ExcelHandler/execute] " + exc.Message;
                        }
                        return null;
                    } else {
                        return "[ExcelHandler/execute] " + error;
                    }
                } catch (Exception ex) {
                    if (wb != null) {
                        wb.Save();
                    }
                    if (app != null) {
                        app.Quit();
                        try {
                            ExcelKiller.killProcess(app);
                        }catch (Exception exc) {
                            return "[ExcelHandler/execute] " + exc.Message;
                        }
                    }
                    return "[ExcelHandler/execute] " + ex.Message;
                }
            } else {
                return "[ExcelHandler/execute] There are no workbooks.";
            }
        }

        private void writeDifferencesToFile(string destination, Dictionary<int, List<KeyValuePair<int[], string>>> diffInTemplate) {
            try {
                FileStream differences = new FileStream(destination + "\\differences.txt", FileMode.Create);
                StreamWriter writer = new StreamWriter(differences);
                if (diffInTemplate.Count > 0) {
                    writer.WriteLine("Template:");
                    foreach (KeyValuePair<int, List<KeyValuePair<int[], string>>> item in diffInTemplate) {
                        writer.WriteLine("\tSolution " + item.Key + ": " + item.Value.Count + " unparsed tag");
                        foreach (KeyValuePair<int[], string> tag in item.Value) {
                            writer.WriteLine("\t\tcell: [" + tag.Key[0] + "," + tag.Key[1] + "] , tag: " + tag.Value);
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine();
                } else {
                    writer.WriteLine("Template:\nAll tags successfully parsed.\n");
                }
                bool allParsed = true;
                writer.WriteLine("Data: ");
                for (int i = 0; i < wbs.Count; i++) {
                    writer.WriteLine("\tDocumet: " + (i + 1));
                    if (wbs[i].unParsedSimpleValues.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed simple values: " + wbs[i].unParsedSimpleValues.Count);
                        foreach (string tag in wbs[i].unParsedSimpleValues) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll simple values parsed");
                    }
                    if (wbs[i].unParsedEnumeratedValues.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed enumerated values: " + wbs[i].unParsedEnumeratedValues.Count);
                        foreach (string tag in wbs[i].unParsedEnumeratedValues) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll enumerated values parsed");
                    }
                    if (wbs[i].unParsedtables.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed tables: " + wbs[i].unParsedtables.Count);
                        foreach (string tag in wbs[i].unParsedtables) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll tables parsed");
                    }
                    writer.WriteLine();
                }
                if (allParsed) {
                    writer.WriteLine("All tags successfully parsed.\n");
                }
                writer.Close();
            } catch (Exception ex) {
                throw;
            }
        }

        private List<string> replaceSimpleValues(Excel.Workbook wb, Dictionary<String, String> simpleValues, List<string> unParsed) {
            try {
                Excel.Worksheet sheet = (Excel.Worksheet)wb.ActiveSheet;
                Excel.Range range = sheet.Cells;
                foreach (KeyValuePair<String, String> item in simpleValues) {
                    if (range.Find(What: "<#<" + item.Key + ">#>") != null) {
                        range.Replace(What: "<#<" + item.Key + ">#>", Replacement: item.Value, MatchCase: true);
                        if (unParsed.Contains(item.Key)) {
                            unParsed.Remove(item.Key);
                        }
                    }
                }
                return unParsed;
            } catch (Exception ex) {
                throw;
            }
        }

        private List<string> replaceEnumeratedValues(Excel.Workbook wb, Dictionary<String, ExcelEnumeration> enumeratedValues, List<string> unParsed) {
            try {
                Excel.Worksheet sheet = (Excel.Worksheet)wb.ActiveSheet;
                Excel.Range range = sheet.Cells;
                foreach (KeyValuePair<String, ExcelEnumeration> item in enumeratedValues) {
                    if (range.Find(What: "<#<" + item.Key + ">#>") != null) {
                        range.Replace(What: "<#<" + item.Key + ">#>", Replacement: item.Value.ToString(), MatchCase: true);
                        if (unParsed.Contains(item.Key)) {
                            unParsed.Remove(item.Key);
                        }
                    }
                }
                return unParsed;
            } catch (Exception ex) {
                throw;
            }
        }

        private List<string> replaceTableValues(Excel.Workbook wb, List<ExcelTable> tables, List<string> unParsed) {
            try {
                Excel.Worksheet sheet = (Excel.Worksheet)wb.ActiveSheet;
                Excel.Range range = sheet.Cells;
                foreach (ExcelTable table in tables) {
                    Excel.Range found = range.Find(What: "<#<" + table.name + ">#>");
                    if ((found != null) && (unParsed.Contains(table.name))) {
                        unParsed.Remove(table.name);
                    }
                    int count = 0;
                    while (count <5 && found != null) {
                        count++;
                        int height = table.items.Length;
                        for (int i = 0; i < height - 1; i++) {
                            Excel.Range line = sheet.Rows[found.Row];
                            line.Insert();
                        }
                        for (int i = 0; i < height; i++) {
                            for (int j=0; j < table.items[i].Length; j++) {
                                sheet.Cells[found.Row - (height - i -1), found.Column + j] = table.items[i][j];
                            }
                        }
                        found = range.Find(What: "<#<" + table.name + ">#>");
                    }
                }
                return unParsed;
            } catch (Exception ex) {
                throw;
            }
        }

        public override MemoryStream toStream(out string error) {
            try {
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
                error = null;
                return stream;
            } catch (Exception ex) {
                error = "[ExcelHandler/toStream] " + ex.Message;
                return null;
            }
        }

        private static class ExcelKiller {
            [DllImport("user32.dll")]
            static extern int GetWindowThreadProcessId(int hWnd, out int processId);

            static public void killProcess(Excel.Application app) {
                try {
                    int id;
                    GetWindowThreadProcessId(app.Hwnd, out id);
                    Process.GetProcessById(id).Kill();
                } catch (Exception ex) {
                    throw;
                }
            }
        }

        protected class ExcelWorkbook {
            public Dictionary<String, String> simpleValues = new Dictionary<string, string>();
            public Dictionary<String, ExcelEnumeration> enumeratedValues = new Dictionary<string, ExcelEnumeration>();
            public List<ExcelTable> tables = new List<ExcelTable>();
            public List<string> unParsedSimpleValues = new List<string>();
            public List<string> unParsedEnumeratedValues = new List<string>();
            public List<string> unParsedtables = new List<string>();
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
