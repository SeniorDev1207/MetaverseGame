﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Helper;
using Common.Logger;
using Common.Network;
using MongoDB.Bson;

namespace TNet
{
	internal class TChannel: AChannel
	{
		private const int SendInterval = 0;
		private TSocket socket;

		private readonly TBuffer recvBuffer = new TBuffer();
		private readonly TBuffer sendBuffer = new TBuffer();

		private ObjectId sendTimer = ObjectId.Empty;
		private Action onParseComplete = () => { };
		private readonly PacketParser parser;
		private readonly string remoteAddress;

		public TChannel(TSocket socket, TService service): base(service)
		{
			this.socket = socket;
			this.service = service;
			this.parser = new PacketParser(this.recvBuffer);
			this.remoteAddress = this.socket.RemoteAddress;

			this.StartRecv();
		}

		private void Dispose(bool disposing)
		{
			if (this.socket == null)
			{
				return;
			}

			this.onDispose(this);

			if (disposing)
			{
				// 释放托管的资源
				this.socket.Dispose();
			}

			// 释放非托管资源
			this.service.Remove(this);
			this.socket = null;
		}

		~TChannel()
		{
			this.Dispose(false);
		}

		public override void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override void SendAsync(
				byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			byte[] size = BitConverter.GetBytes(buffer.Length);
			this.sendBuffer.SendTo(size);
			this.sendBuffer.SendTo(buffer);
			if (this.sendTimer == ObjectId.Empty)
			{
				this.sendTimer = this.service.Timer.Add(TimeHelper.Now() + SendInterval, this.StartSend);
			}
		}

		public override void SendAsync(
			List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			int size = buffers.Select(b => b.Length).Sum();
			byte[] sizeBuffer = BitConverter.GetBytes(size);
			this.sendBuffer.SendTo(sizeBuffer);
			foreach (byte[] buffer in buffers)
			{
				this.sendBuffer.SendTo(buffer);	
			}
			
			if (this.sendTimer == ObjectId.Empty)
			{
				this.sendTimer = this.service.Timer.Add(TimeHelper.Now() + SendInterval, this.StartSend);
			}
		}

		public ObjectId SendTimer
		{
			get
			{
				return this.sendTimer;
			}
		}

		public override Task<byte[]> RecvAsync()
		{
			var tcs = new TaskCompletionSource<byte[]>();

			if (this.parser.Parse())
			{
				tcs.SetResult(this.parser.GetPacket());
			}
			else
			{
				this.onParseComplete = () => this.ParseComplete(tcs);
			}
			return tcs.Task;
		}

		public override async Task<bool> DisconnnectAsync()
		{
			return await this.socket.DisconnectAsync();
		}

		public override string RemoteAddress
		{
			get
			{
				return this.remoteAddress;
			}
		}

		private void ParseComplete(TaskCompletionSource<byte[]> tcs)
		{
			byte[] packet = this.parser.GetPacket();
			this.onParseComplete = () => { };
			tcs.SetResult(packet);
		}

		private async void StartSend()
		{
			try
			{
				while (true)
				{
					if (this.sendBuffer.Count == 0)
					{
						break;
					}
					int sendSize = TBuffer.ChunkSize - this.sendBuffer.FirstIndex;
					if (sendSize > this.sendBuffer.Count)
					{
						sendSize = this.sendBuffer.Count;
					}
					int n =
							await this.socket.SendAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);

					this.sendBuffer.FirstIndex += n;
					if (this.sendBuffer.FirstIndex == TBuffer.ChunkSize)
					{
						this.sendBuffer.FirstIndex = 0;
						this.sendBuffer.RemoveFirst();
					}
				}
			}
			catch (Exception e)
			{
				Log.Debug(e.ToString());
			}

			this.sendTimer = ObjectId.Empty;
		}

		private async void StartRecv()
		{
			try
			{
				while (true)
				{
					int n =
							await
									this.socket.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex,
											TBuffer.ChunkSize - this.recvBuffer.LastIndex);
					if (n == 0)
					{
						break;
					}

					this.recvBuffer.LastIndex += n;
					if (this.recvBuffer.LastIndex == TBuffer.ChunkSize)
					{
						this.recvBuffer.AddLast();
						this.recvBuffer.LastIndex = 0;
					}

					// 解析封包
					if (this.parser.Parse())
					{
						this.onParseComplete();
					}
				}
			}
			catch (Exception e)
			{
				Log.Trace(e.ToString());
			}
		}
	}
}