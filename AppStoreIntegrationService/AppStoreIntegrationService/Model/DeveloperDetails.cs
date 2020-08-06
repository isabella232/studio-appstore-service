using Newtonsoft.Json;

namespace AppStoreIntegrationService.Model
{
	public class DeveloperDetails
	{
		public string DeveloperName { get; set; }
		public string DeveloperDescription { get; set; }
		[JsonProperty("DeveloperURL")]
		public string DeveloperUrl { get; set; }
	}
}
