using System.Text;

namespace Chat.Common.Configs;

public class ClientConfigs
{
    public static readonly ConsoleColor MessageColor = ConsoleColor.Black;
    public static readonly ConsoleColor NotificationColor = ConsoleColor.Cyan;
    public static readonly ConsoleColor RpcFailColor = ConsoleColor.Red;
    public static readonly ConsoleColor RpcWarnColor = ConsoleColor.Yellow;
    public static readonly ConsoleColor RpcSucceedColor = ConsoleColor.Green;

    public static readonly Rune SpeechBalloon = new(0x1f4ac);
    public static readonly Rune InformationSource = new(0x2139);
    public static readonly Rune WarningSign = new(0x26a0);
    public static readonly Rune CrossMark = new(0x274c);
}