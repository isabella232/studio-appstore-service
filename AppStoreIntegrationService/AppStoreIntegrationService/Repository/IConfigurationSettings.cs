using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace AppStoreIntegrationService.Repository
{
	public interface IConfigurationSettings
	{
		public string StorageAccountName { get; set; }
		/// <summary>
		/// Azure Storage account key
		/// </summary>
		public string StorageAccountKey { get; set; }

		/// <summary>
		/// Azure Blob Name
		/// </summary>
		public string BlobName { get; set; }
		/// <summary>
		/// Local path where json file with the plugins details is saved. If the file does not exist the service will create it.
		/// Server supports network local path also (for this the server must be deployed with "NetworkFilePath" option)
		/// </summary>
		public string LocalFolderPath { get; set; }
		/// <summary>
		/// Name of the json file with plugin details (file saved locally or on Azure)
		/// </summary>
		public string ConfigFileName { get; set; }
		/// <summary>
		/// Oos uri from where the server will refresh the json file from Azure if the Configuration is set to "AzureBlob")
		/// </summary>
		public string OosUri { get; set; }
		/// <summary>
		/// NAme of the json file from where service reads the plugins name mapping
		/// </summary>
		public string MappingFileName { get; set; }

		/// <summary>
		/// Azure Telemetry Instrumentation Key
		/// </summary>
		public string InstrumentationKey { get; set; }

		public Enums.DeployMode DeployMode { get; set; }
		public string NameMappingsFilePath { get; set; }
		public string ConfigFolderPath { get; set; }
		public string LocalPluginsConfigFilePath { get; set; }
		public string ConfigFileBackUpPath { get; set; }
		public Task SetFilePathsProperties(IWebHostEnvironment environment);
	}
}
