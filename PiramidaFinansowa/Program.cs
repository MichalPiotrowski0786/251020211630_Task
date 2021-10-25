using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PiramidaFinansowa {
    class Program {
        static string basePath = Path.GetFullPath(@"..\..\..\Input");
        static string piramidaFilePath = $@"{basePath}\piramida.xml";
        static string przelewyFilePath = $@"{basePath}\przelewy.xml";
        static void Main() {
            var piramidaDocument = ReadXmlFile(piramidaFilePath);
            ResolvePiramidaHierarchy(piramidaDocument);
        }

        static void ResolvePiramidaHierarchy(XmlDocument doc) {
            var documentNode = doc.GetElementsByTagName("uczestnik");
            List<Uczestnik> Uczestnicy = new();

            foreach(XmlNode Node in documentNode) {
                Uczestnik User = new ();
                User.id = int.Parse(Node.Attributes["id"].Value);

                int positionIndex = 0;
                User.position = CalculateUserPosition(positionIndex, Node);

                Uczestnicy.Add(User);
            }

            if(Uczestnicy.Count > 0) {
                foreach(Uczestnik User in Uczestnicy) {
                    Console.WriteLine($"{User.id} {User.position} {User.childs} -1");
                }
            }
        }

        static int CalculateUserPosition(int index, XmlNode node) {
            string parentName = node.ParentNode.Name;

            if(parentName != "piramida") {
                index++;
                index = CalculateUserPosition(index, node.ParentNode);
            } else {
                return index;
            }

            return index;
        }

        static XmlDocument ReadXmlFile(string path) {
            if(!IsInputValid(path)) return null;
            XmlDocument doc = new ();
            doc.Load(path);

            return doc;
        }

        static bool IsInputValid(string path) {
            bool inputValid = true;
            if(!File.Exists(path)) inputValid = false;
            return inputValid;
        }
    }

    public class Uczestnik {
        public int id { get; set; }
        public int position { get; set; }
        public int childs { get; set; }

        public Uczestnik(int id, int position, int childs) {
            this.id = id;
            this.position = position;
            this.childs = childs;
        }

        public Uczestnik() {
            this.id = -1;
            this.position = -1;
            this.childs = -1;
        }
    }
}
