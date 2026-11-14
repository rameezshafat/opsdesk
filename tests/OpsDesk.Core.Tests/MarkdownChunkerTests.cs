using OpsDesk.Core;

namespace OpsDesk.Core.Tests;

public sealed class MarkdownChunkerTests
{
    private const string Url = "https://example.org/docs/setup";

    [Fact]
    public void HeadingBoundaryKeepsSourceAndHeadingForCitations()
    {
        var chunks = new MarkdownChunker().Chunk(Url, "Setup guide", "## Install\nRun setup.\n## Verify\nCheck the result.");

        Assert.Collection(chunks,
            first => Assert.Equal((Url, "Setup guide", "Install", "Run setup."),
                (first.SourceUrl, first.Title, first.Heading, first.Text)),
            second => Assert.Equal("Verify", second.Heading));
    }

    [Fact]
    public void LongSectionRepeatsOnlyTheOverlapAtTheNextBoundary()
    {
        var chunks = new MarkdownChunker(maxWords: 4, overlapWords: 1)
            .Chunk(Url, "Guide", "one two three four five six seven");

        Assert.Equal(["one two three four", "four five six seven"], chunks.Select(chunk => chunk.Text));
    }

    [Fact]
    public void HeadingInsideCodeFenceDoesNotSplitTheSection()
    {
        var chunks = new MarkdownChunker().Chunk(Url, "Guide", "## Example\n```sh\n# not a heading\n```\nContinue here.");

        Assert.Single(chunks);
        Assert.Equal("Example", chunks[0].Heading);
    }

    [Fact]
    public void UnheadedTextUsesThePageTitleAsItsCitationHeading()
    {
        var chunk = Assert.Single(new MarkdownChunker().Chunk(Url, "Guide", "Opening paragraph."));

        Assert.Equal("Guide", chunk.Heading);
    }

    [Fact]
    public void SameInputProducesTheSameContentKeys()
    {
        var chunker = new MarkdownChunker(maxWords: 3, overlapWords: 1);
        var first = chunker.Chunk(Url, "Guide", "one two three four five");
        var second = chunker.Chunk(Url, "Guide", "one two three four five");

        Assert.Equal(first.Select(chunk => chunk.Id), second.Select(chunk => chunk.Id));
        Assert.Equal(first.Count, first.Select(chunk => chunk.Id).Distinct().Count());
    }

    [Fact]
    public void ChangedContentChangesItsKey()
    {
        var chunker = new MarkdownChunker();
        var oldChunk = Assert.Single(chunker.Chunk(Url, "Guide", "Restart the agent."));
        var newChunk = Assert.Single(chunker.Chunk(Url, "Guide", "Restart the server."));

        Assert.NotEqual(oldChunk.Id, newChunk.Id);
    }

    [Fact]
    public void DifferentSourcesCannotShareAContentKey()
    {
        var chunker = new MarkdownChunker();
        var first = Assert.Single(chunker.Chunk(Url, "Guide", "Same words."));
        var second = Assert.Single(chunker.Chunk("https://example.org/docs/other", "Guide", "Same words."));

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void EmptyDocumentHasNoChunks()
    {
        Assert.Empty(new MarkdownChunker().Chunk(Url, "Guide", "   "));
    }

    [Fact]
    public void OverlapMustBeSmallerThanChunk()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MarkdownChunker(maxWords: 4, overlapWords: 4));
    }
}
