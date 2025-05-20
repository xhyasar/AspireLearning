using System.Collections.Generic;

namespace AspireLearning.ServiceDefaults.GlobalConstant
{
    public static class Permissions
    {
        public const string ClaimType = "Permission";

        public static class Product
        {
            public const string Read = "Product_Read";
            public const string Add = "Product_Add";
            public const string Update = "Product_Update";
            public const string Delete = "Product_Delete";

            // allocated once, then reused
            public static readonly IReadOnlyList<string> All =
            [
                Read,
                Add,
                Update,
                Delete
            ];
        }

        public static class Warehouse
        {
            public const string Read = "Warehouse_Read";
            public const string Add = "Warehouse_Add";
            public const string Update = "Warehouse_Update";
            public const string Delete = "Warehouse_Delete";

            public static readonly IReadOnlyList<string> All =
            [
                Read,
                Add,
                Update,
                Delete
            ];
        }

        public static class Stock
        {
            public const string Read = "Stock_Read";
            public const string Add = "Stock_Add";
            public const string Update = "Stock_Update";
            public const string Delete = "Stock_Delete";

            public static readonly IReadOnlyList<string> All =
            [
                Read,
                Add,
                Update,
                Delete
            ];
        }

        public static class UserManagement
        {
            public const string Read = "UserManagement_Read";
            public const string Add = "UserManagement_Add";
            public const string Update = "UserManagement_Update";
            public const string Delete = "UserManagement_Delete";

            public static readonly IReadOnlyList<string> All =
            [
                Read,
                Add,
                Update,
                Delete
            ];
        }

        public static readonly IReadOnlyList<string> AllPermissions =
            Product.All
            .Concat(Warehouse.All)
            .Concat(Stock.All)
            .Concat(UserManagement.All)
            .ToList()
            .AsReadOnly();
    }
}