﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Model
{
	public abstract class ActorTask
	{
		public abstract Task<AResponse> Run();

		public abstract void RunFail(int error);
	}

	/// <summary>
	/// 普通消息，不需要response
	/// </summary>
	public class ActorMessageTask: ActorTask
	{
		private readonly ActorProxy proxy;
		private readonly ARequest message;

		public ActorMessageTask(ActorProxy proxy, ARequest message)
		{
			this.proxy = proxy;
			this.message = message;
		}

		public override async Task<AResponse> Run()
		{
			AResponse response = await this.proxy.RealCall<ActorMessageResponse>(this.message, this.proxy.CancellationTokenSource.Token);
			return response;
		}

		public override void RunFail(int error)
		{
		}
	}

	/// <summary>
	/// Rpc消息，需要等待返回
	/// </summary>
	/// <typeparam name="Response"></typeparam>
	public class ActorRpcTask<Response> : ActorTask where Response: AActorResponse
	{
		private readonly ActorProxy proxy;
		private readonly AActorRequest message;

		public readonly TaskCompletionSource<Response> Tcs = new TaskCompletionSource<Response>();

		public ActorRpcTask(ActorProxy proxy, AActorRequest message)
		{
			this.proxy = proxy;
			this.message = message;
		}

		public override async Task<AResponse> Run()
		{
			Response response = await this.proxy.RealCall<Response>(this.message, this.proxy.CancellationTokenSource.Token);
			if (response.Error != ErrorCode.ERR_NotFoundActor)
			{
				this.Tcs.SetResult(response);
			}
			return response;
		}

		public override void RunFail(int error)
		{
			this.Tcs.SetException(new RpcException(error, ""));
		}
	}


	[ObjectEvent]
	public class ActorProxyEvent : ObjectEvent<ActorProxy>, IAwake, IStart
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Start()
		{
			this.Get().Start();
		}
	}

	public sealed class ActorProxy : Entity
	{
		// actor的地址
		public string Address;

		// 已发送等待回应的消息
		public Queue<ActorTask> RunningTasks;

		// 还没发送的消息
		public Queue<ActorTask> WaitingTasks;

		// 发送窗口大小
		public int WindowSize = 1;

		// 最大窗口
		public const int MaxWindowSize = 100;

		private TaskCompletionSource<ActorTask> tcs;

		public CancellationTokenSource CancellationTokenSource;

		private int failTimes;
		
		public void Awake()
		{
			this.RunningTasks = new Queue<ActorTask>();
			this.WaitingTasks = new Queue<ActorTask>();
			this.WindowSize = 1;
			this.CancellationTokenSource = new CancellationTokenSource();
		}
		
		public void Start()
		{
			this.UpdateAsync();
		}

		private void Add(ActorTask task)
		{
			this.WaitingTasks.Enqueue(task);
			this.AllowGet();
		}

		private void Remove()
		{
			this.RunningTasks.Dequeue();
			this.AllowGet();
		}

		private void AllowGet()
		{
			if (this.tcs == null || this.WaitingTasks.Count <= 0 || this.RunningTasks.Count >= this.WindowSize)
			{
				return;
			}

			var t = this.tcs;
			this.tcs = null;
			ActorTask task = this.WaitingTasks.Dequeue();
			this.RunningTasks.Enqueue(task);
			t.SetResult(task);
		}

		private Task<ActorTask> GetAsync()
		{
			if (this.WaitingTasks.Count > 0)
			{
				ActorTask task = this.WaitingTasks.Dequeue();
				this.RunningTasks.Enqueue(task);
				return Task.FromResult(task);
			}

			this.tcs = new TaskCompletionSource<ActorTask>();
			return this.tcs.Task;
		}

		private async void UpdateAsync()
		{
			if (this.Address == null)
			{
				int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(this.Id);
				this.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().Address;
			}
			while (true)
			{
				ActorTask actorTask = await this.GetAsync();
				this.RunTask(actorTask);
			}
		}

		private async void RunTask(ActorTask task)
		{
			try
			{
				AResponse response = await task.Run();

				// 如果没找到Actor,发送窗口减少为1,重试
				if (response.Error == ErrorCode.ERR_NotFoundActor)
				{
					this.CancellationTokenSource.Cancel();
					this.WindowSize = 1;
					++this.failTimes;

					while (this.WaitingTasks.Count > 0)
					{
						ActorTask actorTask = this.WaitingTasks.Dequeue();
						this.RunningTasks.Enqueue(actorTask);
					}
					ObjectHelper.Swap(ref this.RunningTasks, ref this.WaitingTasks);

					// 失败3次则清空actor发送队列，返回失败
					if (this.failTimes > 3)
					{
						while (this.WaitingTasks.Count > 0)
						{
							ActorTask actorTask = this.WaitingTasks.Dequeue();
							actorTask.RunFail(response.Error);
						}
						return;
					}

					// 等待一会再发送
					await this.Parent.GetComponent<TimerComponent>().WaitAsync(this.failTimes * 500);
					int appId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(this.Id);
					this.Address = Game.Scene.GetComponent<StartConfigComponent>().Get(appId).GetComponent<InnerConfig>().Address;
					this.CancellationTokenSource = new CancellationTokenSource();
					this.AllowGet();
					return;
				}

				// 发送成功
				this.failTimes = 0;
				if (this.WindowSize < MaxWindowSize)
				{
					++this.WindowSize;
				}
				this.Remove();
				}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public void Send(AActorMessage message)
		{
			message.Id = this.Id;
			ActorMessageTask task = new ActorMessageTask(this, message);
			this.Add(task);
		}

		public Task<Response> Call<Response>(AActorRequest request)where Response : AActorResponse
		{
			request.Id = this.Id;
			ActorRpcTask<Response> task = new ActorRpcTask<Response>(this, request);
			this.Add(task);
			return task.Tcs.Task;
		}

		public async Task<Response> RealCall<Response>(ARequest request, CancellationToken cancellationToken) where Response: AResponse
		{
			try
			{
				Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.Address);
				Response response = await session.Call<Response>(request, cancellationToken);
				return response;
			}
			catch (RpcException e)
			{
				Log.Error($"{this.Address} {e}");
				throw;
			}
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}