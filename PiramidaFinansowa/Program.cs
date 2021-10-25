using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PiramidaFinansowa {
    class Program {

        static readonly string basePath = Path.GetFullPath(@"..\..\..\Input");
        static readonly string piramidFilePath = $@"{basePath}\piramida.xml";
        static readonly string transfersFilePath = $@"{basePath}\przelewy.xml";

        static void Main() {
            var piramidDoc = ReadXmlFile(piramidFilePath);
            var transfersDoc = ReadXmlFile(transfersFilePath);

            if(piramidDoc != null || transfersDoc != null) ResolvePiramidHierarchy(piramidDoc,transfersDoc);
        }

        static void ResolvePiramidHierarchy(XmlDocument baseDoc, XmlDocument transfersDoc) {
            var mainNode = baseDoc.GetElementsByTagName("uczestnik");
            List<Member> members = new();

            foreach(XmlNode Node in mainNode) {
                int id = int.Parse(Node.Attributes["id"].Value);

                int positionIndex = 0;
                int position = CalculateUserPosition(positionIndex, Node);

                int childsIndex = 0;
                int childs = CalculateChildsWithNoChilds(childsIndex, Node);

                uint transferSum = ReturnSumOfMemberTransfers(id, transfersDoc);

                members.Add(new(id, position, childs, transferSum));
            }

            if(members.Count > 0) {
                foreach(Member member in members) {
                    Console.WriteLine(
                        $"{member.id} {member.position} {member.childs} {member.transfersSum}"
                    );
                }
            }
        }

        static int CalculateUserPosition(int index, XmlNode node) {
            string parentName = node.ParentNode.Name;

            if(parentName != "piramida") {
                index++;
                index = CalculateUserPosition(index, node.ParentNode);
            }

            return index;
        }

        static int CalculateChildsWithNoChilds(int index, XmlNode node) {
            if(node.HasChildNodes) {
                foreach(XmlNode childNode in node.ChildNodes) {
                    if(!childNode.HasChildNodes) index++;
                    else index = CalculateChildsWithNoChilds(index, childNode);
                }
            }

            return index;
        }

        static uint ReturnSumOfMemberTransfers(int id, XmlDocument transfersDoc) {
            var transfersNodeList = transfersDoc.GetElementsByTagName("przelew");
            uint sum = 0;

            if(transfersNodeList.Count > 0) {
                foreach(XmlNode transferNode in transfersNodeList) {
                    if(int.Parse(transferNode.Attributes["od"].Value) == id)
                        sum += uint.Parse(transferNode.Attributes["kwota"].Value);
                }
            }

            return sum;
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

    public class Member {
        // using class to better organize individual members and actions related to them
        public int id { get; set; }
        public int position { get; set; }
        public int childs { get; set; }
        public uint transfersSum { get; set; }

        public Member(int id, int position, int childs, uint transfersSum) {
            this.id = id;
            this.position = position;
            this.childs = childs;
            this.transfersSum = transfersSum;
        }
    }
}
