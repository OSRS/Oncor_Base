//Copyright 2017 Open Science, Engineering, Research and Development Information Systems Open, LLC. (OSRS Open)
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Osrs.Security.Authorization;

namespace Osrs.Oncor.WellKnown.Vegetation.Module
{
    public static class VegModuleUtils
    {
        public static Permission CreatePermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Create, "Vegetation"), VegUtils.CreatePermissionId);
            }
        }

        public static Permission GetPermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Retrive, "Vegetation"), VegUtils.GetPermissionId);
            }
        }
        public static Permission UpdatePermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Update, "Vegetation"), VegUtils.UpdatePermissionId);
            }
        }
        public static Permission DeletePermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Delete, "Vegetation"), VegUtils.DeletePermissionId);
            }
        }
    }
}
