using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace _2延线BOM运行监测系统
{
    class Copy
    {
        static ShowLog sl = Monitor.sl;
        public static void CopySth(string sourcePath, string targetPath, params string[] excludedDirs)
        {
            var files = Directory.GetFiles(sourcePath, "*", SearchOption.TopDirectoryOnly).Where(file => !excludedDirs.Any(excluded => file.StartsWith(Path.Combine(sourcePath, excluded), StringComparison.OrdinalIgnoreCase)));
            var dirs = Directory.GetDirectories(sourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => !excludedDirs.Any(excluded => dir.EndsWith(excluded, StringComparison.OrdinalIgnoreCase)));
            try
            {
                foreach (var dir in dirs)
                {
                    var dirName = Path.GetFileName(dir);
                    var targetDir = Path.Combine(targetPath, dirName);
                    Directory.CreateDirectory(targetDir);
                    CopySth(dir, targetDir, excludedDirs);

                }
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var targetFile = Path.Combine(targetPath, fileName);
                    File.Copy(file, targetFile, true);
                }
            }
            catch (Exception ex)
            {
                sl.showLog(ex.Message);
            }
        }
    }
}
