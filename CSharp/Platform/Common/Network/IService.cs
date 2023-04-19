﻿using System;
using System.Threading.Tasks;
using Common.Base;
using MongoDB.Bson;

namespace Common.Network
{
	public enum NetworkProtocol
	{
		TCP,
		UDP,
	}

	public interface IService: IDisposable
	{
		/// <summary>
		/// 将函数调用加入IService线程
		/// </summary>
		/// <param name="action"></param>
		void Add(Action action);

		AChannel GetChannel(ObjectId id);

		Task<AChannel> GetChannel(string host, int port);

		Task<AChannel> GetChannel(string address);

		Task<AChannel> GetChannel();

		bool HasChannel(string address);

		void Remove(AChannel channel);

		void Update();

		TimerManager Timer { get; }
	}
}