using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPacket;
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

                File.WriteAllText("GenPacket.cs",genPacket);
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
            Tuple<string,string,string> t = ParseMembers(r);
            genPacket = string.Format(PacketFormat.parketFormat,t.Item1, t.Item2, t.Item3);
        }
        /// {1} : 맴버 변수들
        /// {2} : 맴버 변수의 Read
        /// {3} : 맴버 변수들의 Write
        public static Tuple<string,string,string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

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
                    return null;
                }
                
                if(string.IsNullOrEmpty(memberCode)==false)
                {
                    //NewLine을 통해 작성될 코드에 줄 바꿈 처리
                    memberCode += Environment.NewLine;
                }       
                if(string.IsNullOrEmpty(readCode) ==false)
                {
                    //NewLine을 통해 작성될 코드에 줄 바꿈 처리
                    readCode += Environment.NewLine;
                }       
                if(string.IsNullOrEmpty(writeCode) ==false)
                {
                    //NewLine을 통해 작성될 코드에 줄 바꿈 처리
                    writeCode += Environment.NewLine;
                }

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool": 
                    case "byte":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "ushort":
                    case "short":
                        memberCode += string.Format(PacketFormat.memberFormat,memberType,memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "list":
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    default:
                        break;
                }
            }
            memberCode = memberCode.Replace("\n", "\n\t");
            memberCode = memberCode.Replace("\n", "\n\t\t");
            memberCode = memberCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch(memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                case "ushort":
                    return "ToUInt16";
                case "short":
                    return "ToInt16";
            }
            return null;

        }

    }
}