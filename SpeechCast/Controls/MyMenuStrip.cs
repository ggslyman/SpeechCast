using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace SpeechCast.Controls
{
    public class MyMenuStrip : MenuStrip
    {

        private const int WM_MOUSEACTIVATE = 0x0021;

        private const int MA_ACTIVATE = 1;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEACTIVATE:
                    base.WndProc(ref m);
                    m.Result = new System.IntPtr(MA_ACTIVATE);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }

        }
    }
}
