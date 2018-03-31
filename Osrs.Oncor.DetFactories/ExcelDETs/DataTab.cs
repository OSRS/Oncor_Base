using System.Collections;
using System.Collections.Generic;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.Excel;

namespace ExcelDETs
{
    public class DataTab : IEnumerable<BaseDTO>
    {
        private readonly List<BaseDTO> _list;
        public string Name { get; private set; }
        public XlColor Color { get; private set; }
        public XlSchema Schema { get; }
        public int RowCount => _list.Count;

        public DataTab(string name, XlColor color, Schema schema, IEnumerable<BaseDTO> list)
        {
            Name = name;
            Color = color;
            Schema = MakeSchema(schema);
            _list = list == null ? new List<BaseDTO>() : new List<BaseDTO>(list);
        }

        public IEnumerator<BaseDTO> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(BaseDTO dto)
        {
            _list.Add(dto);
        }

        private XlSchema MakeSchema(Schema baseSchema)
        {
            XlSchema newSchema = new XlSchema();
            foreach (SchemaEntry column in baseSchema)
            {
                uint color = ConvertTypeToColor(column.ColumnType);
                newSchema.AddColumn(column.ColumnName, column.ValueType, color);
            }
            return newSchema;
        }

        private uint ConvertTypeToColor(SchemaEntryType columnType)
        {
            uint result;
            switch (columnType)
            {
                case SchemaEntryType.LocalLookupKey:
                case SchemaEntryType.ForeignLookupKey:
                    result = StyleSheetHelper.Orange;
                    break;
                case SchemaEntryType.LocalMeasurementKey:
                case SchemaEntryType.ForeignMeasurementKey:
                    result = StyleSheetHelper.Blue;
                    break;
                default:
                    result = StyleSheetHelper.Normal;
                    break;
            }
            return result;
        }
    }
}