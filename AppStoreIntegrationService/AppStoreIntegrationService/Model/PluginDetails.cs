using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationService.Model
{
	public class PluginDetails
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public IconDetails Icon { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public int DownloadCount { get; set; }
		public int CommentCount { get; set; }
		public string SupportText { get; set; }
		public bool PaidFor { get; set; }
		public string Pricing { get; set; }
		public RatingDetails RatingSummary { get; set; }
		public DeveloperDetails Developer { get; set; }
		public List<IconDetails> Media { get; set; }	
		public List<PluginVersion> Versions { get; set; }

		public List<CategoryDetails> Categories { get; set; }
		public string DownloadUrl { get; set; }
	}
}
