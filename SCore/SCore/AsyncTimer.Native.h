#pragma once
#include <chrono>
#include <asio.hpp>
#include "AutoCriticalSection.h"

namespace SCore
{
	namespace Native
	{
		class AsyncTimer;
		typedef void(CALLBACK *TimerCallback)(AsyncTimer &sender, void *data);

		class AsyncTimer
		{
		private:
			asio::steady_timer m_timer;
			void *m_data;
			TimerCallback m_callback;
			DWORD m_dueTime;
			DWORD m_perTime;
			AutoCriticalSection m_lock;
			BOOL m_running;
			BOOL m_reset;
		public:
			AsyncTimer(TimerCallback clb);
			AsyncTimer(TimerCallback clb, void *data, DWORD dueTime, DWORD perTime);
			void SetDueTime(DWORD dueTime);
			void SetPerTime(DWORD perTime);
			void SetData(void *data);
			void Start();
			void Start(DWORD dueTime, DWORD perTime);
			void Start(DWORD dueTime, DWORD perTime, void *data);
			void Stop();
			void Change(DWORD dueTime, DWORD perTime);
			void Change(DWORD dueTime, DWORD perTime, void *data);
			void Reset(DWORD dueTime, DWORD perTime);
			void Reset(DWORD dueTime, DWORD perTime, void *data);
			BOOL IsRunning();
			~AsyncTimer();
		private:
			void AsyncWaitCompleted(const asio::error_code &ec, asio::steady_timer &t);
		};
	}
}
