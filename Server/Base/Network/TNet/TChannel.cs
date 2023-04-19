﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Base
{
	public class TChannel : AChannel
	{
		private readonly TSocket socket;

		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();

		private bool isSending;
		private readonly PacketParser parser;
		private bool isConnected;

		public Action<long, SocketError> OnError;

		public string RemoteAddress { get; }
		private TaskCompletionSource<byte[]> recvTcs;

		/// <summary>
		/// connect
		/// </summary>
		public TChannel(TSocket socket, string host, int port, TService service) : base(service)
		{
			this.socket = socket;
			this.parser = new PacketParser(this.recvBuffer);
			this.RemoteAddress = host + ":" + port;
			
			bool result = this.socket.ConnectAsync(host, port);
			if (!result)
			{
				this.OnConnected(this.Id, SocketError.Success);
				return;
			}
			this.socket.OnConn += e => OnConnected(this.Id, e);
		}

		/// <summary>
		/// accept
		/// </summary>
		public TChannel(TSocket socket, TService service) : base(service)
		{
			this.socket = socket;
			this.parser = new PacketParser(this.recvBuffer);
			this.RemoteAddress = socket.RemoteAddress;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			this.socket.Dispose();
			this.service.Remove(id);
		}
		
		private void OnConnected(long channelId, SocketError error)
		{
			if (this.service.GetChannel(channelId) == null)
			{
				return;
			}
			if (error != SocketError.Success)
			{
				Log.Error($"connect error: {error}");
				return;
			}
			this.isConnected = true;
			this.StartSend();
			this.StartRecv();
		}

		public override void Send(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			byte[] size = BitConverter.GetBytes(buffer.Length);
			this.sendBuffer.SendTo(size);
			this.sendBuffer.SendTo(buffer);
			if (!this.isSending && this.isConnected)
			{
				this.StartSend();
			}
		}

		public override void Send(List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			int size = buffers.Select(b => b.Length).Sum();
			byte[] sizeBuffer = BitConverter.GetBytes(size);
			this.sendBuffer.SendTo(sizeBuffer);
			foreach (byte[] buffer in buffers)
			{
				this.sendBuffer.SendTo(buffer);
			}
			if (!this.isSending && this.isConnected)
			{
				this.StartSend();
			}
		}

		private void StartSend()
		{
			// 没有数据需要发送
			if (this.sendBuffer.Count == 0)
			{
				this.isSending = false;
				return;
			}

			this.isSending = true;

			int sendSize = TBuffer.ChunkSize - this.sendBuffer.FirstIndex;
			if (sendSize > this.sendBuffer.Count)
			{
				sendSize = this.sendBuffer.Count;
			}

			if (!this.socket.SendAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize))
			{
				this.OnSend(sendSize, SocketError.Success);
				return;
			}
			this.socket.OnSend = this.OnSend;
		}

		private void OnSend(int n, SocketError error)
		{
			if (this.Id == 0)
			{
				return;
			}
			this.socket.OnSend = null;
			if (error != SocketError.Success)
			{
				Log.Info($"socket send fail, error: {error}, n: {n}");
				this.OnError(this.Id, error);
				return;
			}
			this.sendBuffer.FirstIndex += n;
			if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
			{
				this.sendBuffer.FirstIndex = 0;
				this.sendBuffer.RemoveFirst();
			}

			this.StartSend();
		}

		private void StartRecv()
		{
			int size = TBuffer.ChunkSize - this.recvBuffer.LastIndex;
			if (!this.socket.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size))
			{
				this.OnRecv(size, SocketError.Success);
			}
			this.socket.OnRecv = this.OnRecv;
		}

		private void OnRecv(int n, SocketError error)
		{
			if (this.Id == 0)
			{
				return;
			}
			this.socket.OnRecv = null;
			if (error != SocketError.Success)
			{
				Log.Info($"socket recv fail, error: {error}, {n}");
				this.OnError(this.Id, error);
				return;
			}

			this.recvBuffer.LastIndex += n;
			if (this.recvBuffer.LastIndex == TBuffer.ChunkSize)
			{
				this.recvBuffer.AddLast();
				this.recvBuffer.LastIndex = 0;
			}

			if (this.recvTcs != null)
			{
				byte[] packet = this.parser.GetPacket();
				if (packet != null)
				{
					this.recvTcs.SetResult(packet);
					this.recvTcs = null;
				}
			}

			StartRecv();
		}

		public override Task<byte[]> Recv()
		{
			TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
			byte[] packet = this.parser.GetPacket();
			if (packet != null)
			{
				tcs.SetResult(packet);
			}
			else
			{
				recvTcs = tcs;
			}
			return tcs.Task;
		}
	}
}