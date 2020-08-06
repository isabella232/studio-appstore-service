using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppStoreIntegrationService.Model
{
	public class PluginVersion 
	{
		private readonly List<SupportedProductDetails> _supportedProductDetails;
		private SupportedProductDetails _selectedProduct;

		public PluginVersion()
		{
			_supportedProductDetails = new List<SupportedProductDetails>
			{
				new SupportedProductDetails
				{
					Id ="37",
					ProductName ="SDL Trados Studio 2021",
					ParentProductID =14,
					MinimumStudioVersion = "16.0"
				}
				//new SupportedProductDetails
				//{
				//	Id ="38",
				//	ProductName ="SDL Trados Studio 2022",
				//	ParentProductID =14,
				//	MinimumStudioVersion ="17.0"
				//}
			};

			SupportedProductsListItems = new SelectList(_supportedProductDetails, nameof(SupportedProductDetails.Id), nameof(SupportedProductDetails.ProductName));			
		}

		public DateTime? CreatedDate { get; set; }
		public int DownloadCount { get; set; }
		public string Id { get; set; }
		public DateTime? ReleasedDate { get; set; }
		public string TechnicalRequirements { get; set; }
		public string VersionNumber { get; set; }
		public List<SupportedProductDetails> SupportedProducts { get; set; }
		public bool AppHasStudioPluginInstaller { get; set; }


		//TODO: Create a new object for private plugin version
		/// <summary>
		/// For Studio 2021 is 16.0 by default
		/// </summary>
		public string MinimumRequiredVersionOfStudio{get;set;}

		[JsonProperty("SDLHosted")]
		public bool SdlHosted { get; set; }

		public string DownloadUrl { get; set; }
		/// <summary>
		/// For the plugins from private repo (config file) by default will be set to true
		/// </summary>
		public bool IsPrivatePlugin { get; set; }

		// Properties used in Config Tool app	
		[JsonIgnore]
		public string SelectedProductId { get; set; }

		[JsonIgnore]
		[BindProperty]
		public SupportedProductDetails SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				_selectedProduct = value;
				UpdateStudioMinVersion();
			}
		}

		[JsonIgnore]
		[BindProperty]
		public SelectList SupportedProductsListItems { get; set; }


		[JsonIgnore]
		public string VersionName { get; set; }
		[JsonIgnore]
		public bool IsNewVersion { get; set; }

		public void SetSupportedProducts()
		{
			if (SupportedProducts == null)
			{
				SupportedProducts = new List<SupportedProductDetails>();
				SupportedProducts.AddRange(_supportedProductDetails);
			}
		}

		private void UpdateStudioMinVersion()
		{
			if (string.IsNullOrEmpty(MinimumRequiredVersionOfStudio))
			{
				if (string.IsNullOrEmpty(SelectedProduct?.MinimumStudioVersion))
				{
					var productDetails = _supportedProductDetails.FirstOrDefault(v => v.ProductName.Equals(SelectedProduct.ProductName));
					if (productDetails != null)
					{
						var minVersion = productDetails.MinimumStudioVersion;
						SelectedProduct.MinimumStudioVersion = minVersion;
						MinimumRequiredVersionOfStudio = minVersion;
					}
				}
			}
		}
	}
}
