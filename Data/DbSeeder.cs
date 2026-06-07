using Microsoft.AspNetCore.Identity;
using NexusIMS.API.Entities;

namespace NexusIMS.API.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbSeeder(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task SeedAllAsync()
        {
            var defaultWarehouse = await SeedWarehousesAsync();
            await SeedRolesOnlyAsync();
            await SeedAdminUserAsync(defaultWarehouse.Id);
        }
        private async Task<Warehouse> SeedWarehousesAsync()
        {
            var warehouse = _context.Warehouses.FirstOrDefault();
            if (warehouse == null)
            {
                warehouse = new Warehouse
                {
                    Name = "المخزن الرئيسي",
                    Location = "الفرع الرئيسي - القاهرة"
                };
                await _context.Warehouses.AddAsync(warehouse);
                await _context.SaveChangesAsync();
            }
            return warehouse;
        }

        private async Task SeedRolesOnlyAsync()
        {
            string[] roles = { "Admin", "Manager", "GeneralAccountant", "Storekeeper", "Cashier" };
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }


        private async Task SeedAdminUserAsync(int defaultWarehouseId)
        {
            var adminEmail = "admin@gmail.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = "NexusAdmin",
                    FullName = "Hisham Rifaay",
                    Email = adminEmail,
                    WarehouseId = defaultWarehouseId,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "Admin");
                }
            }
        }

    }
}

//{
//    "fullName": "Ahmed Mohamed",
//  "email": "ahmed@NexusIms.com",
//  "phone": "01025420441",
//  "password": "Ahmed@123",
//  "warehouseId": 2,
//  "role": "Storekeeper"
//}

//{
//    "fullName": "Osama Khaled",
//  "email": "osa@NexusIms.com",
//  "phone": "01025420400",
//  "password": "Osama@123",
//  "warehouseId": 2,
//  "role": "Cashier"
//}
