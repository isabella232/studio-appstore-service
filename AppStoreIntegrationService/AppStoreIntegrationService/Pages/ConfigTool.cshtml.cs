using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using AppStoreIntegrationService.Controllers;
using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppStoreIntegrationService
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ConfigToolModel : PageModel
    {
        private readonly PluginsController _pluginsController;
        private readonly IHttpContextAccessor _context;

        [BindProperty]
        public List<PrivatePlugin> PrivatePlugins { get; set; }

        [BindProperty]
        public PrivatePlugin PrivatePlugin { get; set; }

        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public string Name { get; set; }


        public ConfigToolModel(PluginsController pluginsController, IHttpContextAccessor context)
        {
            _pluginsController = pluginsController;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            PrivatePlugins = new List<PrivatePlugin>();
            var pluginFiler = new PluginFilter
            {
                SortOrder = "asc"
            };

            var privatePluginsResult = await _pluginsController.Get(pluginFiler);
            var resultObject = privatePluginsResult as OkObjectResult;

            if (resultObject != null && resultObject.StatusCode == 200)
            {
                var privatePlugins = resultObject.Value as List<PluginDetails>;

                if (privatePlugins != null)
                {
                    InitializePrivatePlugins(privatePlugins);
                }
            }
        }

        public async Task<IActionResult> OnGetShowDeleteModal(int id,string name)
        {
            var privatePlugin = new PrivatePlugin
            {
                Id = id,
                Name = name
            };            

            return Partial("_DeletePluginPartial", privatePlugin);
        }

        public async Task<IActionResult> OnPostDeletePlugin()
        {
            await _pluginsController.DeletePlugin(Id);

            return RedirectToPage("ConfigTool");
        }

        public async Task<IActionResult> OnGetAddPlugin()
        {
            return Partial("_AddPluginPartial", new PrivatePlugin());            
        }

        private void InitializePrivatePlugins(List<PluginDetails> privatePlugins)
        {
            foreach (var pluginDetails in privatePlugins)
            {
                var privatePlugin = new PrivatePlugin
                {
                    Id = pluginDetails.Id,
                    Description = pluginDetails.Description,
                    Name = pluginDetails.Name,
                    Categories = pluginDetails.Categories,
                    Versions = pluginDetails.Versions
                };
                var iconPath = string.Empty;
                if (string.IsNullOrEmpty(pluginDetails.Icon.MediaUrl))
                {
                    var defaultIconResult = _pluginsController.GetDefaultIcon();
                    var resultObject = defaultIconResult as OkObjectResult;

                    if (resultObject != null && resultObject.StatusCode == 200)
                    {
                        iconPath = resultObject.Value as string; 
                    }
                }
                else
                {
                    iconPath = pluginDetails.Icon.MediaUrl;
                }

                privatePlugin.SetIcon(iconPath);
                PrivatePlugins.Add(privatePlugin);
            }
        }
       
    }
}