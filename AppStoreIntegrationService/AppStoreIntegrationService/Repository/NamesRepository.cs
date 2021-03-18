using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppStoreIntegrationService.Model;
using Newtonsoft.Json;

namespace AppStoreIntegrationService.Repository
{
	public class NamesRepository:INamesRepository
	{
		private readonly IAzureRepository _azureRepository;
		private readonly ConfigurationSettings _configurationSettings;
		private readonly List<NameMapping> _nameMappings;

		public NamesRepository(IAzureRepository azureRepository,ConfigurationSettings configurationSettings)
		{
			_azureRepository = azureRepository;
			_configurationSettings = configurationSettings;
			_nameMappings = new List<NameMapping>();
		}

		public async Task<IEnumerable<NameMapping>> GetAllNameMappings(List<string> pluginsNames)
		{
			_nameMappings.Clear();
			if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
			{
				var localMappings = await ReadLocalNameMappings(_configurationSettings.NameMappingsFilePath);
				_nameMappings.AddRange(localMappings);
			}
			else
			{
				var azureMappings = await _azureRepository.GetNameMappingsFromContainer();
				_nameMappings.AddRange(azureMappings);
			}

			return pluginsNames.Select(pluginName => _nameMappings.FirstOrDefault(n => n.OldName.Equals(pluginName))).Where(mapping => mapping != null);
		}

		public async Task<List<NameMapping>> ReadLocalNameMappings(string nameMappingsFilePath)
		{
			var nameMappings = await File.ReadAllTextAsync(nameMappingsFilePath);
			return JsonConvert.DeserializeObject<List<NameMapping>>(nameMappings);
		}
	}
}
