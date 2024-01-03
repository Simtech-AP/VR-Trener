using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/// <summary>
/// Class responsible for resizing and repsitioning application window, using external windows libraRY
/// </summary>
public class HideWindowBorder : MonoBehaviour
{
    public Rect screenPosition = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 200, 600, 400);
    public bool makewinchanges = false;
    public string winname = "CommunicationTest";

    #region dlls
    const int MAXTITLE = 255;
    private static List<String> lstTitles;
    private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

    [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int _GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern int EnumDesktopWindows(int hDesktop, int lpfn, int lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(String sClassName, String sAppName);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point lpPoint);
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, out System.Drawing.Rectangle lpRect);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern bool SetForegroundWindow(IntPtr hwnd);
    #endregion

    #region sysvars
    const uint SWP_SHOWWINDOW = 0x0040;
    const int GWL_EXSTYLE = -20;
    const int GWL_STYLE = -16;
    const int WS_BORDER = 1;
    #endregion

    bool isAttached;
    Vector2 mouseOffset;
    bool isFullscreen;

    void Awake()
    {
        if (makewinchanges == true && !Application.isEditor)
        {
            GetDesktopWindowsTitles();
        }
    }

    private void GetDesktopWindowsTitles()
    {
        lstTitles = new List<String>();
        EnumDelegate delEnumfunc = new EnumDelegate(EnumWindowsProc);
        bool bSuccessful = EnumDesktopWindows(IntPtr.Zero, delEnumfunc, IntPtr.Zero); //for current desktop
    }

    private bool EnumWindowsProc(IntPtr hWnd, int lParam)
    {
        string strTitle = GetWindowText(hWnd);
        if (strTitle.Contains(winname))
        {
            lstTitles.Add(strTitle);
            bool result = SetForegroundWindow(hWnd);
            SetWindowLong(hWnd, GWL_STYLE, WS_BORDER);
            bool result2 = SetWindowPos(hWnd, 0, (int)(Screen.currentResolution.width / 2 - screenPosition.width / 2), (int)(Screen.currentResolution.height / 2 - screenPosition.height / 2), (int)screenPosition.width, (int)screenPosition.height, SWP_SHOWWINDOW);
            return false;
        }
        return true;
    }

    public static string GetWindowText(IntPtr hWnd)
    {
        StringBuilder strbTitle = new StringBuilder(MAXTITLE);
        int nLength = _GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
        strbTitle.Length = nLength;
        return strbTitle.ToString();
    }

    private void Update()
    {
        if (isAttached)
        {
            Point cursorPos;
            GetCursorPos(out cursorPos);
            screenPosition = new Rect(
               cursorPos.X + mouseOffset.x,
               cursorPos.Y + mouseOffset.y,
               (int)screenPosition.width, (int)screenPosition.height);
            SetWindowPos(FindWindow(null, winname), 0,
            cursorPos.X + (int)mouseOffset.x,
            cursorPos.Y + (int)mouseOffset.y,
            (int)screenPosition.width, (int)screenPosition.height, SWP_SHOWWINDOW);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void AttachToCursor()
    {
        Point cursorPos;
        Rectangle windowRect;
        GetCursorPos(out cursorPos);
        GetWindowRect(FindWindow(null, winname), out windowRect);
        mouseOffset = new Vector2Int((int)(cursorPos.X - windowRect.Left), (int)(cursorPos.Y - windowRect.Top));
        mouseOffset *= -1;
        screenPosition = new Rect(
                cursorPos.X + mouseOffset.x,
                cursorPos.Y + mouseOffset.y,
                (int)screenPosition.width, (int)screenPosition.height);
        isAttached = true;
    }

    public void DetachFromCursor()
    {
        isAttached = false;
    }

    public void ToggleFullscreen()
    {
        if (isFullscreen)
        {
            SetWindowPos(FindWindow(null, winname), 0,
            (int)(Screen.currentResolution.width / 2 - screenPosition.width / 2),
            (int)(Screen.currentResolution.height / 2 - screenPosition.height / 2),
            (int)screenPosition.width, (int)screenPosition.height, SWP_SHOWWINDOW);
        }
        else
        {
            SetWindowPos(FindWindow(null, winname), 0,
            0,
            0,
            Screen.currentResolution.width, Screen.currentResolution.height, SWP_SHOWWINDOW);
        }
        isFullscreen = !isFullscreen;
    }
}

