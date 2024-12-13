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
        public async Task<ActionResult<ServiceResponse<int>>> HabitatsSearchResults(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var data = await _downloadService.HabitatsSearchResults(releaseId, habitatGroup, country, bioregion, habitat);
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

        [HttpGet("SitesSearchResults")]
        public async Task<ActionResult<ServiceResponse<int>>> SitesSearchResults(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var data = await _downloadService.SitesSearchResults(releaseId, siteType, country, bioregion, site, habitat, species, sensitive);
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

        [HttpGet("SpeciesSearchResults")]
        public async Task<ActionResult<ServiceResponse<int>>> SpeciesSearchResults(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var data = await _downloadService.SpeciesSearchResults(releaseId, speciesGroup, country, bioregion, species, sensitive);
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
    }
}