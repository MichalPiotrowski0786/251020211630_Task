using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace PiramidaFinansowa {

    public class Program {
        static readonly string basePath = Path.GetFullPath(@"..\..\..\Input");
        static readonly string piramidFilePath = $@"{basePath}\piramida.xml";
        static readonly string transfersFilePath = $@"{basePath}\przelewy.xml";

        static void Main() {
            var piramidDoc = ReadXmlFile(piramidFilePath);
            var transfersDoc = ReadXmlFile(transfersFilePath);

            if(piramidDoc != null || transfersDoc != null) Compute(piramidDoc,transfersDoc);
        }

        static void Compute(XmlDocument baseDoc, XmlDocument transfersDoc) {
            var mainNode = baseDoc.GetElementsByTagName("uczestnik");
            List<Member> members = new();

            foreach(XmlNode Node in mainNode) {
                int id = int.Parse(Node.Attributes["id"].Value);

                int positionIndex = 0;
                int position = CalculateUserPosition(positionIndex, Node);

                int childsIndex = 0;
                int childs = CalculateChildsWithNoChilds(childsIndex, Node);

                int parentId = CalculateUserParent(Node);
                uint transferSum = ReturnSumOfMemberTransfers(id, transfersDoc);

                members.Add(new(id, parentId, position, childs, transferSum));
            }

            if(members.Count > 0) {
                Member owner = null;
                // find owner and assign it to variable
                members.ForEach((member) => { if(member.position == 0) owner = member; });
                // all members transfer money to owner
                members.ForEach((member) => member.transferMoney(member.balance, owner));
                // transfer money from owner to members that fulfilled condition
                members.ForEach((member) => {
                    if(member.childs > 0) {
                        List<Member> childs = new();
                        foreach(Member _member in members) {
                            if(_member.parentId == member.id) childs.Add(_member);
                        }

                        if(childs.Count > 0) {
                            foreach(Member child in childs) {
                                owner.transferMoney(child.init_balance / 2, member);
                            }
                        }
                    }
                });

                members = members.OrderBy((member) => member.id).ToList();
                members.ForEach((member) => {
                    Console.WriteLine(
                        $"{member.id} "+
                        $"{member.position} " +
                        $"{member.childs} " +
                        $"{member.balance} "
                    );
                });
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

        static int CalculateUserParent(XmlNode node) {
            int parentId;
            if(node.ParentNode == null || node.ParentNode.Name == "piramida") parentId = -1;
            else parentId = int.Parse(node.ParentNode.Attributes["id"].Value);

            return parentId;
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
            // load XML document
            if(!IsInputValid(path)) return null;
            XmlDocument doc = new ();
            doc.Load(path);

            return doc;
        }

        static bool IsInputValid(string path) {
            // check if file exists
            bool inputValid = true;
            if(!File.Exists(path)) inputValid = false;
            return inputValid;
        }
    }

    public class Member {
        // using class to better organize individual members and actions related to them
        public int id { get; set; }
        public int parentId { get; set; }
        public int position { get; set; }
        public int childs { get; set; }
        public uint childsBalance { get; set; }
        public uint init_balance { get; set; }
        public uint balance { get; set; }

        public Member(int id, int parentId, int position, int childs, uint balance) {
            this.id = id;
            this.parentId = parentId;
            this.position = position;
            this.childs = childs;
            init_balance = balance;
            this.balance = balance;
        }

        public void transferMoney(uint amount, Member member) {
            if(balance > 0 && amount <= balance) {
                member.balance += amount;
                balance -= amount;
            }
        }
    }
}
