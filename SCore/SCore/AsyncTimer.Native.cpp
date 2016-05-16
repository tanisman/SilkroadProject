#include "stdafx.h"
#include "SCore.h"
#include "AsyncTimer.Native.h"

using namespace SCore::Native;

AsyncTimer::AsyncTimer(TimerCallback clb)
	: m_timer(SCore::ios), m_callback(clb), m_running(FALSE), m_reset (FALSE)
{

}

AsyncTimer::AsyncTimer(TimerCallback clb, void *data, DWORD dueTime, DWORD perTime)
	: m_timer(SCore::ios, std::chrono::milliseconds(dueTime)), m_data(data), m_callback(clb), m_dueTime(dueTime), m_perTime(perTime), m_running(TRUE), m_reset(FALSE)
{
	if (dueTime < 0)
		throw new std::exception("due time cannot be smaller than 0");

	m_timer.async_wait(std::bind(&AsyncTimer::AsyncWaitCompleted, this, std::placeholders::_1, std::ref(m_timer)));
}

AsyncTimer::~AsyncTimer()
{
	AutoLockScope locker(&m_lock);
	asio::error_code ec;
	m_timer.cancel(ec);
	m_running = FALSE;
}

void AsyncTimer::SetDueTime(DWORD dueTime)
{
	AutoLockScope locker(&m_lock);
	m_dueTime = dueTime;
}

void AsyncTimer::SetPerTime(DWORD perTime)
{
	AutoLockScope locker(&m_lock);
	m_perTime = perTime;
}

void AsyncTimer::SetData(void * data)
{
	AutoLockScope locker(&m_lock);
	m_data = data;
}

void AsyncTimer::Start()
{
	AutoLockScope locker(&m_lock);
	if (IsRunning())
	{
		printf("timer is already running\n");
		throw new std::exception("timer is already running");
	}

	m_timer.expires_from_now(std::chrono::milliseconds(m_dueTime));
	m_timer.async_wait(std::bind(&AsyncTimer::AsyncWaitCompleted, this, std::placeholders::_1, std::ref(m_timer)));

	m_running = TRUE;
}

void AsyncTimer::Start(DWORD dueTime, DWORD perTime)
{
	AutoLockScope locker(&m_lock);
	if (IsRunning())
	{
		printf("timer is already running\n");
		throw new std::exception("timer is already running");
	}

	if (dueTime < 0)
		throw new std::exception("due time cannot be smaller than 0");

	SetDueTime(dueTime);
	SetPerTime(perTime);

	m_timer.expires_from_now(std::chrono::milliseconds(m_dueTime));
	m_timer.async_wait(std::bind(&AsyncTimer::AsyncWaitCompleted, this, std::placeholders::_1, std::ref(m_timer)));

	m_running = TRUE;
}

void AsyncTimer::Start(DWORD dueTime, DWORD perTime, void *data)
{
	AutoLockScope locker(&m_lock);
	if (IsRunning())
	{
		printf("timer is already running\n");
		throw new std::exception("timer is already running");
	}

	if (dueTime < 0)
		throw new std::exception("due time cannot be smaller than 0");

	SetDueTime(dueTime);
	SetPerTime(perTime);
	SetData(data);

	m_timer.expires_from_now(std::chrono::milliseconds(m_dueTime));
	m_timer.async_wait(std::bind(&AsyncTimer::AsyncWaitCompleted, this, std::placeholders::_1, std::ref(m_timer)));

	m_running = TRUE;
}

void AsyncTimer::Stop()
{
	AutoLockScope locker(&m_lock);
	m_running = FALSE;
	asio::error_code ec;
	m_timer.cancel(ec);
	if (ec)
		printf("cannot stop timer (exception code: %d)\n", ec.value());
}

void AsyncTimer::Change(DWORD dueTime, DWORD perTime, void *data)
{
	AutoLockScope locker(&m_lock);
	if (IsRunning())
		Stop();

	Start(dueTime, perTime, data);
}

void AsyncTimer::Change(DWORD dueTime, DWORD perTime)
{
	AutoLockScope locker(&m_lock);
	if (IsRunning())
		Stop();

	Start(dueTime, perTime);
}

void AsyncTimer::Reset(DWORD dueTime, DWORD perTime)
{
	AutoLockScope locker(&m_lock);
	SetDueTime(dueTime);
	SetPerTime(perTime);
	m_reset = TRUE;
}

void AsyncTimer::Reset(DWORD dueTime, DWORD perTime, void *data)
{
	AutoLockScope locker(&m_lock);
	SetDueTime(dueTime);
	SetPerTime(perTime);
	SetData(data);
	m_reset = TRUE;
}

BOOL AsyncTimer::IsRunning()
{
	AutoLockScope locker(&m_lock);
	return m_running;
}

void AsyncTimer::AsyncWaitCompleted(const asio::error_code &ec, asio::steady_timer &t)
{
	if (AsyncTimer::IsRunning())
	{
		if (ec)
			printf("error at Native::AsyncTimer::AsyncWaitCompleted %d\n", ec.value());
		else
		{
			m_callback(*this, m_data);

			AutoLockScope locker(&m_lock);
			if (m_reset)
			{
				m_reset = FALSE;
				if (m_dueTime == -1)
				{
					m_running = FALSE;
					return;
				}

				m_timer.expires_from_now(std::chrono::milliseconds(m_dueTime));
				m_timer.async_wait(std::bind(&AsyncTimer::AsyncWaitCompleted, this, std::placeholders::_1, std::ref(m_timer)));
				return;
			}

			if (m_perTime > 0)
			{
				m_timer.expires_from_now(std::chrono::milliseconds(m_perTime));
				m_timer.async_wait(std::bind(&AsyncTimer::AsyncWaitCompleted, this, std::placeholders::_1, std::ref(m_timer)));
			}
			else
				m_running = FALSE;
		}
	}
}