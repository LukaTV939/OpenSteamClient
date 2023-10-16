using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

namespace ClientUI.PlatformSpecific;

/// <summary>
/// Transforms an avalonia Key to an X11 key.
/// </summary>
public static class X11KeyTransform
{
    // this list is taken directly from Avalonia's source code at https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.X11/X11KeyTransform.cs
    private static readonly Dictionary<X11Key, Key> keyFromX11Key = new()
    {
        {X11Key.Cancel, Key.Cancel},
        {X11Key.BackSpace, Key.Back},
        {X11Key.Tab, Key.Tab},
        {X11Key.Linefeed, Key.LineFeed},
        {X11Key.Clear, Key.Clear},
        {X11Key.Return, Key.Return},
        {X11Key.Pause, Key.Pause},
        {X11Key.Caps_Lock, Key.CapsLock},
        {X11Key.Escape, Key.Escape},
        {X11Key.space, Key.Space},
        {X11Key.Prior, Key.Prior},
        {X11Key.Page_Down, Key.PageDown},
        {X11Key.End, Key.End},
        {X11Key.Home, Key.Home},
        {X11Key.Left, Key.Left},
        {X11Key.Up, Key.Up},
        {X11Key.Right, Key.Right},
        {X11Key.Down, Key.Down},
        {X11Key.Select, Key.Select},
        {X11Key.Print, Key.Print},
        {X11Key.Execute, Key.Execute},
        {X11Key.Insert, Key.Insert},
        {X11Key.Delete, Key.Delete},
        {X11Key.Help, Key.Help},
        {X11Key.a, Key.A},
        {X11Key.b, Key.B},
        {X11Key.c, Key.C},
        {X11Key.d, Key.D},
        {X11Key.e, Key.E},
        {X11Key.f, Key.F},
        {X11Key.g, Key.G},
        {X11Key.h, Key.H},
        {X11Key.i, Key.I},
        {X11Key.j, Key.J},
        {X11Key.k, Key.K},
        {X11Key.l, Key.L},
        {X11Key.m, Key.M},
        {X11Key.n, Key.N},
        {X11Key.o, Key.O},
        {X11Key.p, Key.P},
        {X11Key.q, Key.Q},
        {X11Key.r, Key.R},
        {X11Key.s, Key.S},
        {X11Key.t, Key.T},
        {X11Key.u, Key.U},
        {X11Key.v, Key.V},
        {X11Key.w, Key.W},
        {X11Key.x, Key.X},
        {X11Key.y, Key.Y},
        {X11Key.z, Key.Z},
        {X11Key.Super_L, Key.LWin },
        {X11Key.Super_R, Key.RWin },
        {X11Key.Menu, Key.Apps},
        {X11Key.KP_0, Key.NumPad0},
        {X11Key.KP_1, Key.NumPad1},
        {X11Key.KP_2, Key.NumPad2},
        {X11Key.KP_3, Key.NumPad3},
        {X11Key.KP_4, Key.NumPad4},
        {X11Key.KP_5, Key.NumPad5},
        {X11Key.KP_6, Key.NumPad6},
        {X11Key.KP_7, Key.NumPad7},
        {X11Key.KP_8, Key.NumPad8},
        {X11Key.KP_9, Key.NumPad9},
        {X11Key.multiply, Key.Multiply},
        {X11Key.F1, Key.F1},
        {X11Key.F2, Key.F2},
        {X11Key.F3, Key.F3},
        {X11Key.F4, Key.F4},
        {X11Key.F5, Key.F5},
        {X11Key.F6, Key.F6},
        {X11Key.F7, Key.F7},
        {X11Key.F8, Key.F8},
        {X11Key.F9, Key.F9},
        {X11Key.F10, Key.F10},
        {X11Key.F11, Key.F11},
        {X11Key.F12, Key.F12},
        {X11Key.L3, Key.F13},
        {X11Key.F14, Key.F14},
        {X11Key.L5, Key.F15},
        {X11Key.F16, Key.F16},
        {X11Key.F17, Key.F17},
        {X11Key.L8, Key.F18},
        {X11Key.L9, Key.F19},
        {X11Key.L10, Key.F20},
        {X11Key.R1, Key.F21},
        {X11Key.R2, Key.F22},
        {X11Key.F23, Key.F23},
        {X11Key.R4, Key.F24},
        {X11Key.Num_Lock, Key.NumLock},
        {X11Key.Scroll_Lock, Key.Scroll},
        {X11Key.Shift_L, Key.LeftShift},
        {X11Key.Shift_R, Key.RightShift},
        {X11Key.Control_L, Key.LeftCtrl},
        {X11Key.Control_R, Key.RightCtrl},
        {X11Key.Alt_L, Key.LeftAlt},
        {X11Key.Alt_R, Key.RightAlt},
        {X11Key.minus, Key.OemMinus},
        {X11Key.plus, Key.OemPlus},
        {X11Key.bracketleft, Key.OemOpenBrackets},
        {X11Key.bracketright, Key.OemCloseBrackets},
        {X11Key.bar, Key.OemPipe},
        {X11Key.semicolon, Key.OemSemicolon},
        {X11Key.quotedbl, Key.OemQuotes},
        {X11Key.comma, Key.OemComma},
        {X11Key.period, Key.OemPeriod},
        {X11Key.slash, Key.Oem2},
        {X11Key.asciitilde, Key.OemTilde},
        {X11Key.XK_1, Key.D1},
        {X11Key.XK_2, Key.D2},
        {X11Key.XK_3, Key.D3},
        {X11Key.XK_4, Key.D4},
        {X11Key.XK_5, Key.D5},
        {X11Key.XK_6, Key.D6},
        {X11Key.XK_7, Key.D7},
        {X11Key.XK_8, Key.D8},
        {X11Key.XK_9, Key.D9},
        {X11Key.XK_0, Key.D0},
    };

    private static readonly Dictionary<Key, X11Key> internalKeyToX11KeyMap = keyFromX11Key.ToDictionary(x => x.Value, x => x.Key);

    public static Key KeyFromX11Key(X11Key key) 
        => keyFromX11Key.TryGetValue(key, out var result) ? result : Key.None;
        
    public static X11Key X11KeyFromKey(Key key)
        // we use the notsign to indicate an unsupported key.
        => internalKeyToX11KeyMap.TryGetValue(key, out var result) ? result : X11Key.notsign;
}