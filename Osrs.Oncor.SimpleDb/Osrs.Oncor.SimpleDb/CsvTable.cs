using System;
using System.IO;
using Osrs.Data;

namespace Osrs.Oncor.SimpleDb
{
    public sealed class CsvTable : ITable
    {
        private readonly TextWriter writer;
        private readonly int colCount;

        public bool AddRow(IRow row)
        {
            if (row != null && row is CsvRow)
                return this.AddRow((CsvRow)row);
            return false;
        }

        public bool AddRow(CsvRow row)
        {
            if (row!=null && row.colCount==this.colCount)
            {
                this.writer.Write(row[0]);
                for(int i=1;i<row.colCount;i++)
                {
                    this.writer.Write(',');
                    this.writer.Write(row[i]);
                }
                this.writer.WriteLine();
                return true;
            }
            return false;
        }

        public IRow CreateRow()
        {
            return new CsvRow(this.colCount);
        }

        public bool Flush()
        {
            try
            {
                this.writer.Flush();
                return true;
            }
            catch(Exception e)
            { }
            return false;
        }

        internal CsvTable(int colCt, Stream s)
        {
            this.writer = new StreamWriter(s);
            this.colCount = colCt;
        }
    }

    public sealed class CsvRow : IRow
    {
        internal readonly int colCount;
        internal readonly string[] values;

        public string this[int index]
        {
            get
            {
                return this.values[index];
            }

            set
            {
                this.values[index] = ConvertUtils.F(value);
            }
        }

        public bool Write(string[] values)
        {
            return Write(values, false);
        }

        public bool Write(string[] values, bool preQuoted)
        {
            if (values != null && values.Length == this.colCount)
            {
                if (preQuoted)
                {
                    for (int i = 0; i < this.values.Length; i++)
                    {
                        this.values[i] = values[i];
                    }
                }
                else
                {
                    for (int i = 0; i < this.values.Length; i++)
                    {
                        this.values[i] = ConvertUtils.F(values[i]);
                    }
                }
                return true;
            }
            return false;
        }

        public void S(int index, string raw)
        {
            S(index, raw, false);
        }

        public void S(int index, string raw, bool preQuoted)
        {
            if (preQuoted)
                this.values[index] = raw;
            else
                this.values[index] = ConvertUtils.F(raw);
        }

        public void S(int index, CompoundIdentity i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, Guid i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, DateTime i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, int i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, long i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, float i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, double i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, uint i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        public void S(int index, ulong i)
        {
            this.values[index] = ConvertUtils.F(i);
        }

        internal CsvRow(int colCount)
        {
            this.colCount = colCount;
            this.values = new string[this.colCount];
        }
    }
}
