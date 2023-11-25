namespace ImgClasssifier.ControlExtensions;

public static class ListViewExtensions
{
    public static void EnableDoubleBuffering(this Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered",
                     System.Reflection.BindingFlags.NonPublic |
                     System.Reflection.BindingFlags.Instance)!
       .SetValue(control, true, null);
    }
}
