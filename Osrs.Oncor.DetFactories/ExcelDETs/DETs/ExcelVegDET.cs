using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Oncor.Excel;
using System.Collections.Generic;
using System.IO;

namespace ExcelDETs.DETs
{
    public sealed class ExcelVegDET
    {
        private readonly VegDET generalDet;
        private readonly ExcelBaseDet myDet;

        public ExcelVegDET(VegDET det)
        {
            generalDet = det;
            myDet = new ExcelBaseDet(det.Id, det.Owner);
        }

        private DataTab ShrubSpeciesDataTab()
        {
            return new DataTab("LIST_ShrubSpecies", XlColor.Orange, SpeciesDTO.GetSchema(), generalDet.ShrubSpecies.Values);
        }

        private DataTab TreeSpeciesDataTab()
        {
            return new DataTab("LIST_TreeSpecies", XlColor.Orange, SpeciesDTO.GetSchema(), generalDet.TreeSpecies.Values);
        }

        private DataTab HerbSpeciesDataTab()
        {
            return new DataTab("LIST_HerbSpecies", XlColor.Orange, SpeciesDTO.GetSchema(), generalDet.HerbSpecies.Values);
        }

        private DataTab NonLivingDataTab()
        {
            return new DataTab("LIST_NonLiving", XlColor.Orange, SpeciesDTO.GetSchema(), generalDet.NonLiving.Values);
        }

        private DataTab PlotTypesDataTab()
        {
            return new DataTab("LIST_PlotTypes", XlColor.Orange, PlotTypeDTO.GetSchema(), generalDet.PlotTypes.Values);
        }

        private DataTab SitesDataTab()
        {
            return new DataTab("LIST_Sites", XlColor.Orange, SiteDTO.GetSchema(), generalDet.Sites.Values);
        }

        private DataTab SurveyDataTab()
        {
            return new DataTab("DET_Survey", XlColor.White, VegSurveyDTO.GetSchema(), generalDet.Surveys.Values);
        }

        private DataTab ElevationDataTab()
        {
            return new DataTab("DET_Elevation", XlColor.White, VegElevationDTO.GetSchema(), generalDet.Elevations.Values);
        }

        private DataTab TreeDataTab()
        {
            return new DataTab("DET_Tree", XlColor.White, VegTreeDTO.GetSchema(), generalDet.Trees.Values);
        }

        private DataTab HerbDataTab()
        {
            return new DataTab("DET_Herb", XlColor.White, VegHerbDTO.GetSchema(), generalDet.Herbs.Values);
        }

        private DataTab ShrubDataTab()
        {
            return new DataTab("DET_Shrub", XlColor.White, VegShrubDTO.GetSchema(), generalDet.Shrubs.Values);
        }

        private List<DataTab> DataTabList()
        {
            List<DataTab> list = new List<DataTab>();
            list.Add(SurveyDataTab());
            list.Add(TreeDataTab());
            list.Add(HerbDataTab());
            list.Add(ShrubDataTab());
            list.Add(ElevationDataTab());
            list.Add(ShrubSpeciesDataTab());
            list.Add(TreeSpeciesDataTab());
            list.Add(HerbSpeciesDataTab());
            list.Add(SitesDataTab());
            list.Add(PlotTypesDataTab());
            list.Add(NonLivingDataTab());
            return list;
        }

        private void LoadRow(string sheetName, Dictionary<string, string> values)
        {
            ValidationIssues issues = generalDet.ValidationIssues;
            if (sheetName == "DET_Survey")
            {
                VegSurveyDTO newDto = new VegSurveyDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Surveys.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The survey with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Tree")
            {
                VegTreeDTO newDto = new VegTreeDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Trees.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The measurment with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Herb")
            {
                VegHerbDTO newDto = new VegHerbDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Herbs.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The measurment with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Shrub")
            {
                VegShrubDTO newDto = new VegShrubDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Shrubs.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The measurment with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Elevation")
            {
                VegElevationDTO newDto = new VegElevationDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Elevations.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The elevation with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_Sites")
            {
                SiteDTO newDto = new SiteDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Sites.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The site with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_PlotTypes")
            {
                PlotTypeDTO newDto = new PlotTypeDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.PlotTypes.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The plottype with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_NonLiving")
            {
                SpeciesDTO newDto = new SpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.NonLiving.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The nonliving with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_TreeSpecies")
            {
                SpeciesDTO newDto = new SpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.TreeSpecies.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The taxa with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_HerbSpecies")
            {
                SpeciesDTO newDto = new SpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.HerbSpecies.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The taxa with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_ShrubSpecies")
            {
                SpeciesDTO newDto = new SpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.ShrubSpecies.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The taxa with code {0} is not unique.", newDto.LookupKey));
                }
            }
        }

        private void CheckSheetCount()
        {
            myDet.CheckOneSheet("DET_Herb", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Shrub", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Tree", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Elevation", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Survey", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_PlotTypes", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Sites", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_NonLiving", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_TreeSpecies", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_ShrubSpecies", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_HerbSpecies", generalDet.ValidationIssues);
        }

        private void CheckHeaders(XlWorksheet worksheet)
        {
            if (worksheet != null)
            {
                Schema s;
                List<string> h;
                if (worksheet.Name == "DET_Deployments")
                {
                    s = DeploymentDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_Measurements")
                {
                    s = MeasurementDTO.GetSchema();
                }
                else if (worksheet.Name == "LIST_Sites")
                {
                    s = SiteDTO.GetSchema();
                }
                else if (worksheet.Name == "LIST_Sensors")
                {
                    s = SensorDTO.GetSchema();
                }
                else
                    return;

                h = new List<string>();
                List<string> hdrs = ExcelBaseDet.Headers(worksheet);
                bool bad = false;
                for (int i = 0; i < hdrs.Count - 1; i++)
                {
                    string t = hdrs[i];
                    for (int j = i + 1; j < hdrs.Count; j++)
                    {
                        if (t == hdrs[j])
                        {
                            bad = true;
                            break; //inner
                        }
                    }
                    if (bad)
                        break; //outer
                }
                if (bad)
                {
                    generalDet.ValidationIssues.Add(ValidationIssue.Code.DuplicateHeader, "Duplicate column header in " + worksheet.Name);
                }
                foreach (SchemaEntry c in s)
                {
                    h.Add(c.LowerColumnName);
                }
                if (!ExcelBaseDet.HasHeaders(hdrs, h))
                {
                    generalDet.ValidationIssues.Add(ValidationIssue.Code.MissingFieldHeader, "Missing column header in " + worksheet.Name);
                }
            }
        }

        public void Save(Stream stream)
        {
            myDet.Save(stream, DataTabList());
        }

        public void Save(string filename)
        {
            myDet.Save(filename, DataTabList());
        }

        public IDet Load(Stream stream)
        {
            ExcelVegDET newDet = new ExcelVegDET(generalDet);
            newDet.myDet.OpenWorkbook(stream, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }

        public IDet Load(string filename)
        {
            ExcelVegDET newDet = new ExcelVegDET(generalDet);
            newDet.myDet.OpenWorkbook(filename, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }
    }
}
