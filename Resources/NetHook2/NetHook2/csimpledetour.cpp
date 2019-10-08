
#include "csimpledetour.h"


CSimpleDetour::CSimpleDetour(void **old, void *replacement) noexcept
{
	m_fnOld = old;
	m_fnReplacement = replacement;
	m_bAttached = false;
}

void CSimpleDetour::Attach() noexcept
{
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	DetourAttach(m_fnOld, m_fnReplacement);

	DetourTransactionCommit();
	
	m_bAttached = true;
}

void CSimpleDetour::Detach() noexcept
{
	if (!m_bAttached)
		return;

	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	DetourDetach(m_fnOld, m_fnReplacement);

	DetourTransactionCommit();
}
