using System;

namespace TrickModule.Game
{
    [Flags]
    public enum MenuFadeEnableType
    {
        Off = 0,
        In = 1 << 0,
        Out = 1 << 1
    }
}