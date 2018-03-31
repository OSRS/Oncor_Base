namespace Osrs.Oncor.Excel
{
    public class XlCustomProperty
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public XlCustomProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}