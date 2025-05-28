using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UglyToad.PdfPig;
using Serilog;

namespace Processor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 配置 Serilog 控制台日志
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            string rootDir = @"C:\迅雷下载\Ctp";
            ExtractPdfTextToTxt(rootDir);
            ConvertGb2312FilesToUtf8(rootDir);
            
        }

        public static void ExtractPdfTextToTxt(string directory)
        {
            Log.Information("Starting PDF to TXT extraction in directory: {Directory}", directory);

            var pdfFiles = Directory.GetFiles(directory, "*.pdf", SearchOption.AllDirectories);
            Log.Information("Found {Count} PDF files.", pdfFiles.Length);

            foreach (var pdfFile in pdfFiles)
            {
                string txtFile = Path.ChangeExtension(pdfFile, ".txt");
                Log.Debug("Processing file: {PdfFile}", pdfFile);

                if (File.Exists(txtFile))
                {
                    var attributes = File.GetAttributes(txtFile);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        Log.Debug("TXT file is read-only. Removing read-only attribute: {TxtFile}", txtFile);
                        File.SetAttributes(txtFile, attributes & ~FileAttributes.ReadOnly);
                    }
                    Log.Debug("Deleting existing TXT file: {TxtFile}", txtFile);
                    File.Delete(txtFile);
                }
                try
                {
                    using (var pdf = PdfDocument.Open(pdfFile))
                    using (var writer = new StreamWriter(txtFile, false, System.Text.Encoding.Default))
                    {
                        foreach (var page in pdf.GetPages())
                        {
                            writer.WriteLine(page.Text);
                        }
                    }
                    Log.Information("Successfully extracted text to: {TxtFile}", txtFile);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to process {PdfFile}", pdfFile);
                }
            }

            Log.Information("PDF to TXT extraction completed.");
        }

        /// <summary>
        /// Traverses the specified directory and its subdirectories, finds files with specific extensions,
        /// detects if they are encoded in GB2312, and converts them to UTF-8 encoding in the same directory.
        /// </summary>
        /// <param name="directory">The root directory to start traversal.</param>
        public static void ConvertGb2312FilesToUtf8(string directory)
        {
            Log.Information("Starting GB2312 to UTF-8 conversion in directory: {Directory}", directory);

            string[] extensions = { ".h", ".txt", ".htm", ".html", ".cpp", ".xml" };
            var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

            int processed = 0, skipped = 0, converted = 0, failed = 0;

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName.Contains(".utf8", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug("Skipping already UTF-8 file: {File}", file);
                    skipped++;
                    continue;
                }

                try
                {
                    Log.Debug("Processing file: {File}", file);

                    // 尝试以 GB2312 读取文件
                    string content;
                    Encoding gb2312 = Encoding.GetEncoding("GB2312");
                    using (var reader = new StreamReader(file, gb2312, detectEncodingFromByteOrderMarks: true))
                    {
                        content = reader.ReadToEnd();
                    }

                    // 再次用 UTF-8 读取，如果内容一致则说明本来就是 UTF-8，无需转换
                    string utf8Content;
                    using (var reader = new StreamReader(file, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                    {
                        utf8Content = reader.ReadToEnd();
                    }

                    if (content == utf8Content)
                    {
                        Log.Debug("File is already UTF-8 encoded: {File}", file);
                        skipped++;
                        continue;
                    }

                    // 另存为 UTF-8，文件名加 .utf8 以防覆盖
                    string utf8File = Path.Combine(
                        Path.GetDirectoryName(file)!,
                        Path.GetFileNameWithoutExtension(file) + ".utf8" + Path.GetExtension(file)
                    );
                    File.WriteAllText(utf8File, content, Encoding.UTF8);
                    Log.Information("Converted file to UTF-8: {File} -> {Utf8File}", file, utf8File);
                    converted++;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to convert file: {File}", file);
                    failed++;
                }
                processed++;
            }

            Log.Information("GB2312 to UTF-8 conversion completed. Processed: {Processed}, Converted: {Converted}, Skipped: {Skipped}, Failed: {Failed}",
                processed, converted, skipped, failed);
        }
    }
}
