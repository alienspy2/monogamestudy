using System;
using System.Runtime.InteropServices;

public static class ClipboardHelperSDL
{
    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_SetClipboardText(string text);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetClipboardText();

    public static void SetClipboardText(string text)
    {
        SDL_SetClipboardText(text);
    }

    public static string GetClipboardText()
    {
        IntPtr textPtr = SDL_GetClipboardText();
        return Marshal.PtrToStringAnsi(textPtr);
    }
}
