﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base;

namespace Model
{
	[ObjectEvent]
	public class TimerComponentEvent: ObjectEvent<TimerComponent>, IUpdate
	{
		public void Update()
		{
			this.Get().Update();
		}
	}

	public class Timer
	{
		public long Id { get; set; }
		public long Time { get; set; }
		public TaskCompletionSource<bool> tcs;
	}

	public class TimerComponent: Component
	{
		private readonly Dictionary<long, Timer> timers = new Dictionary<long, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();

		private readonly Queue<long> timeoutTimer = new Queue<long>();

		public void Update()
		{
			long timeNow = TimeHelper.Now();
			foreach (long time in this.timeId.Keys)
			{
				if (time > timeNow)
				{
					break;
				}
				this.timeoutTimer.Enqueue(time);
			}

			while (this.timeoutTimer.Count > 0)
			{
				long key = this.timeoutTimer.Dequeue();
				long[] timeOutId = this.timeId.GetAll(key);
				foreach (long id in timeOutId)
				{
					if (!this.timers.TryGetValue(id, out Timer timer))
					{
						continue;
					}
					this.Remove(id);
					timer.tcs.SetResult(true);
				}
			}
		}

		private void Remove(long id)
		{
			if (!this.timers.TryGetValue(id, out Timer timer))
			{
				return;
			}
			this.timers.Remove(id);
			this.timeId.Remove(timer.Time, timer.Id);
		}

		public Task WaitTillAsync(long tillTime, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

		public Task WaitTillAsync(long tillTime)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return tcs.Task;
		}

		public Task WaitAsync(long time, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.Now() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

		public Task WaitAsync(long time)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.Now() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return tcs.Task;
		}
	}
}