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
    }
}