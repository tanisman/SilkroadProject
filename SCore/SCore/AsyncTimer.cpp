#include "stdafx.h"
#include "AsyncTimer.h"

using namespace SCore;

AsyncTimer::AsyncTimer(EventHandler<Object^>^ callback)
		: m_callback( callback )
{
	m_fpCallback = gcnew NativeCallbackDelegate(this, &AsyncTimer::AsyncWaitCompleted);
	m_gcHandle = GCHandle::Alloc(m_fpCallback);
	IntPtr ip = Marshal::GetFunctionPointerForDelegate(m_fpCallback);
	Native::TimerCallback cb = static_cast<Native::TimerCallback>(ip.ToPointer());

	m_nativeHandle = new Native::AsyncTimer(cb);
}

AsyncTimer::AsyncTimer(EventHandler<Object^>^ callback, Object^ data, UInt32 dueTime, UInt32 perTime)
		: m_callback( callback ) , m_data ( data )
{
	m_fpCallback = gcnew NativeCallbackDelegate(this, &AsyncTimer::AsyncWaitCompleted);
	m_gcHandle = GCHandle::Alloc(m_fpCallback);
	IntPtr ip = Marshal::GetFunctionPointerForDelegate(m_fpCallback);
	Native::TimerCallback cb = static_cast<Native::TimerCallback>(ip.ToPointer());
	
	m_nativeHandle = new Native::AsyncTimer(cb, NULL, dueTime, perTime);
}

void AsyncTimer::Start(Int32 dueTime, Int32 perTime)
{
	m_nativeHandle->Start(dueTime, perTime);
}

void AsyncTimer::Start(Int32 dueTime, Int32 perTime, Object^ data)
{
	m_data = data;
	m_nativeHandle->Start(dueTime, perTime);
}

void AsyncTimer::Stop()
{
	m_nativeHandle->Stop();
}

void AsyncTimer::Change(Int32 dueTime, Int32 perTime)
{
	m_nativeHandle->Change(dueTime, perTime);
}

void AsyncTimer::Change(Int32 dueTime, Int32 perTime, Object^ data)
{
	m_data = data;
	m_nativeHandle->Change(dueTime, perTime);
}

void AsyncTimer::Change(Object^ data)
{
	m_data = data;
}

void AsyncTimer::Reset(Int32 dueTime, Int32 perTime)
{
	m_nativeHandle->Reset(dueTime, perTime);
}

void AsyncTimer::Reset(Int32 dueTime, Int32 perTime, Object^ data)
{
	m_data = data;
	m_nativeHandle->Reset(dueTime, perTime);
}

bool AsyncTimer::IsRunning::get()
{
	return m_nativeHandle->IsRunning() == 1;
}

AsyncTimer::~AsyncTimer()
{
	delete m_nativeHandle;
	m_gcHandle.Free();
	m_fpCallback = nullptr;
}

void AsyncTimer::AsyncWaitCompleted(Native::AsyncTimer &sender, void *data)
{
	m_callback(this, m_data);
}
