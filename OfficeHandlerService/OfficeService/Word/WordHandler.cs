using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace OfficeHandlerService.Office {
    public abstract class WordHandler: OfficeHandler {

        protected List<WordDocument> docs = null;

        override public abstract string parse(String path);

        override public string execute(String path, String destination) {
            if (docs != null) {
                Word.Application app = null;
                Word.Document doc = null;
                string error = null;
                try {
                    String[] paths = copyBaseFile(path, destination, docs.Count, out error);
                    if (paths != null) {
                        Dictionary<int,List<KeyValuePair<int, string>>> diffInTemplate = new Dictionary<int,List<KeyValuePair<int, string>>>();
                        app = new Word.Application();
                        for (int i = 0; i < paths.Length; i++) {
                            doc = app.Documents.Open(paths[i]);
                            Word.Range range = doc.Content;
                            docs[i].unParsedSimpleValues = replaceSimpleValues(doc, docs[i].simpleValues,docs[i].unParsedSimpleValues);
                            docs[i].unParsedEnumeratedValues = replaceEnumeratedValues(doc, docs[i].enumeratedValues,docs[i].unParsedEnumeratedValues);
                            docs[i].unParsedtables = replaceTableValues(doc, docs[i].tables, docs[i].unParsedtables);
                            docs[i].unParsedlists = replaceListValues(doc, docs[i].lists, docs[i].unParsedlists);
                            List<KeyValuePair<int, string>> diff = new List<KeyValuePair<int, string>>();
                            int parag = 0;
                            foreach (Word.Paragraph paragraph in doc.Paragraphs) {
                                parag++;
                                int startPos = paragraph.Range.Text.IndexOf("<#<");
                                while (startPos > -1) {
                                    int endPos = paragraph.Range.Text.IndexOf(">#>", startPos);
                                    if (endPos > -1) {
                                        diff.Add(new KeyValuePair<int, string>(parag,paragraph.Range.Text.Substring(startPos + 3, endPos - startPos - 3)));
                                    }
                                    startPos = paragraph.Range.Text.IndexOf("<#<", endPos);
                                }
                            }
                            diffInTemplate[i + 1] = diff;
                            doc.Save();
                        }
                        writeDifferencesToFile(destination, diffInTemplate);
                        app.Quit();
                        int j = 0;
                        while ((j < 100) && (Directory.GetFiles(destination).Length != docs.Count)) {
                            j++;
                            Thread.Sleep(10);
                        }
                        return null;
                    } else {
                        return "[WordHandler/execute] " + error;
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    if (doc != null) {
                        doc.Save();
                    }
                    if (app != null) {
                        app.Quit();
                    }
                    return "[WordHandler/execute] " + ex.Message;
                }
            } else {
                return "[WordHandler/execute] There are no documents.";
            }
        }

        private void writeDifferencesToFile(string destination, Dictionary<int, List<KeyValuePair<int, string>>> diffInTemplate) {
            try {
                FileStream differences = new FileStream(destination + "\\differences.txt", FileMode.Create);
                StreamWriter writer = new StreamWriter(differences);
                if (diffInTemplate.Count > 0) {
                    writer.WriteLine("Template:");
                    foreach (KeyValuePair<int, List<KeyValuePair<int, string>>> item in diffInTemplate) {
                        writer.WriteLine("\tSolution " + item.Key + ": " + item.Value.Count + " unparsed tag");
                        foreach (KeyValuePair<int, string> tag in item.Value) {
                            writer.WriteLine("\t\tparagraph: " + tag.Key + ", tag: " + tag.Value);
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine();
                } else {
                    writer.WriteLine("Template:\nAll tags successfully parsed.\n");
                }
                bool allParsed = true;
                writer.WriteLine("Data: ");
                for (int i = 0; i < docs.Count; i++) {
                    writer.WriteLine("\tDocumet: " + (i + 1));
                    if (docs[i].unParsedSimpleValues.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed simple values: " + docs[i].unParsedSimpleValues.Count);
                        foreach (string tag in docs[i].unParsedSimpleValues) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll simple values parsed");
                    }
                    if (docs[i].unParsedEnumeratedValues.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed enumerated values: " + docs[i].unParsedEnumeratedValues.Count);
                        foreach (string tag in docs[i].unParsedEnumeratedValues) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll enumerated values parsed");
                    }
                    if (docs[i].unParsedtables.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed tables: " + docs[i].unParsedtables.Count);
                        foreach (string tag in docs[i].unParsedtables) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll tables parsed");
                    }
                    if (docs[i].unParsedlists.Count > 0) {
                        allParsed = false;
                        writer.WriteLine("\t\tUnparsed lists: " + docs[i].unParsedlists.Count);
                        foreach (string tag in docs[i].unParsedlists) {
                            writer.WriteLine("\t\t\t" + tag);
                        }
                    } else {
                        writer.WriteLine("\t\tAll lists parsed");
                    }
                    writer.WriteLine();
                }
                if (allParsed) {
                    writer.WriteLine("All tags successfully parsed.\n");
                }
                writer.Close();
            } catch(Exception ex) {
                throw;
            }
        }

        private List<string> replaceSimpleValues(Word.Document doc, Dictionary<String, String> simpleValues, List<string> unParsed) {
            try {
                Word.Range range = doc.Content;
                foreach (KeyValuePair<String, String> item in simpleValues) {
                    range.Find.ClearFormatting();
                    range.Find.Execute(FindText: "<#<" + item.Key + ">#>", ReplaceWith: item.Value, Replace: Word.WdReplace.wdReplaceAll);
                    if (range.Find.Found && unParsed.Contains(item.Key)) {
                        unParsed.Remove(item.Key);
                    }
                }
                return unParsed;
            }catch (Exception ex) {
                throw;
            }
        }

        private List<string> replaceEnumeratedValues(Word.Document doc, Dictionary<String, WordEnumeration> enumeratedValues, List<string> unParsed) {
            try {
                Word.Range range = doc.Content;
                foreach (KeyValuePair<String, WordEnumeration> item in enumeratedValues) {
                    range.Find.ClearFormatting();
                    range.Find.Execute(FindText: "<#<" + item.Key + ">#>", ReplaceWith: item.Value.ToString(), Replace: Word.WdReplace.wdReplaceAll);
                    if (range.Find.Found && unParsed.Contains(item.Key)) {
                        unParsed.Remove(item.Key);
                    }
                }
                return unParsed;
            } catch (Exception ex) {
                throw;
            }
        }

        private List<string> replaceTableValues(Word.Document doc, List<WordTable> tables, List<string> unParsed) {
            try {
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
                        if ((selectedTables.Count > 0) && (unParsed.Contains(item.name))){
                            unParsed.Remove(item.name);
                        }
                    }
                }
                return unParsed;
            } catch (Exception ex) {
                throw;
            }
        }

        private List<string> replaceListValues(Word.Document doc, List<WordList> lists, List<string> unParsed) {
            try {
                if ((lists.Count > 0) && (doc.Lists.Count > 0)) {
                    List<Word.List> wordLists = new List<Word.List>();
                    foreach (Word.List list in doc.Lists) {
                        wordLists.Add(list);
                    }
                    foreach (WordList item in lists) {
                        List<int[]> selectedLists = new List<int[]>();
                        Dictionary<int, int> move = new Dictionary<int, int>();
                        for (int i = 0; i < wordLists.Count; i++) {
                            for (int j = 0; j < wordLists[i].ListParagraphs.Count; j++) {
                                if (wordLists[i].ListParagraphs[j + 1].Range.Text.IndexOf("<#<" + item.name + ">#>") != -1) {
                                    int[] list = new int[2];
                                    list[0] = i;
                                    list[1] = j + 1;
                                    move[i] = 0;
                                    selectedLists.Add(list);
                                }
                            }
                        }
                        foreach (int[] list in selectedLists) {
                            Word.Paragraph paragraph = wordLists[list[0]].ListParagraphs[list[1] + move[list[0]]];
                            Word.ParagraphFormat format = paragraph.Format;
                            paragraph.Range.Find.ClearFormatting();
                            paragraph.Range.Find.Execute(FindText: "<#<" + item.name + ">#>", ReplaceWith: item.items[item.items.Count - 1].value, Replace: Word.WdReplace.wdReplaceOne);
                            paragraph.Range.SetListLevel((short)(item.items[item.items.Count - 1].level + 1));
                            for (int j = item.items.Count - 2; j >= 0; j--) {
                                paragraph.Range.InsertParagraphBefore();
                                paragraph = paragraph.Previous();
                                paragraph.Range.Text = item.items[j].value + '\r';
                                paragraph = paragraph.Previous();
                                paragraph.Format = format;
                                paragraph.Range.SetListLevel((short)(item.items[j].level + 1));
                            }
                            move[list[0]] += item.items.Count - 1;
                        }
                        if((selectedLists.Count>0) && (unParsed.Contains(item.name))) {
                            unParsed.Remove(item.name);
                        }
                    }
                }
                return unParsed;
            } catch (Exception ex) {
                throw;
            }
        }

        override public MemoryStream toStream(out string error) {
            try {
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                if (docs != null) {
                    foreach (WordDocument doc in docs) {
                        writer.WriteLine("Document:");
                        writer.WriteLine("\tSimple values:");
                        foreach (KeyValuePair<String, String> values in doc.simpleValues) {
                            writer.WriteLine("\t\t{0}: {1}", values.Key, values.Value);
                        }
                        writer.WriteLine("\tEnumerated values:");
                        foreach (KeyValuePair<String, WordEnumeration> item in doc.enumeratedValues) {
                            writer.WriteLine("\t\t" + item.Key + ": " + item.Value);
                        }
                        writer.WriteLine("\tList values:");
                        foreach (WordList item in doc.lists) {
                            writer.WriteLine("\t\t" + item.name + ":");
                            foreach (WordListItem listItem in item.items) {
                                writer.WriteLine("\t\t\tlevel: {0}, value: {1}", listItem.level, listItem.value);
                            }
                        }
                        writer.WriteLine("\tTable values:");
                        foreach (WordTable item in doc.tables) {
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
                    writer.Write("There are no documents");
                }
                writer.Flush();
                stream.Position = 0;
                error = null;
                return stream;
            } catch (Exception ex) {
                error = "[WordHandler/toStream] " + ex.Message;
                return null;
            }
        }

        protected class WordDocument {
            public Dictionary<String, String> simpleValues = new Dictionary<string, string>();
            public Dictionary<String, WordEnumeration> enumeratedValues = new Dictionary<string, WordEnumeration>();
            public List<WordTable> tables = new List<WordTable>();
            public List<WordList> lists = new List<WordList>();
            public List<string> unParsedSimpleValues = new List<string>();
            public List<string> unParsedEnumeratedValues = new List<string>();
            public List<string> unParsedtables = new List<string>();
            public List<string> unParsedlists = new List<string>();
        }

        protected class WordTable {
            public String name { get; set; }
            public String[][] items { get; set; }
        }

        protected class WordList {
            public string name { get; set; }
            public List<WordListItem> items = new List<WordListItem>();
        }

        protected class WordListItem {
            public String value { get; set; }
            public int level { get; set; }
        }

        protected class WordEnumeration {
            public String[] values { get; set; }
            public String separator { get; set; }
            public override string ToString() {
                return String.Join(separator + " ", values);
            }
        }
    }
}
