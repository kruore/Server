using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                //주석을 찾지마라
                IgnoreComments = true,
                // 그 띄어쓰기 하짐라ㅏ
                IgnoreWhitespace = true
            };
            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                // document 그 다음 녀석을읽을수 있도록 커서를 옮긴다.
                r.MoveToContent();

                while (r.Read())
                {
                    ////NodeType? 
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(r);
                    }
                    Console.WriteLine($"{r.Name}{r["name"]},{r.Depth}");
                }
            }
        }
        public static void ParsePacket(XmlReader r)
        {
            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invaild packet node");
                return;
            }
            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }
            ParseMembers(r);
        }
        public static void ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            int depth = r.Depth + 1;
            while (r.Read())
            {
                if (r.Depth != depth)
                {
                    break;
                }
                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "string":
                    case "int":
                        break;
                    case "long":
                    case "float":
                    case "double":
                    case "list":
                    case "ushort":
                    case "short":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}