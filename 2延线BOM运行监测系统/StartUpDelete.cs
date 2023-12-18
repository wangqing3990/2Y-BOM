using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace _2延线BOM运行监测系统
{
    class StartUpDelete
    {
        public static void startUpDelete()
        {
            //删除启动文件夹下的启动程序快捷方式
            string startUpPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string appStartUpPath1 = Path.Combine(startUpPath, "Suzhou.APP.BOM.exe.lnk");
            string appStartUpPath2 = Path.Combine(startUpPath, "Suzhou.APP.BOM.lnk");
            string updateStartUpPath1 = Path.Combine(startUpPath, "Suzhou.APP.Update.exe.lnk");
            string updateStartUpPath2 = Path.Combine(startUpPath, "Suzhou.APP.Update.lnk");

            if (File.Exists(appStartUpPath1)) File.Delete(appStartUpPath1);
            if (File.Exists(appStartUpPath2)) File.Delete(appStartUpPath2);
            if (File.Exists(updateStartUpPath1)) File.Delete(updateStartUpPath1);
            if (File.Exists(updateStartUpPath2)) File.Delete(updateStartUpPath2);
        }
    }
}
