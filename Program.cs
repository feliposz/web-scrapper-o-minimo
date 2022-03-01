using System.Diagnostics;

namespace OMinimoScrapper;

public class Program
{
    public static async Task Main(string[] args)
    {
        List<Article>? articles;

        if (File.Exists("o_minimo.json")) // Handle partially processed file
        {
            articles = await ArticleIndex.FromJsonFile("o_minimo.json");
        }
        else if (File.Exists("o_minimo.txt")) // Initial run
        {
            articles = await ArticleIndex.FromTextFile("o_minimo.txt");
        }
        else
        {
            Console.Error.WriteLine("Could not open article index file");
            return;
        }

        /*
        // PATCH: One time patch to amend the alternative titles for articles in the json file
        var tempArticles = await ArticleIndex.FromJsonFile("o_minimo.json");
        var articleDict = new Dictionary<string, Article>();
        foreach (var article in tempArticles)
        {
            articleDict.Add(article.Title, article);
        }

        foreach (var article in articles)
        {
            var tempArticle = articleDict[article.Title];
            if (tempArticle.Metadata?.Date != DateTime.MinValue)
            {
                article.Uri = tempArticle.Uri;
                article.Metadata = tempArticle.Metadata;
            }
        }
        */

        await GetArticles("https://olavodecarvalho.org/", articles);

        await ArticleIndex.ToJsonFile(articles, "o_minimo.json");
    }

    static async Task GetArticles(string baseUri, List<Article> articles)
    {
        var scrapper = new Scrapper(baseUri);

        foreach (var article in articles)
        {
            if (article.Metadata is not null && article.Metadata.Date != DateTime.MinValue)
            {
                continue; // skip articles with correct metadata
            }

            Debug.WriteLine("Chapter: {0}\nSubChapter: {1}\nTitle: {2}", article.Chapter, article.SubChapter, article.Title);

            article.Uri = await scrapper.SearchArticleLinkByTitle(article.Metadata?.Title ?? article.Title);

            if (!string.IsNullOrEmpty(article.Uri))
            {
                article.Metadata = await scrapper.GetArticleMetadata(article.Uri);

                Debug.WriteLine("Title: {0}\nPublication: {1}\nDate: {2}\nCategory: {3}\n Tags: {4}",
                    article.Metadata.Title,
                    article.Metadata.Publication,
                    article.Metadata.Date,
                    article.Metadata.Category,
                    String.Join(", ", article.Metadata.Tags));

                Thread.Sleep(250);
            }
        }
    }

}