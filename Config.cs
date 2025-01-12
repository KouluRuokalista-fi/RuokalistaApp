using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuokalistaApp
{
	internal class Config
	{
		public static async Task<Dictionary<string, string>>  GetServerConfig(string url)
		{
			var dict = new Dictionary<string, string>();

			//get the config from the json api
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			var request = "/api/v1/App/Config";
			HttpResponseMessage response = await client.GetAsync(request);
			if (response.IsSuccessStatusCode)
			{
				var result = response.Content.ReadAsStringAsync().Result;
				var json = JObject.Parse(result);
				foreach (var item in json)
				{
					dict.Add(item.Key, item.Value.ToString());
				}
			}
			else
			{
				throw new Exception(request + " failed with status code " + response.StatusCode.ToString());
			}

			return dict;
		}

		public static string PrimaryFallbackColor = "#0074ff"; //update here and in Colors.xaml
	}
}
