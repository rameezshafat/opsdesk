using System.Security.Cryptography;
using System.Text;

namespace OpsDesk.Core;

public sealed class MarkdownChunker
{
    private readonly int _maxWords;
    private readonly int _overlapWords;

    public MarkdownChunker(int maxWords = 240, int overlapWords = 40)
    {
        if (maxWords <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxWords));
        if (overlapWords < 0 || overlapWords >= maxWords)
            throw new ArgumentOutOfRangeException(nameof(overlapWords));

        _maxWords = maxWords;
        _overlapWords = overlapWords;
    }

    public IReadOnlyList<DocumentChunk> Chunk(string sourceUrl, string title, string markdown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(markdown);

        var chunks = new List<DocumentChunk>();
        var section = new StringBuilder();
        var heading = title;
        var sectionNumber = 0;
        var inCodeFence = false;

        void AddSection()
        {
            var words = section.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var step = _maxWords - _overlapWords;

            for (var start = 0; start < words.Length; start += step)
            {
                var count = Math.Min(_maxWords, words.Length - start);
                var text = string.Join(" ", words, start, count);
                var key = $"{sourceUrl}\n{heading}\n{sectionNumber}\n{start}\n{text}";
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

                chunks.Add(new DocumentChunk(
                    Convert.ToHexString(hash).ToLowerInvariant(), sourceUrl, title, heading, text));

                if (start + count == words.Length)
                    break;
            }

            section.Clear();
            sectionNumber++;
        }

        using var reader = new StringReader(markdown);
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
                inCodeFence = !inCodeFence;

            if (!inCodeFence && TryReadHeading(trimmed, out var nextHeading))
            {
                AddSection();
                heading = nextHeading;
                continue;
            }

            section.AppendLine(line);
        }

        AddSection();
        return chunks;
    }

    private static bool TryReadHeading(string line, out string heading)
    {
        var markerCount = 0;
        while (markerCount < line.Length && line[markerCount] == '#')
            markerCount++;

        heading = string.Empty;
        if (markerCount is < 1 or > 6 || markerCount >= line.Length || line[markerCount] != ' ')
            return false;

        heading = line[(markerCount + 1)..].Trim().TrimEnd('#').Trim();
        return heading.Length > 0;
    }
}
