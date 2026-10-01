#include "pch.h"
#include "MinHook.h"
#include <cstdint>

// Exports for C#
extern "C" __declspec(dllexport) bool InitializeHook(uintptr_t baseAddress, uintptr_t offset);
extern "C" __declspec(dllexport) void SetMultiplier(float multiplier);
extern "C" __declspec(dllexport) void ShutdownHook();

// Implemented in detour_x64.asm
extern "C" void SpeedDetourX64();

// Globals shared with the MASM detour.
extern "C" float g_multiplier = 1.0f;
extern "C" void* g_trampoline = nullptr; // This is new!

static LPVOID g_targetAddress = nullptr;
static bool g_hookInstalled = false;

bool InitializeHook(uintptr_t baseAddress, uintptr_t offset)
{
    if (g_hookInstalled)
        return true;

    if (MH_Initialize() != MH_OK)
        return false;

    g_targetAddress = (LPVOID)(baseAddress + offset);

    // Create the hook. MH_CreateHook will give us the address of the trampoline
    // in the g_trampoline variable.
    if (MH_CreateHook(g_targetAddress, (LPVOID)&SpeedDetourX64, &g_trampoline) != MH_OK)
        return false;

    if (MH_EnableHook(g_targetAddress) != MH_OK)
        return false;

    g_hookInstalled = true;
    return true;
}

void SetMultiplier(float multiplier)
{
    g_multiplier = multiplier;
}

void ShutdownHook()
{
    if (!g_hookInstalled)
        return;

    MH_DisableHook(g_targetAddress);
    MH_RemoveHook(g_targetAddress);
    MH_Uninitialize();
    g_hookInstalled = false;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hModule);
        break;
    }
    return TRUE;
}