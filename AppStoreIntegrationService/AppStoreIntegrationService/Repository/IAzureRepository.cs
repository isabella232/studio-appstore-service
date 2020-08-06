using AppStoreIntegrationService.Model;
using Microsoft.Azure.Storage;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static AppStoreIntegrationService.Enums;

namespace AppStoreIntegrationService.Repository
{
	public interface IAzureRepository
	{
		/// <summary>
		/// Creates a azure container if does not exists already
		/// </summary>
		public void CreateContainer();
		/// <summary>
		/// Set cloud blob for config file and backupfile
		/// If the blob does not exist creates a new one
		/// </summary>
		public void SetCloudBlockBlob(string fileName,string bckupfileName);
		/// <summary>
		/// Refeshes the list of plugins from public appstore service
		/// </summary>
		public Task UploadToContainer(Stream pluginsStream);
		public Task<List<PluginDetails>> GetPluginsListFromContainer();
		public CloudStorageAccount GetCloudStorageAccount();
		public DeployMode GetDeployMode();
		/// <summary>
		/// String which contains the plugins list updated. (Format of the file is json)
		/// </summary>
		public Task UpdatePluginsFileBlob(string fileContent);

		public Task BackupFile(string fileContent);
	}
}
