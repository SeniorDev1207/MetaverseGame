﻿namespace Model
{
	public static class EventType
	{
		public const int BeforeAddBuff = 0;
		public const int AfterAddBuff = 1;
		public const int BeforeRemoveBuff = 2;
		public const int AfterRemoveBuff = 3;
	}

	public static class ActionType
	{
		public const int BuffTimeoutAction = 0;
		public const int MessageAction = 1;
	}
}