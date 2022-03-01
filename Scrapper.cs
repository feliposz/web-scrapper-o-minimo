using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace OMinimoScrapper;

public class Scrapper
{
    string baseUri;
    HtmlWeb htmlWeb;

    public Scrapper(string uri)
    {
        baseUri = uri;
        htmlWeb = new HtmlWeb();
    }

    public async Task<string> SearchArticleLinkByTitle(string title)
    {
        Debug.WriteLine("SearchArticleLinkByTitle: {0}", title);
        var uri = SearchUri(title);
        var doc = await htmlWeb.LoadFromWebAsync(uri);

        var nodes = doc.DocumentNode.Descendants("a");

        int bestDistance = Int32.MaxValue;
        string href = "";

        foreach (var node in nodes)
        {
            var text = node.InnerText;

            // exact match
            if (text == title)
            {
                href = node.Attributes["href"].Value;
                Debug.WriteLine("{0}\t{1}\t{2}\t{3}", text, title, 0, href);
                Debug.WriteLine("Exact match!");
                break;
            }
            else
            {
                // get closest match
                int distance = TextUtility.GetEditDistance(text, title);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    href = node.Attributes["href"].Value;
                    Debug.WriteLine("{0}\t{1}\t{2}\t{3}", text, title, distance, href);
                }
            }
        }

        // titles are too different
        if (bestDistance > title.Length / 4)
        {
            return string.Empty;
        }

        return href;
    }

    public async Task<ArticleMetadata> GetArticleMetadata(string uri)
    {
        var doc = await htmlWeb.LoadFromWebAsync(uri);

        var titleNode = doc.DocumentNode
            .Descendants("h1")
            .Where(x => x.HasClass("single-pagetitle"))
            .FirstOrDefault();

        var regexpPublication = new Regex(@"^(.+), \d+ de \w+ de \d{4}");

        var publicationNode = doc.DocumentNode
            .Descendants("p")
            .Where(x => regexpPublication.IsMatch(x.InnerText.Trim()))
            .FirstOrDefault();

        var metadataNode = doc.DocumentNode
            .Descendants("section")
            .Where(x => x.HasClass("postmetadata"))
            .FirstOrDefault();

        var dateNode = metadataNode?
            .Descendants("span")
            .FirstOrDefault();

        var categoryNode = metadataNode?
            .Descendants("span")
            .Where(x => x.HasClass("postmetadata-categories-link"))
            .FirstOrDefault();

        var tagsNode = metadataNode?
            .Descendants("div")
            .Where(x => x.HasClass("post-tags-wrapper"))
            .FirstOrDefault();

        var metadata = new ArticleMetadata();

        if (titleNode is not null)
            metadata.Title = titleNode.InnerText.Trim();

        if (publicationNode is not null)
            metadata.Publication = regexpPublication.Replace(publicationNode.InnerText.Trim(), "$1");

        if (dateNode is not null)
            metadata.Date = TextUtility.DateFromString(dateNode.InnerText.Trim());

        if (categoryNode is not null)
            metadata.Category = categoryNode.InnerText.Trim();

        if (tagsNode is not null)
        {
            foreach (var tag in tagsNode.Descendants("a"))
            {
                metadata.Tags.Add(tag.InnerText.Trim());
            }
        }

        return metadata;
    }

    private string SearchUri(string searchString)
    {
        var uri = new StringBuilder();
        uri.Append(baseUri);
        uri.Append("?s=");
        uri.Append(Uri.EscapeDataString(searchString.Replace(" \u0097 ", " ")));
        return uri.ToString();
    }

}