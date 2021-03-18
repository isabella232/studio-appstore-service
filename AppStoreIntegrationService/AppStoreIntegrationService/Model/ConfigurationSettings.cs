using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace AppStoreIntegrationService.Model
{
	public class ConfigurationSettings
	{
		/// <summary>
		/// Azure Storage account name
		/// </summary>
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

		public ConfigurationSettings(Enums.DeployMode deployMode)
		{
			DeployMode = deployMode;
		}

		public async Task SetFilePathsProperties(IWebHostEnvironment environment)
		{
			switch (DeployMode)
			{
				case Enums.DeployMode.AzureBlob:
					return;
				case Enums.DeployMode.ServerFilePath:
					ConfigFolderPath = $"{environment.ContentRootPath}{LocalFolderPath}";
					break;
				case Enums.DeployMode.NetworkFilePath:
					ConfigFolderPath = LocalFolderPath;
					break;
			}

			NameMappingsFilePath = Path.Combine(ConfigFolderPath, MappingFileName);
			LocalPluginsConfigFilePath = Path.Combine(ConfigFolderPath, ConfigFileName);

			ConfigFileBackUpPath = Path.Combine(ConfigFolderPath, $"{Path.GetFileNameWithoutExtension(ConfigFileName)}_backup.json");
			await CreateConfigurationFiles();
		}

		private async Task CreateConfigurationFiles()
		{
			if (!string.IsNullOrEmpty(ConfigFolderPath))
			{
				Directory.CreateDirectory(ConfigFolderPath);
				if (!File.Exists(LocalPluginsConfigFilePath))
				{
					await File.Create(LocalPluginsConfigFilePath).DisposeAsync();
				}

				if (!File.Exists(NameMappingsFilePath))
				{
					await File.Create(NameMappingsFilePath).DisposeAsync();
				}

				if (!File.Exists(ConfigFileBackUpPath))
				{
					await File.Create(ConfigFileBackUpPath).DisposeAsync();
				}
			}
		}

		public void LoadVariables()
		{
			StorageAccountName = GetVariable(ServiceResource.StorageAccountName);
			StorageAccountKey = GetVariable(ServiceResource.StorageAccountKey);
			BlobName = GetVariable(ServiceResource.BlobName);
			LocalFolderPath = GetVariable(ServiceResource.LocalFolderPath);
			ConfigFileName = GetVariable(ServiceResource.ConfigFileName);
			OosUri = GetVariable(ServiceResource.OosUri);
			InstrumentationKey = GetVariable(ServiceResource.TelemetryInstrumentationKey);
			MappingFileName = GetVariable(ServiceResource.MappingFileName);
		}

		private string GetVariable(string key)
		{
			// by default it gets a process variable. Allow getting user as well
			return
				Environment.GetEnvironmentVariable(key) ??
				Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
		}
	}
}
