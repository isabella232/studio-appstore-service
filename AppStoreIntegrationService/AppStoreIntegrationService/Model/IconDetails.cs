using Newtonsoft.Json;

namespace AppStoreIntegrationService.Model
{
	public class IconDetails
	{
		[JsonProperty("MediaURL")]
		public string MediaUrl { get; set; }
	}
}
