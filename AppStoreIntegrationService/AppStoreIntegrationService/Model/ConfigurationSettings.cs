using System;

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
		/// Azure Telemetry Instrumentation Key
		/// </summary>
		public string InstrumentationKey { get; set; }

		public void LoadVariables()
		{
			StorageAccountName = GetVariable(ServiceResource.StorageAccountName);
			StorageAccountKey = GetVariable(ServiceResource.StorageAccountKey);
			BlobName = GetVariable(ServiceResource.BlobName);
			LocalFolderPath = GetVariable(ServiceResource.LocalFolderPath);
			ConfigFileName = GetVariable(ServiceResource.ConfigFileName);
			OosUri = GetVariable(ServiceResource.OosUri);
			InstrumentationKey = GetVariable(ServiceResource.TelemetryInstrumentationKey);
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
