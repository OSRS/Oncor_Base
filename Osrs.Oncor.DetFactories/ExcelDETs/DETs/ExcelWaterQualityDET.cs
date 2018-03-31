using System.Collections.Generic;
using System.IO;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Oncor.Excel;

namespace ExcelDETs.DETs
{
    public sealed class ExcelWaterQualityDET
    {
        private readonly WaterQualityDET generalDet;
        private readonly ExcelBaseDet myDet;

        public ExcelWaterQualityDET(WaterQualityDET det)
        {
            generalDet = det;
            myDet = new ExcelBaseDet(det.Id, det.Owner);
        }

        private DataTab SensorsDataTab()
        {
            return new DataTab("LIST_Sensors", XlColor.Orange, InstrumentDTO.GetSchema(), generalDet.Instruments.Values);
        }

        private DataTab SitesDataTab()
        {
            return new DataTab("LIST_Sites", XlColor.Orange, SiteDTO.GetSchema(), generalDet.Sites.Values);
        }

        private DataTab MeasurementsDataTab()
        {
            return new DataTab("DET_Measurements", XlColor.White, MeasurementDTO.GetSchema(), generalDet.Measurements.Values);
        }

        private DataTab DeploymentsDataTab()
        {
            return new DataTab("DET_Deployments", XlColor.White, DeploymentDTO.GetSchema(), generalDet.Deployments.Values);
        }

        private List<DataTab> DataTabList()
        {
            List<DataTab> list = new List<DataTab>();
            list.Add(DeploymentsDataTab());
            list.Add(MeasurementsDataTab());
            list.Add(SitesDataTab());
            list.Add(SensorsDataTab());
            return list;
        }

        private void LoadRow(string sheetName, Dictionary<string, string> values)
        {
            ValidationIssues issues = generalDet.ValidationIssues;
            if (sheetName == "DET_Deployments")
            {
                DeploymentDTO newDto = new DeploymentDTO(values);
                newDto.Validate();
                //issues.Merge(newDto.ValidationIssues);
                issues.Add(newDto.ValidationIssues.Collapse("Deployment "+newDto.DeployCode));
                bool success = generalDet.Deployments.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The deployment with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Measurements")
            {
                values["measuredatetime"] = ExcelBaseDet.ParseDate(values["measuredatetime"]);
                MeasurementDTO newDto = new MeasurementDTO(values);
                newDto.Validate();
                //issues.Merge(newDto.ValidationIssues);
                issues.Add(newDto.ValidationIssues.Collapse("Measurement "+newDto.DeployCode+" "+newDto.MeasureDateTime));
                bool success = generalDet.Measurements.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The measurment with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_Sites")
            {
                SiteDTO newDto = new SiteDTO(values);
                newDto.Validate();
                //issues.Merge(newDto.ValidationIssues);
                issues.Add(newDto.ValidationIssues.Collapse("Site "+newDto.Key));
                bool success = generalDet.Sites.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The site with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_Sensors")
            {
                InstrumentDTO newDto = new InstrumentDTO(values);
                newDto.Validate();
                //issues.Merge(newDto.ValidationIssues);
                issues.Add(newDto.ValidationIssues.Collapse("Instrument "+newDto.Key));
                bool success = generalDet.Instruments.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The instrument with code {0} is not unique.", newDto.LookupKey));
                }
            }
        }

        private void CheckSheetCount()
        {
            myDet.CheckOneSheet("DET_Deployments", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Measurements", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Sites", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Sensors", generalDet.ValidationIssues);
        }

        private void CheckHeaders(XlWorksheet worksheet)
        {
            if (worksheet!=null)
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
                for(int i=0;i<hdrs.Count-1;i++)
                {
                    string t = hdrs[i];
                    for(int j=i+1;j<hdrs.Count;j++)
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
            ExcelWaterQualityDET newDet = new ExcelWaterQualityDET(generalDet);
            newDet.myDet.OpenWorkbook(stream, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }

        public IDet Load(string filename)
        {
            ExcelWaterQualityDET newDet = new ExcelWaterQualityDET(generalDet);
            newDet.myDet.OpenWorkbook(filename, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }
    }
}
