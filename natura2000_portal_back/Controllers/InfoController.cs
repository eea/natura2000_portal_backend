using Microsoft.AspNetCore.Mvc;
using natura2000_portal_back.ServiceResponse;
using AutoMapper;
using natura2000_portal_back.Models.ViewModel;
using natura2000_portal_back.Services;

namespace natura2000_portal_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IInfoService _infoService;
        private readonly IMapper _mapper;

        public InfoController(IInfoService controllerInfo, IMapper mapper)
        {
            _infoService = controllerInfo;
            _mapper = mapper;
        }

        [HttpGet("GetOfficialReleases")]
        public async Task<ActionResult<ServiceResponse<List<ReleasesCatalog>>>> GetOfficialReleases()
        {
            var response = new ServiceResponse<List<ReleasesCatalog>>();
            try
            {
                var data = await _infoService.GetOfficialReleases();
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
                response.Data = new List<ReleasesCatalog>();
                return Ok(response);
            }
        }

        [HttpGet("GetParameteredSites")]
        public async Task<ActionResult<ServiceResponse<List<SitesParametered>>>> GetParameteredSites(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive)
        {
            var response = new ServiceResponse<List<SitesParametered>>();
            try
            {
                var data = await _infoService.GetParameteredSites(releaseId, siteType, country, bioregion, site, habitat, species, sensitive);
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
                response.Data = new List<SitesParametered>();
                return Ok(response);
            }
        }

        [HttpGet("GetParameteredHabitats")]
        public async Task<ActionResult<ServiceResponse<List<HabitatsParametered>>>> GetParameteredHabitats(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat)
        {
            var response = new ServiceResponse<List<HabitatsParametered>>();
            try
            {
                var data = await _infoService.GetParameteredHabitats(releaseId, habitatGroup, country, bioregion, habitat);
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
                response.Data = new List<HabitatsParametered>();
                return Ok(response);
            }
        }

        [HttpGet("GetParameteredSpecies")]
        public async Task<ActionResult<ServiceResponse<List<SpeciesParametered>>>> GetParameteredSpecies(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive)
        {
            var response = new ServiceResponse<List<SpeciesParametered>>();
            try
            {
                var data = await _infoService.GetParameteredSpecies(releaseId, speciesGroup, country, bioregion, species, sensitive);
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
                response.Data = new List<SpeciesParametered>();
                return Ok(response);
            }
        }

        [HttpGet("GetLatestReleaseCounters")]
        public async Task<ActionResult<ServiceResponse<ReleaseCounters>>> GetLatestReleaseCounters()
        {
            var response = new ServiceResponse<ReleaseCounters>();
            try
            {
                var data = await _infoService.GetLatestReleaseCounters();
                response.Success = true;
                response.Message = "";
                response.Data = data;
                response.Count = (data == null) ? 0 : 1;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Count = 0;
                response.Data = new ReleaseCounters();
                return Ok(response);
            }
        }
    }
}