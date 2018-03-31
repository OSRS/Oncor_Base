using System;
using System.IO;
using System.IO.Compression;

namespace Osrs.Oncor.SimpleDb
{
    public sealed class CsvDb : IDb
    {
        private readonly ZipArchive source;

        public ITable Create(string name, string[] columnHeaders)
        {
            if (!string.IsNullOrEmpty(name) && columnHeaders != null || columnHeaders.Length > 0)
            {
                try
                {
                    if (!Path.HasExtension(name))
                        name = name + ".csv";
                    ZipArchiveEntry fil = this.source.CreateEntry(name);
                    Stream s = fil.Open();
                    if (s != null)
                    {
                        CsvTable tab = new CsvTable(columnHeaders.Length, s);
                        IRow head = tab.CreateRow();
                        head.Write(columnHeaders);
                        tab.AddRow(head);
                        return tab;
                    }
                }
                catch (Exception e)
                { }
            }
            return null;
        }

        public void Dispose()
        {
            try
            {
                this.source.Dispose();
            }
            catch { }
        }

        public bool Exists(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach(ZipArchiveEntry e in this.source.Entries)
                {
                    if (e.Name.Equals(name))
                        return true;
                }
            }
            return false;
        }

        public bool Flush()
        {
            return true;
        }

        private CsvDb(ZipArchive s)
        {
            this.source = s;
        }

        public static CsvDb Create(Stream baseStream)
        {
            if (baseStream!=null)
            {
                try
                {
                    ZipArchive source = new ZipArchive(baseStream, ZipArchiveMode.Update, true);
                    return new CsvDb(source);
                }
                catch
                { }

            }
            return null;
        }
    }
}
