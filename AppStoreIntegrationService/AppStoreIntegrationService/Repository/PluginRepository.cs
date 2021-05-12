using AppStoreIntegrationService.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

namespace AppStoreIntegrationService.Repository
{
	public class PluginRepository : IPluginRepository
	{
		private const int RefreshDuration = 10;
		private const int CategoryId_AutomatedTranslation = 6;
		private const int CategoryId_TranslationMemory = 3;
		private const int CategoryId_Terminology = 4;
		private const int CategoryId_FileFiltersConverters = 2;

		private readonly Timer _pluginsCacheRenewer;
		private readonly IConfigurationSettings _configurationSettings;
		private readonly IAzureRepository _azureRepository;
		private List<CategoryDetails> _availableCategories;
		private readonly HttpClient _httpClient;

		public PluginRepository(IAzureRepository azureRepository, IConfigurationSettings configurationSettings,HttpClient httpClient)
		{
			_azureRepository = azureRepository;
			_configurationSettings = configurationSettings;
			_httpClient = httpClient;
			_pluginsCacheRenewer = new Timer(OnCacheExpiredCallback,
				this,
				TimeSpan.FromMinutes(RefreshDuration),
				TimeSpan.FromMilliseconds(-1));

			InitializeCategoryList();
		}

		public async Task<List<PluginDetails>> GetAll(string sortOrder)
		{
			var pluginsList =await GetPlugins();

			if (pluginsList == null || pluginsList?.Count == 0)
			{
				await RefreshCacheList();

				pluginsList = await GetPlugins();
			}

			if (!string.IsNullOrEmpty(sortOrder) && !sortOrder.ToLower().Equals("asc"))
			{
				return pluginsList?.OrderByDescending(p => p.Name).ToList();
			}
			return pluginsList?.OrderBy(p => p.Name).ToList();
		}

		private async Task<List<PluginDetails>> GetPlugins()
		{
			if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob) //json file is saved locally on server
			{
				return await GetPluginsListFromLocalFile(); // read plugins from json file
			}

			return await _azureRepository.GetPluginsListFromContainer(); // json file is on Azure Blob
		}

		private async Task<List<PluginDetails>> GetPluginsListFromLocalFile()
		{
			var pluginsDetails = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsConfigFilePath);

