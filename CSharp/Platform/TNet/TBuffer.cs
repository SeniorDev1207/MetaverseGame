﻿using System;
using System.Collections.Generic;

namespace TNet
{
	public class TBuffer
	{
		public const int ChunkSize = 8096;

		private readonly LinkedList<byte[]> bufferList = new LinkedList<byte[]>();

		public int LastIndex { get; set; }

		public int FirstIndex { get; set; }

		public int Count
		{
			get
			{
				if (this.bufferList.Count == 0)
				{
					return 0;
				}
				return (this.bufferList.Count - 1) * ChunkSize + this.LastIndex - this.FirstIndex;
			}
		}

		public void AddLast()
		{
			this.bufferList.AddLast(new byte[ChunkSize]);
		}

		public void RemoveFirst()
		{
			this.bufferList.RemoveFirst();
		}

		public byte[] First
		{
			get
			{
				if (this.bufferList.First == null)
				{
					this.AddLast();
				}
				return this.bufferList.First.Value;
			}
		}

		public byte[] Last
		{
			get
			{
				if (this.bufferList.Last == null)
				{
					this.AddLast();
				}
				return this.bufferList.Last.Value;
			}
		}

		public void RecvFrom(byte[] buffer)
		{
			if (this.Count < buffer.Length || buffer.Length == 0)
			{
				throw new Exception(string.Format("bufferList size < n, bufferList: {0} buffer length: {1}",
				                                  this.Count, buffer.Length));
			}
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < buffer.Length)
			{
				int n = buffer.Length - alreadyCopyCount;
				if (ChunkSize - this.FirstIndex > n)
				{
					Array.Copy(this.bufferList.First.Value, this.FirstIndex, buffer, alreadyCopyCount, n);
					this.FirstIndex += n;
					alreadyCopyCount += n;
				}
				else
				{
					Array.Copy(this.bufferList.First.Value, this.FirstIndex, buffer, alreadyCopyCount,
					           ChunkSize - this.FirstIndex);
					alreadyCopyCount += ChunkSize - this.FirstIndex;
					this.FirstIndex = 0;
					this.bufferList.RemoveFirst();
				}
			}
		}

		public void SendTo(byte[] buffer)
		{
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < buffer.Length)
			{
				if (this.LastIndex == 0)
				{
					this.bufferList.AddLast(new byte[ChunkSize]);
				}
				int n = buffer.Length - alreadyCopyCount;
				if (ChunkSize - this.LastIndex > n)
				{
					Array.Copy(buffer, alreadyCopyCount, this.bufferList.Last.Value, this.LastIndex, n);
					this.LastIndex += buffer.Length - alreadyCopyCount;
					alreadyCopyCount += n;
				}
				else
				{
					Array.Copy(buffer, alreadyCopyCount, this.bufferList.Last.Value, this.LastIndex,
					           ChunkSize - this.LastIndex);
					alreadyCopyCount += ChunkSize - this.LastIndex;
					this.LastIndex = 0;
				}
			}
		}
	}
}