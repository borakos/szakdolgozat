using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateHandler.Parsers {
    abstract class OfficeHandler {

        public abstract string parse(String path);

        public abstract string execute(String path, String destination);

        public abstract MemoryStream toStream();

        protected String[] copyBaseFile(String path, String destinationFolder, int piece) {
            List<String> paths = new List<string>();
            String[] pathArray = path.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            int filenamepos = pathArray.Length - 1;
            String[] filename = pathArray[filenamepos].Split('.');
            if (!Directory.Exists(destinationFolder)) {
                Directory.CreateDirectory(destinationFolder);
            }
            for (int i = 1; i <= piece; i++) {
                string solutionpath = destinationFolder + "\\" + filename[0] + "_solution_" + i + "." + filename[1];
                paths.Add(solutionpath);
                File.Copy(path, solutionpath, true);
            }
            return paths.ToArray();
        }
    }
}
