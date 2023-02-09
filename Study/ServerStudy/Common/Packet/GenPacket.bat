START ../../PacketGenerator/Debug/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPacket.cs "../../DummyClient/Packet"
XCOPY /Y GenPacket.cs "../../Server/Packet"
XCOPY /Y PacketManager.cs "../../DummyClient/Packet"
XCOPY /Y PacketManager.cs "../../Server/Packet"
