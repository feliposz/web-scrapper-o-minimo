namespace OMinimoScrapper;

public class ArticleMetadata
{
    public string Title { get; set; } = string.Empty;
    public string Publication { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.MinValue;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();
}