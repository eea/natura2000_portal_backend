using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using natura2000_portal_back.Data;
using natura2000_portal_back.Hubs;
using natura2000_portal_back.Models;
using natura2000_portal_back.Models.release_db;
using natura2000_portal_back.Models.ViewModel;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using System;
using System.Diagnostics.Metrics;
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
        private readonly IHubContext<ChatHub> _hubContext;

        public DownloadService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IHubContext<ChatHub> hubContext, IOptions<ConfigSettings> app, IInfoService infoService)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _appSettings = app;
            _infoService = infoService;
            _hubContext = hubContext;
        }

        public async Task<int> ComputingSAC(long releaseId, string email)
        {
            string title = "SAC_Computation_Release_" + releaseId;


            string fmeFlowBaseUrl = _appSettings.Value.fme_service_sac_computation.server_url;
            string repository = _appSettings.Value.fme_service_sac_computation.repository;
            string workspace = _appSettings.Value.fme_service_sac_computation.workspace;


            //call the FME in Async mode and do not wait for it.
            //FME will send an email to the user when it´s finished
            HttpClient client = new();
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Launch FME SAC Computation creation", "DownloadService - ComputingSAC", "", _dataContext.Database.GetConnectionString());
                client.Timeout = TimeSpan.FromHours(5);

                // Add headers
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.Add("Authorization", $"fmetoken token={_appSettings.Value.fme_security_token}");
                
                // Prepare request body exactly as in your curl

                string body = string.Format(@"{{
                    ""repository"":""{0}"",
                    ""workspace"":""{1}"",                
                    ""publishedParameters"":{{                     
                    ""VersionId"":{2}, 
                    ""DestDatasetFolder"":""{3}"", 
                    ""OutputName"":""{4}"", 
                    ""Environment"":""{5}"",
                    ""EMail"": ""{6}""
                    }}
                    }}", repository, workspace, releaseId, _appSettings.Value.SACComputationDestDatasetFolder, _appSettings.Value.Environment, "stg", email);


                HttpRequestMessage request = new(HttpMethod.Post, fmeFlowBaseUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")//CONTENT-TYPE header
                };

                //call the FME script in async 
                var res = await client.SendAsync(request);
                //get the JobId 
                var json = await res.Content.ReadAsStringAsync();
                var response_dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);


                string jobId = response_dict["id"].ToString();
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

        public async Task<int> SubmissionComparer(string CountryCode, int VersionFrom, int VersionTo, string email)
        {
            string fmeFlowBaseUrl = _appSettings.Value.fme_service_submission_comparer.server_url;
            string repository = _appSettings.Value.fme_service_submission_comparer.repository;
            string workspace = _appSettings.Value.fme_service_submission_comparer.workspace;

            HttpClient client = new HttpClient();
            try
            {
                client.Timeout = TimeSpan.FromHours(5);
                // Add headers
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.Add("Authorization", $"fmetoken token={_appSettings.Value.fme_security_token}");


                string url = "https://fme.discomap.eea.europa.eu/fmeapiv4/jobs";
                // Prepare request body exactly as in your curl

                string body = string.Format(@"{{
                    ""repository"":""{0}"",
                    ""workspace"":""{1}"",                
                    ""publishedParameters"":{{ 
                    ""CountryCode"":""{2}"", 
                    ""VersionTo"":""{3}"", 
                    ""VersionFrom"":""{4}"", 
                    ""MailNotifier"":""{5}"" 
                    }}
                    }}", repository, workspace, CountryCode,VersionTo ,VersionFrom,email);

                HttpRequestMessage request = new(HttpMethod.Post, fmeFlowBaseUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")//CONTENT-TYPE header
                };

                //call the FME script in async 
                var res = await client.SendAsync(request);
                //get the JobId 
                var json = await res.Content.ReadAsStringAsync();
                var response_dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                string jobId = response_dict["id"].ToString();
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("FME SubmissionComparer Launched with jobId:{0}", jobId), "SubmissionComparer", "", _dataContext.Database.GetConnectionString());
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("Error Launching FME:{0}", ex.Message), "DownloadService - SubmissionComparer", "", _dataContext.Database.GetConnectionString());
                return 0;
            }
            finally
            {
                client.Dispose();
            }
        }
        public async Task<FileContentResult> SpatialDataSDI(long releaseId)
        {                    
            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_spatial_data_sdi, _appSettings.Value.Environment, releaseId, _appSettings.Value.fme_security_token);
            try
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Start SpatialDataSDI generation"), "DownloadService - SpatialDataSDI", "", _dataContext.Database.GetConnectionString());
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SpatialDataSDI", "", _dataContext.Database.GetConnectionString());
                return null;
            }
            finally
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End SpatialDataSDI generation"), "DownloadService - SpatialDataSDI", "", _dataContext.Database.GetConnectionString());
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
            SqlConnection con = new(_releaseContext.Database.GetConnectionString());
            con.Open();
            try
            {
                await _releaseContext.Database.ExecuteSqlRawAsync(query);
                foreach (HabitatsParametered line in data)
                {
                    string insertQuery = "INSERT INTO [dbo].[" + tableName + "]([HabitatCode],[HabitatName],[SitesNumber])" +
                        "VALUES(@HabitatCode,@HabitatName,@SitesNumber)";
                    SqlCommand cmd = new(insertQuery, con);

                    cmd.Parameters.AddWithValue("@HabitatCode", line.HabitatCode ?? "");
                    cmd.Parameters.AddWithValue("@HabitatName", line.HabitatName ?? "");
                    cmd.Parameters.AddWithValue("@SitesNumber", line.SitesNumber);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - HabitatsSearchResults", "", _dataContext.Database.GetConnectionString());
            }
            con.Close();
            con.Dispose();

            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_results_download, _appSettings.Value.Environment, "habitats", tableName, _appSettings.Value.fme_security_token);
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
            SqlConnection con = new(_releaseContext.Database.GetConnectionString());
            con.Open();
            try
            {
                await _releaseContext.Database.ExecuteSqlRawAsync(query);
                foreach (SitesParametered line in data)
                {
                    string sensitiveText = line.IsSensitive == true ? "Yes" : "No";
                    //Added Replace("'", "''") to prevent text from breaking; Added Replace(",", ".") to prevent format issues in excel
                    string insertQuery = "INSERT INTO [dbo].[" + tableName + "]([SiteCode],[SiteName],[SiteType],[Area],[HabitatsNumber],[SpeciesNumber],[SensitiveSpecies])" +
                        "VALUES(@SiteCode,@SiteName,@SiteType,@Area,@HabitatsNumber,@SpeciesNumber,@SensitiveSpecies)";
                    SqlCommand cmd = new(insertQuery, con);

                    cmd.Parameters.AddWithValue("@SiteCode", line.SiteCode ?? "");
                    cmd.Parameters.AddWithValue("@SiteName", line.SiteName ?? "");
                    cmd.Parameters.AddWithValue("@SiteType", line.SiteTypeCode ?? "");
                    cmd.Parameters.AddWithValue("@Area", line.SiteArea.ToString().Replace(",", ".") ?? "");
                    cmd.Parameters.AddWithValue("@HabitatsNumber", line.HabitatsNumber);
                    cmd.Parameters.AddWithValue("@SpeciesNumber", line.SpeciesNumber);
                    cmd.Parameters.AddWithValue("@SensitiveSpecies", sensitiveText);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SitesSearchResults", "", _dataContext.Database.GetConnectionString());
            }
            con.Close();
            con.Dispose();

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
            SqlConnection con = new(_releaseContext.Database.GetConnectionString());
            con.Open();
            try
            {
                await _releaseContext.Database.ExecuteSqlRawAsync(query);
                foreach (SpeciesParametered line in data)
                {
                    string sensitiveText = line.IsSensitive == true ? "Yes" : "No";
                    string insertQuery = "INSERT INTO [dbo].[" + tableName + "]([SpeciesCode],[SpeciesScientificName],[SpeciesCommonName],[SpeciesGroup],[SitesNumber],[SitesNumberSensitive],[SensitiveSpecies])" +
                        "VALUES(@SpeciesCode,@SpeciesScientificName,@SpeciesCommonName,@SpeciesGroup,@SitesNumber,@SitesNumberSensitive,@SensitiveSpecies)";
                    SqlCommand cmd = new(insertQuery, con);

                    cmd.Parameters.AddWithValue("@SpeciesCode", line.SpeciesCode ?? "");
                    cmd.Parameters.AddWithValue("@SpeciesScientificName", line.SpeciesScientificName ?? "");
                    cmd.Parameters.AddWithValue("@SpeciesCommonName", line.SpeciesName ?? "");
                    cmd.Parameters.AddWithValue("@SpeciesGroup", line.SpeciesGroupCode ?? "");
                    cmd.Parameters.AddWithValue("@SitesNumber", line.SitesNumber);
                    cmd.Parameters.AddWithValue("@SitesNumberSensitive", line.SitesNumberSensitive);
                    cmd.Parameters.AddWithValue("@SensitiveSpecies", sensitiveText);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - SpeciesSearchResults", "", _dataContext.Database.GetConnectionString());
            }
            con.Close();
            con.Dispose();

            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_results_download, _appSettings.Value.Environment, "species", tableName, _appSettings.Value.fme_security_token);
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

        public async Task<List<string>> FileFinder(string section)
        {
            List<string> result = new();
            try
            {
                string path = _appSettings.Value.dowloads.base_url + "\\" + section;

                if (Directory.Exists(path))
                {
                    // Recurse into subdirectories of this directory.
                    string[] subdirectoryEntries = Directory.GetDirectories(path);
                    foreach (string subdirectory in subdirectoryEntries)
                    {
                        // Process the list of files found in the directory.
                        DirectoryInfo di = new DirectoryInfo(subdirectory);
                        FileSystemInfo[] files = di.GetFileSystemInfos();
                        List<System.IO.FileSystemInfo> fileEntries = files.OrderBy(f => f.Name).ToList();

                        foreach (System.IO.FileSystemInfo fileName in fileEntries)
                            result.Add(Path.GetFileName(subdirectory) + "\\" + Path.GetFileName(fileName.Name));
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - FileFinder", "", _dataContext.Database.GetConnectionString());
                return null;
            }
        }

        public async Task<FileContentResult> FileDownloader(string section, string filename)
        {
            string path = _appSettings.Value.dowloads.base_url + "\\" + section + "\\" + filename;
            try
            {
                var file_bytes = await System.IO.File.ReadAllBytesAsync(path);
                return new FileContentResult(file_bytes, "application/octet-stream")
                {
                    FileDownloadName = Path.GetFileName(filename)
                };
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - FileDownloader", "", _dataContext.Database.GetConnectionString());
                return null;
            }
        }

        public async Task<FileContentResult> DownloadFromCwsfiles(long? releaseId)
        {
            try
            {
                string path_name = @"\\cwsfileserver.eea.dmz1\projects\Nature\Biodiversity\Natura2000Backbone\Releases\Official release end2021\Official release end2021_MDB_Public.zip";
                //path_name = string.Format(@"C:\Proyectos\N2kBackbone\Code\{0}.txt", userName);


                var file_bytes = await System.IO.File.ReadAllBytesAsync(path_name);
                return new FileContentResult(file_bytes, "application/octet-stream")
                {
                    FileDownloadName = "Official release end2021_MDB_Public.zip"
                    //FileDownloadName = string.Format("{0}.zip",userName)
                };
            }
            catch
            {
                throw;
                //await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DownloadService - DownloadFromCwsfiles", "", _dataContext.Database.GetConnectionString());
                //return null;
            }
            finally
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End DownloadFromCwsfiles"), "DownloadService - SpeciesSearchResults", "", _dataContext.Database.GetConnectionString());

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