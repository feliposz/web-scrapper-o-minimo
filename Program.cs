using System.Diagnostics;
using System.Text.Json;

namespace OMinimoScrapper;

public class Program
{
    static string baseUri = "https://olavodecarvalho.org/";
    static string articleIndexFile = "o_minimo.txt";
    static string resultFile = "o_minimo.json";

    static async Task<List<Article>> GetArticles()
    {
        var scrapper = new Scrapper(baseUri);

        var index = await ArticleIndex.FromFile(articleIndexFile);
        foreach (var article in index)
        {
            Debug.WriteLine("Chapter: {0}\nSubChapter: {1}\nTitle: {2}", article.Chapter, article.SubChapter, article.Title);

            article.Uri = await scrapper.SearchArticleLinkByTitle(article.Title);
            article.Metadata = await scrapper.GetArticleMetadata(article.Uri);

            Debug.WriteLine("Title: {0}\nPublication: {1}\nDate: {2}\nCategory: {3}\n Tags: {4}",
                article.Metadata.Title,
                article.Metadata.Publication,
                article.Metadata.Date,
                article.Metadata.Category,
                String.Join(", ", article.Metadata.Tags));

            Thread.Sleep(500);
        }

        return index;
    }

    public static async Task WriteResult(List<Article> articles)
    {
        using (FileStream fs = File.Open(resultFile, FileMode.OpenOrCreate))
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            await JsonSerializer.SerializeAsync(fs, articles, options);
        }
    }

    public static async Task Main(string[] args)
    {
        var articles = await GetArticles();
        await WriteResult(articles);
    }
}