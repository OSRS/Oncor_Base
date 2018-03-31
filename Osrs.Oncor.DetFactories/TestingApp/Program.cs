using System;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string fname = "B:\\data\\tmp\\WQ1_WQ_single_year.xlsx"; //"B:\\data\\tmp\\WQ_data_sample.xlsx";
            //TestWaterQuality.WriteFile("B:\\data\\tmp\\WQ1_WQ_single_yearTEST.xlsx");
            //PauseForEffect("writing WaterQuality");
            TestWaterQuality.ReadFile(fname);
            PauseForEffect("reading WaterQuality");
            //TestExcelWaterQuality();
            //TestExcelFish();
            //TestExcelCrossSection();
            //TestExcelPreyAvailability();
            //TestExcelSedimentAccretion();
        }

        private static void TestExcelVeg()
        {

        }

        private static void TestExcelCrossSection()
        {
            string fName = "C:\\Data\\OncorDet_CrossSection_Template.xlsx";
            TestCrossSection.WriteFile(fName);
            PauseForEffect("writing CrossSection");
            TestCrossSection.ReadFile(fName);
            PauseForEffect("reading CrossSection");
        }

        private static void TestExcelPreyAvailability()
        {
            string fName = "C:\\Data\\OncorDet_PreyAvailability_Template.xlsx";
            Test_PreyAvailability.WriteFile(fName);
            PauseForEffect("writing PreyAvailability");
            Test_PreyAvailability.ReadFile(fName);
            PauseForEffect("reading PreyAvailability");
        }

        private static void TestExcelSedimentAccretion()
        {
            string fName = "C:\\Data\\OncorDet_SedimentAccretion_Template.xlsx";
            TestSedimentAccretion.WriteFile(fName);
            PauseForEffect("writing SedimentAccretion");
            TestSedimentAccretion.ReadFile(fName);
            PauseForEffect("reading SedimentAccretion");
        }

        private static void TestExcelFish()
        {
            string fName = "C:\\Data\\OncorDet_Fish_Template.xlsx";
            TestFish.WriteFile(fName);
            PauseForEffect("writing Fish");
            TestFish.ReadFile(fName);
            PauseForEffect("reading Fish");
        }

        private static void TestExcelWaterQuality()
        {
            string fName = "C:\\Data\\OncorDet_WaterQuality_Template.xlsx";
            TestWaterQuality.WriteFile(fName);
            PauseForEffect("writing WaterQuality");
            TestWaterQuality.ReadFile(fName);
            PauseForEffect("reading WaterQuality");
        }

        private static void PauseForEffect(string name)
        {
            Console.Write("Finished {0} hit enter to continue -->", name);
            Console.ReadLine();
        }
    }
}
