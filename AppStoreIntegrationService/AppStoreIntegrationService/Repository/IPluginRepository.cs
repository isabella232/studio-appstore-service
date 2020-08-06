using AppStoreIntegrationService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Repository
{
	public interface IPluginRepository
	{
		Task<List<PluginDetails>> GetAll(string sortOrder);
		List<PluginDetails> SearchPlugins(List<PluginDetails>pluginsList,PluginFilter pluginFilter);
		Task UpdatePrivatePlugin(PrivatePlugin privatePlugin); 
		Task<PluginDetails> GetPluginById(int id);
		Task<List<CategoryDetails>> GetCategories();
		Task AddPrivatePlugin(PrivatePlugin plugin);
		Task RemovePlugin(int id);
	}
}
