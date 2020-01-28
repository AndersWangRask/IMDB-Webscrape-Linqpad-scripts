<Query Kind="Program">
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>Microsoft.VisualBasic</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>HtmlAgilityPack</Namespace>
</Query>

const string rootUrl = @"https://www.imdb.com/";

void Main()
{
	IEnumerable<string> personIds =
		new List<string>
		{
			@"nm0202970", //Larry David
			@"nm0385644", //Cheryl Hines
			@"nm0507659", //Richard Lewis
		};

	List<string> jointFilmos = null;

	foreach (string personId in personIds)
	{
		IEnumerable<string> personFilmos = GetFilmos(personId);
		jointFilmos = jointFilmos?.Intersect(personFilmos).ToList() ?? personFilmos.ToList();
	}

	Dictionary<string, string> jointFilmosWithTitles =
		jointFilmos
			.ToDictionary(
				f => f,
				f => GetTitle(f));

	jointFilmosWithTitles.Dump();
}

IEnumerable<string> GetFilmos(string personId)
{
	string personUrl = $"{rootUrl}name/{personId}/";
	HtmlDocument doc = GetHtmlDoc(personUrl);

	var filmos =
		doc.DocumentNode.Descendants("div")
			.Where(dni => dni.GetClasses().Contains("filmo-row"))
			.Select(dni => dni.Descendants("a").FirstOrDefault())
			.Where(dni => dni != null)
			.Select(dni => dni.Attributes["href"].Value)
			.Where(si => !string.IsNullOrWhiteSpace(si))
			.Select(si => si.IndexOf("?") > -1 ? si.Substring(0, si.IndexOf("?")) : si)
			.Select(si => si.ToLowerInvariant())
			.Select(si => si.Replace("/title/", "").TrimEnd('/'))
			.Distinct()
			.ToList();

	//-->
	return filmos;
}

Dictionary<string, string> titles = new Dictionary<string, string>();

string GetTitle(string titleId)
{
	if (string.IsNullOrWhiteSpace(titleId))
	{
		return null;
	}

	titleId = titleId.ToLowerInvariant();

	if (!titles.ContainsKey(titleId))
	{
		string titleUrl = $"{rootUrl}title/{titleId}/";
		HtmlDocument doc = GetHtmlDoc(titleUrl);

		string title =
			doc.DocumentNode.Descendants("h1").FirstOrDefault()?
				.InnerText?
				.Trim()
			?? "";

		title = title.Replace("&nbsp;", " ").Trim();

		titles.Add(titleId, title);
	}

	//-->
	return titles[titleId];
}

static HtmlDocument GetHtmlDoc(string url)
{
	using (WebClient wc = new WebClient())
	{
		//Stopwatch sw = Stopwatch.StartNew();
		
		string html = wc.DownloadString(url);

		//sw.Stop();
		//Console.WriteLine($"Web: Elapsed: {sw.ElapsedMilliseconds.ToString("N0").PadLeft(8)}, Got URL: {url}");

		HtmlDocument newDoc = new HtmlDocument();
		newDoc.LoadHtml(html);

		return newDoc;
	}
}