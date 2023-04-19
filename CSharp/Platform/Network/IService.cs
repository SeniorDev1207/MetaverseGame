﻿using System;
using System.Threading.Tasks;

namespace Network
{
	public interface IService: IDisposable
	{
		/// <summary>
		/// 将函数调用加入IService线程
		/// </summary>
		/// <param name="action"></param>
		void Add(Action action);

		Task<IChannel> GetChannel(string host, int port, uint channel = 255);

		Task<IChannel> GetChannel(string address, uint channel = 255);

		Task<IChannel> GetChannel();

		void Remove(IChannel channel);

		void RunOnce(int timeout);

		void Run();
	}
}
