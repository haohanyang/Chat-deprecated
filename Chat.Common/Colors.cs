namespace Chat.Common;

public class Colors
{
    public static readonly string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
    public static readonly string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
    public static readonly string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
    public static readonly string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
}