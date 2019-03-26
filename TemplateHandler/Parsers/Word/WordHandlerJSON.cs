using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace TemplateHandler.Parsers {
    class WordHandlerJSON : WordHandler {
        public override string parse(String path) {
            List<WordDocument> words = new List<WordDocument>(); ;
            try {
                String json = getDataJSON(path);
                if (json == null) {
                    docs = null;
                    return "JSON cannot be read";
                }
                StringBuilder sb = new StringBuilder(json);
                int bracesLevel = 0;
                for(int i = 0; i < sb.Length - 1; i++) {
                    if (sb[i] == '{') {
                        bracesLevel++;
                    } else if(sb[i] == '}'){
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
                for(int i = 0; i < documentsJSON.Length; i++) {
                    WordDocument doc = new WordDocument();
                    documentsJSON[i] = extractComplexFromJSON(documentsJSON[i].TrimStart(simpleTrim), doc);
                    String[] simpleLines = documentsJSON[i].Split(',');
                    for (int j = 0; j < simpleLines.Length; j++) {
                        if (simpleLines[j].IndexOf(":") > -1) {
                            String[] pairs = simpleLines[j].Split(':');
                            doc.simpleValues.Add(pairs[0].Trim(simpleTrim).Trim('"'), pairs[1].Trim(simpleTrim).Trim('"'));
                        }
                    }
                    words.Add(doc);
                }
                docs = words;
                return null;
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                docs = null;
                return ex.Message;
            }
        }

        private string extractComplexFromJSON(String json, WordDocument doc) {
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
                                if ((j!=sb.Length-1) && (sb[closepos + 1] == ',')) {
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
            doc.enumeratedValues = convertJSONToEnumeration(enumrartions, generateTrimList("[],:\""));
            doc.tables = convertJSONToTable(tables, generateTrimList("[],:"));
            doc.lists = convertJSONToList(lists, generateTrimList("[],:\""));
            return sb.ToString();
        }
        
        private Dictionary<String, WordEnumeration> convertJSONToEnumeration(List<String> enumerationsJSON, char[] enumTrim) {
            Dictionary<String, WordEnumeration> enumeratedValues = new Dictionary<string, WordEnumeration>();
            char[] valueTrim = generateTrimList("[],:");
            foreach (String item in enumerationsJSON) {
                String[] datas = item.Trim(valueTrim).Split('{');
                String name = datas[0].Trim(enumTrim);
                WordEnumeration enumeration = new WordEnumeration();
                if (datas[1].IndexOf(']')==-1){
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
                    for(int i = 0; i < arrayValues.Length; i++) {
                        arrayValues[i] = arrayValues[i].Trim(enumTrim);
                    }
                    enumeration.values = arrayValues;
                    enumeration.separator = subValues[1].Split(':')[1].Trim(valueTrim).Trim('"');
                }
                enumeratedValues[name] = enumeration;
            }
            return enumeratedValues;
        }

        private List<WordTable> convertJSONToTable(List<String> tablesJSON, char[] tableTrim) {
            List<WordTable> tableValues = new List<WordTable>();
            foreach (String item in tablesJSON) {
                WordTable table = new WordTable();
                String[] lines = item.Split(new char[] { '[' },StringSplitOptions.RemoveEmptyEntries);
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
        }

        private List<WordList> convertJSONToList(List<String>listsJSON, Char[] listTrim) {
            List<WordList> lists = new List<WordList>();
            foreach (String item in listsJSON) {
                String[] lines = item.Split('{');
                WordList listItem = new WordList();
                listItem.name = lines[0].Trim(listTrim);
                for(int i = 1; i < lines.Length; i++) {
                    WordListItem subItem = new WordListItem();
                    String[] subValues = lines[i].Trim(listTrim).Split(',');
                    int levelIndex = 0, valueIndex = 1;
                    if (subValues[0].IndexOf("level") == -1) {
                        levelIndex = 1;
                        valueIndex = 0;
                    }
                    String[] values = subValues[levelIndex].Split(':');
                    subItem.level = Int32.Parse(values[1].Trim(listTrim));
                    values = subValues[valueIndex].Split(':');
                    subItem.value = values[1].Trim(listTrim);
                    listItem.items.Add(subItem);
                }
                lists.Add(listItem);
            }
            return lists;
        }

        private String getDataJSON(String path){
            StreamReader reader=null;
            try {
                reader = new StreamReader(path);
                String items = reader.ReadToEnd();
                reader.Close();
                return items;
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                if (reader != null) {
                    reader.Close();
                }
                return null;
            }
        }
        
        public Char[] generateTrimList(string addition = null) {
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
        }
    }
}
