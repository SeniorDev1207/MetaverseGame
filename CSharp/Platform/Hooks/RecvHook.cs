﻿using System;
using System.Runtime.InteropServices;
using EasyHook;
using ELog;

namespace Hooks
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
	public delegate int DRecv(IntPtr handle, IntPtr buf, int count, int flag);

	public class RecvHook : IDisposable
	{
		[DllImport("Ws2_32.dll", EntryPoint = "recv")]
		public static extern int Recv(IntPtr handle, IntPtr buf, int count, int flag);

		private readonly LocalHook localHook;

		public RecvHook(DRecv dRecv)
		{
			try
			{
				localHook = LocalHook.Create(LocalHook.GetProcAddress("Ws2_32.dll", "recv"), new DRecv(dRecv), this);
				localHook.ThreadACL.SetInclusiveACL(new[] { 0 });
			}
			catch (Exception)
			{
				Log.Debug("Error creating recv Hook");
				throw;
			}
		}

		public void Dispose()
		{
			localHook.Dispose();
		}

		//static int RecvHooked(IntPtr socketHandle, IntPtr buf, int count, int socketFlags)
		//{
		//	int bytesCount = recv(socketHandle, buf, count, socketFlags);
		//	if (bytesCount > 0)
		//	{
		//		var newBuffer = new byte[bytesCount];
		//		Marshal.Copy(buf, newBuffer, 0, bytesCount);
		//		string s = Encoding.ASCII.GetString(newBuffer);
		//		TextWriter tw = new StreamWriter("recv.txt");
		//		tw.Write(s);
		//		tw.Close();
		//		Log.Debug(string.Format("Hooked:>{0}", s));
		//	}
		//	return bytesCount;
		//}
	}
}
