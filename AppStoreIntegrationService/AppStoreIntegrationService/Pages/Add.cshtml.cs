using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStoreIntegrationService.Controllers;
using AppStoreIntegrationService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationService
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AddModel : PageModel
    {
        [BindProperty]
        public PrivatePlugin PrivatePlugin { get; set; }
        [BindProperty]
        public List<int> SelectedCategories { get; set; }
        [BindProperty]
        public List<PluginVersion> Versions { get; set; }

        [BindProperty]
        public List<CategoryDetails> Categories { get; set; }

        [BindProperty]
        public PluginVersion SelectedVersionDetails { get; set; }

        [BindProperty]
        public SupportedProductDetails SelectedProduct { get; set; }

        [BindProperty]
        public string SelectedVersionId { get; set; }

        public SelectList CategoryListItems { get; set; }

        private PluginsController _pluginsController;
        private CategoriesController _categoriesController;

        public AddModel(PluginsController pluginsController, CategoriesController categoriesController)
        {
            _pluginsController = pluginsController;
            _categoriesController = categoriesController;
        }

        public async Task<IActionResult> OnPostAddPluginAsync()
        {
            SetDefaultIcon();
            await SetAvailableCategories();

            SelectedVersionDetails = new PluginVersion
            {
                VersionName = PrivatePlugin.NewVersionNumber,
                VersionNumber= PrivatePlugin.NewVersionNumber,
                IsPrivatePlugin = true,
                AppHasStudioPluginInstaller=true,
                Id = Guid.NewGuid().ToString(),
            };
            SelectedVersionId = SelectedVersionDetails.Id;
            SelectedVersionDetails.SetSupportedProducts();

            PrivatePlugin.Versions = new List<PluginVersion>();
            PrivatePlugin.Versions.Add(SelectedVersionDetails);
            Versions.Add(SelectedVersionDetails);

            SetSelectedProducts(Versions, string.Empty);

            return Page();
        }

        public async Task<IActionResult> OnPostSavePlugin()
        {
            var modalDetails = new ModalMessage
            {
                RequestPage = "add",
            };
            if (IsValid())
            {
                await SetValues();
                var response = await _pluginsController.PostAddPlugin(PrivatePlugin);
                var statusCode = (response as StatusCodeResult).StatusCode;

                if (statusCode.Equals(200))
                {
                    modalDetails.ModalType = ModalType.SuccessMessage;
                    modalDetails.Message = $"{PrivatePlugin.Name} was added.";
                }
            }
            else
            {
                modalDetails.Title = string.Empty;
                modalDetails.Message = "Please fill all required values.";
                modalDetails.ModalType =ModalType.WarningMessage;
            }
            return Partial("_ModalPartial", modalDetails);
        }      

        public async Task<IActionResult> OnPostAddVersion()
        {
            SelectedVersionDetails = new PluginVersion
            {
                VersionNumber = string.Empty,
                IsPrivatePlugin = true,
                IsNewVersion = true,
                Id = Guid.NewGuid().ToString(),
            };
            SelectedVersionDetails.SetSupportedProducts();

            Versions.Add(SelectedVersionDetails);
            SetSelectedProducts(Versions, "New plugin version");

            ModelState.Clear();

            return Partial("_PluginVersionDetailsPartial", SelectedVersionDetails);
        }

        public async Task<IActionResult> OnPostShowVersionDetails()
        {
            var version = Versions.FirstOrDefault(v => v.Id.Equals(SelectedVersionId));

            ModelState.Clear();
            return Partial("_PluginVersionDetailsPartial", version);
        }

        private void SetDefaultIcon()
        {
            var defaultIconResult = _pluginsController.GetDefaultIcon();
            var resultObject = defaultIconResult as OkObjectResult;

            if (resultObject != null && resultObject.StatusCode == 200)
            {
                PrivatePlugin.IconUrl = resultObject.Value as string;
            }            
        }

        private async Task SetAvailableCategories()
        {
            var categoriesResult = await _categoriesController.Get();
            var resultObject = categoriesResult as OkObjectResult;

            if (resultObject != null && resultObject.StatusCode == 200)
            {
                Categories = resultObject.Value as List<CategoryDetails>;
                CategoryListItems = new SelectList(Categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name));
            }
        }

        private bool IsValid()
        {
            var generalDetailsContainsNull = AnyNull(PrivatePlugin.Name, PrivatePlugin.Description, PrivatePlugin.IconUrl);

            if (!string.IsNullOrEmpty(SelectedVersionId) && SelectedVersionDetails != null)
            {
                var detailsContainsNull = AnyNull(SelectedVersionDetails.VersionNumber, SelectedVersionDetails.MinimumRequiredVersionOfStudio, SelectedVersionDetails.DownloadUrl);
                if (generalDetailsContainsNull || detailsContainsNull)
                {
                    return false;
                }
            }
            return !generalDetailsContainsNull;
        }
        private async Task SetValues()
        {
            SetVersionList();
            await SetCategoryList();
            // This method will be removed later after studio release. We had to move the download url  from plugin to version details. Studio still uses the url from the plugin details
            SetDownloadUrl();
        }

        private void SetDownloadUrl()
        {
            PrivatePlugin.DownloadUrl = PrivatePlugin.Versions.LastOrDefault()?.DownloadUrl;
        }

        private void SetVersionList()
        {
            var editedVersion = Versions.FirstOrDefault(v => v.Id.Equals(SelectedVersionDetails.Id));

            if (editedVersion != null)
            {
                var indexOfEditedVersion = Versions.IndexOf(editedVersion);

                if (SelectedVersionDetails?.SelectedProduct != null)
                {
                    SelectedVersionDetails.SupportedProducts.Clear();
                    SelectedVersionDetails.SupportedProducts.Add(SelectedVersionDetails.SelectedProduct);
                   // Versions.Add(SelectedVersionDetails);
                }
                Versions[indexOfEditedVersion] = SelectedVersionDetails;
            }
            //else if (SelectedVersionDetails?.SelectedProduct != null)
            //{
            //    //This is a new version and we need to add it to the list
            //    SelectedVersionDetails.SupportedProducts.Clear();
            //    SelectedVersionDetails.SupportedProducts.Add(SelectedVersionDetails.SelectedProduct);
            //    Versions.Add(SelectedVersionDetails);
            //}

            PrivatePlugin.Versions = Versions;
        }

        private async Task SetCategoryList()
        {
            PrivatePlugin.Categories = new List<CategoryDetails>();
            foreach (var categoryId in SelectedCategories)
            {
                var category = Categories.FirstOrDefault(c => c.Id.Equals(categoryId));
                if (category != null)
                {
                    PrivatePlugin.Categories.Add(category);
                }
            }
        }

        private void SetSelectedProducts(List<PluginVersion> versions, string versionName)
        {
            foreach (var version in versions)
            {
                if (version.SelectedProduct is null)
                {
                    var lastSupportedProduct = version.SupportedProducts?.LastOrDefault();
                    if (lastSupportedProduct != null)
                    {
                        version.SelectedProductId = lastSupportedProduct.Id;
                        version.SelectedProduct = lastSupportedProduct;
                        if (!version.IsNewVersion)
                        {
                            version.VersionName = $"{version.SelectedProduct.ProductName} - {version.VersionNumber}";
                        }
                        else
                        {
                            version.VersionName = versionName;
                        }
                    }
                }
            }
        }

        public static bool AnyNull(params object[] objects)
        {
            return objects.Any(s => s == null);
        }
    }
}