#include "Stdafx.h"
#include "AutoCriticalSection.h"
using namespace SCore::Native;

AutoCriticalSection::AutoCriticalSection()
{
	InitializeCriticalSection(&m_cs);
}


AutoCriticalSection::~AutoCriticalSection()
{
	DeleteCriticalSection(&m_cs);
}

void AutoCriticalSection::Lock()
{
	EnterCriticalSection(&m_cs);
}

void AutoCriticalSection::Unlock()
{
	LeaveCriticalSection(&m_cs);
}