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

            if (args.Length != 1) {
                Console.WriteLine("Supply the path to bigfile.txt.gz as the some cmd argument.");
                Console.ReadKey();
                return;
            }

            var rootDir = args[0];
            var path = Path.Combine(rootDir, "bigfile.txt.gz");
            if (!File.Exists(path)) {
                Console.WriteLine("Could not find bigfile.txt.gz at that path.");
                Console.ReadKey();
                return;
            }

#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var sb = FindAnswer(new FileInfo(path), rootDir);
#if DEBUG
            stopWatch.Stop();
            // 619a1508fa3690f2419ce6b7c5a8e796 0:37
            Console.WriteLine($"{ComputeHash(sb)} {stopWatch.Elapsed}");
            Console.ReadKey();
#endif
            Cleanup(rootDir);
        }

        static StringBuilder FindAnswer(FileInfo fileToDecompress, string path) {

            var tempFileCount = DecompressToTempFiles(fileToDecompress, path);

            var stringBuilder =  BuildAnswerFromFiles(tempFileCount, path);

            WriteCompressedAnswer(stringBuilder, path);

            return stringBuilder;
        }

        static int DecompressToTempFiles(FileInfo fileToDecompress, string path) {
            int tempFileCount1 = 0;

            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                    using (StreamReader streamReader = new StreamReader(decompressionStream)) {

                        var listOfStrings = new List<string>();
                        while (!streamReader.EndOfStream) {

                            for (int i = 0; i < 5000 && !streamReader.EndOfStream; i++) 
                                listOfStrings.Add(streamReader.ReadLine());

                            listOfStrings.Sort();

                            File.WriteAllLines(Path.Combine(path, $@"file_{tempFileCount1}"), listOfStrings);
                            tempFileCount1++;
                            listOfStrings.Clear();
                        }
                    }
                }

            }

            return tempFileCount1;
        }

        static StringBuilder BuildAnswerFromFiles(int tempFileCount, string path) {
            var mergedLinesArray = new List<string>();
            var fileArray = new List<StreamReader>();
            var lineFileLookup = new Dictionary<string, StreamReader>();

            for (int i = 0; i < tempFileCount; i++) {
                var fileInfo = new FileInfo(Path.Combine(path, $"file_{i}"));
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
                //Console.Write(smallest[28]);

                mergedLinesArray.Remove(smallest);

                var nextLineStream = lineFileLookup[smallest];
                lineFileLookup.Remove(smallest);

                var nextLine = nextLineStream.ReadLine();

                if (nextLine == null) {
                    fileArray.Remove(nextLineStream);
                    if (fileArray.Count == 0)
                        return sb;
                }

                if (nextLine == null)
                    continue;

                lineFileLookup.Add(nextLine, nextLineStream);

                int insertionIndex = mergedLinesArray.BinarySearch(nextLine);
                if (mergedLinesArray.BinarySearch(nextLine) < 0)
                    mergedLinesArray.Insert(~insertionIndex, nextLine);
            }
        }

        static void Cleanup(string path) {
            var tempFiles = Directory.GetFiles(path).Where(x => x.Contains("file_"));
            foreach (string tempFile in tempFiles) {
                File.Delete(tempFile);
            }
            
        }
        static string ComputeHash(StringBuilder stringBuilder) {
            using (MD5 md5Hash = MD5.Create()) {
                byte[] data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
                StringBuilder sBuilder = new StringBuilder();

                foreach (byte t in data)
                    sBuilder.Append(t.ToString("x2"));

                return sBuilder.ToString();

            }
        }

        static void WriteCompressedAnswer(StringBuilder stringBuilder, string path) {
            using (FileStream fileToCompress = File.Create(Path.Combine(path, "answer.gz"))) {
                using (GZipStream compressionStream = new GZipStream(fileToCompress, CompressionMode.Compress)) {
                    compressionStream.Write(Encoding.ASCII.GetBytes(stringBuilder.ToString()), 0, stringBuilder.Length);
                }
            }

        }
    }
}
