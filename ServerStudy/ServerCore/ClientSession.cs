using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{
	abstract class Packet
	{
		public ushort size;
		public ushort packetId;

		public abstract ArraySegment<byte> Write();
		public abstract void Read(ArraySegment<byte> s);
	}

	class PlayerInfoReq : Packet
	{
		public long playerId;
		public PlayerInfoReq()
		{
			this.packetId = (ushort)PacketID.PlayerInfoReq;
		}
		public override void Read(ArraySegment<byte> s)
		{
			int pos = 0;

			//ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
			pos += 2;
			// ushort id = BitConverter.ToUInt16(s.Array, s.Offset + pos);
			pos += 2;
			long playerId = BitConverter.ToInt16(s.Array, s.Offset + pos);
			// TODO

		}

		public override ArraySegment<byte> Write()
		{
			ArraySegment<byte> s = SendBufferHelper.Open(4096);
			ushort size = 0;
			bool success = true;

			size += 2;
			success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.packetId);
			size += 2;
			success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.playerId);
			size += 8;
			success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), size);

			if (success == false)
			{
				return null;
			}

			ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);
			return sendBuff;
		}
	}
	public enum PacketID
	{
		PlayerInfoReq = 1,
		PlayerInfoOk = 2,
	}
	class ClientSession : PacketSession
	{
		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");
			Thread.Sleep(5000);
			Disconnect();
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			int pos = 0;

			ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
			pos += 2;
			ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
			pos += 2;

			// TODO
			switch ((PacketID)id)
			{
				case PacketID.PlayerInfoReq:
					{
						PlayerInfoReq p = new PlayerInfoReq();
						p.Read(buffer);
						pos += 8;
					}
					break;
				case PacketID.PlayerInfoOk:
					{
						int hp = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
						pos += 4;
						int attack = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
						pos += 4;
					}
					//Handle_PlayerInfoOk();
					break;
				default:
					break;
			}

			Console.WriteLine($"RecvPacketId: {id}, Size {size}");
		}

		// TEMP
		public void Handle_PlayerInfoOk(ArraySegment<byte> buffer)
		{

		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
