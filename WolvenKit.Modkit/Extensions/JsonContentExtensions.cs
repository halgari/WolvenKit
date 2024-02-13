using System.Diagnostics.CodeAnalysis;
using System;
using SharpGLTF.IO;

namespace WolvenKit.Modkit.Extensions;

internal static class JsonContentExtensions
{
    internal static bool TryGetNode(this JsonContent content, [NotNullWhen(true)] out JsonContent value, params IConvertible[] path)
    {
        try
        {
            value = content.GetNode(path);
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    internal static bool TryGetValue<T>(this JsonContent content, [NotNullWhen(true)] out T? value, params IConvertible[] path) where T : IConvertible
    {
        try
        {
            value = content.GetValue<T>(path);
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }
}