using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppStoreIntegrationService.Model;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;
using static AppStoreIntegrationService.Enums;

namespace AppStoreIntegrationService.Repository
{
	public class AzureRepository : IAzureRepository
	{
		private CloudBlobContainer _cloudBlobContainer;
		private CloudBlockBlob _pluginsListBlockBlob;
		private CloudBlockBlob _pluginsBackupBlockBlob;
		private CloudBlockBlob _nameMappingsBlockBlob;
		private readonly BlobRequestOptions _blobRequestOptions;
		private readonly IConfigurationSettings _configurationSettings;

		public AzureRepository(IConfigurationSettings configurationSettings)
		{
			_configurationSettings = configurationSettings;
			if (_configurationSettings.DeployMode != DeployMode.AzureBlob ||
			    string.IsNullOrEmpty(_configurationSettings.ConfigFileName)) return;

			_blobRequestOptions = new BlobRequestOptions
			{
				MaximumExecutionTime = TimeSpan.FromSeconds(120),
				RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 3),
				DisableContentMD5Validation = true,
				StoreBlobContentMD5 = false
			};
			
			var cloudStorageAccount = GetCloudStorageAccount();
			if (cloudStorageAccount != null)
			{
				CreateContainer(cloudStorageAccount);
			}
		}

		public async Task<List<PluginDetails>> GetPluginsListFromContainer()
		{
			var containterContent = await _pluginsListBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions,null);
			var stream = new MemoryStream();

			await _pluginsListBlockBlob.DownloadToStreamAsync(stream,null,_blobRequestOptions,null);
			var pluginsList = JsonConvert.DeserializeObject<PluginsResponse>(containterContent)?.Value;

			await stream.DisposeAsync();

			return pluginsList ?? new List<PluginDetails>();
		}

		public async Task<List<NameMapping>> GetNameMappingsFromContainer()
		{
		    if (_nameMappingsBlockBlob is null) return new List<NameMapping>();
			var containterContent = await _nameMappingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
			var stream = new MemoryStream();

			await _nameMappingsBlockBlob.DownloadToStreamAsync(stream, null, _blobRequestOptions, null);
			var nameMappings = JsonConvert.DeserializeObject<List<NameMapping>>(containterContent);
			await stream.DisposeAsync();
			return nameMappings ?? new List<NameMapping>();
		}

		public async Task UploadToContainer (Stream pluginsStream)
		{
			await _pluginsListBlockBlob.UploadFromStreamAsync(pluginsStream,null,_blobRequestOptions,null);
		}

		public async Task UpdatePluginsFileBlob(string fileContent)
		{
			await _pluginsListBlockBlob.UploadTextAsync(fileContent);
		}

		public async Task BackupFile(string fileContent)
		{
			await _pluginsBackupBlockBlob.UploadTextAsync(fileContent);
		}

		private CloudStorageAccount GetCloudStorageAccount()
		{
			if (string.IsNullOrEmpty(_configurationSettings?.StorageAccountName) ||
			    string.IsNullOrEmpty(_configurationSettings?.StorageAccountKey)) return null;

			var storageCredentils = new StorageCredentials(_configurationSettings?.StorageAccountName, _configurationSettings?.StorageAccountKey);
			var storageAccount = new CloudStorageAccount(storageCredentils, true);
			return storageAccount;
		}

		/// <summary>
		/// Creates a azure container if does not exists already
		/// </summary>
		private void CreateContainer(CloudStorageAccount cloudStorageAccount)
		{
			var blobClient = cloudStorageAccount.CreateCloudBlobClient();
			var blobName =NormalizeBlobName();
			_cloudBlobContainer = blobClient.GetContainerReference(blobName);

			if (_cloudBlobContainer.CreateIfNotExists())
			{
				_cloudBlobContainer.SetPermissionsAsync(new
					BlobContainerPermissions
					{
						PublicAccess = BlobContainerPublicAccessType.Blob
					});
			}

			SetCloudBlockBlobs();
			InitializeBlockBlobs();
		}

		/// <summary>
		/// Azure requirements for blob name: only letters and digits, lowercase, Container names must be > 3 characters
		/// </summary>
		private string NormalizeBlobName()
		{
			if (string.IsNullOrEmpty(_configurationSettings.BlobName))
			{
				_configurationSettings.BlobName = "defaultblobname";
			}
			var regex = new Regex("[A-Za-z0-9]+");
			var matchCollection = regex.Matches(_configurationSettings.BlobName);
			var normalizedName = string.Concat(matchCollection.Select(m => m.Value));
			if (normalizedName.Length < 3)
			{
				normalizedName = $"{normalizedName}appstore";
			}

			return normalizedName.ToLower();
		}

		/// <summary>
		/// Set cloud blob for config file and backupfile
		/// If the blob does not exist creates a new one
		/// </summary>
		private void InitializeBlockBlobs()
		{
			CreateEmptyFile(_pluginsListBlockBlob);
			CreateEmptyFile(_pluginsBackupBlockBlob);
			CreateEmptyFile(_nameMappingsBlockBlob);
		}

		private void CreateEmptyFile(CloudBlockBlob cloudBlockBlob)
		{
			if (cloudBlockBlob is null) return;
			var fileBlobExists = cloudBlockBlob.Exists();
			if (!fileBlobExists)
			{
				cloudBlockBlob.UploadText(string.Empty);
			}
		}

		/// <summary>
		/// Get reference to Cloud blobs/ files (json file with plugins, backup of the plugins file, name mapping file
		/// </summary>
		private void SetCloudBlockBlobs()
		{
			if (!string.IsNullOrEmpty(_configurationSettings.ConfigFileName))
			{
				_pluginsListBlockBlob = GetBlockBlobReference(_configurationSettings.ConfigFileName);

				var backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.ConfigFileName)}_backupFile.json";
				_pluginsBackupBlockBlob = GetBlockBlobReference(backupFileName);
			}

			if (!string.IsNullOrEmpty(_configurationSettings.MappingFileName))
				_nameMappingsBlockBlob =GetBlockBlobReference(_configurationSettings.MappingFileName);
		}

		private CloudBlockBlob GetBlockBlobReference(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return null;
			var cloudBlob = _cloudBlobContainer.GetBlockBlobReference(fileName);
			cloudBlob.Properties.ContentType = Path.GetExtension(fileName);
			return cloudBlob;
		}
	}
}
