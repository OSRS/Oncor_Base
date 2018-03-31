using Osrs.Security.Authorization;
using System;

namespace Osrs.Oncor.EntityBundles
{
    public static class EntityBundleUtils
    {
        public static readonly Guid EntityBundleCreatePermissionId = new Guid("{AA72AF5B-8C1F-4A61-B6F6-8436FAF55711}");
        public static readonly Guid EntityBundleGetPermissionId = new Guid("{2D43A884-237D-4F08-8F53-E719102095A2}");
        public static readonly Guid EntityBundleUpdatePermissionId = new Guid("{523DF458-C983-48BC-B37B-DA065AC0F975}");
        public static readonly Guid EntityBundleDeletePermissionId = new Guid("{C326E119-DAC9-42B0-8F6E-7B60967671B9}");

        public static Permission EntityBundleCreatePermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Create, "EntityBundle"), EntityBundleUtils.EntityBundleCreatePermissionId);
            }
        }

        public static Permission EntityBundleGetPermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Retrive, "EntityBundle"), EntityBundleUtils.EntityBundleGetPermissionId);
            }
        }
        public static Permission EntityBundleUpdatePermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Update, "EntityBundle"), EntityBundleUtils.EntityBundleUpdatePermissionId);
            }
        }
        public static Permission EntityBundleDeletePermission
        {
            get
            {
                return new Permission(PermissionUtils.PermissionName(OperationType.Delete, "EntityBundle"), EntityBundleUtils.EntityBundleDeletePermissionId);
            }
        }
    }
}
