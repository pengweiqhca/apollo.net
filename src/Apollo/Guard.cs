using System.Runtime.CompilerServices;

namespace Com.Ctrip.Framework.Apollo;

/// <summary>
///     Methods for guarding against exception throwing values.
/// </summary>
internal static class Guard
{
    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNull<T>([NotNull][ValidatedNotNull] this T? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument == null) ThrowArgumentNullException(paramName);

        return argument;
    }

    [DoesNotReturn]
    [DebuggerHidden]
    private static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    [DebuggerHidden]
    private static void ThrowArgumentException(string message, string? paramName) =>
        throw new ArgumentException(message, paramName);

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyCollection<T> ThrowIfNullOrEmpty<T>(
        [NotNull][ValidatedNotNull] this IReadOnlyCollection<T>? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument.ThrowIfNull(paramName).Count < 1) ThrowArgumentException("The value cannot be empty.", paramName);

        return argument;
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ThrowIfNullOrEmpty([NotNull][ValidatedNotNull] this string? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument.ThrowIfNull(paramName).Length < 1)
            ThrowArgumentException("The value cannot be an empty string.", paramName);

        return argument;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class ValidatedNotNullAttribute : Attribute { }
}
