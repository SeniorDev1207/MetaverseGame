﻿using System;
using System.Collections.Generic;

namespace Model
{
	[Flags]
	public enum AppType
	{
		None = 0,
		Manager = 1,
		Realm = 2,
		Gate = 4,
		Http = 8,
		DB = 16,
		Location = 32,

		Robot = 64,
		Benchmark = 128,
		Client = 256,

		// 7
		AllServer = Manager | Realm | Gate | Http | DB | Location
	}

	public static class AppTypeHelper
	{
		public static List<AppType> GetServerTypes()
		{
			List<AppType> appTypes = new List<AppType> { AppType.Manager, AppType.Realm, AppType.Gate };
			return appTypes;
		}

		public static bool Is(this AppType a, AppType b)
		{
			if ((a & b) != 0)
			{
				return true;
			}
			return false;
		}
	}
}