// Copyright (c) 2021 Lukin Aleksandr
// See https://aka.ms/new-console-template for more information
// See https://aka.ms/new-console-template for more information
using HtmlAgilityPack;
using System.Net;
using System.Text;

const int GOS_DUMA_SOZIV = 8;
const String GOS_DUMA_URL = "http://duma.gov.ru/duma/deputies/";
const String INPUT_FILE_NAME = "input.txt";
const String OUTPUT_FILE_NAME = "output.csv";

String _HtmlContent = "";

ExtractDeputatsFIOAction(8);
var resultFile = Path.Combine(Directory.GetCurrentDirectory(), OUTPUT_FILE_NAME);
File.WriteAllText(OUTPUT_FILE_NAME, Parsing());
Console.WriteLine($"Результат сохранен в файл:\n\t {resultFile}");

String GetHtmlContent() => _HtmlContent;
String SetHtmlContent(String value) => _HtmlContent = value;

String GetInputFileName()
    => Path.Combine(Directory.GetCurrentDirectory(), INPUT_FILE_NAME);

void ExtractDeputatsFIOAction(int sozivNumber)
{
    string htmlContent = "";
    string file = GetInputFileName();
    string urlString = GOS_DUMA_URL + GOS_DUMA_SOZIV + '/';
    if (!File.Exists(file))
    {
        htmlContent = LoadWebContent(urlString);
        File.WriteAllText(file, htmlContent);
    }
    else
    {
        htmlContent = File.ReadAllText(file);
    }
    SetHtmlContent(htmlContent);
}

String LoadWebContent(String url)
{
    String result = "";
    Uri uri = new Uri(url);
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
    request.MaximumAutomaticRedirections = 1;
    request.MaximumResponseHeadersLength = 4;
    request.Credentials = CredentialCache.DefaultCredentials;

    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
    using (Stream receiveStream = response.GetResponseStream())
    {
        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
        result = readStream.ReadToEnd();
        readStream.Close();
        response.Close();
    }
    return result;
}

String Parsing()
{
    StringBuilder sb = new StringBuilder();
    var doc = new HtmlDocument();
    doc.LoadHtml(GetHtmlContent());
    HtmlNode[] nodes = doc.DocumentNode.SelectNodes("//div")
        .Where(x => x.GetAttributeValue("class", "") == "person person--s")
        .ToArray();

    foreach (var node in nodes)
    {
        var nodeHtml = node.InnerHtml;
        var nodeBlockDoc = new HtmlDocument();
        nodeBlockDoc.LoadHtml(nodeHtml);
        var htmlBlock = nodeBlockDoc.DocumentNode.SelectNodes("//span")
            .Where(x => x.GetAttributeValue("itemprop", "") == "name")
            .FirstOrDefault();
        var fioDoc = new HtmlDocument();
        fioDoc.LoadHtml(htmlBlock?.InnerHtml);
        var personLastName = fioDoc.DocumentNode.SelectNodes("//strong")
            .FirstOrDefault()?.InnerHtml;
        var s = fioDoc.DocumentNode.SelectNodes("//span")
            .Where(x => x.GetAttributeValue("class", "") == "second-name")
            .FirstOrDefault()?.InnerHtml?.Trim();
        int posSpace = s.IndexOf(' ');
        var personName = s.Substring(0, posSpace);
        var personMiddleName = s.Remove(0, posSpace)?.Trim();
        sb.Append($"{personLastName};{personName};{personMiddleName};\n\r");
    }
    return sb.ToString();
}



