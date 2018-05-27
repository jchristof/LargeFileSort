using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LargeFileSort {
    class Program {

        static void Main(string[] args) {

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var fileInfo = new FileInfo(@"D:\dl\BigSort\bigfile.txt.gz");
            var sb = Decompress(fileInfo);
            stopWatch.Stop();
            var elapsed = stopWatch.Elapsed;
            using (MD5 md5Hash = MD5.Create()) {
                byte[] data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(sb.ToString()));
                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++) {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                //hash = "619a1508fa3690f2419ce6b7c5a8e796"
                var hash = sBuilder.ToString();
                
            }
        }

        static int DecompressToTempFiles(FileInfo fileToDecompress) {
            int tempFileCount1 = 0;

            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                    using (StreamReader streamReader = new StreamReader(decompressionStream)) {

                        var listOfStrings = new List<string>();
                        while (!streamReader.EndOfStream) {

                            for (int i = 0; i < 5000 && !streamReader.EndOfStream; i++) {
                                listOfStrings.Add(streamReader.ReadLine());
                            }

                            listOfStrings.Sort();

                            File.WriteAllLines($@"D:\dl\BigSort\file_{tempFileCount1}", listOfStrings);
                            tempFileCount1++;
                            listOfStrings.Clear();
                        }
                    }
                }

            }

            return tempFileCount1;
        }

        public static StringBuilder Decompress(FileInfo fileToDecompress) {
            
            int tempFileCount = DecompressToTempFiles(fileToDecompress);
           

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

            var sb = new StringBuilder();

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
                        //var wordCount = sb.ToString().Split(' ').Where(x=>x.Trim() != string.Empty).ToArray().Length;

                        using (FileStream fileToCompress = File.Create(@"D:\dl\BigSort\answer.gz")) {
                            using (GZipStream compressionStream = new GZipStream(fileToCompress, CompressionMode.Compress)) {
                                compressionStream.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, sb.Length);
                            }
                        }
                            
                        return sb;
                    }
                }

                if (nextLine == null)
                    continue;

                lineFileLookup.Add(nextLine, nextLineStream);

                int insertionIndex = mergedLinesArray.BinarySearch(nextLine);
                if (insertionIndex < 0) {
                    mergedLinesArray.Insert(~insertionIndex, nextLine);
                }
    
            }

        }
    }
}
