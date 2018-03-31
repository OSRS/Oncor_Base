using System.Collections.Generic;
using System.IO;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Oncor.Excel;

namespace ExcelDETs.DETs
{
    public sealed class ExcelFishDET
    {
        private readonly FishDET generalDet;
        private readonly ExcelBaseDet myDet;

        public ExcelFishDET(FishDET det)
        {
            generalDet = det;
            myDet = new ExcelBaseDet(det.Id, det.Owner);
        }

        private DataTab CatchEffortsDataTab()
        {
            return new DataTab("DET_CatchEffort", XlColor.White, CatchEffortDTO.GetSchema(), generalDet.CatchEfforts.Values);
        }

        private DataTab NetHaulEventsDataTab()
        {
            return new DataTab("DET_NetHaulEvent", XlColor.White, NetHaulEventDTO.GetSchema(), generalDet.NetHaulEvents.Values);
        }

        private DataTab FishCountsDataTab()
        {
            return new DataTab("DET_FishCount", XlColor.White, FishCountDTO.GetSchema(), generalDet.FishCounts.Values);
        }

        private DataTab CatchMetricsDataTab()
        {
            return new DataTab("DET_CatchMetric", XlColor.White, CatchMetricDTO.GetSchema(), generalDet.CatchMetrics.Values);
        }

        private DataTab FishDataTab()
        {
            return new DataTab("DET_Fish", XlColor.White, FishDTO.GetSchema(), generalDet.Fish.Values);
        }

        private DataTab IdTagsDataTab()
        {
            return new DataTab("DET_IdTags", XlColor.White, IdTagDTO.GetSchema(), generalDet.IdTags.Values);
        }

        private DataTab GeneticsDataTab()
        {
            return new DataTab("DET_Genetics", XlColor.White, GeneticDTO.GetSchema(), generalDet.Genetics.Values);
        }

        private DataTab DietDataTab()
        {
            return new DataTab("DET_Diet", XlColor.White, DietDTO.GetSchema(), generalDet.Diet.Values);
        }

        private DataTab FishSpeciesDataTab()
        {
            return new DataTab("LIST_FishSpecies", XlColor.Orange, FishSpeciesDTO.GetSchema(), generalDet.FishSpecies.Values);
        }

        private DataTab MacroSpeciesDataTab()
        {
            return new DataTab("LIST_MacroSpecies", XlColor.Orange, MacroSpeciesDTO.GetSchema(), generalDet.MacroSpecies.Values);
        }

        private DataTab SitesDataTab()
        {
            return new DataTab("LIST_Sites", XlColor.Orange, SiteDTO.GetSchema(), generalDet.Sites.Values);
        }

        private DataTab NetsDataTab()
        {
            return new DataTab("LIST_Nets", XlColor.Orange, NetDTO.GetSchema(), generalDet.Nets.Values);
        }

        private List<DataTab> DataTabList()
        {
            List<DataTab> list = new List<DataTab>();
            list.Add(CatchEffortsDataTab());
            list.Add(NetHaulEventsDataTab());
            list.Add(FishCountsDataTab());
            list.Add(CatchMetricsDataTab());
            list.Add(FishDataTab());
            list.Add(IdTagsDataTab());
            list.Add(GeneticsDataTab());
            list.Add(DietDataTab());
            list.Add(FishSpeciesDataTab());
            list.Add(MacroSpeciesDataTab());
            list.Add(SitesDataTab());
            list.Add(NetsDataTab());
            return list;
        }

        private void LoadRow(string sheetName, Dictionary<string, string> values)
        {
            ValidationIssues issues = generalDet.ValidationIssues;
            if (sheetName == "DET_CatchEffort")
            {
                CatchEffortDTO newDto = new CatchEffortDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.CatchEfforts.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The catch effort with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_NetHaulEvent")
            {
                NetHaulEventDTO newDto = new NetHaulEventDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.NetHaulEvents.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The net haul event with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_FishCount")
            {
                FishCountDTO newDto = new FishCountDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.FishCounts.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The fish count with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_CatchMetric")
            {
                CatchMetricDTO newDto = new CatchMetricDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.CatchMetrics.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The catch metric with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Fish")
            {
                FishDTO newDto = new FishDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Fish.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The fish with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_IdTags")
            {
                IdTagDTO newDto = new IdTagDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.IdTags.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The ID tag with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Genetics")
            {
                GeneticDTO newDto = new GeneticDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Genetics.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The genetic with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Diet")
            {
                DietDTO newDto = new DietDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Diet.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The diet with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_FishSpecies")
            {
                FishSpeciesDTO newDto = new FishSpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.FishSpecies.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The fish-species with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_MacroSpecies")
            {
                MacroSpeciesDTO newDto = new MacroSpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.MacroSpecies.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The macro-species with code {0} is not unique.", newDto.LookupKey));
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
            else if (sheetName == "LIST_Nets")
            {
                NetDTO newDto = new NetDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Nets.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The net with code {0} is not unique.", newDto.LookupKey));
                }
            }
        }

        private void CheckSheetCount()
        {
            myDet.CheckOneSheet("DET_CatchEffort", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_NetHaulEvent", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_FishCount", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_CatchMetric", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Fish", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_IdTags", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Genetics", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Diet", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_FishSpecies", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_MacroSpecies", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Sites", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Nets", generalDet.ValidationIssues);
        }

        private void CheckHeaders(XlWorksheet worksheet)
        {
            if (worksheet != null)
            {
                Schema s;
                List<string> h;
                if (worksheet.Name == "DET_CatchEffort")
                {
                    s = CatchEffortDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_NetHaulEvent")
                {
                    s = NetHaulEventDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_FishCount")
                {
                    s = FishCountDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_CatchMetric")
                {
                    s = CatchMetricDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_Fish")
                {
                    s = FishDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_IdTags")
                {
                    s = IdTagDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_Genetics")
                {
                    s = GeneticDTO.GetSchema();
                }
                else if (worksheet.Name == "DET_Diet")
                {
                    s = DietDTO.GetSchema();
                }
                else if (worksheet.Name == "LIST_FishSpecies")
                {
                    s = FishSpeciesDTO.GetSchema();
                }
                else if (worksheet.Name == "LIST_MacroSpecies")
                {
                    s = MacroSpeciesDTO.GetSchema();
                }
                else if (worksheet.Name == "LIST_Sites")
                {
                    s = SiteDTO.GetSchema();
                }
                else if (worksheet.Name == "LIST_Nets")
                {
                    s = NetDTO.GetSchema();
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
            ExcelFishDET newDet = new ExcelFishDET(generalDet);
            newDet.myDet.OpenWorkbook(stream, newDet.LoadRow, newDet.CheckSheetCount, CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }

        public IDet Load(string filename)
        {
            ExcelFishDET newDet = new ExcelFishDET(generalDet);
            newDet.myDet.OpenWorkbook(filename, newDet.LoadRow, newDet.CheckSheetCount, CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }
    }
}
