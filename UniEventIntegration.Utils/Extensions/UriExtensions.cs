namespace UniEventIntegration.Utils.Extensions;

public static class UriExtensions
{
    #region String

    /// <summary>
    /// Appends the specified path to the URI if it is missing.
    /// </summary>
    /// <param name="uri">The URI to append the path to.</param>
    /// <param name="path">The path to append.</param>
    /// <returns>The modified URI with the appended path.</returns>
    public static string AppendPathIfMissing(this string? uri, ReadOnlySpan<char> path)
    {
        if (path.IsEmpty || path.IsWhiteSpace())
            return StringWithSlash(uri);
        if (string.IsNullOrWhiteSpace(uri))
            return StringWithSlash(path);
        while (path[0] == '/')
        {
            path = path[1..];
            if (path.Length == 0)
                return StringWithSlash(uri);
        }
        while (path[^1] == '/') path = path[..^1];
        var uriSpan = uri.AsSpan();
        while (uriSpan[^1] == '/')
        {
            uriSpan = uriSpan[..^1];
            if (uriSpan.Length == 0) break;
        }
        return uriSpan.EndsWith(path)
            ? StringWithSlash(uri)
            : InternalAppendPath(uriSpan, path);
    }

    /// <summary>
    /// Appends the specified path to the URI.
    /// </summary>
    /// <param name="uri">The URI to append the path to.</param>
    /// <param name="path">The path to append.</param>
    /// <returns>The modified URI with the appended path.</returns>
    public static string AppendPath(this string? uri, ReadOnlySpan<char> path)
    {
        if (path.IsEmpty || path.IsWhiteSpace())
            return StringWithSlash(uri);
        if (string.IsNullOrWhiteSpace(uri))
            return StringWithSlash(path);
        while (path[0] == '/')
        {
            path = path[1..];
            if (path.Length == 0)
                return StringWithSlash(uri);
        }
        while (path[^1] == '/') path = path[..^1];
        var uriSpan = uri.AsSpan();
        while (uriSpan[^1] == '/')
        {
            uriSpan = uriSpan[..^1];
            if (uriSpan.Length == 0) break;
        }
        return InternalAppendPath(uriSpan, path);
    }

    /// <summary>
    /// Appends a slash to the URI if it is missing.
    /// </summary>
    /// <param name="uri">The URI to append the slash to.</param>
    /// <returns>The modified URI with the appended slash.</returns>
    public static string AppendSlash(this string? uri)
        => uri switch
        {
            null => "/",
            var x when string.IsNullOrWhiteSpace(x) => "/",
            _ => StringWithSlash(uri)
        };

    private static string StringWithSlash(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty || input.IsWhiteSpace())
            return "/";
        var worker = input;
        while (worker[^1] == '/')
        {
            worker = worker[..^1];
            if (worker.Length == 0)
                return "/";
        }
        if (worker.Length < input.Length)
            return input[..^(input.Length - worker.Length - 1)].ToString();
        return AddSlash(worker);
    }

    #endregion

    #region Uri

    /// <summary>
    /// Appends the specified path to the URI if it is missing.
    /// </summary>
    /// <param name="uri">The URI to append the path to.</param>
    /// <param name="path">The path to append.</param>
    /// <returns>The modified URI with the appended path.</returns>
    public static Uri? AppendPathIfMissing(this Uri? uri, ReadOnlySpan<char> path)
    {
        if (path.IsEmpty || path.IsWhiteSpace())
            return uri.AppendSlash();
        while (path[0] == '/')
        {
            path = path[1..];
            if (path.Length == 0)
                return uri.AppendSlash();
        }
        while (path[^1] == '/') path = path[..^1];
        if (uri is null)
        {
            _ = Uri.TryCreate(StringWithSlash(path), UriKind.RelativeOrAbsolute, out Uri? result);
            return result;
        }
        var uriSpan = uri.AbsoluteUri.AsSpan();
        while (uriSpan[^1] == '/')
        {
            uriSpan = uriSpan[..^1];
            if (uriSpan.Length == 0) break;
        }
        return uriSpan.EndsWith(path)
            ? UriWithSlash(uri)
            : new Uri(InternalAppendPath(uriSpan, path));
    }

    /// <summary>
    /// Appends the specified path to the URI.
    /// </summary>
    /// <param name="uri">The URI to append the path to.</param>
    /// <param name="path">The path to append.</param>
    /// <returns>The modified URI with the appended path.</returns>
    public static Uri? AppendPath(this Uri? uri, ReadOnlySpan<char> path)
    {
        if (path.IsEmpty || path.IsWhiteSpace())
            return uri.AppendSlash();
        if (uri is null)
        {
            _ = Uri.TryCreate(StringWithSlash(path), UriKind.RelativeOrAbsolute, out Uri? result);
            return result;
        }
        while (path[0] == '/')
        {
            path = path[1..];
            if (path.Length == 0)
                return UriWithSlash(uri);
        }
        while (path[^1] == '/') path = path[..^1];
        var uriSpan = uri.AbsoluteUri.AsSpan();
        while (uriSpan[^1] == '/')
        {
            uriSpan = uriSpan[..^1];
            if (uriSpan.Length == 0) break;
        }
        return new Uri(InternalAppendPath(uriSpan, path));
    }

    /// <summary>
    /// Appends a slash to the URI if it is missing.
    /// </summary>
    /// <param name="uri">The URI to append the slash to.</param>
    /// <returns>The modified URI with the appended slash.</returns>
    public static Uri? AppendSlash(this Uri? uri)
        => uri is null
            ? new Uri("/", UriKind.Relative)
            : UriWithSlash(uri);

    private static Uri UriWithSlash(Uri uri)
    {
        var uriSpan = uri.AbsoluteUri.AsSpan();
        if (uriSpan[^2] != '/' && uriSpan[^1] == '/')
            return uri;
        var worker = uriSpan;
        while (worker[^1] == '/')
        {
            worker = worker[..^1];
            if (worker.Length == 0)
                throw new ArgumentException("Input uri is not well formed.");
        }
        if (worker.Length < uriSpan.Length)
            return new Uri(uriSpan[..^(uriSpan.Length - worker.Length - 1)].ToString());
        return new Uri(AddSlash(worker));
    }

    #endregion

    private static string InternalAppendPath(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    {
        var newUriLength = path1.Length + path2.Length + 2;
        Span<char> newPath = newUriLength < 1000
            ? stackalloc char[newUriLength]
            : new char[newUriLength];
        path1.CopyTo(newPath);
        newPath[path1.Length] = '/';
        path2.CopyTo(newPath[(path1.Length + 1)..]);
        newPath[^1] = '/';
        return newPath.ToString();
    }

    private static string AddSlash(ReadOnlySpan<char> input)
    {
        var newLength = input.Length + 1;
        Span<char> retVal = newLength < 1000
            ? stackalloc char[newLength]
            : new char[newLength];
        input.CopyTo(retVal);
        retVal[^1] = '/';
        return retVal.ToString();
    }
}
