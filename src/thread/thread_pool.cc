#include <boost/foreach.hpp>
#include <glog/logging.h>
#include "thread/thread_pool.h"

namespace hainan
{
	ThreadPool::ThreadPool():
			num(0), running(false), work_num(0)
	{
	}
	ThreadPool::~ThreadPool()
	{
	}

	void ThreadPool::Start()
	{
		running = true;
		for(int i = 0; i < num; ++i)
		{
			shared_ptr<thread> t(new thread(bind(&ThreadPool::Loop, ref(this))));
			threads.push_back(t);
			t->detach();
		}
		work_num = num;
	}

	void ThreadPool::Stop()
	{
		VLOG(2) << "Stop";
		mutex::scoped_lock lock(mtx);
		running = false;
		cond.notify_all();
		while(work_num > 0)
		{
			VLOG(2) << "done tasks size = " << tasks.size();
			done.wait(lock);
		}
	}

	void ThreadPool::Loop()
	{
		VLOG(2) << "thread start";
		bool continued = true;
		while(continued)
		{
			function<void (void)> task(0);
			{
				mutex::scoped_lock lock(mtx);
				if(running && tasks.empty())
				{
					cond.wait(lock);
					VLOG(2) << "get cond";
				}

				if(!tasks.empty())
				{
					VLOG(2) << "fetch task";
					task = tasks.front();
					tasks.pop_front();
				}
				continued = running || !tasks.empty();
				VLOG(2) << "running = " << running;
			}

			if(task)
			{
				task();
			}
		}
		if(__sync_sub_and_fetch(&work_num, 1) == 0)
		{
			VLOG(2) << "work_num = " << work_num;
			done.notify_one();
		}
	}

	bool ThreadPool::PushTask(function<void (void)> task)
	{
		VLOG(2) << "push task";
		{
			mutex::scoped_lock lock(mtx);
			if(!running)
			{
				return false;
			}
			tasks.push_back(task);
		}
		cond.notify_one();
		return true;
	}

	void ThreadPool::SetNum(int32_t n)
	{
		num = n;
	}
}
