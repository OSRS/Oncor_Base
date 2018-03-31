using System;
using System.Collections.Generic;
using System.IO;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Oncor.Excel;

namespace ExcelDETs.DETs
{
    public sealed class ExcelCrossSectionDET
    {
        private readonly CrossSectionDET generalDet;
        private readonly ExcelBaseDet myDet;

        public ExcelCrossSectionDET(CrossSectionDET det)
        {
            generalDet = det;
            myDet = new ExcelBaseDet(det.Id, det.Owner);
        }

        private DataTab SurveysDataTab()
        {
            return new DataTab("DET_Surveys", XlColor.White, CrossSectionSurveyDTO.GetSchema(), generalDet.Surveys.Values);
        }

        private DataTab ElevationsDataTab()
        {
            return new DataTab("DET_Elevations", XlColor.White, CrossSectionElevationDTO.GetSchema(), generalDet.Elevations.Values);
        }

        private DataTab InstrumentsDataTab()
        {
            return new DataTab("LIST_Instruments", XlColor.Orange, InstrumentDTO.GetSchema(), generalDet.Instruments.Values);
        }

        private DataTab SitesDataTab()
        {
            return new DataTab("LIST_Sites", XlColor.Orange, SiteDTO.GetSchema(), generalDet.Sites.Values);
        }

        private List<DataTab> DataTabList()
        {
            List<DataTab> list = new List<DataTab>();
            list.Add(SurveysDataTab());
            list.Add(ElevationsDataTab());
            list.Add(InstrumentsDataTab());
            list.Add(SitesDataTab());
            return list;
        }

        private void LoadRow(string sheetName, Dictionary<string, string> values)
        {
            ValidationIssues issues = generalDet.ValidationIssues;
            if (sheetName == "DET_Surveys")
            {
                CrossSectionSurveyDTO newDto = new CrossSectionSurveyDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Surveys.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The survey with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Elevations")
            {
                CrossSectionElevationDTO newDto = new CrossSectionElevationDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Elevations.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The elevation with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_Instruments")
            {
                InstrumentDTO newDto = new InstrumentDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Instruments.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The instrument with code {0} is not unique.", newDto.LookupKey));
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
        }

        private void CheckSheetCount()
        {
            myDet.CheckOneSheet("DET_Surveys", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Elevations", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Instruments", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Sites", generalDet.ValidationIssues);
        }

        private void CheckHeaders(XlWorksheet worksheet)
        {
            if (worksheet != null)
            {
                Schema s=null;
                List<string> h;
                //if (worksheet.Name == "DET_Deployments")
                //{
                //    s = DeploymentDTO.GetSchema();
                //}
                //else
                //    return;

                //h = new List<string>();
                //foreach (SchemaEntry c in s)
                //{
                //    h.Add(c.ColumnName);
                //}
                //if (!ExcelBaseDet.HasHeaders(ExcelBaseDet.Headers(worksheet), h))
                //{
                //    generalDet.ValidationIssues.Add(ValidationIssue.Code.MissingFieldHeader, "Missing column header in " + worksheet.Name);
                //}
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
            ExcelCrossSectionDET newDet = new ExcelCrossSectionDET(generalDet);
            newDet.myDet.OpenWorkbook(stream, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }

        public IDet Load(string filename)
        {
            ExcelCrossSectionDET newDet = new ExcelCrossSectionDET(generalDet);
            newDet.myDet.OpenWorkbook(filename, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }
    }
}
