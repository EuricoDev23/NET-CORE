using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LinqToDB.Mapping;

namespace CORE.MVC
{
    [Source(null, null, "LOG")]
    public class LogModel:Entity
    {
        [Pk(PK.Database)]
        public long IDLog { get; set; }
        public string Descricao { get; set; }
        public string Type { get; set; }
        [Column]
        public string Value { get; set; }
    }
    public static class Log
    {
        private static string path = "Log";
        private static string fileName = path+"/"+DateTime.Now.Ticks.ToString()+".txt";
        public static void WriteLog(this Entity model, string descricao) {
        }
        public static void WriteLog(this Entity model, string descricao, string values)
        {
        }
        public static void Write(LogModel model)
        {
        }
        public static void Write(string message){
            try
            {
                if(Directory.Exists(path)==false){
                    Directory.CreateDirectory(path);
                }
                File.AppendAllLines(fileName, new string[]{ message });
            }
            catch (Exception ex)
            {

            }
        }

        public static string[] GetLines()
        {
            try
            {
            return File.ReadAllLines(Directory.EnumerateFiles(path).LastOrDefault());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
