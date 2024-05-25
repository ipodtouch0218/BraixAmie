using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TutorialRunner.Monitoring {
    public sealed class KeyboardHook {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProcess lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        private delegate IntPtr KeyboardProcess(int nCode, IntPtr wParam, IntPtr lParam);

        public static event EventHandler<KeyPressedEventArgs> KeyPressed;
        private const int WhKeyboard = 13;
        private const int WmKeydown = 0x0100;
        private static KeyboardProcess keyboardProc = HookCallback;
        private static IntPtr hookID = IntPtr.Zero;

        private static readonly Dictionary<int, KeyCode> VKeyMappings = new();
        private static readonly HashSet<int> DownKeys = new();

        public static void CreateHook() {
            hookID = SetHook(keyboardProc);
            DownKeys.Clear();
            VKeyMappings.Clear();
            KeyCodes.SetMappings(VKeyMappings);
        }

        public static void DisposeHook() {
            UnhookWindowsHookEx(hookID);
        }

        private static IntPtr SetHook(KeyboardProcess keyboardProc) {
            using Process currentProcess = Process.GetCurrentProcess();
            using ProcessModule currentProcessModule = currentProcess.MainModule;

            return SetWindowsHookEx(WhKeyboard, keyboardProc, GetModuleHandle(currentProcessModule.ModuleName), 0);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {

            if (nCode >= 0) {
                bool callEvent = true;
                int vkCode = Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr) WmKeydown) {
                    // Key down
                    if (DownKeys.Contains(vkCode)) {
                        callEvent = false;
                    } else {
                        DownKeys.Add(vkCode);
                    }
                } else {
                    // Key up
                    DownKeys.Remove(vkCode);
                    callEvent = false;
                }

                if (callEvent && VKeyMappings.TryGetValue(vkCode, out KeyCode code)) {
                    KeyPressed?.Invoke(null, new KeyPressedEventArgs(code));
                }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }

    public class KeyPressedEventArgs : EventArgs {
        public KeyCode Info { get; set; }
        public KeyPressedEventArgs(KeyCode key) {
            Info = key;
        }
    }

    public class KeyCodes {
        public const int VkAbntC1 = 0xC1;//	Abnt C1
        public const int VkAbntC2 = 0xC2;//	Abnt C2
        public const int VkAdd = 0x6B;//	Numpad +
        public const int VkAttn = 0xF6;//	Attn
        public const int VkBack = 0x08;//	Backspace
        public const int VkCancel = 0x03;//	Break
        public const int VkClear = 0x0C;//	Clear
        public const int VkCrsel = 0xF7;//	Cr Sel
        public const int VkDecimal = 0x6E;//	Numpad.
        public const int VkDivide = 0x6F;//	Numpad /
        public const int VkEreof = 0xF9;//	Er Eof
        public const int VkEscape = 0x1B;//	Esc
        public const int VkExecute = 0x2B;//	Execute
        public const int VkExsel = 0xF8;//	Ex Sel
        public const int VkIcoClear = 0xE6;//	IcoClr
        public const int VkIcoHelp = 0xE3;//	IcoHlp

        public const int VkKey0 = 0x30;// ('0')	0
        public const int VkKey1 = 0x31;// ('1')	1
        public const int VkKey2 = 0x32;// ('2')	2
        public const int VkKey3 = 0x33;// ('3')	3
        public const int VkKey4 = 0x34;// ('4')	4
        public const int VkKey5 = 0x35;// ('5')	5
        public const int VkKey6 = 0x36;// ('6')	6
        public const int VkKey7 = 0x37;// ('7')	7
        public const int VkKey8 = 0x38;// ('8')	8
        public const int VkKey9 = 0x39;// ('9')	9
        public const int VkKeyA = 0x41;// ('A')	A
        public const int VkKeyB = 0x42;// ('B')	B
        public const int VkKeyC = 0x43;// ('C')	C
        public const int VkKeyD = 0x44;// ('D')	D
        public const int VkKeyE = 0x45;// ('E')	E
        public const int VkKeyF = 0x46;// ('F')	F
        public const int VkKeyG = 0x47;// ('G')	G
        public const int VkKeyH = 0x48;// ('H')	H
        public const int VkKeyI = 0x49;// ('I')	I
        public const int VkKeyJ = 0x4A;// ('J')	J
        public const int VkKeyK = 0x4B;// ('K')	K
        public const int VkKeyL = 0x4C;// ('L')	L
        public const int VkKeyM = 0x4D;// ('M')	M
        public const int VkKeyN = 0x4E;// ('N')	N
        public const int VkKeyO = 0x4F;// ('O')	O
        public const int VkKeyP = 0x50;// ('P')	P
        public const int VkKeyQ = 0x51;// ('Q')	Q
        public const int VkKeyR = 0x52;// ('R')	R
        public const int VkKeyS = 0x53;// ('S')	S
        public const int VkKeyT = 0x54;// ('T')	T
        public const int VkKeyU = 0x55;// ('U')	U
        public const int VkKeyV = 0x56;// ('V')	V
        public const int VkKeyW = 0x57;// ('W')	W
        public const int VkKeyX = 0x58;// ('X')	X
        public const int VkKeyY = 0x59;// ('Y')	Y
        public const int VkKeyZ = 0x5A;// ('Z')	Z

        public const int VkMultiply = 0x6A;//	Numpad X
        public const int VkNoname = 0xFC;//	NoName

        public const int VkNumpad0 = 0x60;//	Numpad 0
        public const int VkNumpad1 = 0x61;//	Numpad 1
        public const int VkNumpad2 = 0x62;//	Numpad 2
        public const int VkNumpad3 = 0x63;//	Numpad 3
        public const int VkNumpad4 = 0x64;//	Numpad 4
        public const int VkNumpad5 = 0x65;//	Numpad 5
        public const int VkNumpad6 = 0x66;//	Numpad 6
        public const int VkNumpad7 = 0x67;//	Numpad 7
        public const int VkNumpad8 = 0x68;//	Numpad 8
        public const int VkNumpad9 = 0x69;//	Numpad 9

        public const int VkOem1 = 0xBA;//	OEM_1(: ;)
        public const int VkOem102 = 0xE2;//	OEM_102(> <)
        public const int VkOem2 = 0xBF;//	OEM_2(? /)
        public const int VkOem3 = 0xC0;//	OEM_3(~ `)
        public const int VkOem4 = 0xDB;//	OEM_4({ [)
        public const int VkOem5 = 0xDC;//	OEM_5(\| \)
        public const int VkOem6 = 0xDD;//	OEM_6(} ])
        public const int VkOem7 = 0xDE;//	OEM_7(" ')
        public const int VkOem8 = 0xDF;//	OEM_8 (§ !)
        public const int VkOemAttn = 0xF0;//	Oem Attn
        public const int VkOemAuto = 0xF3;//	Auto
        public const int VkOemAx = 0xE1;//	Ax
        public const int VkOemBacktab = 0xF5;//	Back Tab
        public const int VkOemClear = 0xFE;//	OemClr
        public const int VkOemComma = 0xBC;//	OEM_COMMA(< ,)
        public const int VkOemCopy = 0xF2;//	Copy
        public const int VkOemCusel = 0xEF;//	Cu Sel
        public const int VkOemEnlw = 0xF4;//	Enlw
        public const int VkOemFinish = 0xF1;//	Finish
        public const int VkOemFjLoya = 0x95;//	Loya
        public const int VkOemFjMasshou = 0x93;//	Mashu
        public const int VkOemFjRoya = 0x96;//	Roya
        public const int VkOemFjTouroku = 0x94;//	Touroku
        public const int VkOemJump = 0xEA;//	Jump
        public const int VkOemMinus = 0xBD;//	OEM_MINUS(_ -)
        public const int VkOemPa1 = 0xEB;//	OemPa1
        public const int VkOemPa2 = 0xEC;//	OemPa2
        public const int VkOemPa3 = 0xED;//	OemPa3
        public const int VkOemPeriod = 0xBE;//	OEM_PERIOD(> .)
        public const int VkOemPlus = 0xBB;//	OEM_PLUS(+ =)
        public const int VkOemReset = 0xE9;//	Reset
        public const int VkOemWsctrl = 0xEE;//	WsCtrl
        public const int VkPa1 = 0xFD;//	Pa1
        public const int VkPacket = 0xE7;//	Packet
        public const int VkPlay = 0xFA;//	Play
        public const int VkProcesskey = 0xE5;//	Process
        public const int VkReturn = 0x0D;//	Enter
        public const int VkSelect = 0x29;//	Select
        public const int VkSeparator = 0x6C;//	Separator
        public const int VkSpace = 0x20;//	Space
        public const int VkSubtract = 0x6D;//	Num -
        public const int VkTab = 0x09;//	Tab
        public const int VkZoom = 0xFB;//	Zoom

        public const int VkNone = 0xFF;//	no VK mapping
        public const int VkAccept = 0x1E;//	Accept
        public const int VkApps = 0x5D;//	Context Menu
        public const int VkBrowserBack = 0xA6;//	Browser Back
        public const int VkBrowserFavorites = 0xAB;//	Browser Favorites
        public const int VkBrowserForward = 0xA7;//	Browser Forward
        public const int VkBrowserHome = 0xAC;//	Browser Home
        public const int VkBrowserRefresh = 0xA8;//	Browser Refresh
        public const int VkBrowserSearch = 0xAA;//	Browser Search
        public const int VkBrowserStop = 0xA9;//	Browser Stop
        public const int VkCapital = 0x14;//	Caps Lock
        public const int VkConvert = 0x1C;//	Convert
        public const int VkDelete = 0x2E;//	Delete
        public const int VkDown = 0x28;//	Arrow Down
        public const int VkEnd = 0x23;//	End

        public const int VkF1 = 0x70;//	F1
        public const int VkF10 = 0x79;//	F10
        public const int VkF11 = 0x7A;//	F11
        public const int VkF12 = 0x7B;//	F12
        public const int VkF13 = 0x7C;//	F13
        public const int VkF14 = 0x7D;//	F14
        public const int VkF15 = 0x7E;//	F15
        public const int VkF16 = 0x7F;//	F16
        public const int VkF17 = 0x80;//	F17
        public const int VkF18 = 0x81;//	F18
        public const int VkF19 = 0x82;//	F19
        public const int VkF2 = 0x71;//	F2
        public const int VkF20 = 0x83;//	F20
        public const int VkF21 = 0x84;//	F21
        public const int VkF22 = 0x85;//	F22
        public const int VkF23 = 0x86;//	F23
        public const int VkF24 = 0x87;//	F24
        public const int VkF3 = 0x72;//	F3
        public const int VkF4 = 0x73;//	F4
        public const int VkF5 = 0x74;//	F5
        public const int VkF6 = 0x75;//	F6
        public const int VkF7 = 0x76;//	F7
        public const int VkF8 = 0x77;//	F8
        public const int VkF9 = 0x78;//	F9

        public const int VkFinal = 0x18;//	Final
        public const int VkHelp = 0x2F;//	Help
        public const int VkHome = 0x24;//	Home
        public const int VkIco00 = 0xE4;//	Ico00*
        public const int VkInsert = 0x2D;//	Insert
        public const int VkJunja = 0x17;//	Junja
        public const int VkKana = 0x15;//	Kana
        public const int VkKanji = 0x19;//	Kanji
        public const int VkLaunchApp1 = 0xB6;//	App1
        public const int VkLaunchApp2 = 0xB7;//	App2
        public const int VkLaunchMail = 0xB4;//	Mail
        public const int VkLaunchMediaSelect = 0xB5;//	Media
        public const int VkLbutton = 0x01;//	Left Button **
        public const int VkLcontrol = 0xA2;//	Left Ctrl
        public const int VkLeft = 0x25;//	Arrow Left
        public const int VkLmenu = 0xA4;//	Left Alt
        public const int VkLshift = 0xA0;//	Left Shift
        public const int VkLwin = 0x5B;//	Left Win
        public const int VkMbutton = 0x04;//	Middle Button **
        public const int VkMediaNextTrack = 0xB0;//	Next Track
        public const int VkMediaPlayPause = 0xB3;//	Play / Pause
        public const int VkMediaPrevTrack = 0xB1;//	Previous Track
        public const int VkMediaStop = 0xB2;//	Stop
        public const int VkModechange = 0x1F;//	Mode Change
        public const int VkNext = 0x22;//	Page Down
        public const int VkNonconvert = 0x1D;//	Non Convert
        public const int VkNumlock = 0x90;//	Num Lock
        public const int VkOemFjJisho = 0x92;//	Jisho
        public const int VkPause = 0x13;//	Pause
        public const int VkPrint = 0x2A;//	Print
        public const int VkPrior = 0x21;//	Page Up
        public const int VkRbutton = 0x02;//	Right Button **
        public const int VkRcontrol = 0xA3;//	Right Ctrl
        public const int VkRight = 0x27;//	Arrow Right
        public const int VkRmenu = 0xA5;//	Right Alt
        public const int VkRshift = 0xA1;//	Right Shift
        public const int VkRwin = 0x5C;//	Right Win
        public const int VkScroll = 0x91;//	Scrol Lock
        public const int VkSleep = 0x5F;//	Sleep
        public const int VkSnapshot = 0x2C;//	Print Screen
        public const int VkUp = 0x26;//	Arrow Up
        public const int VkVolumeDown = 0xAE;//	Volume Down
        public const int VkVolumeMute = 0xAD;//	Volume Mute
        public const int VkVolumeUp = 0xAF;//	Volume Up
        public const int VkXbutton1 = 0x05;//	X Button 1 **
        public const int VkXbutton2 = 0x06;//	X Button 2 **

        public static void SetMappings(Dictionary<int, KeyCode> map) {
            map[VkAdd] = KeyCode.Plus;// 0x6B;//	Numpad +
            map[VkBack] = KeyCode.Backspace;// 0x08;//	Backspace
            map[VkCancel] = KeyCode.Break;// 0x03;//	Break
            map[VkClear] = KeyCode.Clear;//0x0C;//	Clear
            map[VkDecimal] = KeyCode.Numlock;//0x6E;//	Numpad.
            map[VkDivide] = KeyCode.KeypadDivide;//0x6F;//	Numpad /
            map[VkEscape] = KeyCode.Escape;//0x1B;//	Esc
            map[VkIcoHelp] = KeyCode.Help;//0xE3;//	IcoHlp

            map[VkKey0] = KeyCode.Alpha0;//0x30;// ('0')	0
            map[VkKey1] = KeyCode.Alpha1;//0x31;// ('1')	1
            map[VkKey2] = KeyCode.Alpha2;//0x32;// ('2')	2
            map[VkKey3] = KeyCode.Alpha3;//0x33;// ('3')	3
            map[VkKey4] = KeyCode.Alpha4;//0x34;// ('4')	4
            map[VkKey5] = KeyCode.Alpha5;//0x35;// ('5')	5
            map[VkKey6] = KeyCode.Alpha6;//0x36;// ('6')	6
            map[VkKey7] = KeyCode.Alpha7;//0x37;// ('7')	7
            map[VkKey8] = KeyCode.Alpha8;//0x38;// ('8')	8
            map[VkKey9] = KeyCode.Alpha9;//0x39;// ('9')	9
            map[VkKeyA] = KeyCode.A;//0x41;// ('A')	A
            map[VkKeyB] = KeyCode.B;//0x42;// ('B')	B
            map[VkKeyC] = KeyCode.C;//0x43;// ('C')	C
            map[VkKeyD] = KeyCode.D;//0x44;// ('D')	D
            map[VkKeyE] = KeyCode.E;//0x45;// ('E')	E
            map[VkKeyF] = KeyCode.F;//0x46;// ('F')	F
            map[VkKeyG] = KeyCode.G;//0x47;// ('G')	G
            map[VkKeyH] = KeyCode.H;//0x48;// ('H')	H
            map[VkKeyI] = KeyCode.I;//0x49;// ('I')	I
            map[VkKeyJ] = KeyCode.J;//0x4A;// ('J')	J
            map[VkKeyK] = KeyCode.K;//0x4B;// ('K')	K
            map[VkKeyL] = KeyCode.L;//0x4C;// ('L')	L
            map[VkKeyM] = KeyCode.M;//0x4D;// ('M')	M
            map[VkKeyN] = KeyCode.N;//0x4E;// ('N')	N
            map[VkKeyO] = KeyCode.O;//0x4F;// ('O')	O
            map[VkKeyP] = KeyCode.P;//0x50;// ('P')	P
            map[VkKeyQ] = KeyCode.Q;//0x51;// ('Q')	Q
            map[VkKeyR] = KeyCode.R;//0x52;// ('R')	R
            map[VkKeyS] = KeyCode.S;//0x53;// ('S')	S
            map[VkKeyT] = KeyCode.T;//0x54;// ('T')	T
            map[VkKeyU] = KeyCode.U;//0x55;// ('U')	U
            map[VkKeyV] = KeyCode.V;//0x56;// ('V')	V
            map[VkKeyW] = KeyCode.W;//0x57;// ('W')	W
            map[VkKeyX] = KeyCode.X;//0x58;// ('X')	X
            map[VkKeyY] = KeyCode.Y;//0x59;// ('Y')	Y
            map[VkKeyZ] = KeyCode.Z;// 0x5A;// ('Z')	Z

            map[VkMultiply] = KeyCode.KeypadMultiply;// 0x6A;//	Numpad*
            map[VkNoname] = KeyCode.None;// 0xFC;//	NoName

            map[VkNumpad0] = KeyCode.Keypad0;//0x60;//	Numpad 0
            map[VkNumpad1] = KeyCode.Keypad1;//0x61;//	Numpad 1
            map[VkNumpad2] = KeyCode.Keypad2;//0x62;//	Numpad 2
            map[VkNumpad3] = KeyCode.Keypad3;//0x63;//	Numpad 3
            map[VkNumpad4] = KeyCode.Keypad4;//0x64;//	Numpad 4
            map[VkNumpad5] = KeyCode.Keypad5;//0x65;//	Numpad 5
            map[VkNumpad6] = KeyCode.Keypad6;//0x66;//	Numpad 6
            map[VkNumpad7] = KeyCode.Keypad7;//0x67;//	Numpad 7
            map[VkNumpad8] = KeyCode.Keypad8;//0x68;//	Numpad 8
            map[VkNumpad9] = KeyCode.Keypad9;//0x69;//	Numpad 9

            map[VkOem1] = KeyCode.Colon;// 0xBA;//	OEM_1(: ;)
            map[VkOem102] = KeyCode.Greater;//0xE2;//	OEM_102(> <)
            map[VkOem2] = KeyCode.Question;//0xBF;//	OEM_2(? /)
            map[VkOem3] = KeyCode.BackQuote;//0xC0;//	OEM_3(~ `)
            map[VkOem4] = KeyCode.LeftBracket;//0xDB;//	OEM_4({ [)
            map[VkOem5] = KeyCode.Backslash;//0xDC;//	OEM_5(| \)
            map[VkOem6] = KeyCode.RightBracket;//0xDD;//	OEM_6(} ])
            map[VkOem7] = KeyCode.DoubleQuote;//0xDE;//	OEM_7(" ')
            map[VkOem8] = KeyCode.Exclaim;//0xDF;//	OEM_8 (§ !)
            map[VkOemAttn] = KeyCode.At;//0xF0;//	Oem Attn
            map[VkOemClear] = KeyCode.Clear;//0xFE;//	OemClr
            map[VkOemComma] = KeyCode.Comma;//0xBC;//	OEM_COMMA(< ,)
            map[VkOemMinus] = KeyCode.Minus;//0xBD;//	OEM_MINUS(_ -)
            map[VkOemPeriod] = KeyCode.Period;//0xBE;//	OEM_PERIOD(> .)
            map[VkOemPlus] = KeyCode.Plus;//0xBB;//	OEM_PLUS(+ =)
            map[VkReturn] = KeyCode.Return;//0x0D;//	Enter
            map[VkSpace] = KeyCode.Space;//0x20;//	Space
            map[VkSubtract] = KeyCode.KeypadMinus;//0x6D;//	Num -
            map[VkTab] = KeyCode.Tab;//0x09;//	Tab

            map[VkNone] = KeyCode.None;//0xFF;//	no VK mapping
            map[VkCapital] = KeyCode.CapsLock;//0x14;//	Caps Lock
            map[VkDelete] = KeyCode.Delete;//0x2E;//	Delete
            map[VkDown] = KeyCode.DownArrow;//0x28;//	Arrow Down
            map[VkEnd] = KeyCode.End;//0x23;//	End

            map[VkF1] = KeyCode.F1;//0x70;//	F1
            map[VkF2] = KeyCode.F2;//0x71;//	F2
            map[VkF3] = KeyCode.F3;//0x72;//	F3
            map[VkF4] = KeyCode.F4;//0x73;//	F4
            map[VkF5] = KeyCode.F5;//0x74;//	F5
            map[VkF6] = KeyCode.F6;//0x75;//	F6
            map[VkF7] = KeyCode.F7;//0x76;//	F7
            map[VkF8] = KeyCode.F8;//0x77;//	F8
            map[VkF9] = KeyCode.F9;//0x78;//	F9
            map[VkF10] = KeyCode.F10;//0x79;//	F10
            map[VkF11] = KeyCode.F11;//0x7A;//	F11
            map[VkF12] = KeyCode.F12;//0x7B;//	F12
            map[VkF13] = KeyCode.F13;//0x7C;//	F13
            map[VkF14] = KeyCode.F14;//0x7D;//	F14
            map[VkF15] = KeyCode.F15;//0x7E;//	F15

            map[VkHelp] = KeyCode.Help;//0x2F;//	Help
            map[VkHome] = KeyCode.Home;//0x24;//	Home
            map[VkInsert] = KeyCode.Insert;//0x2D;//	Insert
            map[VkLcontrol] = KeyCode.LeftControl;//0xA2;//	Left Ctrl
            map[VkLeft] = KeyCode.LeftArrow;//0x25;//	Arrow Left
            map[VkLmenu] = KeyCode.LeftAlt;//0xA4;//	Left Alt
            map[VkLshift] = KeyCode.LeftShift;//0xA0;//	Left Shift
            map[VkLwin] = KeyCode.LeftWindows;//0x5B;//	Left Win
            map[VkNext] = KeyCode.PageDown;//0x22;//	Page Down
            map[VkNumlock] = KeyCode.Numlock;//0x90;//	Num Lock
            map[VkPause] = KeyCode.Pause;//0x13;//	Pause
            map[VkPrint] = KeyCode.Print;//0x2A;//	Print
            map[VkPrior] = KeyCode.PageUp;//0x21;//	Page Up
            map[VkRcontrol] = KeyCode.RightControl;//0xA3;//	Right Ctrl
            map[VkRight] = KeyCode.RightArrow;//0x27;//	Arrow Right
            map[VkRmenu] = KeyCode.RightAlt;//0xA5;//	Right Alt
            map[VkRshift] = KeyCode.RightShift;//0xA1;//	Right Shift
            map[VkRwin] = KeyCode.RightWindows;//0x5C;//	Right Win
            map[VkScroll] = KeyCode.ScrollLock;//0x91;//	Scrol Lock
            map[VkSnapshot] = KeyCode.SysReq;//0x2C;//	Print Screen
            map[VkUp] = KeyCode.UpArrow;//0x26;//	Arrow Up
        }
    }

}
