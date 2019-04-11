using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeHandlerService.Office {
    class ExcelHandlerJSON : ExcelHandler {
        public override string parse(string path) {
            List<ExcelWorkbook> excels = new List<ExcelWorkbook>(); ;
            try {
                string error = null;
                String json = getDataJSON(path, out error);
                if (json == null) {
                    wbs = null;
                    return "[ExcelHandlerJSON/parse] " + error;
                }
                StringBuilder sb = new StringBuilder(json);
                int bracesLevel = 0;
                for (int i = 0; i < sb.Length - 1; i++) {
                    if (sb[i] == '{') {
                        bracesLevel++;
                    } else if (sb[i] == '}') {
                        bracesLevel--;
                        if (bracesLevel == 1) {
                            sb[i + 1] = '*';
                        }
                    }
                }
                json = sb.ToString();
                String[] documentsJSON = json.Split(new String[] { "}*" }, StringSplitOptions.RemoveEmptyEntries);
                documentsJSON = documentsJSON.Take(documentsJSON.Length - 1).ToArray();
                documentsJSON[0] = documentsJSON[0].TrimStart(generateTrimList("\"documents:"));
                Char[] simpleTrim = generateTrimList("[]");
                for (int i = 0; i < documentsJSON.Length; i++) {
                    ExcelWorkbook wb = new ExcelWorkbook();
                    documentsJSON[i] = extractComplexFromJSON(documentsJSON[i].TrimStart(simpleTrim), wb, out error);
                    if (documentsJSON[i] != null) {
                        String[] simpleLines = documentsJSON[i].Split(',');
                        for (int j = 0; j < simpleLines.Length; j++) {
                            if (simpleLines[j].IndexOf(":") > -1) {
                                String[] pairs = simpleLines[j].Split(':');
                                wb.simpleValues.Add(pairs[0].Trim(simpleTrim).Trim('"'), pairs[1].Trim(simpleTrim).Trim('"'));
                                wb.unParsedSimpleValues.Add(pairs[0].Trim(simpleTrim).Trim('"'));
                            }
                        }
                        excels.Add(wb);
                    } else {
                        return "[ExcelHandlerJSON/parse] " + error;
                    }
                }
                wbs = excels;
                return null;
            } catch (Exception ex) {
                wbs = null;
                return "[ExcelHandlerJSON/parse] " + ex.Message;
            }
        }
        private string extractComplexFromJSON(String json, ExcelWorkbook wb, out string error) {
            try {
                StringBuilder sb = new StringBuilder(json);
                List<String> enumrartions = new List<String>();
                List<String> lists = new List<String>();
                List<String> tables = new List<String>();
                int bracesLevel = 0, bracketLevel = 0;
                for (int i = 0; i < sb.Length - 1; i++) {
                    if (sb[i] == '{') {
                        bracesLevel++;
                        if ((bracesLevel == 1) && (bracketLevel == 0)) {
                            String enumeration = "";
                            int j = i - 1;
                            while ((j > 0) && (sb[j] != ',') && (sb[j] != '{')) {
                                j--;
                            }
                            if (j > 0) {
                                j++;
                            }
                            while ((j < sb.Length) && (sb[j] != '}')) {
                                enumeration += sb[j];
                                sb[j] = ' ';
                                j++;
                            }
                            if (j < sb.Length - 1) {
                                sb[j] = ' ';
                                if (sb[j + 1] == ',') {
                                    sb[j + 1] = ' ';
                                }
                                bracesLevel--;
                                enumrartions.Add(enumeration);
                            }
                        }
                    } else if (sb[i] == '}') {
                        bracesLevel--;
                    } else if (sb[i] == '[') {
                        bracketLevel++;
                        if (bracketLevel == 1) {
                            int j = i + 1;
                            while ((j < sb.Length) && (sb[j] != '{') && (sb[j] != '[')) {
                                j++;
                            }
                            if (j < sb.Length) {
                                char b = sb[j];
                                j = i - 1;
                                while ((j > 0) && (sb[j] != ',') && (sb[j] != '{')) {
                                    j--;
                                }
                                if (b == '{') {
                                    String item = "";
                                    if (j > 0) {
                                        j++;
                                    }
                                    while ((j < sb.Length) && (sb[j] != ']')) {
                                        item += sb[j];
                                        sb[j] = ' ';
                                        j++;
                                    }
                                    if (j < sb.Length - 1) {
                                        sb[j] = ' ';
                                        if (sb[j + 1] == ',') {
                                            sb[j + 1] = ' ';
                                        }
                                        lists.Add(item);
                                    }
                                    bracketLevel--;
                                } else if (b == '[') {
                                    String item = "";
                                    if (j > 0) {
                                        j++;
                                    }
                                    bool endbracket = false;
                                    int closepos = j, start = j;
                                    while ((j < sb.Length) && (!endbracket || (sb[j] != '}') && (sb[j] != '"'))) {
                                        if (sb[j] == ']') {
                                            closepos = j;
                                            endbracket = true;
                                        } else if (sb[j] == '[') {
                                            endbracket = false;
                                        }
                                        j++;
                                    }
                                    for (int n = start; n <= closepos; n++) {
                                        item += sb[n];
                                        sb[n] = ' ';
                                    }
                                    if ((j != sb.Length - 1) && (sb[closepos + 1] == ',')) {
                                        sb[closepos + 1] = ' ';
                                    }
                                    tables.Add(item);
                                    bracketLevel--;
                                }
                            }
                        }
                    } else if (sb[i] == ']') {
                        bracketLevel--;
                    }
                }
                wb.enumeratedValues = convertJSONToEnumeration(enumrartions, generateTrimList("[],:\""));
                foreach (KeyValuePair<String, ExcelEnumeration> item in wb.enumeratedValues) {
                    wb.unParsedEnumeratedValues.Add(item.Key);
                }
                wb.tables = convertJSONToTable(tables, generateTrimList("[],:"));
                foreach (ExcelTable item in wb.tables) {
                    wb.unParsedtables.Add(item.name);
                }
                error = null;
                return sb.ToString();
            } catch (Exception ex) {
                error = "[ExcelHandlerJSON/extractComplexFromJSON] " + ex.Message;
                return null;
            }
        }

        private Dictionary<String, ExcelEnumeration> convertJSONToEnumeration(List<String> enumerationsJSON, char[] enumTrim) {
            try {
                Dictionary<String, ExcelEnumeration> enumeratedValues = new Dictionary<string, ExcelEnumeration>();
                char[] valueTrim = generateTrimList("[],:");
                foreach (String item in enumerationsJSON) {
                    String[] datas = item.Trim(valueTrim).Split('{');
                    String name = datas[0].Trim(enumTrim);
                    ExcelEnumeration enumeration = new ExcelEnumeration();
                    if (datas[1].IndexOf(']') == -1) {
                        String[] subValues = datas[1].Trim(valueTrim).Split(new String[] { "\"values\"" }, StringSplitOptions.RemoveEmptyEntries);
                        String[] arrayValues = subValues[1].Trim(valueTrim).Split(',');
                        for (int i = 0; i < arrayValues.Length; i++) {
                            arrayValues[i] = arrayValues[i].Trim(enumTrim);
                        }
                        enumeration.values = arrayValues;
                        enumeration.separator = subValues[0].Trim(valueTrim).Split(':')[1].Trim(valueTrim).Trim('"');
                    } else {
                        String[] subValues = datas[1].Trim(valueTrim).Split(']');
                        String arrayValue = subValues[0].Split(':')[1].Trim(enumTrim);
                        String[] arrayValues = arrayValue.Split(',');
                        for (int i = 0; i < arrayValues.Length; i++) {
                            arrayValues[i] = arrayValues[i].Trim(enumTrim);
                        }
                        enumeration.values = arrayValues;
                        enumeration.separator = subValues[1].Split(':')[1].Trim(valueTrim).Trim('"');
                    }
                    enumeratedValues[name] = enumeration;
                }
                return enumeratedValues;
            } catch (Exception ex) {
                throw;
            }
        }

        private List<ExcelTable> convertJSONToTable(List<String> tablesJSON, char[] tableTrim) {
            try {
                List<ExcelTable> tableValues = new List<ExcelTable>();
                foreach (String item in tablesJSON) {
                    ExcelTable table = new ExcelTable();
                    String[] lines = item.Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                    table.name = lines[0].Trim(tableTrim).Trim('"');
                    List<String[]> linesArray = new List<string[]>();
                    for (int i = 1; i < lines.Length; i++) {
                        if (lines[i].IndexOf(',') > -1) {
                            List<String> valuesArray = new List<string>();
                            String[] values = lines[i].Trim(tableTrim).Split(',');
                            foreach (String value in values) {
                                valuesArray.Add(value.Trim(tableTrim).Trim('"'));
                            }
                            linesArray.Add(valuesArray.ToArray());
                        }
                    }
                    table.items = linesArray.ToArray();
                    tableValues.Add(table);
                }
                return tableValues;
            } catch (Exception ex) {
                throw;
            }
        }

        private String getDataJSON(String path, out string error) {
            StreamReader reader = null;
            try {
                reader = new StreamReader(path);
                String items = reader.ReadToEnd();
                reader.Close();
                error = null;
                return items;
            } catch (Exception ex) {
                if (reader != null) {
                    reader.Close();
                }
                error = "[ExcelHandlerJSON/parse] " + ex.Message;
                return null;
            }
        }

        public Char[] generateTrimList(string addition = null) {
            try {
                List<Char> list = new List<char>();
                list.Add(' ');
                list.Add('{');
                list.Add('}');
                list.Add('\t');
                if (addition != null) {
                    for (int i = 0; i < addition.Length; i++) {
                        list.Add(addition[i]);
                    }
                }
                Char[] enter = Environment.NewLine.ToCharArray();
                for (int i = 0; i < enter.Length; i++) {
                    list.Add(enter[i]);
                }
                return list.ToArray();
            } catch (Exception ex) {
                throw;
            }
        }
    }
}
