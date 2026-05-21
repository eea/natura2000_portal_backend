using Microsoft.EntityFrameworkCore;
using natura2000_portal_back.Data;
using natura2000_portal_back.Models.ViewModel;
using Microsoft.Extensions.Options;
using natura2000_portal_back.Models;
using natura2000_portal_back.Models.backbone_db;
using natura2000_portal_back.Models.release_db;

namespace natura2000_portal_back.Services
{
    public class SDFService : ISDFService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2KReleasesLegacyContext _releaseLegacyContext;
        private readonly N2KReleasesContext _releaseContext;
        private readonly N2K_ProductionContext _productionContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public SDFService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, N2KReleasesLegacyContext releaseLegacyContext, N2K_ProductionContext productionContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _releaseLegacyContext = releaseLegacyContext;
            _productionContext = productionContext;
            _appSettings = app;
        }

        public async Task<long> GetLastReleaseId(string SiteCode, int ReleaseId, Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated, bool showSensitive = true)
        {
            try
            {
                long result = -1;

                List<ReleaseVisibility> releaseVisibility = await _releaseContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync();
                releaseVisibility.AddRange(await _releaseLegacyContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync());
                #region filters
                if (initialValidation)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InitialValidation == true)];
                if (internalViewers)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalViewers == true)];
                if (internalBarometer)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalBarometer == true)];
                if (internalPortalSDFSensitive)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalPortalSDFSensitive == true)];
                if (publicViewers)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.PublicViewers == true)];
                if (publicBarometer)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.PublicBarometer == true)];
                if (sdfPublic)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.SDFPublic == true)];
                if (naturaOnlineList)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.NaturaOnlineList == true)];
                if (productsCreated)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.ProductsCreated == true)];
                if (jediDimensionCreated)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.JediDimensionCreated == true)];
                #endregion
                List<long> releaseVisibilityIDs = [.. releaseVisibility.Select(s => s.ReleaseID)];

                if (releaseVisibilityIDs == null || releaseVisibilityIDs.Count == 0)
                    return result;

                List<ReleasesExtended> releases = await _releaseContext.Set<Releases>().Where(w => releaseVisibilityIDs.Contains(w.ID)).AsNoTracking()
                    .Select(r => new ReleasesExtended
                    {
                        ID = r.ID,
                        Title = r.Title,
                        Author = r.Author,
                        CreateDate = r.CreateDate,
                        Final = r.Final,
                        ModifyUser = r.ModifyUser,
                        ModifyDate = r.ModifyDate,
                        Character = r.Character,
                        Legacy = false
                    })
                    .ToListAsync();
                releases.AddRange(await _releaseLegacyContext.Set<Releases>().Where(w => releaseVisibilityIDs.Contains(w.ID)).AsNoTracking()
                    .Select(r => new ReleasesExtended
                    {
                        ID = r.ID,
                        Title = r.Title,
                        Author = r.Author,
                        CreateDate = r.CreateDate,
                        Final = r.Final,
                        ModifyUser = r.ModifyUser,
                        ModifyDate = r.ModifyDate,
                        Character = r.Character,
                        Legacy = true
                    })
                    .ToListAsync());
                Releases? release = null;

                List<Natura2000Sites> sites = await _releaseContext.Set<Natura2000Sites>().Where(a => a.F_1_2_site_code == SiteCode).ToListAsync();
                List<Natura2000Sites> sitesFilter = [.. sites.Where(a => a.F_1_2_site_code == SiteCode && releaseVisibilityIDs.Contains(a.releaseId))];
                List<NATURA2000SITES> sitesLegacy = await _releaseLegacyContext.Set<NATURA2000SITES>().Where(a => a.SITECODE == SiteCode).ToListAsync();
                List<NATURA2000SITES> sitesFilterLegacy = [.. sitesLegacy.Where(a => a.SITECODE == SiteCode && releaseVisibilityIDs.Contains(a.ReleaseId))];

                if (sitesFilter.Any() || sitesFilterLegacy.Any()) //If the site is included in a Release that complies with the filters we fetch the Site data
                {
                    if (ReleaseId == -1)
                    {
                        List<long> siteReleaseIDs = [.. sites.Select(s => s.releaseId)];
                        siteReleaseIDs.AddRange([.. sitesLegacy.Select(s => s.ReleaseId)]);
                        release = releases.Where(w => siteReleaseIDs.Contains(w.ID)).OrderBy(r => r.CreateDate).Last();
                        result = release.ID;
                    }
                    else
                    {
                        result = ReleaseId;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetLastReleaseId", "", _dataContext.Database.GetConnectionString());
                throw;
            }
        }

        public async Task<ReleaseSDF> GetReleaseData(string SiteCode, int ReleaseId, Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated, bool showSensitive = true)
        {
            try
            {
                string booleanTrue = "Yes";
                string booleanFalse = "No";
                string booleanChecked = "x";
                string booleanUnchecked = "";
                ReleaseSDF result = new();

                List<ReleaseVisibility> releaseVisibility = await _releaseContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync();
                releaseVisibility.AddRange(await _releaseLegacyContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync());
                #region filters
                if (initialValidation)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InitialValidation == true)];
                if (internalViewers)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalViewers == true)];
                if (internalBarometer)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalBarometer == true)];
                if (internalPortalSDFSensitive)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalPortalSDFSensitive == true)];
                if (publicViewers)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.PublicViewers == true)];
                if (publicBarometer)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.PublicBarometer == true)];
                if (sdfPublic)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.SDFPublic == true)];
                if (naturaOnlineList)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.NaturaOnlineList == true)];
                if (productsCreated)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.ProductsCreated == true)];
                if (jediDimensionCreated)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.JediDimensionCreated == true)];
                #endregion
                List<long> releaseVisibilityIDs = [.. releaseVisibility.Select(s => s.ReleaseID)];

                if (releaseVisibilityIDs == null || releaseVisibilityIDs.Count == 0)
                    return result;

                List<ReleasesExtended> releases = await _releaseContext.Set<Releases>().Where(w => releaseVisibilityIDs.Contains(w.ID)).AsNoTracking()
                    .Select(r => new ReleasesExtended
                    {
                        ID = r.ID,
                        Title = r.Title,
                        Author = r.Author,
                        CreateDate = r.CreateDate,
                        Final = r.Final,
                        ModifyUser = r.ModifyUser,
                        ModifyDate = r.ModifyDate,
                        Character = r.Character,
                        Legacy = false
                    })
                    .ToListAsync();
                releases.AddRange(await _releaseLegacyContext.Set<Releases>().Where(w => releaseVisibilityIDs.Contains(w.ID)).AsNoTracking()
                    .Select(r => new ReleasesExtended
                    {
                        ID = r.ID,
                        Title = r.Title,
                        Author = r.Author,
                        CreateDate = r.CreateDate,
                        Final = r.Final,
                        ModifyUser = r.ModifyUser,
                        ModifyDate = r.ModifyDate,
                        Character = r.Character,
                        Legacy = true
                    })
                    .ToListAsync());
                Releases? release = null;

                List<Natura2000Sites> sites = await _releaseContext.Set<Natura2000Sites>().Where(a => a.F_1_2_site_code == SiteCode).ToListAsync();
                List<Natura2000Sites> sitesFilter = [.. sites.Where(a => a.F_1_2_site_code == SiteCode && releaseVisibilityIDs.Contains(a.releaseId))];
                Natura2000Sites? site = null;

                if (sitesFilter.Count != 0) //If the site is included in a Release that complies with the filters we fetch the Site data
                {
                    if (ReleaseId == -1)
                    {
                        List<long> siteReleaseIDs = [.. sites.Select(s => s.releaseId)];
                        release = releases.Where(w => siteReleaseIDs.Contains(w.ID) && w.Legacy == false).OrderBy(r => r.CreateDate).Last();
                        site = sites.Where(a => a.F_1_2_site_code == SiteCode && a.releaseId == release.ID).FirstOrDefault();
                    }
                    else
                    {
                        release = releases.Where(r => r.ID == ReleaseId && r.Legacy == false).FirstOrDefault();
                        site = sites.Where(a => a.F_1_2_site_code == SiteCode && a.releaseId == ReleaseId).FirstOrDefault();
                    }
                }

                if (release == null || site == null)
                    return result;

                //Catalogues
                List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();
                List<Countries> countries = await _dataContext.Set<Countries>().AsNoTracking().ToListAsync();
                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();
                List<SpeciesGroups> speciesGroups = await _dataContext.Set<SpeciesGroups>().AsNoTracking().ToListAsync();

                //Data
                List<Models.release_db.Habitats> habitats = await _releaseContext.Set<Models.release_db.Habitats>().Where(h => h.site_code == SiteCode && h.releaseId == release.ID).AsNoTracking().ToListAsync();
                List<Models.release_db.Species> species = await _releaseContext.Set<Models.release_db.Species>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID && (!(a.F_3_2_4_species_sensitive ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<OtherSpecies> speciesOther = await _releaseContext.Set<OtherSpecies>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID && (!(a.F_3_3_4_species_sensitive ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<BiogeographicalMarineRegions> bioRegions = await _releaseContext.Set<BiogeographicalMarineRegions>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                ManagementBody? managementBody = await _releaseContext.Set<ManagementBody>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().FirstOrDefaultAsync();
                List<Models.release_db.AdministrativeRegion> administrativeRegion = await _releaseContext.Set<Models.release_db.AdministrativeRegion>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                List<Pressure> pressures = await _releaseContext.Set<Pressure>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                List<ManagementInfo> managementInfo = await _releaseContext.Set<ManagementInfo>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                List<ManagementPlanList> managementPlanList = await _releaseContext.Set<ManagementPlanList>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();


                //List<DescribeSites> describeSites = await _releaseContext.Set<DescribeSites>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                //List<IsImpactedBy> isImpactedBy = await _releaseContext.Set<IsImpactedBy>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                //List<SiteOwnerType> siteOwnerType = await _releaseContext.Set<SiteOwnerType>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                //List<DocumentationLinks> documentationLinks = await _releaseContext.Set<DocumentationLinks>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                //List<HasNationalProtection> hasNationalProtection = await _releaseContext.Set<HasNationalProtection>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                //List<DetailedProtectionStatus> detailedProtectionStatus = await _releaseContext.Set<DetailedProtectionStatus>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().ToListAsync();
                //ReferenceMap referenceMap = await _releaseContext.Set<ReferenceMap>().Where(a => a.site_code == SiteCode && a.releaseId == release.ID).AsNoTracking().FirstOrDefaultAsync();

                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.F_1_3_site_name;
                    result.SiteInfo.SiteType = site.F_1_1_site_type;
                    result.SiteInfo.SiteCode = SiteCode;
                }
                sites.ForEach(st =>
                {
                    if (releaseVisibilityIDs.Contains(st.releaseId))
                    {
                        ReleaseInfo temp = new()
                        {
                            ReleaseId = st.releaseId,
                            ReleaseName = releases.Where(w => w.ID == st.releaseId).Select(s => s.Title).FirstOrDefault(),
                            ReleaseDate = releases.Where(w => w.ID == st.releaseId).Select(s => s.CreateDate).FirstOrDefault(),
                            Legacy = releases.Where(w => w.ID == st.releaseId).Select(s => s.Legacy).FirstOrDefault()
                        };
                        result.SiteInfo.Releases?.Add(temp);
                    }
                });
                #endregion

                #region SiteIdentification
                if (site != null)
                {
                    result.SiteIdentification.F_1_1_site_type = site.F_1_1_site_type;
                    result.SiteIdentification.F_1_2_site_code = SiteCode;
                    result.SiteIdentification.F_1_3_site_name = site.F_1_3_site_name;
                    result.SiteIdentification.F_1_3_1_site_name_nonLatin = site.F_1_3_1_site_name_nonLatin;
                    result.SiteIdentification.F_1_4_1_respond_name = site.F_1_4_1_respond_name;
                    result.SiteIdentification.F_1_4_2_respond_contact = site.F_1_4_2_respond_contact;
                    result.SiteIdentification.F_1_4_3_respond_address = site.F_1_4_3_respond_address;
                    result.SiteIdentification.F_1_4_4_respond_email = site.F_1_4_4_respond_email;
                    result.SiteIdentification.F_1_4_5_respond_URI = site.F_1_4_5_respond_URI;
                    result.SiteIdentification.F_1_5_1_SPA_date = site.F_1_5_1_SPA_date;
                    result.SiteIdentification.F_1_5_2_a_SPA_act_URI = site.F_1_5_2_a_SPA_act_URI;
                    result.SiteIdentification.F_1_5_2_b_SPA_act_freetext = site.F_1_5_2_b_SPA_act_freetext;
                    result.SiteIdentification.F_1_5_3_pSCI_date = site.F_1_5_3_pSCI_date;
                    result.SiteIdentification.cSCI_date = site.cSCI_date;
                    result.SiteIdentification.F_1_5_4_SAC_date = site.F_1_5_4_SAC_date;
                    result.SiteIdentification.F_1_5_5_a_SAC_act_URI = site.F_1_5_5_a_SAC_act_URI;
                    result.SiteIdentification.F_1_5_5_b_SAC_act_freetext = site.F_1_5_5_b_SAC_act_freetext;
                    result.SiteIdentification.F_1_5_6_site_text = site.F_1_5_6_site_text;
                }
                #endregion

                #region SiteLocation
                if (site != null)
                {
                    result.SiteLocation.longitude = site.longitude;
                    result.SiteLocation.latitude = site.latitude;
                    result.SiteLocation.F_2_1_1_site_area = site.F_2_1_1_site_area;
                    result.SiteLocation.F_2_1_2_a_area_diff = site.F_2_1_2_a_area_diff != null && site.F_2_1_2_a_area_diff != "" && site.F_2_1_2_a_area_diff == "Yes" ? site.F_2_1_2_b_area_diff_reason : null;
                    result.SiteLocation.F_2_1_3_a_area_diff_text = site.F_2_1_3_a_area_diff_text;
                }
                if (administrativeRegion != null && administrativeRegion.Count > 0)
                {
                    administrativeRegion.ForEach(nbs =>
                    {
                        Models.ViewModel.AdministrativeRegion temp = new()
                        {
                            F_2_2_1_site_nutscode = nbs.F_2_2_1_site_nutscode,
                            F_2_2_2_site_nutsname = nbs.F_2_2_2_site_nutsname
                        };
                        result.SiteLocation.F_2_2_administrative_region.Add(temp);
                    });
                }
                if (bioRegions != null && bioRegions.Count > 0)
                {
                    bioRegions.ForEach(br =>
                    {
                        BiogeographicalRegions temp = new()
                        {
                            F_2_3_1_biogeo_marine_code = br.F_2_3_1_biogeo_marine_code,
                            F_2_3_2_biogeo_marine_percent = br.F_2_3_2_biogeo_marine_percent,
                            F_2_3_3_marine_relationship_biogeo = br.F_2_3_3_marine_relationship_biogeo
                        };
                        result.SiteLocation.F_2_3_biogeographical_regions.Add(temp);
                    });
                }
                #endregion

                #region EcologicalInformation
                if (habitats != null && habitats.Count > 0)
                {
                    habitats.ForEach(h =>
                    {
                        EssentialInformationHabitat temp = new()
                        {
                            F_3_1_1_habitat_code = h.F_3_1_1_habitat_code,
                            habitat_name = h.habitat_name,
                            F_3_1_4_habitat_cover = h.F_3_1_4_habitat_cover,
                            F_3_1_5_habitat_caves = !String.IsNullOrEmpty(h.F_3_1_5_habitat_caves) && !h.F_3_1_5_habitat_caves.Replace(".00", "").Equals("0") ? Int32.Parse(h.F_3_1_5_habitat_caves.Replace(".00", "")) : null,
                            F_3_1_6_habitat_method = h.F_3_1_6_habitat_method
                        };
                        if (h.F_3_1_2_habitat_priority != null)
                            temp.F_3_1_2_habitat_priority = (h.F_3_1_2_habitat_priority == true) ? booleanTrue : booleanFalse;
                        if (h.F_3_1_3_habitat_notPresent != null)
                            temp.F_3_1_3_not_present = (h.F_3_1_3_habitat_notPresent == 1) ? booleanChecked : booleanUnchecked;
                        if (h.F_3_1_7_a_habitat_startdate != null || h.F_3_1_7_b_habitat_enddate != null)
                        {
                            temp.F_3_1_7_habitat_date = h.F_3_1_7_a_habitat_startdate != null && h.F_3_1_7_a_habitat_startdate != "" ? DateOnly.ParseExact(h.F_3_1_7_a_habitat_startdate, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture).ToString() : null + " - " + h.F_3_1_7_b_habitat_enddate != null && h.F_3_1_7_b_habitat_enddate != "" ? DateOnly.ParseExact(h.F_3_1_7_b_habitat_enddate ?? string.Empty, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture).ToString() : null;

                        }
                        else if (h.F_3_1_7_c_habitat_before2022 != null && h.F_3_1_7_c_habitat_before2022 == "True")
                        {
                            temp.F_3_1_7_habitat_date = "Survey older than 2022";
                        }
                        result.EcologicalInformation.F_3_1_a_essential_information.Add(temp);
                    });
                    result.EcologicalInformation.F_3_1_a_essential_information = [.. result.EcologicalInformation.F_3_1_a_essential_information.OrderBy(o => o.F_3_1_1_habitat_code)];
                }
                if (habitats != null && habitats.Count > 0)
                {
                    habitats.ForEach(h =>
                    {
                        SiteAssessmentHabitat temp = new()
                        {
                            F_3_1_1_habitat_code = h.F_3_1_1_habitat_code,
                            habitat_name = h.habitat_name,
                            F_3_1_8_habitat_significance = h.F_3_1_8_habitat_significance,
                            F_3_1_9_habitat_representivity = h.F_3_1_9_habitat_representivity,
                            F_3_1_10_habitat_relative_surface = h.F_3_1_10_habitat_relative_surface,
                            F_3_1_12_1_habitat_conserv_degree = h.F_3_1_12_1_habitat_conserv_degree,
                            F_3_1_12_3_habitat_conserv_method = h.F_3_1_12_3_habitat_conserv_method,
                            F_3_1_13_habitat_objectives = h.F_3_1_13_habitat_objectives,
                            F_3_1_14_a_habitat_objectives_text = h.F_3_1_14_a_habitat_objectives_text,
                            F_3_1_15_habitat_global = h.F_3_1_15_habitat_global,
                            F_3_1_16_habitat_date = h.F_3_1_16_habitat_date != null && h.F_3_1_16_habitat_date != "" ? DateOnly.ParseExact(h.F_3_1_16_habitat_date, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture) : null
                        };
                        if (h.F_3_1_12_2_a_habitat_area_condition_good != null || h.F_3_1_12_2_b_habitat_area_condition_notgood != null || h.F_3_1_12_2_c_habitat_area_condition_unknown != null)
                            temp.F_3_1_12_2_habitat_area_condition = h.F_3_1_12_2_a_habitat_area_condition_good + ", " + h.F_3_1_12_2_b_habitat_area_condition_notgood + ", " + h.F_3_1_12_2_c_habitat_area_condition_unknown;
                        if (h.F_3_1_2_habitat_priority != null)
                            temp.F_3_1_2_habitat_priority = (h.F_3_1_2_habitat_priority == true) ? booleanTrue : booleanFalse;
                        result.EcologicalInformation.F_3_1_b_site_assessment.Add(temp);
                    });
                    result.EcologicalInformation.F_3_1_b_site_assessment = [.. result.EcologicalInformation.F_3_1_b_site_assessment.OrderBy(o => o.F_3_1_1_habitat_code)];
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        EssentialInformationSpecies temp = new()
                        {
                            F_3_2_1_species_group = h.F_3_2_1_species_group,
                            F_3_2_2_species_code = h.F_3_2_2_species_code,
                            F_3_2_3_species_name = h.F_3_2_3_species_name,
                            F_3_2_6_species_poptype = h.F_3_2_6_species_poptype,
                            F_3_2_7_1_a_species_pop_min = !h.F_3_2_7_1_a_species_pop_min.Equals(0) ? h.F_3_2_7_1_a_species_pop_min : null,
                            F_3_2_7_1_b_species_pop_max = !h.F_3_2_7_1_b_species_pop_max.Equals(0) ? h.F_3_2_7_1_b_species_pop_max : null,
                            F_3_2_7_2_species_pop_unit = h.F_3_2_7_2_species_pop_unit,
                            F_3_2_8_species_abundance = h.F_3_2_8_species_abundance,
                            F_3_2_9_species_method = h.F_3_2_9_species_method
                        };
                        if (h.F_3_2_4_species_sensitive != null)
                            temp.F_3_2_4_species_sensitive = (h.F_3_2_4_species_sensitive == true) ? booleanTrue : booleanUnchecked;
                        if (h.F_3_2_5_species_notPresent != null)
                            temp.F_3_2_5_species_NP = (h.F_3_2_5_species_notPresent == true) ? booleanChecked : booleanUnchecked;
                        if (h.F_3_2_10_a_species_startdate != null || h.F_3_2_10_b_species_enddate != null)
                        {
                            temp.F_3_2_10_species_date = h.F_3_2_10_a_species_startdate != null && h.F_3_2_10_a_species_startdate != "" ? DateOnly.ParseExact(h.F_3_2_10_a_species_startdate, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture).ToString() : null + " - " + h.F_3_2_10_b_species_enddate != null && h.F_3_2_10_b_species_enddate != "" ? DateOnly.ParseExact(h.F_3_2_10_b_species_enddate ?? string.Empty, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture).ToString() : null;
                        }
                        else if (h.F_3_2_10_c_species_before_2022 != null && h.F_3_2_10_c_species_before_2022 == "True")
                        {
                            temp.F_3_2_10_species_date = "Survey older than 2022";
                        }
                        result.EcologicalInformation.F_3_2_a_essential_information.Add(temp);
                    });
                    result.EcologicalInformation.F_3_2_a_essential_information = [.. result.EcologicalInformation.F_3_2_a_essential_information.OrderBy(o => o.F_3_2_1_species_group).ThenBy(o => o.F_3_2_3_species_name)];
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        SiteAssessmentSpecies temp = new()
                        {
                            F_3_2_1_species_group = h.F_3_2_1_species_group,
                            F_3_2_2_species_code = h.F_3_2_2_species_code,
                            F_3_2_3_species_name = h.F_3_2_3_species_name,
                            F_3_2_6_species_poptype = h.F_3_2_6_species_poptype,
                            F_3_2_11_species_significance = h.F_3_2_11_species_significance,
                            F_3_2_12_species_SPA_class = h.F_3_2_12_species_SPA_class,
                            F_3_2_13_species_rel_pop = h.F_3_2_13_species_rel_pop,
                            F_3_2_14_a_species_rel_pop_text = h.F_3_2_14_a_species_rel_pop_text,
                            F_3_2_15_1_species_conserv_degree = h.F_3_2_15_1_species_conserv_degree,
                            F_3_2_16_species_objectives = h.F_3_2_16_species_objectives,
                            F_3_2_17_a_species_objectives_text = h.F_3_2_17_a_species_objectives_text,
                            F_3_2_18_species_isolation = h.F_3_2_18_species_isolation,
                            F_3_2_19_species_global = h.F_3_2_19_species_global,
                            F_3_2_20_species_date = h.F_3_2_20_species_date != null && h.F_3_2_20_species_date != "" ? DateOnly.ParseExact(h.F_3_2_20_species_date, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture) : null
                        };
                        if (h.F_3_2_15_2_a_species_qual_suf != null || h.F_3_2_15_2_b_species_qual_non_suf != null || h.F_3_2_15_2_c_species_qual_unknown != null)
                            temp.F_3_2_15_2_species_qual = h.F_3_2_15_2_a_species_qual_suf + ", " + h.F_3_2_15_2_b_species_qual_non_suf + ", " + h.F_3_2_15_2_c_species_qual_unknown;
                        if (h.F_3_2_15_3_a_species_qual_class_suf != null || h.F_3_2_15_3_b_species_qual_class_non_suf != null || h.F_3_2_15_3_c_species_qual_class_unknown != null)
                            temp.F_3_2_15_3_species_qual_class = h.F_3_2_15_3_a_species_qual_class_suf + ", " + h.F_3_2_15_3_b_species_qual_class_non_suf + ", " + h.F_3_2_15_3_c_species_qual_class_unknown;
                        result.EcologicalInformation.F_3_2_b_site_assessment.Add(temp);
                    });
                    result.EcologicalInformation.F_3_2_b_site_assessment = [.. result.EcologicalInformation.F_3_2_b_site_assessment.OrderBy(o => o.F_3_2_1_species_group).ThenBy(o => o.F_3_2_3_species_name)];
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        OtherSpeciesSDF temp = new()
                        {
                            F_3_3_1_species_group = h.F_3_3_1_species_group,
                            F_3_3_2_species_code = h.F_3_3_2_species_code ?? "-",
                            F_3_3_3_species_name = h.F_3_3_3_species_name,
                            F_3_3_6_1_a_species_pop_min = !h.F_3_3_6_1_a_species_pop_min.Equals(0) ? h.F_3_3_6_1_a_species_pop_min : null,
                            F_3_3_6_1_b_species_pop_max = !h.F_3_3_6_1_b_species_pop_max.Equals(0) ? h.F_3_3_6_1_b_species_pop_max : null,
                            F_3_3_6_2_species_pop_unit = h.F_3_3_6_2_species_pop_unit,
                            F_3_3_7_species_abundance = h.F_3_3_7_species_abundance
                        };
                        if (h.F_3_3_4_species_sensitive != null)
                            temp.F_3_3_4_species_sensitive = (h.F_3_3_4_species_sensitive == true) ? booleanTrue : booleanUnchecked;
                        if (h.F_3_3_5_species_notPresent != null)
                            temp.F_3_3_5_species_NP = (h.F_3_3_5_species_notPresent == true) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.F_3_3_other_species.Add(temp);
                    });
                    result.EcologicalInformation.F_3_3_other_species = [.. result.EcologicalInformation.F_3_3_other_species.OrderBy(o => o.F_3_3_1_species_group).ThenBy(o => o.F_3_3_3_species_name)];
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        SpeciesMotivation temp = new()
                        {
                            F_3_3_2_species_code = h.F_3_3_2_species_code ?? "-",
                            F_3_3_3_species_name = h.F_3_3_3_species_name
                        };
                        if (h.F_3_3_8_species_motivation != null)
                        {
                            string annex = h.F_3_3_8_species_motivation;
                            temp.F_3_3_8_hd_annexii = annex.Contains("annexii") ? booleanChecked : booleanUnchecked;
                            annex = annex.Replace("annexii", "");
                            temp.F_3_3_8_hd_annexiv = annex.Contains("annexiv") ? booleanChecked : booleanUnchecked;
                            annex = annex.Replace("annexiv", "");
                            temp.F_3_3_8_hd_annexv = annex.Contains("annexv") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_bd_annexi = annex.Contains("typicalAnnexi") ? booleanChecked : booleanUnchecked;
                            annex = annex.Replace("typicalAnnexi", "");
                            temp.F_3_3_8_migratory = annex.Contains("migratory") ? booleanChecked : booleanUnchecked;
                            //temp.F_3_3_8_cfp_prohib = annex.Contains("") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_national_red_list = annex.Contains("nationalRedLists") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_eu_red_list = annex.Contains("EURedList") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_global_red_list = annex.Contains("globalRedList") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_endemic = annex.Contains("endemic") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_international_convention = annex.Contains("internationalConvention") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_hd_annexi = annex.Contains("annexi") ? booleanChecked : booleanUnchecked;
                            //temp.F_3_3_8_cwr_frg = annex.Contains("") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_ias_union = annex.Contains("ias") ? booleanChecked : booleanUnchecked;
                            temp.F_3_3_8_other = annex.Contains("other") ? booleanChecked : booleanUnchecked;
                        }
                        result.EcologicalInformation.F_3_3_8_species_motivation.Add(temp);
                    });
                    result.EcologicalInformation.F_3_3_8_species_motivation = [.. result.EcologicalInformation.F_3_3_8_species_motivation.OrderBy(o => o.F_3_3_3_species_name)];
                }
                #endregion

                #region SiteDescription
                if (site != null)
                {
                    result.SiteDescription.F_4_1_site_characteristics = site.F_4_1_site_characteristics;
                    result.SiteDescription.F_4_2_site_quality = site.F_4_2_site_quality;
                    if (site.F_4_3_5_pressure_date != null)
                        result.SiteDescription.F_4_3_5_pressure_date = DateTime.ParseExact(site.F_4_3_5_pressure_date, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture);
                    result.SiteDescription.F_4_4_a_documentation = site.F_4_4_a_documentation;
                    result.SiteDescription.F_4_4_1_documentation_URI = site.F_4_4_1_documentation_URI;
                    if (site.F_4_4_2_documentation_date != null)
                        result.SiteDescription.F_4_4_2_documentation_date = DateTime.ParseExact(site.F_4_4_2_documentation_date, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture);
                }
                if (pressures != null && pressures.Count > 0)
                {
                    pressures.ForEach(h =>
                    {
                        Pressures temp = new()
                        {
                            F_4_3_1_pressure_code = h.F_4_3_1_pressure_code,
                            F_4_3_2_pressure_rank = h.F_4_3_2_pressure_rank,
                            F_4_3_3_pressure_location = h.F_4_3_3_pressure_location,
                            F_4_3_4_a_pressure_text = h.F_4_3_4_a_pressure_text
                        };
                        result.SiteDescription.F_4_3_pressures.Add(temp);
                    });
                    result.SiteDescription.F_4_3_pressures = [.. result.SiteDescription.F_4_3_pressures.OrderBy(o => o.F_4_3_1_pressure_code)];
                }
                #endregion

                #region SiteManagement
                if (managementBody != null)
                {
                    result.SiteManagement.F_5_1_1_a_management_body = managementBody.F_5_1_1_a_management_body;
                    result.SiteManagement.F_5_1_2_management_body_contact = managementBody.F_5_1_2_management_body_contact;
                    result.SiteManagement.F_5_1_3_a_management_body_address = managementBody.F_5_1_3_a_management_body_address;
                    result.SiteManagement.F_5_1_4_management_body_email = managementBody.F_5_1_4_management_body_email;
                    result.SiteManagement.F_5_1_5_management_URI = managementBody.F_5_1_5_management_URI;
                }
                if (managementInfo != null && managementInfo.Count > 0)
                {
                    result.SiteManagement.F_5_2_1_management_plans_exist = managementInfo.FirstOrDefault()?.F_5_2_1_management_plans_exist;
                    result.SiteManagement.F_5_2_3_a_management_text = managementInfo.FirstOrDefault()?.F_5_2_3_a_management_text;
                    result.SiteManagement.F_5_3_1_a_measures_MP_included = managementInfo.FirstOrDefault()?.F_5_3_1_a_measures_MP_included;
                    result.SiteManagement.F_5_3_1_d_measures_text = managementInfo.FirstOrDefault()?.F_5_3_1_d_measures_text;
                    result.SiteManagement.F_5_3_2_a_measures_established = managementInfo.FirstOrDefault()?.F_5_3_2_a_measures_established;
                    result.SiteManagement.F_5_3_2_b_measures_implemented = managementInfo.FirstOrDefault()?.F_5_3_2_b_measures_implemented;
                    result.SiteManagement.F_5_4_a_measures_effectiveness = managementInfo.FirstOrDefault()?.F_5_4_a_measures_effectiveness;
                    result.SiteManagement.F_5_4_b_measures_results = managementInfo.FirstOrDefault()?.F_5_4_b_measures_results;
                    managementInfo.ForEach(sld =>
                    {
                        Models.ViewModel.MeasuresList temp1 = new()
                        {
                            F_5_3_1_b_measures_title = sld.F_5_3_1_b_measures_title,
                            F_5_3_1_c_measures_URI = sld.F_5_3_1_c_measures_URI
                        };
                        if (temp1.F_5_3_1_b_measures_title != null || temp1.F_5_3_1_c_measures_URI != null)
                            result.SiteManagement.F_5_3_1_measures_list.Add(temp1);
                    });
                }
                if (managementPlanList != null && managementPlanList.Count > 0)
                {
                    managementPlanList.ForEach(sld =>
                    {
                        Models.ViewModel.ManagementList temp = new()
                        {
                            F_5_2_2_a_management_plan_name = sld.F_5_2_2_a_management_plan_name,
                            F_5_2_2_b_management_plan_URI = sld.F_5_2_2_b_management_plan_URI,
                            F_5_2_2_c_management_startdate = sld.F_5_2_2_c_management_startdate != null && sld.F_5_2_2_c_management_startdate != "" ? DateOnly.ParseExact(sld.F_5_2_2_c_management_startdate, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture) : null,
                            F_5_2_2_d_management_duration = sld.F_5_2_2_d_management_duration
                        };
                        if (temp.F_5_2_2_a_management_plan_name != null || temp.F_5_2_2_b_management_plan_URI != null || temp.F_5_2_2_c_management_startdate != null || temp.F_5_2_2_d_management_duration != null)
                            result.SiteManagement.F_5_2_2_management_list.Add(temp);
                    });
                }
                #endregion

                #region GeospatialRepresentation
                if (site != null)
                {
                    result.GeospatialRepresentation.F_6_1_1_inspire_namespace = site.F_6_1_1_inspire_namespace;
                    result.GeospatialRepresentation.F_6_1_2_inspire_local_id = site.F_6_1_2_inspire_local_id;
                    result.GeospatialRepresentation.F_6_1_3_inspire_version = site.F_6_1_3_inspire_version;
                }
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetReleaseData", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<ReleaseSDFLegacy> GetReleaseDataLegacy(string SiteCode, int ReleaseId, Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated, bool showSensitive = true)
        {
            try
            {
                string booleanTrue = "Yes";
                string booleanFalse = "No";
                string booleanChecked = "x";
                string booleanUnchecked = "";
                ReleaseSDFLegacy result = new();

                List<ReleaseVisibility> releaseVisibility = await _releaseLegacyContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync();
                releaseVisibility.AddRange(await _releaseContext.Set<ReleaseVisibility>().AsNoTracking().ToListAsync());
                #region filters
                if (initialValidation)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InitialValidation == true)];
                if (internalViewers)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalViewers == true)];
                if (internalBarometer)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalBarometer == true)];
                if (internalPortalSDFSensitive)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.InternalPortalSDFSensitive == true)];
                if (publicViewers)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.PublicViewers == true)];
                if (publicBarometer)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.PublicBarometer == true)];
                if (sdfPublic)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.SDFPublic == true)];
                if (naturaOnlineList)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.NaturaOnlineList == true)];
                if (productsCreated)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.ProductsCreated == true)];
                if (jediDimensionCreated)
                    releaseVisibility = [.. releaseVisibility.Where(w => w.JediDimensionCreated == true)];
                #endregion
                List<long> releaseVisibilityIDs = [.. releaseVisibility.Select(s => s.ReleaseID)];

                if (releaseVisibilityIDs == null || releaseVisibilityIDs.Count == 0)
                    return result;

                List<ReleasesExtended> releases = await _releaseLegacyContext.Set<Releases>().Where(w => releaseVisibilityIDs.Contains(w.ID)).AsNoTracking()
                    .Select(r => new ReleasesExtended
                    {
                        ID = r.ID,
                        Title = r.Title,
                        Author = r.Author,
                        CreateDate = r.CreateDate,
                        Final = r.Final,
                        ModifyUser = r.ModifyUser,
                        ModifyDate = r.ModifyDate,
                        Character = r.Character,
                        Legacy = true
                    })
                    .ToListAsync();
                releases.AddRange(await _releaseContext.Set<Releases>().Where(w => releaseVisibilityIDs.Contains(w.ID)).AsNoTracking()
                    .Select(r => new ReleasesExtended
                    {
                        ID = r.ID,
                        Title = r.Title,
                        Author = r.Author,
                        CreateDate = r.CreateDate,
                        Final = r.Final,
                        ModifyUser = r.ModifyUser,
                        ModifyDate = r.ModifyDate,
                        Character = r.Character,
                        Legacy = false
                    })
                    .ToListAsync());
                Releases? release = null;

                List<NATURA2000SITES> sites = await _releaseLegacyContext.Set<NATURA2000SITES>().Where(a => a.SITECODE == SiteCode).ToListAsync();
                List<NATURA2000SITES> sitesFilter = [.. sites.Where(a => a.SITECODE == SiteCode && releaseVisibilityIDs.Contains(a.ReleaseId))];
                NATURA2000SITES? site = null;

                if (sitesFilter.Count != 0) //If the site is included in a Release that complies with the filters we fetch the Site data
                {
                    if (ReleaseId == -1)
                    {
                        List<long> siteReleaseIDs = [.. sites.Select(s => s.ReleaseId)];
                        release = releases.Where(w => siteReleaseIDs.Contains(w.ID) && w.Legacy == true).OrderBy(r => r.CreateDate).Last();
                        site = sites.Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).FirstOrDefault();
                    }
                    else
                    {
                        release = releases.Where(r => r.ID == ReleaseId && r.Legacy == true).FirstOrDefault();
                        site = sites.Where(a => a.SITECODE == SiteCode && a.ReleaseId == ReleaseId).FirstOrDefault();
                    }
                }

                if (release == null || site == null)
                    return result;

                //Catalogues
                List<Countries> countries = await _dataContext.Set<Countries>().AsNoTracking().ToListAsync();
                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();

                //Data
                List<HABITATS> habitats = await _releaseLegacyContext.Set<HABITATS>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<HABITATCLASS> habitatClass = await _releaseLegacyContext.Set<HABITATCLASS>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SPECIES> species = await _releaseLegacyContext.Set<SPECIES>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID && (!(a.SENSITIVE ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<OTHERSPECIES> speciesOther = await _releaseLegacyContext.Set<OTHERSPECIES>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID && (!(a.SENSITIVE ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<CONTACTS> contacts = await _releaseLegacyContext.Set<CONTACTS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<MANAGEMENT> management = await _releaseLegacyContext.Set<MANAGEMENT>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<NUTSBYSITE> nutsBySite = await _releaseLegacyContext.Set<NUTSBYSITE>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<BIOREGION> bioRegions = await _releaseLegacyContext.Set<BIOREGION>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<IMPACT> isImpactedBy = await _releaseLegacyContext.Set<IMPACT>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SITEOWNERTYPE> siteOwnerType = await _releaseLegacyContext.Set<SITEOWNERTYPE>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<DOCUMENTATIONLINKS> documentationLinks = await _releaseLegacyContext.Set<DOCUMENTATIONLINKS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<DESIGNATIONSTATUS> designationStatus = await _releaseLegacyContext.Set<DESIGNATIONSTATUS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                REFERENCEMAP? referenceMap = await _releaseLegacyContext.Set<REFERENCEMAP>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().FirstOrDefaultAsync();

                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.SITENAME;
                    result.SiteInfo.Country = site.COUNTRY_CODE != null ? (countries.Where(c => c.Code.Equals(site.COUNTRY_CODE, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault() != null ? countries.Where(c => c.Code.Equals(site.COUNTRY_CODE, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Country : site.COUNTRY_CODE) : null;
                    result.SiteInfo.Directive = site.SITETYPE; //UNSURE
                    result.SiteInfo.SiteCode = SiteCode;
                    result.SiteInfo.Area = site.AREAHA;
                    result.SiteInfo.Est = site.DATE_COMPILATION; //UNSURE
                    result.SiteInfo.MarineArea = site.MARINE_AREA_PERCENTAGE;
                }
                if (habitats != null && habitats.Count > 0)
                    result.SiteInfo.Habitats = habitats.Count;
                if (species != null && species.Count > 0)
                    result.SiteInfo.Species = species.Count;
                sites.ForEach(st =>
                {
                    if (releaseVisibilityIDs.Contains(st.ReleaseId))
                    {
                        ReleaseInfo temp = new()
                        {
                            ReleaseId = st.ReleaseId,
                            ReleaseName = releases.Where(w => w.ID == st.ReleaseId).Select(s => s.Title).FirstOrDefault(),
                            ReleaseDate = releases.Where(w => w.ID == st.ReleaseId).Select(s => s.CreateDate).FirstOrDefault(),
                            Legacy = releases.Where(w => w.ID == st.ReleaseId).Select(s => s.Legacy).FirstOrDefault()
                        };
                        result.SiteInfo.Releases?.Add(temp);
                    }
                });
                #endregion

                #region SiteIdentification
                if (site != null)
                {
                    result.SiteIdentification.Type = site.SITETYPE;
                    result.SiteIdentification.SiteCode = SiteCode;
                    result.SiteIdentification.SiteName = site.SITENAME;
                    result.SiteIdentification.FirstCompletionDate = site.DATE_COMPILATION;
                    result.SiteIdentification.UpdateDate = site.DATE_UPDATE;
                    SiteDesignation siteDesignation = new()
                    {
                        ClassifiedSPA = site.DATE_SPA,
                        ReferenceSPA = site.SPA_LEGAL_REFERENCE,
                        ProposedSCI = site.DATE_PROP_SCI,
                        ConfirmedSCI = site.DATE_CONF_SCI,
                        DesignatedSAC = site.DATE_SAC,
                        ReferenceSAC = site.SAC_LEGAL_REFERENCE,
                        Explanations = site.EXPLANATIONS
                    }; //UNSURE HOW COULD THERE BE MORE THAN ONE
                    result.SiteIdentification.SiteDesignation.Add(siteDesignation);
                }
                if (contacts != null && contacts.Count > 0)
                {
                    CONTACTS? contact = contacts.Where(r => r.NAME != null).FirstOrDefault();
                    if (contact != null)
                    {
                        result.SiteIdentification.Respondent.Name = contact.NAME;
                        result.SiteIdentification.Respondent.Address = contact.ADDRESS;
                        result.SiteIdentification.Respondent.Email = contact.EMAIL;
                    }
                }
                #endregion

                #region SiteLocation
                if (site != null)
                {
                    result.SiteLocation.Longitude = site.LONGITUDE;
                    result.SiteLocation.Latitude = site.LATITUDE;
                    result.SiteLocation.Area = site.AREAHA;
                    result.SiteLocation.MarineArea = site.MARINE_AREA_PERCENTAGE;
                    result.SiteLocation.SiteLength = site.LENGTHKM;
                }
                if (nutsBySite != null && nutsBySite.Count > 0)
                {
                    nutsBySite.ForEach(nbs =>
                    {
                        Models.ViewModel.Region temp = new()
                        {
                            NUTSLevel2Code = nbs.NUTID,
                            RegionName = nbs.NUTID != null ? (nuts.Where(t => t.Code == nbs.NUTID).FirstOrDefault() != null ? nuts.Where(t => t.Code == nbs.NUTID).FirstOrDefault()?.Region : null) : null
                        };
                        result.SiteLocation.Region.Add(temp);
                    });
                }
                if (bioRegions != null && bioRegions.Count > 0)
                {
                    bioRegions.ForEach(br =>
                    {
                        BiogeographicalRegionsLegacy temp = new()
                        {
                            Name = br.BIOGEOGRAPHICREG,
                            Value = br.PERCENTAGE
                        };
                        result.SiteLocation.BiogeographicalRegions.Add(temp);
                    });
                }
                #endregion

                #region EcologicalInformation
                if (habitats != null && habitats.Count > 0)
                {
                    habitats.ForEach(h =>
                    {
                        HabitatSDF temp = new()
                        {
                            HabitatName = h.DESCRIPTION,
                            Code = h.HABITATCODE,
                            Cover = h.COVER_HA,
                            Cave = !String.IsNullOrEmpty(h.CAVES) && !h.CAVES.Replace(".00", "").Equals("0") ? Int32.Parse(h.CAVES.Replace(".00", "")) : null,
                            DataQuality = h.DATAQUALITY != null ? (dataQualityTypes.Where(c => c.HabitatCode == h.DATAQUALITY).FirstOrDefault() != null ? dataQualityTypes.Where(c => c.HabitatCode == h.DATAQUALITY).FirstOrDefault()?.HabitatCode : null) : null,
                            Representativity = h.REPRESENTATIVITY,
                            RelativeSurface = h.RELSURFACE,
                            Conservation = h.CONSERVATION,
                            Global = h.GLOBAL_ASSESSMENT
                        };
                        if (h.PRIORITY_FORM_HABITAT_TYPE != null)
                            temp.PF = (h.PRIORITY_FORM_HABITAT_TYPE == true) ? booleanChecked : booleanUnchecked;
                        if (h.NON_PRESENCE_IN_SITE != null)
                            temp.NP = (h.NON_PRESENCE_IN_SITE == 1) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.HabitatTypes.Add(temp);
                    });
                    result.EcologicalInformation.HabitatTypes = [.. result.EcologicalInformation.HabitatTypes.OrderBy(o => o.Code)];
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SPECIESNAME,
                            Code = h.SPECIESCODE,
                            Group = h.SPGROUP,
                            Type = h.POPULATION_TYPE,
                            Min = !String.IsNullOrEmpty(h.LOWERBOUND) && !h.LOWERBOUND.Equals(0) ? Int32.Parse(h.LOWERBOUND) : null,
                            Max = !String.IsNullOrEmpty(h.UPPERBOUND) && !h.UPPERBOUND.Equals(0) ? Int32.Parse(h.UPPERBOUND) : null,
                            Unit = h.COUNTING_UNIT,
                            Category = h.ABUNDANCE_CATEGORY,
                            DataQuality = h.DATAQUALITY,
                            Population = h.POPULATION,
                            Conservation = h.CONSERVATION,
                            Isolation = h.ISOLATION,
                            Global = h.GLOBAL
                        };
                        if (h.SENSITIVE != null)
                            temp.Sensitive = (h.SENSITIVE == true) ? booleanTrue : booleanUnchecked;
                        if (h.NONPRESENCEINSITE != null)
                            temp.NP = (h.NONPRESENCEINSITE == true) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.Species.Add(temp);
                    });
                    result.EcologicalInformation.Species = [.. result.EcologicalInformation.Species.OrderBy(o => o.Group).ThenBy(o => o.SpeciesName)];
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SPECIESNAME,
                            Code = h.SPECIESCODE ?? "-",
                            Group = h.SPECIESGROUP,
                            Min = !h.LOWERBOUND.Equals(0) ? h.LOWERBOUND : null,
                            Max = !h.UPPERBOUND.Equals(0) ? h.UPPERBOUND : null,
                            Unit = h.COUNTING_UNIT,
                            Category = h.ABUNDANCE_CATEGORY
                        };
                        if (h.SENSITIVE != null)
                            temp.Sensitive = (h.SENSITIVE == true) ? booleanTrue : booleanUnchecked;
                        if (h.NONPRESENCEINSITE != null)
                            temp.NP = (h.NONPRESENCEINSITE == true) ? booleanChecked : booleanUnchecked;
                        if (h.MOTIVATION != null)
                        {
                            temp.AnnexIV = h.MOTIVATION.Contains("IV") ? booleanChecked : booleanUnchecked;
                            string annex = h.MOTIVATION.Replace("IV", "");
                            temp.AnnexV = annex.Contains('V') ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesA = h.MOTIVATION.Contains('A') ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesB = h.MOTIVATION.Contains('B') ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesC = h.MOTIVATION.Contains('C') ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesD = h.MOTIVATION.Contains('D') ? booleanChecked : booleanUnchecked;
                        }
                        result.EcologicalInformation.OtherSpecies.Add(temp);
                    });
                    result.EcologicalInformation.OtherSpecies = [.. result.EcologicalInformation.OtherSpecies.OrderBy(o => o.Group).ThenBy(o => o.SpeciesName)];
                }
                #endregion

                #region SiteDescription
                if (habitatClass != null && habitatClass.Count > 0)
                {
                    habitatClass.ForEach(h =>
                    {
                        CodeCover temp = new()
                        {
                            Code = h.HABITATCODE,
                            Cover = h.PERCENTAGECOVER
                        };
                        result.SiteDescription.GeneralCharacter.Add(temp);
                    });
                    result.SiteDescription.GeneralCharacter = [.. result.SiteDescription.GeneralCharacter.OrderBy(o => o.Cover)];
                }
                if (isImpactedBy != null && isImpactedBy.Count > 0)
                {
                    isImpactedBy.ForEach(h =>
                    {
                        Threats temp = new()
                        {
                            Rank = (h.INTENSITY != null && h.INTENSITY.Length > 0) ? h.INTENSITY?[..1].ToUpper() : null,
                            Impacts = h.IMPACTCODE,
                            Pollution = h.POLLUTIONCODE,
                            Origin = (h.OCCURRENCE != null && h.OCCURRENCE.Length > 0) ? h.OCCURRENCE?[..1].ToLower() : null
                        };
                        if (h.IMPACT_TYPE == "N")
                        {
                            result.SiteDescription.NegativeThreats.Add(temp);
                        }
                        else if (h.IMPACT_TYPE == "P")
                        {
                            result.SiteDescription.PositiveThreats.Add(temp);
                        }
                    });
                    result.SiteDescription.NegativeThreats = [.. result.SiteDescription.NegativeThreats.OrderBy(o => o.Rank).ThenBy(o => o.Impacts)];
                    result.SiteDescription.PositiveThreats = [.. result.SiteDescription.PositiveThreats.OrderBy(o => o.Rank).ThenBy(o => o.Impacts)];
                }
                if (siteOwnerType != null && siteOwnerType.Count > 0)
                {
                    siteOwnerType.ForEach(h =>
                    {
                        Models.ViewModel.Ownership temp = new()
                        {
                            Type = h.TYPE?.ToLower(),
                            Percent = h.PERCENT
                        };
                        result.SiteDescription.Ownership.Add(temp);
                    });
                }
                result.SiteDescription.Quality = site?.QUALITY;
                result.SiteDescription.Documents = site?.DOCUMENTATION;
                result.SiteDescription.OtherCharacteristics = site?.OTHERCHARACT;
                if (documentationLinks != null && documentationLinks.Count > 0)
                {
                    documentationLinks.ForEach(h =>
                    {
                        if (!string.IsNullOrEmpty(h.LINK))
                            result.SiteDescription.Links.Add(h.LINK);
                    });
                }
                #endregion

                #region SiteProtectionStatus
                if (designationStatus != null && designationStatus.Count > 0)
                {
                    designationStatus.ForEach(h =>
                    {
                        if (h.DESIGNATEDSITENAME == null && h.OVERLAPCODE == null)
                        {
                            CodeCover temp = new()
                            {
                                Code = h.DESIGNATIONCODE,
                                Cover = h.OVERLAPPERC
                            };
                            result.SiteProtectionStatus.DesignationTypes.Add(temp);
                        }
                        else
                        {
                            RelationSites temp = new()
                            {
                                DesignationLevel = (h.DESIGNATIONCODE != null && h.DESIGNATIONCODE != "") ? "National or regional" : "International",
                                TypeCode = h.DESIGNATIONCODE,
                                SiteName = h.DESIGNATEDSITENAME,
                                Type = h.OVERLAPCODE,
                                Percent = h.OVERLAPPERC
                            };
                            result.SiteProtectionStatus.RelationSites.Add(temp);
                            result.SiteProtectionStatus.RelationSites = [.. result.SiteProtectionStatus.RelationSites.OrderByDescending(o => o.DesignationLevel)];
                        }
                    });
                }
                result.SiteProtectionStatus.SiteDesignation = site?.DESIGNATION;
                #endregion

                #region SiteManagement
                if (management != null && management.Count > 0)
                {
                    management.ForEach(h =>
                    {
                        if (h.ORG_NAME != null)
                        {
                            BodyResponsible temp = new()
                            {
                                Organisation = h.ORG_NAME,
                                Address = h.ORG_ADDRESS,
                                Email = h.ORG_EMAIL
                            };
                            result.SiteManagement.BodyResponsible.Add(temp);
                        }
                    });
                }
                if (management != null && management.Count > 0)
                {
                    management.ForEach(h =>
                    {
                        ManagementPlan temp = new()
                        {
                            Name = h.MANAG_PLAN,
                            Link = h.MANAG_PLAN_URL,
                            Exists = h.MANAG_STATUS
                        };
                        result.SiteManagement.ManagementPlan.Add(temp);
                    });
                    result.SiteManagement.ConservationMeasures = management.FirstOrDefault()?.MANAG_CONSERV_MEASURES;
                }
                #endregion

                #region MapOfTheSite
                if (referenceMap != null)
                {
                    result.MapOfTheSite.INSPIRE = referenceMap.INSPIRE;
                    result.MapOfTheSite.MapDelivered = (referenceMap.PDFPROVIDED != null && referenceMap.PDFPROVIDED == 1) ? booleanTrue : booleanFalse;
                }
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetReleaseDataLegacy", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
