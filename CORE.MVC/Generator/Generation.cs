using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CORE.MVC
{
    public class Generation
    {
        private readonly static object syncRoot = new Object();

        public readonly static long Chave = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));

        //static Random Random_;
        static long last = 0;
        //static Random Random { get{
        //        Random_ = Random_ ?? new Random();
        //        return Random_;
        //} }
        public static long ID
        {
            get
            {
                //int max= Math.Abs(((int)DateTime.Now.Ticks) / (365*2));
                //last = Random.Next(0,max+1);
                lock (syncRoot)
                {
                    last++;
                    return Chave + last;
                }
            }
        }

        private static List<long> IDsTest()
        {
            List<long> list = new List<long>();

            for (int i = 0; i < 50; i++)
            {
                list.Add(ID);
            }
            return list;
        }
        private static Dictionary<long, int> GetDuplicates(List<long> list)
        {
            Dictionary<long, int> d = new Dictionary<long, int>();
            list.GroupBy(i => i).ToList().ForEach(i =>
            {
                var c = list.Where(a => a == i.Key).Count();
                d.Add(i.Key, c);
            });
            return d;
        }
        private static void TestOnTasksAsync()
        {
            var tasks = new Task[50];
            StringBuilder wr = new StringBuilder();
            Random r = new Random();
            int count = 0;
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    count++;
                    int pos = count;
                    foreach (var item in GetDuplicates(Generation.IDsTest()).OrderBy(k => k.Value))
                    {
                        wr.AppendLine($"Task[{(pos)}] =>[{item.Key}] = {item.Value}");

                    }
                    wr.AppendLine("\n==========================================\n");
                });
            }
            Task.WhenAll(tasks).Wait();

            wr.AppendLine($"\nTarefas concluidas: ");
            Console.WriteLine(wr.ToString());
            File.WriteAllText($"Keys_{DateTime.Now.ToString("yyyMMddmmss")}.txt", wr.ToString());
        }

        public static string XML(object model, string path = "")
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlSerializer xmlSerializer = new XmlSerializer(model.GetType());
            string XML;

            using (MemoryStream xmlStream = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                xmlSerializer.Serialize(xmlStream, model, ns);
                xmlStream.Position = 0;
                xmlDoc.Load(xmlStream);
                XML = xmlDoc.InnerXml;

                //mudar encoding para suprotar caracteres portugueses como acentos
                /*
                XMLBensDados = XMLBensDados.Replace("xml version=\"1.0\"",
                                "xml version=\"1.0\" encoding=\"ISO-8859-1\"");
                    * */
                //XML = XML.Replace("xml version=\"1.0\"",
                //         "xml version=\"1.0\" encoding=\"UTF-16\"");
                if (string.IsNullOrWhiteSpace(path) == false)
                {
                    xmlDoc.Save(path);
                    //File.WriteAllText(path, xml);
                }
                return XML;
            }

        }

        static public Object XMLToObject(string XMLString, Object oObject)
        {

            XmlSerializer oXmlSerializer = new XmlSerializer(oObject.GetType());
            oObject = oXmlSerializer.Deserialize(new StringReader(XMLString));
            return oObject;
        }
    }
}