			return JsonConvert.DeserializeObject<PluginsResponse>(pluginsDetails)?.Value;
		}

		private async Task RefreshCacheList()
		{
			if (!string.IsNullOrEmpty(_configurationSettings.OosUri) && _configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
			{
				var httpRequestMessage = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri($"{_configurationSettings.OosUri}/Apps?$expand=Categories,Versions($expand=SupportedProducts)")
				};
				var pluginsResponse = await _httpClient.SendAsync(httpRequestMessage);
				if (pluginsResponse.IsSuccessStatusCode)
				{
					if (pluginsResponse.Content != null)
					{
						var contentStream = await pluginsResponse.Content?.ReadAsStreamAsync();
						await _azureRepository.UploadToContainer(contentStream);
					}
				}
			}
		}

		public List<PluginDetails> SearchPlugins(List<PluginDetails> pluginsList, PluginFilter filter)
		{
			if(pluginsList is null)
			{
				pluginsList = new List<PluginDetails>();
			}
			var searchedPluginList = new List<PluginDetails>(pluginsList);

			if (!string.IsNullOrEmpty(filter?.Query))
			{
				searchedPluginList = FilterByQuery(searchedPluginList, filter.Query);
			}
			if (!string.IsNullOrEmpty(filter?.Price))
			{
				searchedPluginList = FilterByPrice(searchedPluginList, filter.Price);

			}
			if (!string.IsNullOrEmpty(filter?.StudioVersion))
			{
				searchedPluginList = FilterByVersion(searchedPluginList, filter.StudioVersion);
			}
			if (filter?.CategoryId?.Count > 0)
			{
				searchedPluginList = FilterByCategory(searchedPluginList, filter.CategoryId);
			}
			if (filter.DownloadCount)
			{
				searchedPluginList = FilterByDownloadCount(searchedPluginList);
			}
			if (filter.ReviewCount)
			{
				searchedPluginList = FilterByReviewCount(searchedPluginList);
			}
			if (filter.TopRated)
			{
				searchedPluginList = FilterByRatings(searchedPluginList);
			}

			return searchedPluginList;
		}
		public async Task<PluginDetails> GetPluginById(int id)
		{
			var pluginList = await GetAll("asc");
			if (pluginList != null)
			{
				return pluginList.FirstOrDefault(p => p.Id.Equals(id));
			}
			return new PluginDetails();
		}

		public Task<List<CategoryDetails>> GetCategories()
		{
			return Task.Run(() => _availableCategories);
		}

		private async void OnCacheExpiredCallback(object stateInfo)
		{
			try
			{
				await RefreshCacheList();
			}
			finally
			{
				_pluginsCacheRenewer?.Change(TimeSpan.FromMinutes(RefreshDuration), TimeSpan.FromMilliseconds(-1));
			}
		}

		private List<PluginDetails> FilterByCategory(List<PluginDetails> pluginsList, List<int> categoryIds)
		{
			var searchedPluginsResult = new List<PluginDetails>();

			foreach (var categoryId in categoryIds)
			{
				foreach (var plugin in pluginsList)
				{
					if (plugin.Categories != null)
					{
						var containsCategory = plugin.Categories.Any(c => c.Id.Equals(categoryId));
						if (containsCategory)
						{
							var pluginExist = searchedPluginsResult.Any(p => p.Id.Equals(plugin.Id));
							if (!pluginExist)
							{
								searchedPluginsResult.Add(plugin);
							}
						}
					}
				}
			}
			return searchedPluginsResult;
		}

		private List<PluginDetails> FilterByRatings(List<PluginDetails> pluginsList)
		{
			return pluginsList.OrderByDescending(p => p.RatingSummary?.AverageOverallRating).ThenBy(p => p.Name).ToList();
		}

		private List<PluginDetails> FilterByDownloadCount(List<PluginDetails> pluginsList)
		{
			return pluginsList.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Name).ToList();
		}

		private List<PluginDetails> FilterByReviewCount(List<PluginDetails> pluginsList)
		{
			return pluginsList.OrderByDescending(p => p.RatingSummary?.RatingsCount).ThenBy(p => p.Name).ToList();
		}

		private List<PluginDetails> FilterByQuery(List<PluginDetails> pluginsList, string query)
		{
			var searchedPluginsResult = new List<PluginDetails>();
			foreach (var plugin in pluginsList)
			{
				var matchName = Regex.IsMatch(plugin.Name.ToLower(), query.ToLower());
				if (matchName)
				{
					searchedPluginsResult.Add(plugin);
				}
			}
			return searchedPluginsResult;

		}

		private List<PluginDetails> FilterByPrice(List<PluginDetails> pluginsList, string price)
		{
			var paidFor = false;

			if (!string.IsNullOrEmpty(price))
			{
				if (price.ToLower().Equals("paid"))
				{
					paidFor = true;
				}
			}

			return pluginsList.Where(p => p.PaidFor.Equals(paidFor)).ToList();
		}

		private List<PluginDetails> FilterByVersion(List<PluginDetails> pluginsList, string studioVersion)
		{
			var plugins = new List<PluginDetails>();

			var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
			var versionNumber = expression.Match(studioVersion);
			var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
			var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

			foreach (var plugin in pluginsList)
			{
				foreach (var pluginVersion in plugin.Versions)
				{
					//there are some apps in the oos which are working for all studio version. So the field is "SDL Trados Studio" without any studio specific version
					var version = pluginVersion.SupportedProducts?.FirstOrDefault(s =>
						s.ProductName.Equals(oldTradosName) || s.ProductName.Equals(rebrandedStudioName)
						                                    || s.ProductName.Equals("SDL Trados Studio") ||
						                                    s.ProductName.Equals("Trados Studio"));
					if (version != null)
					{
						plugins.Add(plugin);
						break;
					}
				}
			}

			return plugins;
		}

		private void InitializeCategoryList()
		{
			_availableCategories = new List<CategoryDetails>
			{
				new CategoryDetails
				{
					Name = ServiceResource.CategoryAutomatedTranslation,
					Id = CategoryId_AutomatedTranslation
				},
				new CategoryDetails
				{
					Name = ServiceResource.CategoryTranslationMemory,
					Id = CategoryId_TranslationMemory
				},
				new CategoryDetails
				{
					Name = ServiceResource.CategoryTerminology,
					Id = CategoryId_Terminology
				},
				new CategoryDetails
				{
					Name = ServiceResource.CategoryFileFiltersConverters,
					Id = CategoryId_FileFiltersConverters
				}
			};
		}

		public async Task UpdatePrivatePlugin(PrivatePlugin privatePlugin)
		{
			var pluginsList = await GetPlugins();
			if(pluginsList != null)
			{
				await BackupFile(pluginsList);

				var pluginToBeUpdated = pluginsList.FirstOrDefault(p => p.Id.Equals(privatePlugin.Id));

				if (pluginToBeUpdated != null)
				{
					pluginToBeUpdated.Name = privatePlugin.Name;
					pluginToBeUpdated.Description = privatePlugin.Description;
					pluginToBeUpdated.Icon.MediaUrl = privatePlugin.IconUrl;
					pluginToBeUpdated.PaidFor = privatePlugin.PaidFor;
					pluginToBeUpdated.Categories = privatePlugin.Categories;
					pluginToBeUpdated.Versions = privatePlugin.Versions;
					pluginToBeUpdated.DownloadUrl = privatePlugin.DownloadUrl;
				}
				await SaveToFile(pluginsList);
			}
		}

		public async Task AddPrivatePlugin(PrivatePlugin privatePlugin)
		{
			if (privatePlugin != null)
			{
				var newPlugin = new PluginDetails
				{
					Name = privatePlugin.Name,
					Description = privatePlugin.Description,
					PaidFor = privatePlugin.PaidFor,
					Categories = privatePlugin.Categories,
					Versions = privatePlugin.Versions,
					DownloadUrl = privatePlugin.DownloadUrl,
					Id = 1,
					Icon = new IconDetails { MediaUrl = privatePlugin.IconUrl }
				};

				var pluginsList = await GetPlugins();

				if (pluginsList is null)
				{
					pluginsList = new List<PluginDetails>
					{
						newPlugin
					};
				}
				else
				{		
					var pluginExists = pluginsList.Any(p => p.Name.Equals(privatePlugin.Name));
					if (!pluginExists)
					{
						await BackupFile(pluginsList);

						var lastPlugin = pluginsList.OrderBy(p => p.Id).ToList().LastOrDefault();
						if (lastPlugin != null)
						{
							newPlugin.Id = lastPlugin.Id++;
						}
						pluginsList.Add(newPlugin);
					}
					else
					{
						throw new Exception($"Another plugin with the name {privatePlugin.Name} already exists");
					}
				}
				await SaveToFile(pluginsList);
			}
		}

		private async Task BackupFile(List<PluginDetails> pluginsList)
		{
			var pluginResponse = new PluginsResponse
			{
				Value = pluginsList
			};

			var updatedPluginsText = JsonConvert.SerializeObject(pluginResponse);
			if (_configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
			{
				await _azureRepository.BackupFile(updatedPluginsText);
			}
			else
			{
				//json file is saved locally on server or in File Newtork location
				await File.WriteAllTextAsync(_configurationSettings.ConfigFileBackUpPath, updatedPluginsText);
			}
		}

		private async Task SaveToFile(List<PluginDetails> pluginsList)
		{
			var pluginResponse = new PluginsResponse
			{
				Value = pluginsList
			};
			var updatedPluginsText = JsonConvert.SerializeObject(pluginResponse);
			
			if (_configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
			{
				await _azureRepository.UpdatePluginsFileBlob(updatedPluginsText);
			}
			else
			{
				//json file is saved locally on server or in File Newtork location
				await File.WriteAllTextAsync(_configurationSettings.LocalPluginsConfigFilePath, updatedPluginsText);
			}
		}

		public async Task RemovePlugin(int id)
		{
			var pluginsList = await GetPlugins();
			var pluginToBeDeleted = pluginsList.FirstOrDefault(p=>p.Id.Equals(id));
			if (pluginToBeDeleted !=null)
			{
				await BackupFile(pluginsList);
				pluginsList.Remove(pluginToBeDeleted);
				await SaveToFile(pluginsList);
			}
		}
	}
}
