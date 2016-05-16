#pragma once
#include "AsyncTimer.Native.h"

using namespace System;
using namespace System::Runtime::InteropServices;


namespace SCore
{
	public ref class AsyncTimer : IDisposable
	{
	private:
		delegate void NativeCallbackDelegate(Native::AsyncTimer &, void *);
		Native::AsyncTimer *m_nativeHandle;
		Object^ m_data;
		EventHandler<Object^>^ m_callback;

		GCHandle m_gcHandle;
		NativeCallbackDelegate^ m_fpCallback;
	public:
		AsyncTimer(EventHandler<Object^>^ callback);
		AsyncTimer(EventHandler<Object^>^ callback, Object^ data, UInt32 dueTime, UInt32 perTime);
		void Start(Int32 dueTime, Int32 perTime);
		void Start(Int32 dueTime, Int32 perTime, Object^ data);
		void Stop();
		void Change(Int32 dueTime, Int32 perTime);
		void Change(Int32 dueTime, Int32 perTime, Object^ data);
		void Change(Object^ data);
		void Reset(Int32 dueTime, Int32 perTime);
		void Reset(Int32 dueTime, Int32 perTime, Object^ data);
		~AsyncTimer();
	private:
		void AsyncWaitCompleted(Native::AsyncTimer &sender, void *data);
	public:
		property bool IsRunning
		{
			bool get();
		}
	};
}

