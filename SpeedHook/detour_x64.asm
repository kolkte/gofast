; detour_x64.asm
.DATA
    EXTERN g_multiplier: DWORD
    EXTERN g_trampoline: QWORD ; This will hold the address of the original code

.CODE

SpeedDetourX64 PROC
    ; Load the multiplier into XMM0
    movss xmm0, dword ptr [g_multiplier]
    ; Multiply XMM6 by the multiplier
    mulss xmm6, xmm0
    
    ; Jump to the trampoline to execute the original instruction
    ; and then return to the game.
    jmp qword ptr [g_trampoline]
SpeedDetourX64 ENDP

END