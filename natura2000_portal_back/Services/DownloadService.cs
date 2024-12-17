using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using natura2000_portal_back.Data;
using natura2000_portal_back.Models;
using natura2000_portal_back.Models.ViewModel;
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
        private readonly IInfoService _infoService;

        public DownloadService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IOptions<ConfigSettings> app, IInfoService infoService)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _appSettings = app;
            _infoService = infoService;
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
                    @"{{""name"":""VersionId"",""value"":{0}}}," +
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

        public async Task<FileContentResult> HabitatsSearchResults(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat)
        {
            List<HabitatsParametered> data = await _infoService.GetParameteredHabitats(releaseId, habitatGroup, country, bioregion, habitat);

            string tableName = "TempResultsHabitats_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss");
            string query = @"CREATE TABLE [dbo].[" + tableName + @"](
	            [HabitatCode] [nvarchar](6) NULL,
	            [HabitatName] [nvarchar](255) NULL,
	            [SitesNumber] [int] NULL
            )";
            try
            {
                await _releaseContext.Database.ExecuteSqlRawAsync(query);
                foreach (HabitatsParametered line in data)
                {
                    string insertQuery = "INSERT INTO [dbo].[" + tableName + "]([HabitatCode],[HabitatName],[SitesNumber]) VALUES('" + line.HabitatCode + "','" + line.HabitatName + "'," + line.SitesNumber + ")";
                    await _releaseContext.Database.ExecuteSqlRawAsync(insertQuery);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - HabitatsSearchResults", "", _dataContext.Database.GetConnectionString());
            }

            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_results_download, _appSettings.Value.Environment, "sites", tableName, _appSettings.Value.fme_security_token);
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Start HabitatsSearchResults generation"), "DownloadService - HabitatsSearchResults", "", _dataContext.Database.GetConnectionString());
                client.Timeout = TimeSpan.FromHours(5);
                Task<HttpResponseMessage> response = client.GetAsync(serverUrl, HttpCompletionOption.ResponseHeadersRead);
                Stream content = await response.Result.Content.ReadAsStreamAsync(); //  .ReadAsStringAsync();
                string filename = response.Result.Content.Headers.ContentDisposition.FileNameStar;

                return new FileContentResult(StreamToByteArray(content), "application/octet-stream")
                {
                    FileDownloadName = filename
                };
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - HabitatsSearchResults", "", _dataContext.Database.GetConnectionString());
                return null;
            }
            finally
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End HabitatsSearchResults generation"), "DownloadService - HabitatsSearchResults", "", _dataContext.Database.GetConnectionString());
                client.Dispose();
            }
        }

        public async Task<FileContentResult> SitesSearchResults(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive)
        {
            List<SitesParametered> data = await _infoService.GetParameteredSites(releaseId, siteType, country, bioregion, site, habitat, species, sensitive);

            string tableName = "TempResultsSites_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss");
            string query = @"CREATE TABLE [dbo].[" + tableName + @"](
	            [SiteCode] [nvarchar](9) NULL,
	            [SiteName] [nvarchar](250) NULL,
	            [SiteType] [nvarchar](1) NULL,
	            [Area] [nvarchar](50) NULL,
	            [HabitatsNumber] [int] NULL,
	            [SpeciesNumber] [int] NULL,
	            [SensitiveSpecies] [nvarchar](5) NULL
            )";
            try
            {
                await _releaseContext.Database.ExecuteSqlRawAsync(query);
                foreach (SitesParametered line in data)
                {
                    string sensitiveText = line.IsSensitive == true ? "Yes" : "No";
                    string insertQuery = "INSERT INTO [dbo].[" + tableName + "]([SiteCode],[SiteName],[SiteType],[Area],[HabitatsNumber],[SpeciesNumber],[SensitiveSpecies]) VALUES('" + line.SiteCode + "','" + line.SiteName + "','" + line.SiteTypeCode + "'," + line.SiteArea.ToString().Replace(",", ".") + "," + line.HabitatsNumber + "," + line.SpeciesNumber + ",'" + sensitiveText + "')";
                    await _releaseContext.Database.ExecuteSqlRawAsync(insertQuery);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SitesSearchResults", "", _dataContext.Database.GetConnectionString());
            }

            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_results_download, _appSettings.Value.Environment, "sites", tableName, _appSettings.Value.fme_security_token);
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Start SitesSearchResults generation"), "DownloadService - SitesSearchResults", "", _dataContext.Database.GetConnectionString());
                client.Timeout = TimeSpan.FromHours(5);
                Task<HttpResponseMessage> response = client.GetAsync(serverUrl, HttpCompletionOption.ResponseHeadersRead);
                Stream content = await response.Result.Content.ReadAsStreamAsync(); //  .ReadAsStringAsync();
                string filename = response.Result.Content.Headers.ContentDisposition.FileNameStar;

                return new FileContentResult(StreamToByteArray(content), "application/octet-stream")
                {
                    FileDownloadName = filename
                };
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SitesSearchResults", "", _dataContext.Database.GetConnectionString());
                return null;
            }
            finally
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End SitesSearchResults generation"), "DownloadService - SitesSearchResults", "", _dataContext.Database.GetConnectionString());
                client.Dispose();
            }
        }

        public async Task<FileContentResult> SpeciesSearchResults(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive)
        {
            List<SpeciesParametered> data = await _infoService.GetParameteredSpecies(releaseId, speciesGroup, country, bioregion, species, sensitive);

            string tableName = "TempResultsSpecies_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss");
            string query = @"CREATE TABLE [dbo].[" + tableName + @"](
	            [SpeciesCode] [nvarchar](10) NULL,
	            [SpeciesScientificName] [nvarchar](150) NULL,
	            [SpeciesCommonName] [nvarchar](150) NULL,
	            [SpeciesGroup] [nvarchar](255) NULL,
	            [SitesNumber] [int] NULL,
	            [SitesNumberSensitive] [int] NULL,
	            [SensitiveSpecies] [nvarchar](5) NULL
            )";
            try
            {
                await _releaseContext.Database.ExecuteSqlRawAsync(query);
                foreach (SpeciesParametered line in data)
                {
                    string sensitiveText = line.IsSensitive == true ? "Yes" : "No";
                    string insertQuery = "INSERT INTO [dbo].[" + tableName + "]([SpeciesCode],[SpeciesScientificName],[SpeciesCommonName],[SpeciesGroup],[SitesNumber],[SitesNumberSensitive],[SensitiveSpecies]) VALUES('" + line.SpeciesCode + "','" + line.SpeciesScientificName + "','" + line.SpeciesName + "','" + line.SpeciesGroupCode + "'," + line.SitesNumber + "," + line.SitesNumberSensitive + ",'" + sensitiveText + "')";
                    await _releaseContext.Database.ExecuteSqlRawAsync(insertQuery);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SpeciesSearchResults", "", _dataContext.Database.GetConnectionString());
            }

            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_results_download, _appSettings.Value.Environment, "sites", tableName, _appSettings.Value.fme_security_token);
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Start SpeciesSearchResults generation"), "DownloadService - SpeciesSearchResults", "", _dataContext.Database.GetConnectionString());
                client.Timeout = TimeSpan.FromHours(5);
                Task<HttpResponseMessage> response = client.GetAsync(serverUrl, HttpCompletionOption.ResponseHeadersRead);
                Stream content = await response.Result.Content.ReadAsStreamAsync(); //  .ReadAsStringAsync();
                string filename = response.Result.Content.Headers.ContentDisposition.FileNameStar;

                return new FileContentResult(StreamToByteArray(content), "application/octet-stream")
                {
                    FileDownloadName = filename
                };
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SpeciesSearchResults", "", _dataContext.Database.GetConnectionString());
                return null;
            }
            finally
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End SpeciesSearchResults generation"), "DownloadService - SpeciesSearchResults", "", _dataContext.Database.GetConnectionString());
                client.Dispose();
            }
        }

        private byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}