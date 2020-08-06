using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AppStoreIntegrationLoadTests
{
	[TestClass]
	public class PluginsTests
	{
		[TestMethod]
		public void GetPlugins()
		{
			var client = new RestClient();
			var request = new RestRequest("https://appstoreintegrationservice20191104013058.azurewebsites.net/plugins", Method.GET);
			request.AddHeader("Content-Type", "application/json");

			var response = client.Execute(request);

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}
