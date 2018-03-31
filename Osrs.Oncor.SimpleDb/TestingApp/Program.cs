using Osrs.Data;
using Osrs.Oncor.SimpleDb;
using System;
using System.IO;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string fil = "B:\\Data\\test.zip";
            FileStream s = File.Open(fil, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            CsvDb tmp = CsvDb.Create(s);
            CreateTable(tmp, "mytab1");
            CreateTable(tmp, "my tabs\\my tab2.off");
            tmp.Flush();
            tmp.Dispose();
        }

        static void CreateTable(CsvDb db, string name)
        {
            string[] cols = new string[] { "ColA", "ColB", "Col\"Tricky", "Col S" };

            ITable tab = db.Create(name, cols);

            string[] vals = new string[] { "klsdkfs", "1231432", "sdfsdj' sdf'' sdf\"sdfs\"'sd sd ", ConvertUtils.F(DateTime.Now)};

            for (int i = 0; i < 1000; i++)
            {
                IRow r = tab.CreateRow();

                r.Write(vals);
                tab.AddRow(r);
            }
            tab.Flush();

            for(int i=0;i<1000;i++)
            {
                IRow r = tab.CreateRow();

                r[0] = ConvertUtils.F(new CompoundIdentity(Guid.NewGuid(), Guid.NewGuid()));
                r[1] = ConvertUtils.F((float)i);
                r[2] = "Hoos " + i;
                r[3] = ConvertUtils.F(Math.PI);

                tab.AddRow(r);
            }
            tab.Flush();

            for (int i = 0; i < 1000; i++)
            {
                IRow r = tab.CreateRow();

                r.S(0, new CompoundIdentity(Guid.NewGuid(), Guid.NewGuid()));
                r.S(1, (float)i);
                r.S(2, "Hoos " + i);
                r.S(3, Math.PI);

                tab.AddRow(r);
            }
            tab.Flush();
        }
    }
}
