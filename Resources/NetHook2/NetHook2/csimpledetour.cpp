#include <cstdio>
#include "csimpledetour.h"


CSimpleDetour::CSimpleDetour(void **old, void *replacement) noexcept
{
    m_fnOld = old;
    m_fnReplacement = replacement;
    m_hFunchook = funchook_create();
    m_bAttached = false;
}

void CSimpleDetour::Attach() noexcept
{
    if(funchook_prepare(m_hFunchook, m_fnOld, m_fnReplacement))
    {
        return;
    }

    if(funchook_install(m_hFunchook, 0))
    {
        return;
    }

    m_bAttached = true;
}

void CSimpleDetour::Detach() noexcept
{
    if (!m_bAttached)
        return;

    funchook_uninstall(m_hFunchook, 0);
}
