using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static ushort packetId;
        static string packetEnums;
        static string genPackets;

        static string clientManagerRegister;
        static string serverManagerRegister;
        static void Main(string[] args)
        {
            string pdlPath = "../PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                //주석을 찾지마라
                IgnoreComments = true,
                // 그 띄어쓰기 하지 마라
                IgnoreWhitespace = true
            };


            if (args.Length >= 1)
            {
                pdlPath = args[0];
            }

            using (XmlReader r = XmlReader.Create(pdlPath, settings))
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
                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPacket.cs", fileText);
                string clientManagerText = string.Format(PacketFormat.managerFormat, clientManagerRegister);
                File.WriteAllText("ClientManager.cs", clientManagerText); 
                string serverManagerText = string.Format(PacketFormat.managerFormat, serverManagerRegister);
                File.WriteAllText("ServerManager.cs", serverManagerText);
            }
        }
        public static void ParsePacket(XmlReader r)
        {
            // PDL의 패킷을 End로 취급했을 경우
            if (r.NodeType == XmlNodeType.EndElement)
            {
                return;
            }
            // Packet으로 시작할 경우
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
            Tuple<string, string, string> t = ParseMembers(r);
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine+ '\t';
            clientManagerRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            serverManagerRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
        }
        /// {1} : 맴버 변수들
        /// {2} : 맴버 변수의 Read
        /// {3} : 맴버 변수들의 Write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
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

                if (string.IsNullOrEmpty(memberCode) == false)
                {
                    //NewLine을 통해 작성될 코드에 줄 바꿈 처리
                    memberCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(readCode) == false)
                {
                    //NewLine을 통해 작성될 코드에 줄 바꿈 처리
                    readCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(writeCode) == false)
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
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
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
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }
        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                return null;
            }
            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1, t.Item2, t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName)
                );
            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName)
                );

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }
        public static string ToMemberType(string memberType)
        {
            switch (memberType)
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
        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            return input[0].ToString().ToUpper() + input.Substring(1);
        }
        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}