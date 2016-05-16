// SCore.h

#pragma once
#include "stdafx.h"
#include <asio.hpp>

using namespace System;

namespace SCore 
{
	static asio::io_service ios(2);
	static asio::io_service::work w(ios);
	static std::list<asio::thread> ioworkers;

	void run(int numthreads)
	{
		unsigned int worker_count = numthreads;

		if (worker_count < 1)
		{
			SYSTEM_INFO sysinfo;
			GetSystemInfo(&sysinfo);
			worker_count = sysinfo.dwNumberOfProcessors * 2;
		}

		if (worker_count == 0)
			worker_count = 16;
		
		for (int i = 0; i < worker_count; i++)
			ioworkers.emplace_back([&] { ios.run(); });
	}
	
	public ref class TimerService
	{
	internal:

	public:
		static void Initialize(int numthreads)
		{
			run(numthreads);
		}

		static void Initialize()
		{
			run(0);
		}
	};
}
