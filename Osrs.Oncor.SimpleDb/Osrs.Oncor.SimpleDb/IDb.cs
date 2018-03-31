using Osrs.Data;
using Osrs.Numerics;
using System;

namespace Osrs.Oncor.SimpleDb
{
    public interface IDb
    {
        bool Exists(string name);
        ITable Create(string name, string[] columnHeaders);

        bool Flush();

        void Dispose();
    }

    public interface ITable
    {
        IRow CreateRow();

        bool AddRow(IRow row);

        bool Flush();
    }

    public interface IRow
    {
        bool Write(string[] values);

        bool Write(string[] values, bool preQuoted);

        string this[int index]{ get; set;}

        void S(int index, string raw);
        void S(int index, string raw, bool preQuoted);
        void S(int index, CompoundIdentity i);

        void S(int index, Guid i);

        void S(int index, DateTime i);

        void S(int index, int i);

        void S(int index, long i);

        void S(int index, float i);

        void S(int index, double i);

        void S(int index, uint i);

        void S(int index, ulong i);
    }

    public static class ConvertUtils
    {
        public static string F(string raw)
        {
            if (raw == null)
                return "";
            else if (raw.Length>0)
                raw = '\"' + raw.Replace("'", "''").Replace('\"', '\'') + '\"';
            return raw;
        }

        public static string F(CompoundIdentity i)
        {
            return "\"" + i.DataStoreIdentity.ToString() + ":" + i.Identity.ToString() + "\"";
        }

        public static string F(Guid i)
        {
            return "\"" + i.ToString() + "\"";
        }

        public static string F(DateTime i)
        {
            return "\"" + FixDate(i).ToString("s") + "\"";
        }

        public static string F(int i)
        {
            return i.ToString();
        }

        public static string F(long i)
        {
            return i.ToString();
        }

        public static string F(float i)
        {
            if (!MathUtils.IsInfiniteOrNaN(i))
                return i.ToString();
            if (float.IsNaN(i))
                return "\"NaN\"";
            if (float.IsPositiveInfinity(i))
                return "\"+INF\"";
            return "\"-INF\"";
        }

        public static string F(double i)
        {
            if (!MathUtils.IsInfiniteOrNaN(i))
                return i.ToString();
            if (double.IsNaN(i))
                return "\"NaN\"";
            if (double.IsPositiveInfinity(i))
                return "\"+INF\"";
            return "\"-INF\"";
        }

        public static string F(uint i)
        {
            return i.ToString();
        }

        public static string F(ulong i)
        {
            return i.ToString();
        }

        internal static DateTime FixDate(DateTime when)
        {
            if (when.Kind == DateTimeKind.Local)
                when = when.ToUniversalTime();
            if (when.Kind == DateTimeKind.Unspecified)
                when = new DateTime(when.Ticks, DateTimeKind.Utc); //assume UTC

            DateTime now = DateTime.UtcNow;
            if (now.Kind == DateTimeKind.Unspecified)
                now = new DateTime(now.Ticks, DateTimeKind.Utc);
            if (when > now)
                return now;
            return when;
        }
    }
}
