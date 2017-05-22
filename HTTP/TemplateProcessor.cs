using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashCam.HTTP
{
    public class TemplateProcessor
    {
        private static Dictionary<string, Func<string>> ElementHandlers = new Dictionary<string, Func<string>>();

        public static void AddEelment(string name, Func<string> handler)
        {
            ElementHandlers.Add(name, handler);
        }


        public static string Process(string filename, Dictionary<string, Func<string>> localHandlers)
        {
            string data = File.ReadAllText(filename);
            if (data.Contains("${") && data.Contains("}"))
            {
                //Step through until we reach an opening marker
                bool Found = true;
                int index = 0;

                do
                {
                    if (index >= data.Length)
                        break;

                    int Begin = data.IndexOf("${", index);
                    if (Begin == -1) break;
                    int End = data.IndexOf("}", Begin);
                    if (Begin == -1 || End == -1)
                        Found = false;
                    else
                    {
                        string block = data.Substring(Begin + 2, End - Begin - 2);
                        data = data.Remove(Begin, End - Begin + 1);
                        block = ProcessVariable(block, localHandlers);
                        data = data.Insert(Begin, block);
                        index = Begin + block.Length;
                    }
                    
                } while (Found);
            }
            return data;
        }

        private static string ProcessVariable(string data, Dictionary<string, Func<string>> localHandlers)
        {
            if (localHandlers.ContainsKey(data))
                return localHandlers[data]();
            if (ElementHandlers.ContainsKey(data))
                return ElementHandlers[data]();
            return data;
        }


    }
}
