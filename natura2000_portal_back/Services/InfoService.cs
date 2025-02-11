using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using natura2000_portal_back.Data;
using natura2000_portal_back.Models;
using natura2000_portal_back.Models.release_db;
using natura2000_portal_back.Models.ViewModel;

namespace natura2000_portal_back.Services
{
    public class InfoService : IInfoService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2KReleasesContext _releaseContext;

        public InfoService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
        }

        public async Task<List<ReleasesCatalog>> GetOfficialReleases(Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated)
        {
            try
            {
                List<ReleaseVisibility> releaseVisibility = await _releaseContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync();
                #region filters
                if (initialValidation)
                    releaseVisibility = releaseVisibility.Where(w => w.InitialValidation == true).ToList();
                if (internalViewers)
                    releaseVisibility = releaseVisibility.Where(w => w.InternalViewers == true).ToList();
                if (internalBarometer)
                    releaseVisibility = releaseVisibility.Where(w => w.InternalBarometer == true).ToList();
                if (internalPortalSDFSensitive)
                    releaseVisibility = releaseVisibility.Where(w => w.InternalPortalSDFSensitive == true).ToList();
                if (publicViewers)
                    releaseVisibility = releaseVisibility.Where(w => w.PublicViewers == true).ToList();
                if (publicBarometer)
                    releaseVisibility = releaseVisibility.Where(w => w.PublicBarometer == true).ToList();
                if (sdfPublic)
                    releaseVisibility = releaseVisibility.Where(w => w.SDFPublic == true).ToList();
                if (naturaOnlineList)
                    releaseVisibility = releaseVisibility.Where(w => w.NaturaOnlineList == true).ToList();
                if (productsCreated)
                    releaseVisibility = releaseVisibility.Where(w => w.ProductsCreated == true).ToList();
                if (jediDimensionCreated)
                    releaseVisibility = releaseVisibility.Where(w => w.JediDimensionCreated == true).ToList();
                #endregion
                List<long> releaseVisibilityIDs = releaseVisibility.Select(s => s.ReleaseID).ToList();

                return await _releaseContext.Set<Releases>()
                    .Where(w => w.Final == true && releaseVisibilityIDs.Contains(w.ID))
                    .AsNoTracking()
                    .Select(c => new ReleasesCatalog
                    {
                        ReleaseId = c.ID,
                        ReleaseName = c.Title,
                        ReleaseDate = c.CreateDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetOfficialReleases", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SitesParametered>> GetParameteredSites(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive)
        {
            try
            {
                if (releaseId == null)
                    releaseId = await _releaseContext.Set<Releases>().OrderByDescending(o => o.CreateDate).Select(s => s.ID).FirstOrDefaultAsync();

                SqlParameter param1 = new("@releaseId", releaseId);
                List<SitesParameteredExtended> result = await _releaseContext.Set<SitesParameteredExtended>().FromSqlRaw($"exec dbo.GetParameteredSites @releaseId", param1).AsNoTracking().ToListAsync();

                if (siteType != null)
                {
                    result = result.Where(w => (w.SiteTypeCode != null && siteType.ToLower().Contains(w.SiteTypeCode.ToLower()))).ToList();
                }
                if (country != null)
                {
                    result = result.Where(w => country.ToLower().Contains(w.SiteCode.ToLower()[..2])).ToList();
                }
                if (bioregion != null)
                {
                    List<string> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(w => bioregion.Contains(w.BioRegionShortCode)).Select(s => s.RefBioGeoName).ToListAsync();
                    if (bioRegionTypes.Any())
                        result = result.Where(w => bioRegionTypes.Contains(w.BioRegion)).ToList();
                }
                if (site != null)
                {
                    result = result.Where(w => w.SiteCode.ToLower().Contains(site.ToLower())
                        || w.SiteName.ToLower().Contains(site.ToLower())).ToList();
                }
                if (habitat != null)
                {
                    result = result.Where(w => (w.HABITATCODE != null && w.HABITATCODE.ToLower().Contains(habitat.ToLower()))
                        || (w.HABITATNAME != null && w.HABITATNAME.ToLower().Contains(habitat.ToLower()))).ToList();
                }
                if (species != null)
                {
                    result = result.Where(w => (w.SPECIESCODE != null && w.SPECIESCODE.ToLower().Contains(species.ToLower()))
                        || (w.SPECIESNAME != null && w.SPECIESNAME.ToLower().Contains(species.ToLower()))).ToList();
                }
                if (sensitive != null && sensitive == true)
                {
                    result = result.Where(w => w.IsSensitive != null && w.IsSensitive == true).ToList();
                }

                List<SitesParametered> resultFinal = result.Select(c => new SitesParametered
                {
                    SiteCode = c.SiteCode,
                    SiteName = c.SiteName,
                    SiteTypeCode = c.SiteTypeCode,
                    SiteArea = c.SiteArea,
                    HabitatsNumber = c.HabitatsNumber,
                    SpeciesNumber = c.SpeciesNumber,
                    IsSensitive = c.IsSensitive == null ? false : c.IsSensitive
                }).ToList();

                return resultFinal.DistinctBy(d => d.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetParameteredSites", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<HabitatsParametered>> GetParameteredHabitats(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat)
        {
            try
            {
                if (releaseId == null)
                    releaseId = await _releaseContext.Set<Releases>().OrderByDescending(o => o.CreateDate).Select(s => s.ID).FirstOrDefaultAsync();

                SqlParameter param1 = new("@releaseId", releaseId);
                List<HabitatsParameteredExtended> result = await _releaseContext.Set<HabitatsParameteredExtended>().FromSqlRaw($"exec dbo.GetParameteredHabitats @releaseId", param1).AsNoTracking().ToListAsync();

                if (habitatGroup != null)
                {
                    result = result.Where(w => w.HabitatCode != null && w.HabitatCode[0].ToString() == habitatGroup).ToList();
                }
                if (country != null)
                {
                    result = result.Where(w => country.ToLower().Contains(w.Country.ToLower())).ToList();
                }
                if (bioregion != null)
                {
                    List<string> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(w => bioregion.Contains(w.BioRegionShortCode)).Select(s => s.RefBioGeoName).ToListAsync();
                    if (bioRegionTypes.Any())
                        result = result.Where(w => bioRegionTypes.Contains(w.BioRegion)).ToList();
                }
                if (habitat != null)
                {
                    result = result.Where(w => (w.HabitatCode != null && w.HabitatCode.ToLower().Contains(habitat.ToLower()))
                        || (w.HabitatName != null && w.HabitatName.ToLower().Contains(habitat.ToLower()))).ToList();
                }

                result = result.DistinctBy(d => new { d.HabitatCode, d.Country }).ToList();

                List<HabitatsParametered> resultFinal = new List<HabitatsParametered>();

                foreach (HabitatsParameteredExtended c in result)
                {
                    int sitesNumber = result.Where(w => w.HabitatCode == c.HabitatCode).Sum(s => Convert.ToInt32(s.SitesNumber));
                    resultFinal.Add(new HabitatsParametered
                    {
                        HabitatCode = c.HabitatCode,
                        HabitatName = c.HabitatName,
                        HabitatImageUrl = c.HabitatImageUrl,
                        SitesNumber = sitesNumber
                    });
                }

                return resultFinal.DistinctBy(d => d.HabitatCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetParameteredHabitats", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SpeciesParametered>> GetParameteredSpecies(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive)
        {
            try
            {
                if (releaseId == null)
                    releaseId = await _releaseContext.Set<Releases>().OrderByDescending(o => o.CreateDate).Select(s => s.ID).FirstOrDefaultAsync();

                SqlParameter param1 = new("@releaseId", releaseId);
                List<SpeciesParameteredExtended> result = await _releaseContext.Set<SpeciesParameteredExtended>().FromSqlRaw($"exec dbo.GetParameteredSpecies @releaseId", param1).AsNoTracking().ToListAsync();

                if (speciesGroup != null)
                {
                    result = result.Where(w => w.SpeciesGroupCode != null && w.SpeciesGroupCode.ToLower() == speciesGroup.ToLower()).ToList();
                }
                if (country != null)
                {
                    result = result.Where(w => country.ToLower().Contains(w.Country.ToLower())).ToList();
                }
                if (bioregion != null)
                {
                    List<string> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(w => bioregion.Contains(w.BioRegionShortCode)).Select(s => s.RefBioGeoName).ToListAsync();
                    if (bioRegionTypes.Any())
                        result = result.Where(w => bioRegionTypes.Contains(w.BioRegion)).ToList();
                }
                if (species != null)
                {
                    result = result.Where(w => (w.SpeciesCode != null && w.SpeciesCode.ToLower().Contains(species.ToLower()))
                        || (w.SpeciesName != null && w.SpeciesName.ToLower().Contains(species.ToLower()))).ToList();
                }
                if (sensitive != null && sensitive == true)
                {
                    result = result.Where(w => w.IsSensitive != null && w.IsSensitive == true).ToList();
                }

                result = result.DistinctBy(d => new { d.SpeciesCode, d.Country }).ToList();

                List<SpeciesParametered> resultFinal = new List<SpeciesParametered>();

                foreach (SpeciesParameteredExtended c in result)
                {
                    int sitesNumber = result.Where(w => w.SpeciesCode == c.SpeciesCode).Sum(s => Convert.ToInt32(s.SitesNumber));
                    int sitesNumberSensitive = result.Where(w => w.SpeciesCode == c.SpeciesCode).Sum(s => Convert.ToInt32(s.SitesNumberSensitive));
                    resultFinal.Add(new SpeciesParametered
                    {
                        SpeciesCode = c.SpeciesCode,
                        SpeciesName = c.SpeciesName,
                        SpeciesScientificName = c.SpeciesScientificName,
                        SpeciesGroupCode = c.SpeciesGroupCode,
                        SpeciesEunisId = c.SpeciesEunisId,
                        SpeciesImageUrl = c.SpeciesImageUrl,
                        IsSensitive = c.IsSensitive == null ? false : c.IsSensitive,
                        SitesNumber = sitesNumber,
                        SitesNumberSensitive = sitesNumberSensitive
                    });
                }

                return resultFinal.DistinctBy(d => d.SpeciesCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetParameteredSpecies", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<ReleaseCounters> GetLatestReleaseCounters()
        {
            try
            {
                Releases release = await _releaseContext.Set<Releases>().Where(w => w.Final == true).OrderByDescending(o => o.CreateDate).AsNoTracking().FirstOrDefaultAsync();

                //List<NATURA2000SITES> sites = await _releaseContext.Set<NATURA2000SITES>().Where(w => w.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<string> sites = await _releaseContext.Set<NATURA2000SITES>().Where(w => w.ReleaseId == release.ID).Select(s => s.SITECODE.ToUpper()).AsNoTracking().ToListAsync();

                //List<HABITATS> habitats = await _releaseContext.Set<HABITATS>().Where(w => w.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<string> habitats = await _releaseContext.Set<HABITATS>().Where(w => w.ReleaseId == release.ID).Select(s => s.HABITATCODE.ToUpper()).AsNoTracking().ToListAsync();

                //List<SPECIES> species = await _releaseContext.Set<SPECIES>().Where(w => w.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<string> species = await _releaseContext.Set<SPECIES>().Where(w => w.ReleaseId == release.ID).Select(s => s.SPECIESCODE.ToUpper()).AsNoTracking().ToListAsync();

                ReleaseCounters result = new()
                {
                    SitesNumber = sites.Distinct().Count(),
                    HabitatsNumber = habitats.Distinct().Count(),
                    SpeciesNumber = species.Distinct().Count(),
                    ReleaseDate = release.CreateDate
                };

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetLatestReleaseCounters", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}