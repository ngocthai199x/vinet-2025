using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data;

public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context, UserManager<AppUser> userManager)
    {
        if(!userManager.Users.Any(x=>x.UserName == "admin@test.com"))
        {
            var user = new AppUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com"
            };
            await userManager.CreateAsync(user, "Luongthai@123");
            await userManager.AddToRoleAsync(user, "Admin");
        }
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if(!context.Products.Any())
        {
            //var productsData = await File.ReadAllTextAsync("../Infrastructure/Data/SeedData/products.json");
            var productsData = await File.ReadAllTextAsync(path +@"/Data/SeedData/products.json");
            var products = JsonSerializer.Deserialize<List<Product>>(productsData);
            if(products == null) return;
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        if(!context.DeliveryMethods.Any())
        {
            //var deliveryMethodsData = await File.ReadAllTextAsync("../Infrastructure/Data/SeedData/delivery.json");
            var deliveryMethodsData = await File.ReadAllTextAsync(path + @"/Data/SeedData/delivery.json");
            var methods = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethodsData);
            if(methods == null) return;
            context.DeliveryMethods.AddRange(methods);
            await context.SaveChangesAsync();
        }
    }
}
