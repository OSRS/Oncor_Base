using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osrs.Oncor.Wellknown.Persons
{
    public static class PersonUtils
    {
		public static readonly Guid PersonCreatePermissionId = new Guid("{5EADD9AA-40D4-46E5-8705-E33CF785A5FA}");
		public static readonly Guid PersonGetPermissionId = new Guid("{3A7E74BC-53D3-46DD-80DE-05149A94362D}");
		public static readonly Guid PersonUpdatePermissionId = new Guid("{AC2AEEF7-90DD-4EDC-8220-AD0534FD9FFD}");
		public static readonly Guid PersonDeletePermissionId = new Guid("{982F705D-0A34-4FBF-BEAE-73D55E5B2CC2}");
    }
}
