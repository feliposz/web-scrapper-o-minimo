namespace OMinimoScrapper;

public class Article
{
    public string Chapter { get; set; } = string.Empty;
    public string SubChapter { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public ArticleMetadata? Metadata { get; set; }
}