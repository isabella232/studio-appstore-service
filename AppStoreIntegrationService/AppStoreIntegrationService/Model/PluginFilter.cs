using System.Collections.Generic;

namespace AppStoreIntegrationService.Model
{
	public class PluginFilter
	{
		public string Query { get; set; }
		public string StudioVersion { get; set; }
		public string SortOrder { get; set; }
		public string Price { get; set; }
		public bool DownloadCount { get; set; }
		public bool ReviewCount { get; set; }
		public bool TopRated { get; set; }	
		public List<int> CategoryId { get; set; }
	}
}
