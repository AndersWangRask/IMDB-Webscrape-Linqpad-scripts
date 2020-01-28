<Query Kind="Program">
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>Microsoft.VisualBasic</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>HtmlAgilityPack</Namespace>
</Query>

const string rootUrl = @"https://www.imdb.com/";

void Main()
{
	string person1Id = @"nm0202970"; //Larry David
	string person2Id = @"nm0385644"; //Cheryl Hines
	
	var filmos1 = GetFilmos(person1Id);
	var filmos2 = GetFilmos(person2Id);
	
	var jointFilmos = 
		filmos1.Intersect(filmos2)
			.ToDictionary(
				f => f,
				f => GetTitle(f));
	
	//Console.WriteLine(person1Html);
    //Console.WriteLine(filmos1.Count());
    //Console.WriteLine(filmos1);
	
	jointFilmos.Dump();
}

IEnumerable<string> GetFilmos(string personId)
{
	using (WebClient wc = new WebClient())
	{
		string nameUrl = $"{rootUrl}name/{personId}/";
	
		string html = wc.DownloadString(nameUrl);
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(html);

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
		using (WebClient wc = new WebClient())
		{
			string titleUrl = $"{rootUrl}title/{titleId}/";
			
			string html = wc.DownloadString(titleUrl);
	
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);
			
			string title = 
				doc.DocumentNode.Descendants("h1").FirstOrDefault()?
					.InnerText?
					.Trim()
				?? "";
				
			title = title.Replace("&nbsp;", " ").Trim();
				
			return title;
		}
	}

	//-->
	return titles[titleId];
}
