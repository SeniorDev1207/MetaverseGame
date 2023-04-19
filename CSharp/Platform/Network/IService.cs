﻿using System;
using System.Threading.Tasks;

namespace Network
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

		Task<AChannel> GetChannel(string host, int port);

		Task<AChannel> GetChannel();

		void Remove(AChannel channel);

		void RunOnce(int timeout);

		void Run();
	}
}