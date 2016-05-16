#pragma once

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
namespace SCore
{
	namespace Native
	{
		class AutoCriticalSection
		{
		private:
			CRITICAL_SECTION m_cs;
		public:
			AutoCriticalSection();
			AutoCriticalSection(DWORD dwSpinCount);
			~AutoCriticalSection();
			void Lock();
			bool TryLock();
			void Unlock();
		};

		class AutoLockScope
		{
		private:
			AutoCriticalSection *m_pAcs;
		public:
			AutoLockScope(AutoCriticalSection *pAcs)
				: m_pAcs(pAcs)
			{
				m_pAcs->Lock();
			}
			~AutoLockScope()
			{
				m_pAcs->Unlock();
			}
		};
	}
}