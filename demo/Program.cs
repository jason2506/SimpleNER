/*
 * File Name: Program.cs
 * Author   : Chi-En Wu
 * Date     : 2012/01/04
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

using Utils.DataStructure;
using Utils.NamedEntity;

namespace Demo
{
    public class Program
    {
        // args[0]: The MeSH descriptors file path
        //          (format: http://www.nlm.nih.gov/mesh/filelist.html)
        // args[1]: The content file path
        // args[2]: The NER result file path
        public static void Main(string[] args)
        {
            if (args.Length < 3) { return; }

            NamedEntityRecognizer<Descriptor> recognizer
                = new NamedEntityRecognizer<Descriptor>();

            Console.Out.WriteLine("Loading Descriptor List...");
            IEnumerable<Descriptor> descriptorList = Program.LoadDescriptorList(args[0]);

            Program.UpdateDictionary(recognizer.Dictionary, descriptorList);

            Console.Out.WriteLine("Reading Content List...");
            Dictionary<int, string> content = new Dictionary<int, string>();
            using (StreamReader reader = new StreamReader(args[1]))
            {
                while (!reader.EndOfStream)
                {
                    string[] token = reader.ReadLine().Split("\t".ToCharArray(), 2);
                    int id = Convert.ToInt32(token[0]);
                    string text = token[1];
                    content.Add(id, text);
                }
            }

            Console.Out.WriteLine("Recognizing Name Entity...");
            using (StreamWriter writer = new StreamWriter(args[2]))
            {
                foreach (int id in content.Keys)
                {
                    ICollection<NamedEntityInfo<Descriptor>> result = recognizer.Recognize(content[id]);
                    foreach (NamedEntityInfo<Descriptor> entity in result)
                    {
                        writer.WriteLine(id.ToString() + '\t' + entity.Info.Id + '\t' +
                            entity.Index + '\t' + entity.Length);
                    }
                }
            }
        }

        public static IList<Descriptor> LoadDescriptorList(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);

            List<Descriptor> result = new List<Descriptor>();
            foreach (XmlNode node in document.SelectNodes("/DescriptorRecordSet/DescriptorRecord"))
            {
                Descriptor descriptor = new Descriptor();
                descriptor.Id = node.SelectSingleNode("./DescriptorUI").InnerText;

                foreach (XmlNode innerNode in node.SelectNodes("./TreeNumberList/TreeNumber"))
                {
                    descriptor.TreeNumberList.Add(innerNode.InnerText);
                }

                foreach (XmlNode innerNode in node.SelectNodes(".//TermList/Term/String"))
                {
                    descriptor.TermList.Add(innerNode.InnerText);
                }

                result.Add(descriptor);
            }

            return result;
        }

        private static void UpdateDictionary(IDictionary<string, Descriptor> dictionary,
            IEnumerable<Descriptor> descriptorList)
        {
            foreach (Descriptor descriptor in descriptorList)
            {
                foreach (string term in descriptor.TermList)
                {
                    //dictionary.Add(term, descriptor); // CAUSES ERROR
                    dictionary[term] = descriptor;
                }
            }
        }
    }

    public class Descriptor
    {
        private string id;
        public string Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        private List<string> treeNumberList = new List<string>();
        public List<string> TreeNumberList
        {
            get { return this.treeNumberList; }
        }

        private List<string> termList = new List<string>();
        public List<string> TermList
        {
            get { return this.termList; }
        }
    }
}
