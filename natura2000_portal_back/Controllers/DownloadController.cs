using Microsoft.AspNetCore.Mvc;
using natura2000_portal_back.ServiceResponse;
using AutoMapper;
using natura2000_portal_back.Services;

namespace natura2000_portal_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;
        private readonly IMapper _mapper;

        public DownloadController(IDownloadService controllerDownload, IMapper mapper)
        {
            _downloadService = controllerDownload;
            _mapper = mapper;
        }

        [HttpGet("ComputingSAC")]
        public async Task<ActionResult<ServiceResponse<int>>> ComputingSAC(long releaseId, string email)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var data = await _downloadService.ComputingSAC(releaseId, email);
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = 0;
                return Ok(response);
            }
        }

        [HttpGet("HabitatsSearchResults")]
        public async Task<ActionResult<ServiceResponse<FileContentResult>>> HabitatsSearchResults(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                return await _downloadService.HabitatsSearchResults(releaseId, habitatGroup, country, bioregion, habitat);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }

        [HttpGet("SitesSearchResults")]
        public async Task<ActionResult<ServiceResponse<FileContentResult>>> SitesSearchResults(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                return await _downloadService.SitesSearchResults(releaseId, siteType, country, bioregion, site, habitat, species, sensitive);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }

        [HttpGet("SpeciesSearchResults")]
        public async Task<ActionResult<ServiceResponse<FileContentResult>>> SpeciesSearchResults(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                return await _downloadService.SpeciesSearchResults(releaseId, speciesGroup, country, bioregion, species, sensitive);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }


        [HttpGet("FileFinder")]
        public async Task<ActionResult<ServiceResponse<List<string>>>> FileFinder(string section)
        {
            ServiceResponse<List<string>> response = new();
            try
            {
                var data = await _downloadService.FileFinder(section);
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : data.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }


        [HttpGet("FileDownloader")]
        public async Task<ActionResult<ServiceResponse<FileContentResult>>> FileDownloader(string section, string filename)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                return await _downloadService.FileDownloader(section, filename);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }


        [HttpGet("DownloadFromCwsfiles")]
        public async Task<ActionResult<ServiceResponse<FileContentResult>>> DownloadFromCwsfiles(long? releaseId)
        {
            ServiceResponse<FileContentResult> response = new();
            try
            {
                return await _downloadService.DownloadFromCwsfiles(releaseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = null;
                return Ok(response);
            }
        }





    }
}