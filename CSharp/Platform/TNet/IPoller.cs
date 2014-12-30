﻿using System;

namespace TNet
{
	public interface IPoller
	{
		void Add(Action action);

		void Run(int timeout);
	}
}
