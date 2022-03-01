using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.RegularExpressions;

namespace OMinimoScrapper;

public static class ArticleIndex
{
    public static async Task<List<Article>> FromTextFile(string filename, Encoding encoding = null!)
    {
        var list = new List<Article>();

        var lines = await File.ReadAllLinesAsync(filename, encoding ?? Encoding.UTF8);

        string chapter = string.Empty;
        string subChapter = string.Empty;

        var regexpSubChapter = new Regex(@"^ +\d+\. (.*)$");

        foreach (var line in lines)
        {
            if (!line.StartsWith(" "))
            {
                chapter = TextUtility.CapitalizeFirst(line.Trim().ToLower());
                subChapter = string.Empty;
            }
            else if (regexpSubChapter.IsMatch(line))
            {
                subChapter = regexpSubChapter.Replace(line, "$1").Trim();
            }
            else
            {
                var title = line.Trim();
                var article = new Article
                {
                    Chapter = chapter,
                    SubChapter = subChapter,
                    Title = title
                };
                if (title.Contains("[in:"))
                {
                    var regexpAlternativeTitle = new Regex(@"^(.+) \[in: (.+)\]$");
                    article.Title = regexpAlternativeTitle.Replace(title, "$1");
                    article.Metadata = new ArticleMetadata();
                    article.Metadata.Title = regexpAlternativeTitle.Replace(title, "$2");
                }
                list.Add(article);
            }
        }

        return list;
    }

    static JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    internal static async Task ToJsonFile(List<Article> articles, string resultFile)
    {
        using FileStream fs = File.Open(resultFile, FileMode.Create);
        await JsonSerializer.SerializeAsync(fs, articles, options);
    }

    internal static async Task<List<Article>?> FromJsonFile(string jsonFile)
    {
        using FileStream fs = File.Open(jsonFile, FileMode.Open);
        return await JsonSerializer.DeserializeAsync<List<Article>>(fs, options);
    }
}
