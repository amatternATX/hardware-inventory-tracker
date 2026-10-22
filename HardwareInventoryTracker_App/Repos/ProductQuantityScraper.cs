using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public sealed class ProductQuantityScraper(HttpClient httpClient)
{
    private const int MaxResponseBytes = 2 * 1024 * 1024;
    private const int MaxRedirects = 5;

    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "www.amazon.com",
        "www.rockler.com",
        "www.homedepot.com"
    };

    private static readonly Regex ScriptAndStyleRegex = new(
        @"<(script|style)\b[^>]*>.*?</\1\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex HtmlTagRegex = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex[] QuantityPatterns =
    [
        new Regex(
            @"(?:pack(?:age)?\s+of|set\s+of|contains|quantity|qty)\s*[:#-]?\s*(?<quantity>\d{1,6})\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(
            @"(?<quantity>\d{1,6})\s*[- ]?(?:pack|count|ct|pcs?\.?|pieces|units?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled)
    ];

    public async Task<ProductQuantityLookupResult> FindAsync(string url)
    {
        var currentUri = GetAllowedUri(url);
        if (currentUri is null)
        {
            return new(null, "Package quantity lookup supports HTTPS links from Amazon, Rockler, or Home Depot only.");
        }

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, currentUri);
                using var response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);

                if (IsRedirect(response.StatusCode))
                {
                    if (redirectCount == MaxRedirects || response.Headers.Location is null)
                    {
                        return new(null, "The product page redirected too many times or returned an invalid redirect.");
                    }

                    var redirectUri = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(currentUri, response.Headers.Location);
                    currentUri = GetAllowedUri(redirectUri.ToString());
                    if (currentUri is null)
                    {
                        return new(null, "The product page redirected to a host that is not approved for lookup.");
                    }

                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new(null, $"The product page could not be read (HTTP {(int)response.StatusCode}).");
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (mediaType is not null &&
                    !mediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase) &&
                    !mediaType.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase))
                {
                    return new(null, "The reorder link did not return an HTML product page.");
                }

                var html = await ReadLimitedContentAsync(response.Content, timeout.Token);
                if (html is null)
                {
                    return new(null, "The product page was too large to inspect.");
                }

                var descriptionText = WebUtility.HtmlDecode(
                    HtmlTagRegex.Replace(ScriptAndStyleRegex.Replace(html, " "), " "));
                foreach (var pattern in QuantityPatterns)
                {
                    var match = pattern.Match(descriptionText);
                    if (match.Success &&
                        int.TryParse(match.Groups["quantity"].Value, out var quantity) &&
                        quantity > 0)
                    {
                        return new(quantity, $"Found a possible package quantity of {quantity}. Please verify it.");
                    }
                }

                return new(null, "No package quantity was found in the product page text. Enter it manually.");
            }
        }
        catch (HttpRequestException)
        {
            return new(null, "The product page could not be reached. Enter the package quantity manually.");
        }
        catch (OperationCanceledException)
        {
            return new(null, "The product page lookup timed out. Enter the package quantity manually.");
        }

        return new(null, "The product page could not be inspected. Enter the package quantity manually.");
    }

    private static Uri? GetAllowedUri(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var parsedUri) &&
            parsedUri.Scheme == Uri.UriSchemeHttps &&
            parsedUri.IsDefaultPort &&
            string.IsNullOrEmpty(parsedUri.UserInfo) &&
            AllowedHosts.Contains(parsedUri.Host))
        {
            return parsedUri;
        }

        return null;
    }

    private static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.MovedPermanently or
            HttpStatusCode.Redirect or
            HttpStatusCode.RedirectMethod or
            HttpStatusCode.TemporaryRedirect or
            HttpStatusCode.PermanentRedirect;

    private static async Task<string?> ReadLimitedContentAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength is > MaxResponseBytes)
        {
            return null;
        }

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        while (true)
        {
            var bytesRead = await stream.ReadAsync(chunk.AsMemory(), cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            if (buffer.Length + bytesRead > MaxResponseBytes)
            {
                return null;
            }

            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead));
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}

public sealed record ProductQuantityLookupResult(int? Quantity, string Message);
