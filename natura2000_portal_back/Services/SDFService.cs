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
        private readonly N2KReleasesContext _releaseContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public SDFService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _appSettings = app;
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

                List<Releases> releases = await _releaseContext.Set<Releases>().AsNoTracking().ToListAsync();
                Releases release;

                if (ReleaseId == -1)
                {
                    release = releases.OrderBy(r => r.CreateDate).Last();
                }
                else
                {
                    release = releases.Where(r => r.ID == ReleaseId).FirstOrDefault();
                }

                if (release == null)
                    return result;

                List<NATURA2000SITES> sites = await _releaseContext.Set<NATURA2000SITES>().Where(a => a.SITECODE == SiteCode && releaseVisibilityIDs.Contains(a.ReleaseId)).ToListAsync();
                if (sites.Any()) //If the site is included in a Release that complies with the filters we fetch the Site data
                    sites = await _releaseContext.Set<NATURA2000SITES>().Where(a => a.SITECODE == SiteCode).ToListAsync();
                NATURA2000SITES site = sites.Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).FirstOrDefault();

                if (site == null)
                    return result;

                //Catalogues
                List<Countries> countries = await _dataContext.Set<Countries>().AsNoTracking().ToListAsync();
                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();
                List<OwnerShipTypes> ownerShipTypes = await _dataContext.Set<OwnerShipTypes>().AsNoTracking().ToListAsync();

                //Data
                List<HABITATS> habitats = await _releaseContext.Set<HABITATS>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<HABITATCLASS> habitatClass = await _releaseContext.Set<HABITATCLASS>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SPECIES> species = await _releaseContext.Set<SPECIES>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID && (!(a.SENSITIVE ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<OTHERSPECIES> speciesOther = await _releaseContext.Set<OTHERSPECIES>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID && (!(a.SENSITIVE ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<CONTACTS> contacts = await _releaseContext.Set<CONTACTS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<MANAGEMENT> management = await _releaseContext.Set<MANAGEMENT>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<NUTSBYSITE> nutsBySite = await _releaseContext.Set<NUTSBYSITE>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<BIOREGION> bioRegions = await _releaseContext.Set<BIOREGION>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<IMPACT> isImpactedBy = await _releaseContext.Set<IMPACT>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SITEOWNERTYPE> siteOwnerType = await _releaseContext.Set<SITEOWNERTYPE>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<DOCUMENTATIONLINKS> documentationLinks = await _releaseContext.Set<DOCUMENTATIONLINKS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<DESIGNATIONSTATUS> designationStatus = await _releaseContext.Set<DESIGNATIONSTATUS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                REFERENCEMAP referenceMap = await _releaseContext.Set<REFERENCEMAP>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().FirstOrDefaultAsync();

                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.SITENAME;
                    result.SiteInfo.Country = countries.Where(c => c.Code == site.COUNTRY_CODE.ToLower()).FirstOrDefault().Country;
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
                            ReleaseDate = releases.Where(w => w.ID == st.ReleaseId).Select(s => s.CreateDate).FirstOrDefault()
                        };
                        result.SiteInfo.Releases.Add(temp);
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
                    CONTACTS contact = contacts.Where(r => r.NAME != null).FirstOrDefault();
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
                            RegionName = nuts.Where(t => t.Code == nbs.NUTID).FirstOrDefault().Region
                        };
                        result.SiteLocation.Region.Add(temp);
                    });
                }
                if (bioRegions != null && bioRegions.Count > 0)
                {
                    bioRegions.ForEach(br =>
                    {
                        BiogeographicalRegions temp = new()
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
                            Cave = h.CAVES,
                            DataQuality = h.DATAQUALITY != null ? dataQualityTypes.Where(c => c.HabitatCode == h.DATAQUALITY).FirstOrDefault().HabitatCode : null,
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
                    result.EcologicalInformation.HabitatTypes = result.EcologicalInformation.HabitatTypes.OrderBy(o => o.Code).ToList();
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
                            Min = h.LOWERBOUND,
                            Max = h.UPPERBOUND,
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
                    result.EcologicalInformation.Species = result.EcologicalInformation.Species.OrderBy(o => o.Group).ThenBy(o => o.SpeciesName).ToList();
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
                            Min = h.LOWERBOUND.ToString(),
                            Max = h.UPPERBOUND.ToString(),
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
                            temp.AnnexV = annex.Contains("V") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesA = h.MOTIVATION.Contains("A") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesB = h.MOTIVATION.Contains("B") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesC = h.MOTIVATION.Contains("C") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesD = h.MOTIVATION.Contains("D") ? booleanChecked : booleanUnchecked;
                        }
                        result.EcologicalInformation.OtherSpecies.Add(temp);
                    });
                    result.EcologicalInformation.OtherSpecies = result.EcologicalInformation.OtherSpecies.OrderBy(o => o.Group).ThenBy(o => o.SpeciesName).ToList();
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
                    result.SiteDescription.GeneralCharacter = result.SiteDescription.GeneralCharacter.OrderBy(o => o.Code).ToList();
                }
                if (isImpactedBy != null && isImpactedBy.Count > 0)
                {
                    isImpactedBy.ForEach(h =>
                    {
                        Threats temp = new()
                        {
                            Rank = (h.INTENSITY != null && h.INTENSITY.Length > 0) ? h.INTENSITY?.Substring(0, 1).ToUpper() : null,
                            Impacts = h.IMPACTCODE,
                            Pollution = h.POLLUTIONCODE,
                            Origin = (h.OCCURRENCE != null && h.OCCURRENCE.Length > 0) ? h.OCCURRENCE?.Substring(0, 1).ToLower() : null
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
                    result.SiteDescription.NegativeThreats = result.SiteDescription.NegativeThreats.OrderBy(o => o.Rank).ThenBy(o => o.Impacts).ToList();
                    result.SiteDescription.PositiveThreats = result.SiteDescription.PositiveThreats.OrderBy(o => o.Rank).ThenBy(o => o.Impacts).ToList();
                }
                if (siteOwnerType != null && siteOwnerType.Count > 0)
                {
                    siteOwnerType.ForEach(h =>
                    {
                        natura2000_portal_back.Models.ViewModel.Ownership temp = new()
                        {
                            Type = h.TYPE == null ? null : h.TYPE.ToLower(),
                            Percent = h.PERCENT
                        };
                        result.SiteDescription.Ownership.Add(temp);
                    });
                }
                result.SiteDescription.Quality = site.QUALITY;
                result.SiteDescription.Documents = site.DOCUMENTATION;
                result.SiteDescription.OtherCharacteristics = site.OTHERCHARACT;
                if (documentationLinks != null && documentationLinks.Count > 0)
                {
                    documentationLinks.ForEach(h =>
                    {
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
                            result.SiteProtectionStatus.RelationSites = result.SiteProtectionStatus.RelationSites.OrderByDescending(o => o.DesignationLevel).ToList();
                        }
                    });
                }
                result.SiteProtectionStatus.SiteDesignation = site.DESIGNATION;
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
                    result.SiteManagement.ConservationMeasures = management.FirstOrDefault().MANAG_CONSERV_MEASURES;
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetReleaseData", "", _dataContext.Database.GetConnectionString());
                throw;
            }
        }
    }
}
