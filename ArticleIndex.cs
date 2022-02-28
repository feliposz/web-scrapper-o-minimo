using System.Text;
using System.Text.RegularExpressions;

namespace OMinimoScrapper;

public static class ArticleIndex
{
    public static async Task<List<Article>> FromFile(string filename, Encoding encoding = null!)
    {
        var list = new List<Article>();

        var lines = await File.ReadAllLinesAsync(filename, encoding ?? Encoding.Latin1);

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
                list.Add(new Article
                {
                    Chapter = chapter,
                    SubChapter = subChapter,
                    Title = line.Trim()
                });
            }
        }

        return list;
    }
}
