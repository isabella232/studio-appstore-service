using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
		private readonly CloudStorageAccount _cloudStorageAccount;
		private CloudBlobContainer _cloudBlobContainer;
		private CloudBlockBlob _cloudBlockBlob;
		private CloudBlockBlob _backupCloudBlockBlob;
		private readonly int _maxRetryCount = 3;
		private readonly BlobRequestOptions _blobRequestOptions;
		private readonly DeployMode _deployMode;
		private readonly ConfigurationSettings _configurationSettings;
		private readonly string _backupFileName;

		public AzureRepository(DeployMode deployMode, ConfigurationSettings configurationSettings)
		{
			_deployMode = deployMode;
			_configurationSettings = configurationSettings;
			_blobRequestOptions = new BlobRequestOptions
			{
				MaximumExecutionTime = TimeSpan.FromSeconds(120),
				RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), _maxRetryCount),
				DisableContentMD5Validation = true,
				StoreBlobContentMD5 = false
			};
			if(deployMode == DeployMode.AzureBlob)
			{
				_cloudStorageAccount = GetCloudStorageAccount();
				_backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.ConfigFileName)}_backupFile.json";

				CreateContainer();
			}			
		}

		public void CreateContainer()
		{
			var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
			_cloudBlobContainer = blobClient.GetContainerReference(_configurationSettings.BlobName.Trim().ToLower());

			if (_cloudBlobContainer.CreateIfNotExists())
			{
				_cloudBlobContainer.SetPermissionsAsync(new
				BlobContainerPermissions
				{
					PublicAccess = BlobContainerPublicAccessType.Blob
				});
			}

			if (_deployMode == DeployMode.AzureBlob && !string.IsNullOrEmpty(_configurationSettings.ConfigFileName))
			{
				SetCloudBlockBlob(_configurationSettings.ConfigFileName,_backupFileName);

				InitializeBlockBlobs();				
			}
		}

		private void InitializeBlockBlobs()
		{
			var fileBlobExists = _cloudBlockBlob.Exists();
			if (!fileBlobExists)
			{
				_cloudBlockBlob.UploadText(string.Empty);
			}

			var backupBlobExists = _backupCloudBlockBlob.Exists();
			if (!backupBlobExists)
			{
				_backupCloudBlockBlob.UploadText(string.Empty);
			}
		}

		public void SetCloudBlockBlob(string fileName,string backupFileName)
		{
			_cloudBlockBlob = _cloudBlobContainer.GetBlockBlobReference(fileName);
			_cloudBlockBlob.Properties.ContentType = Path.GetExtension(fileName);

			_backupCloudBlockBlob= _cloudBlobContainer.GetBlockBlobReference(backupFileName);
			_cloudBlockBlob.Properties.ContentType = Path.GetExtension(backupFileName);

		}

		public CloudStorageAccount GetCloudStorageAccount()
		{
			var storageCredentils = new StorageCredentials(_configurationSettings?.StorageAccountName, _configurationSettings?.StorageAccountKey);
			var storageAccount = new CloudStorageAccount(storageCredentils, true);
			return storageAccount;
		}

		public async Task<List<PluginDetails>> GetPluginsListFromContainer()
		{
			var containterContent = await _cloudBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions,null);
			var stream = new MemoryStream();

			await _cloudBlockBlob.DownloadToStreamAsync(stream,null,_blobRequestOptions,null);

			var pluginsList = JsonConvert.DeserializeObject<PluginsResponse>(containterContent)?.Value;

			if(pluginsList is null)
			{
				return new List<PluginDetails>();
			}
			return pluginsList;
		}

		public async Task UploadToContainer (Stream pluginsStream)
		{
			await _cloudBlockBlob.UploadFromStreamAsync(pluginsStream,null,_blobRequestOptions,null);
		}

		public DeployMode GetDeployMode()
		{
			return _deployMode;
		}

		public async Task UpdatePluginsFileBlob(string fileContent)
		{
			await _cloudBlockBlob.UploadTextAsync(fileContent);
		}

		public async Task BackupFile(string fileContent)
		{
			await _backupCloudBlockBlob.UploadTextAsync(fileContent);
		}
	}
}
