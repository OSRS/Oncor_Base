using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Dets
{
    internal sealed class KnownDets
    {
        private readonly HashSet<string> names = new HashSet<string>();
        internal readonly string WQ = "wq";
        internal readonly string Fish = "fish";
        internal readonly string Veg = "veg";
        internal readonly string SedAcc = "sedimentaccretion";
        internal readonly string Photo = "photopoint";

        public string Clean(string value)
        {
            if (value != null)
            {
                value = value.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    return value.ToLowerInvariant();
                }
            }
            return null;
        }

        public bool IsValid(string value)
        {
            value = Clean(value);
            if (value!=null)
                return names.Contains(value);
            return false;
        }

        internal static KnownDets Instance
        {
            get { return instance; }
        }
        private static readonly KnownDets instance = new KnownDets();

        private KnownDets()
        {
            names.Add(WQ); //water quality DET
            names.Add(Fish); //fish DET
            names.Add(Veg); //vegetation DET
            //names.Add(SedAcc); //sediment accretion DET
            //names.Add(Photo); //photo point DET
        }
    }
}
