using System.Text;
using Microsoft.Extensions.Logging;
using Ude;

namespace TextEncodingConverter;

internal sealed partial class Program
{
    private static readonly ILogger Logger = CreateLogger();
    
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".h", ".cpp", ".xml", ".hhc", ".hhk"
    };

    // 版本目录的匹配模式
    private const string VersionDirectoryPattern = "v*";
    
    // 需要处理的源目录和目标目录映射
    private static readonly Dictionary<string, string> DirectoryMappings = new()
    {
        { Path.Combine("chm", "Original"), Path.Combine("chm", "Utf8") },
        { Path.Combine("api", "Original"), Path.Combine("api", "Utf8") },
        { Path.Combine("demo", "Original"), Path.Combine("demo", "Utf8") }
    };

    // Logger message delegates for better performance
    [LoggerMessage(Level = LogLevel.Information, Message = "开始文件编码转换程序")]
    private static partial void LogProgramStart(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "文件编码转换完成")]
    private static partial void LogProgramComplete(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "程序运行出错")]
    private static partial void LogProgramError(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "源目录不存在: {sourcePath}")]
    private static partial void LogSourceDirectoryNotExists(ILogger logger, string sourcePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "源目录: {sourcePath}")]
    private static partial void LogSourceDirectory(ILogger logger, string sourcePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "目标目录: {targetPath}")]
    private static partial void LogTargetDirectory(ILogger logger, string targetPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "创建目标目录: {targetPath}")]
    private static partial void LogCreateTargetDirectory(ILogger logger, string targetPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "处理文件: {fileName}")]
    private static partial void LogProcessingFile(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "检测到文件 {fileName} 的编码: {encoding}")]
    private static partial void LogDetectedEncoding(ILogger logger, string fileName, string encoding);

    [LoggerMessage(Level = LogLevel.Warning, Message = "无法检测文件 {fileName} 的编码，使用UTF-8编码")]
    private static partial void LogEncodingDetectionFailed(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "文件转换完成: {fileName} -> {targetPath}")]
    private static partial void LogFileConversionComplete(ILogger logger, string fileName, string targetPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "处理文件 {fileName} 时出错")]
    private static partial void LogFileProcessingError(ILogger logger, Exception exception, string fileName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "检测文件 {filePath} 编码时出错")]
    private static partial void LogEncodingDetectionError(ILogger logger, Exception exception, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始处理版本目录: {versionDirectory}")]
    private static partial void LogProcessingVersionDirectory(ILogger logger, string versionDirectory);

    [LoggerMessage(Level = LogLevel.Warning, Message = "版本目录不存在，跳过: {versionDirectory}")]
    private static partial void LogVersionDirectoryNotExists(ILogger logger, string versionDirectory);

    [LoggerMessage(Level = LogLevel.Information, Message = "发现 {count} 个版本目录需要处理")]
    private static partial void LogFoundVersionDirectories(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "完成版本目录处理: {versionDirectory}")]
    private static partial void LogVersionDirectoryComplete(ILogger logger, string versionDirectory);

    private static void Main(string[] args)
    {
        try
        {
            // 注册编码提供程序以支持更多编码格式（如 GB18030）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            LogProgramStart(Logger);
            
            ProcessAllVersionDirectories();
            
            LogProgramComplete(Logger);
        }
        catch (Exception ex)
        {
            LogProgramError(Logger, ex);
            Environment.Exit(1);
        }
    }

    private static void ProcessAllVersionDirectories()
    {
        // 自动发现版本目录
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        var versionDirectories = currentDirectory.GetDirectories(VersionDirectoryPattern, SearchOption.TopDirectoryOnly)
            .Select(dir => dir.Name)
            .OrderBy(name => name)
            .ToList();
        
        LogFoundVersionDirectories(Logger, versionDirectories.Count);
        
        foreach (var versionDirectory in versionDirectories)
        {
            LogProcessingVersionDirectory(Logger, versionDirectory);
            
            // 处理每个版本目录下的所有映射目录
            foreach (var (sourceRelativePath, targetRelativePath) in DirectoryMappings)
            {
                var sourcePath = Path.Combine(versionDirectory, sourceRelativePath);
                var targetPath = Path.Combine(versionDirectory, targetRelativePath);
                
                ProcessDirectoryMapping(sourcePath, targetPath);
            }
            
            LogVersionDirectoryComplete(Logger, versionDirectory);
        }
    }

    private static void ProcessDirectoryMapping(string sourcePath, string targetPath)
    {
        var sourceDir = new DirectoryInfo(sourcePath);
        var targetDir = new DirectoryInfo(targetPath);

        if (!sourceDir.Exists)
        {
            LogVersionDirectoryNotExists(Logger, sourcePath);
            return; // 跳过不存在的目录，不抛出异常
        }

        LogSourceDirectory(Logger, sourceDir.FullName);
        LogTargetDirectory(Logger, targetDir.FullName);

        // 确保目标目录存在
        if (!targetDir.Exists)
        {
            targetDir.Create();
            LogCreateTargetDirectory(Logger, targetDir.FullName);
        }

        ProcessDirectoryRecursive(sourceDir, targetDir);
    }

    private static void ProcessDirectoryRecursive(DirectoryInfo sourceDir, DirectoryInfo targetDir)
    {
        // 处理当前目录的文件
        foreach (var file in sourceDir.GetFiles())
        {
            if (SupportedExtensions.Contains(file.Extension))
            {
                ProcessFile(file, targetDir);
            }
        }

        // 递归处理子目录
        foreach (var subDir in sourceDir.GetDirectories())
        {
            var targetSubDir = new DirectoryInfo(Path.Combine(targetDir.FullName, subDir.Name));
            if (!targetSubDir.Exists)
            {
                targetSubDir.Create();
            }
            ProcessDirectoryRecursive(subDir, targetSubDir);
        }
    }

    private static void ProcessFile(FileInfo sourceFile, DirectoryInfo targetDir)
    {
        try
        {
            LogProcessingFile(Logger, sourceFile.FullName);

            var targetFilePath = Path.Combine(targetDir.FullName, sourceFile.Name);

            // 检测原文件编码
            var detectedEncoding = DetectFileEncoding(sourceFile.FullName);
            LogDetectedEncoding(Logger, sourceFile.Name, detectedEncoding?.EncodingName ?? "Unknown");

            // 读取文件内容
            string content;
            if (detectedEncoding != null)
            {
                content = File.ReadAllText(sourceFile.FullName, detectedEncoding);
            }
            else
            {
                // 如果检测失败，尝试使用默认编码
                LogEncodingDetectionFailed(Logger, sourceFile.Name);
                content = File.ReadAllText(sourceFile.FullName, Encoding.UTF8);
            }

            // 以UTF-8编码保存文件
            File.WriteAllText(targetFilePath, content, new UTF8Encoding(false));
            
            LogFileConversionComplete(Logger, sourceFile.Name, targetFilePath);
        }
        catch (Exception ex)
        {
            LogFileProcessingError(Logger, ex, sourceFile.FullName);
        }
    }

    private static Encoding? DetectFileEncoding(string filePath)
    {
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var detector = new CharsetDetector();
            detector.Feed(fileStream);
            detector.DataEnd();

            if (detector.Charset != null)
            {
                return Encoding.GetEncoding(detector.Charset);
            }
        }
        catch (Exception ex)
        {
            LogEncodingDetectionError(Logger, ex, filePath);
        }

        return null;
    }

    private static ILogger CreateLogger()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Information);
        });
        return loggerFactory.CreateLogger<Program>();
    }
}
