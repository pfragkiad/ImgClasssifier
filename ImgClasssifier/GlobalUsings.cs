global using static ImgClasssifier.GlobalUsings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgClasssifier;

public static class GlobalUsings
{
    public static void Wait()
    {
        Cursor.Current = Cursors.WaitCursor;
        Application.UseWaitCursor = true;

    }

    public static void StopWaiting()
    {
        Cursor.Current = Cursors.Default;
        Application.UseWaitCursor = false;
    }
}
