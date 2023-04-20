﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base;
using MongoDB.Bson;

namespace Model
{
	public sealed class Session : Entity
	{
		private static uint RpcId { get; set; }
		private readonly NetworkComponent network;
		private readonly Dictionary<uint, Action<byte[], int, int>> requestCallback = new Dictionary<uint, Action<byte[], int, int>>();
		private readonly AChannel channel;
		private bool isRpc;

		public Session(NetworkComponent network, AChannel channel)
		{
			this.network = network;
			this.channel = channel;
			this.StartRecv();
		}

		public string RemoteAddress
		{
			get
			{
				return this.channel.RemoteAddress;
			}
		}

		public ChannelType ChannelType
		{
			get
			{
				return this.channel.ChannelType;
			}
		}

		private async void StartRecv()
		{
			TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
			while (true)
			{
				if (this.Id == 0)
				{
					return;
				}

				byte[] messageBytes;
				try
				{
					if (this.isRpc)
					{
						this.isRpc = false;
						await timerComponent.WaitAsync(0);
					}
					messageBytes = await channel.Recv();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
					continue;
				}

				if (messageBytes.Length < 6)
				{
					continue;
				}

				ushort opcode = BitConverter.ToUInt16(messageBytes, 0);

				try
				{
					this.Run(opcode, messageBytes);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		private void Run(ushort opcode, byte[] messageBytes)
		{
			int offset = 0;
			uint flag = BitConverter.ToUInt32(messageBytes, 2);

			bool isCompressed = (flag & 0x80000000) > 0;
			if (isCompressed) // 最高位为1,表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, 6, messageBytes.Length - 6);
				offset = 0;
			}
			else
			{
				offset = 6;
			}

			this.RunDecompressedBytes(opcode, flag, messageBytes, offset);
		}

		private void RunDecompressedBytes(ushort opcode, uint flag, byte[] messageBytes, int offset)
		{
			uint rpcFlag = flag & 0x40000000;
			uint rpcId = flag & 0x3fffffff;

			// 普通消息或者是Rpc请求消息
			if (rpcFlag == 0)
			{
				MessageInfo messageInfo = new MessageInfo(opcode, messageBytes, offset, rpcId);
				Type messageType = this.network.Owner.GetComponent<OpcodeTypeComponent>().GetType(messageInfo.Opcode);
				object message = MongoHelper.FromBson(messageType, messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
				messageInfo.Message = message;
				if (message is AActorMessage)
				{
					this.network.Owner.GetComponent<ActorMessageDispatherComponent>().Handle(this, messageInfo);
				}
				else
				{
					this.network.Owner.GetComponent<MessageDispatherComponent>().Handle(this, messageInfo);
				}
				return;
			}

			// rpcFlag>0 表示这是一个rpc响应消息
			// Rpc回调有找不着的可能，因为client可能取消Rpc调用
			if (!this.requestCallback.TryGetValue(rpcId, out Action<byte[], int, int> action))
			{
				return;
			}
			this.requestCallback.Remove(rpcId);
			action(messageBytes, offset, messageBytes.Length - offset);
		}

		/// <summary>
		/// Rpc调用
		/// </summary>
		public Task<Response> Call<Request, Response>(Request request, CancellationToken cancellationToken) where Request : ARequest
				where Response : AResponse
		{

			this.SendMessage(++RpcId, request);

			var tcs = new TaskCompletionSource<Response>();

			this.requestCallback[RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					if (response.Error != 0)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					//Log.Debug($"recv: {response.ToJson()}");
					this.isRpc = true;
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(RpcId); });

			return tcs.Task;
		}

		/// <summary>
		/// Rpc调用,发送一个消息,等待返回一个消息
		/// </summary>
		public Task<Response> Call<Request, Response>(Request request) where Request : ARequest where Response : AResponse
		{
			this.SendMessage(++RpcId, request);

			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					if (response.Error != 0)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					//Log.Info($"recv: {response.ToJson()}");
					this.isRpc = true;
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

			return tcs.Task;
		}

		public void Send<Message>(Message message) where Message : AMessage
		{
			if (this.Id == 0)
			{
				throw new Exception("session已经被Dispose了");
			}
			this.SendMessage(0, message);
		}

		public void Reply<Response>(uint rpcId, Response message)
		{
			if (this.Id == 0)
			{
				throw new Exception("session已经被Dispose了");
			}
			this.SendMessage(rpcId, message, false);
		}

		private void SendMessage(uint rpcId, object message, bool isCall = true)
		{
			//Log.Debug($"send: {message.ToJson()}");
			ushort opcode = this.network.Owner.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			byte[] opcodeBytes = BitConverter.GetBytes(opcode);
			if (!isCall)
			{
				rpcId = rpcId | 0x40000000;
			}

			byte[] messageBytes = message.ToBson();
			if (messageBytes.Length > 100)
			{
				byte[] newMessageBytes = ZipHelper.Compress(messageBytes);
				if (newMessageBytes.Length < messageBytes.Length)
				{
					messageBytes = newMessageBytes;
					rpcId = rpcId | 0x80000000;
				}
			}

			byte[] seqBytes = BitConverter.GetBytes(rpcId);

			channel.Send(new List<byte[]> { opcodeBytes, seqBytes, messageBytes });
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			this.channel.Dispose();
			this.network.Remove(id);
		}
	}
}