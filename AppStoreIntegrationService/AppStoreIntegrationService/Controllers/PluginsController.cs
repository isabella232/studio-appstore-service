using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Authorization;

namespace AppStoreIntegrationService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[Route("")]
	[Produces(MediaTypeNames.Application.Json)]
	[Authorize]
	public class PluginsController : Controller
	{
		public IPluginRepository PluginRepository { get; set; }

		private readonly IHttpContextAccessor _contextAccessor;

		public PluginsController(IPluginRepository pluginRepository, IHttpContextAccessor contextAccessor)
		{
			PluginRepository = pluginRepository;
			_contextAccessor = contextAccessor;
		}

		[ProducesResponseType(StatusCodes.Status200OK)]
		[AllowAnonymous]
		[ResponseCache(Duration = 540, Location = ResponseCacheLocation.Any,VaryByQueryKeys =new[] {"*"})]
		public async Task<IActionResult> Get([FromQuery]PluginFilter filter)
		{
			List<PluginDetails> pluginsList;
			if (string.IsNullOrEmpty(filter?.SortOrder))
			{
				pluginsList = await PluginRepository.GetAll("asc");
			}
			else
			{
				pluginsList = await PluginRepository.GetAll(filter.SortOrder);
			}

			if (!string.IsNullOrEmpty(filter.Price) || !string.IsNullOrEmpty(filter.Query) || 
				!string.IsNullOrEmpty(filter.StudioVersion) || !string.IsNullOrEmpty(filter.SortOrder))
			{
				var plguins = PluginRepository.SearchPlugins(pluginsList,filter);

				return Ok(plguins);
			}
			return Ok(pluginsList);
		}

		[HttpPut]
		public async Task<IActionResult> PutPrivatePlugin(PrivatePlugin plugin)
		{
			try
			{
				await PluginRepository.UpdatePrivatePlugin(plugin);
				return StatusCode(StatusCodes.Status200OK);
			}catch(Exception ex)
			{
				return Json(new { Success = false, ex.Message });
			}
		}

		[HttpPost("/addPlugin")]
		public async Task<IActionResult> PostAddPlugin(PrivatePlugin plugin)
		{
			try
			{
				await PluginRepository.AddPrivatePlugin(plugin);
				return StatusCode(StatusCodes.Status200OK);
			}
			catch (Exception ex)
			{
				return Json(new { Success = false, ex.Message });
			}
		}

		[HttpGet("/defaultIcon")]
		public IActionResult GetDefaultIcon()
		{
			var scheme = _contextAccessor.HttpContext?.Request?.Scheme;
			var host = _contextAccessor.HttpContext?.Request?.Host.Value;
			var iconPath = $"{scheme}://{host}/images/plugin.ico";
			return Ok(iconPath);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePlugin(int id)
		{
			try
			{
				await PluginRepository.RemovePlugin(id);
				return StatusCode(StatusCodes.Status200OK);
			}
			catch (Exception ex)
			{
				return Json(new { Success = false, ex.Message });
			}
		}
	}
}