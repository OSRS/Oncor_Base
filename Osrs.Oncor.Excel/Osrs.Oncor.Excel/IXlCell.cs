using System;

namespace Osrs.Oncor.Excel
{
    public interface IXlCell
    {
        Type CellType { get; }
        uint CellStyle { get; }
        string CellValue { get; }
    }
}