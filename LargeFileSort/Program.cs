using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace LargeFileSort {
    class Program {
        static void Main(string[] args) {

            //-i -o
            var fileInfo = new FileInfo(@"D:\dl\BigSort\bigfile.txt.gz");
            Decompress(fileInfo);
        }
       
        public static void Decompress(FileInfo fileToDecompress) {
            int tempFileCount = 0;
            StringBuilder sb = new StringBuilder();
            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                    using (StreamReader streamReader = new StreamReader(decompressionStream)) {
                    
                        
                        var listOfStrings = new List<string>();
                        while (!streamReader.EndOfStream) {

                            for (int i = 0; i < 5000 && !streamReader.EndOfStream; i++) {
                                listOfStrings.Add(streamReader.ReadLine());
                            }

                            listOfStrings.Sort();

                            File.WriteAllLines($@"D:\dl\BigSort\file_{tempFileCount}", listOfStrings);
                            tempFileCount++;
                            listOfStrings.Clear();
                        }
                    }
                }

            }

            var mergedLinesArray = new List<string>();
            var fileArray = new List<StreamReader>();
            var lineFileLookup = new Dictionary<string, StreamReader>();

            for (int i = 0; i < tempFileCount; i++) {
                var fileInfo = new FileInfo($@"D:\dl\BigSort\file_{i}");
                var fileStream = fileInfo.OpenRead();
                var streamReader = new StreamReader(fileStream);
                var firstLine = streamReader.ReadLine();

                mergedLinesArray.Add(firstLine);
                fileArray.Add(streamReader);
                lineFileLookup.Add(firstLine, streamReader);
            }

            mergedLinesArray.Sort();

            while (true) {
                var smallest = mergedLinesArray[0];
                sb.Append(smallest[28]);
                Console.Write(smallest[28]);

                mergedLinesArray.Remove(smallest);

                var nextLineStream = lineFileLookup[smallest];
                lineFileLookup.Remove(smallest);

                
                var nextLine = nextLineStream.ReadLine();

                if (nextLine == null) {
                    fileArray.Remove(nextLineStream);
                    if (fileArray.Count == 0) {
                        var wordCount = sb.ToString().Split(' ').Where(x=>x.Trim() != string.Empty).ToArray().Length;
                        return;
                    }
                }

                if (nextLine == null)
                    continue;

                lineFileLookup.Add(nextLine, nextLineStream);

                mergedLinesArray.Add(nextLine);
                mergedLinesArray.Sort();         
            }

        }
    }
}
