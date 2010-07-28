
#include "csimpledetour.h"


CSimpleDetour::CSimpleDetour(void **old, void *replacement)
{
	m_fnOld = old;
	m_fnReplacement = replacement;
}

void CSimpleDetour::Attach()
{
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	DetourAttach(m_fnOld, m_fnReplacement);

	DetourTransactionCommit();
	
	m_bAttached = true;
}

void CSimpleDetour::Detach()
{
	if (!m_bAttached)
		return;

	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	DetourDetach(m_fnOld, m_fnReplacement);

	DetourTransactionCommit();
}