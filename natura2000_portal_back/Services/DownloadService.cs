using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using natura2000_portal_back.Data;
using natura2000_portal_back.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace natura2000_portal_back.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2KReleasesContext _releaseContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public DownloadService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _appSettings = app;
        }

        public async Task<int> ComputingSAC(long releaseId, string email)
        {
            string title = "SAC_Computation_Release_" + releaseId;

            //call the FME in Async mode and do not wait for it.
            //FME will send an email to the user when it´s finished
            HttpClient client = new();
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Launch FME SAC Computation creation", "DownloadService - ComputingSAC", "", _dataContext.Database.GetConnectionString());
                client.Timeout = TimeSpan.FromHours(5);
                string url = string.Format("{0}/fmerest/v3/transformations/submit/{1}/{2}",
                   _appSettings.Value.fme_service_sac_computation.server_url,
                   _appSettings.Value.fme_service_sac_computation.repository,
                   _appSettings.Value.fme_service_sac_computation.workspace);

                string body = string.Format(@"{{""publishedParameters"":[" +
                    @"{{""name"":""ReleaseId"",""value"":{0}}}," +
                    @"{{""name"":""DestDatasetFolder"",""value"":""{1}""}}," +
                    @"{{""name"":""OutputName"",""value"": ""{2}""}}," +
                    @"{{""name"":""Environment"",""value"": ""{3}""}}," +
                    @"{{""name"":""EMail"",""value"": ""{4}""}}]" +
                    @"}}", releaseId, _appSettings.Value.SACComputationDestDatasetFolder, title, _appSettings.Value.Environment, email);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + _appSettings.Value.fme_security_token);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                HttpRequestMessage request = new(HttpMethod.Post, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")//CONTENT-TYPE header
                };

                //call the FME script in async 
                var res = await client.SendAsync(request);
                //get the JobId 
                var json = await res.Content.ReadAsStringAsync();
                JObject jResponse = JObject.Parse(json);
                string jobId = jResponse.GetValue("id").ToString();
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("FME SAC Computation creation launched with jobId:{0}", jobId), "DownloadService - ComputingSAC", "", _dataContext.Database.GetConnectionString());
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("Error Launching FME:{0}", ex.Message), "DownloadService - ComputingSAC", "", _dataContext.Database.GetConnectionString());
                return 0;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}