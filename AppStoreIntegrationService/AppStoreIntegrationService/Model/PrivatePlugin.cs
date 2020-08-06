using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationService.Model
{
	public class PrivatePlugin
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Plugin Name is required")]
		[MinLength(5)]
		public string Name { get; set; }

		[Required(ErrorMessage = "Plugin Descriotion is required")]
		[MinLength(20)]
		public string Description { get; set; }

		public bool PaidFor { get; set; }

		[Required(ErrorMessage = "The url from where the plugin should be downloaded is required")]
		[MinLength(5)]
		public string DownloadUrl { get; set; }

		public List<CategoryDetails> Categories { get; set; }

		public List<PluginVersion> Versions { get; set; }
		public string NewVersionNumber { get; set; }

		public string IconUrl{get; set;}

		public void SetIcon(string iconPath)
		{
			IconUrl = iconPath;
		}
	}
}
	
