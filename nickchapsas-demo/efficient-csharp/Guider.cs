using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace EfficientCsharp;

public static class Guider
{
    private const char ForwardSlash = '/';
    private const char Underscore = '_';
    private const char Hyphen = '-';
    private const char PlusSign = '+';
    private const char EqualsChar = '=';
    
    public static string ToStringFromGuid(Guid id)
    {
        return Convert.ToBase64String(id.ToByteArray())
            .Replace('/', '-')
            .Replace('+', '_')
            .Replace("=", string.Empty);
    }
    
    public static string ToStringFromGuidOptimized(Guid id)
    {
        Span<byte> idBytes = stackalloc byte[16];
        Span<byte> base64Bytes = stackalloc byte[24];

        MemoryMarshal.TryWrite(idBytes, ref id);
        Base64.EncodeToUtf8(idBytes, base64Bytes, out _, out _);

        Span<char> finalCharacters = stackalloc char[22];
        for (int i = 0; i < 22; i++)
        {
            finalCharacters[i] = base64Bytes[i] switch
            {
                (byte)ForwardSlash => Hyphen,
                (byte)PlusSign => Underscore,
                _ => (char)base64Bytes[i]
            };
        }

        return new string(finalCharacters);
    }

    public static Guid ToGuidFromString(string id)
    {
        var efficientBase64 = Convert.FromBase64String(
            id
                .Replace('-', '/')
                .Replace('_', '+')
            + "==");
        
        return new Guid(efficientBase64);
    }
    
    public static Guid ToGuidFromStringOptimized(ReadOnlySpan<char> id)
    {
        Span<char> base64Chars = stackalloc char[24];

        for (int i = 0; i < 22; i++) // ignore the two last padding chars "=="
        {
            base64Chars[i] = id[i] switch
            {
            Hyphen => ForwardSlash,
            Underscore => PlusSign,
            _ => id[i]
            };
        }
        
        base64Chars[22] = EqualsChar;
        base64Chars[23] = EqualsChar;

        Span<byte> idBytes = stackalloc byte[16]; // Guids have 16 bytes

        Convert.TryFromBase64Chars(base64Chars, idBytes, out _);

        return new Guid(idBytes);
    }
}