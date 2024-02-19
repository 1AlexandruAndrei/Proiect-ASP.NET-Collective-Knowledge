using CollectionKnowledgeProject.Data;
using CollectionKnowledgeProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
options.SignIn.RequireConfirmedAccount = true)
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>(
);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
*/

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
var services = scope.ServiceProvider;
SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "homeRoute",
    pattern: "/",
    defaults: new { controller = "Categories", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "questions.show",
    pattern: "{controller=Questions}/{action=Show}/{id}");

app.MapControllerRoute(
    name: "questions.new",
    pattern: "{controller=Questions}/{action=New}");

app.MapControllerRoute(
    name: "questions.index",
    pattern: "{controller=Questions}/{action=Index}");

app.MapControllerRoute(
    name: "questions.delete",
    pattern: "{controller=Questions}/{action=Delete}/{id}");

app.MapControllerRoute(
    name: "questions.edit",
    pattern: "{controller=Questions}/{action=Edit}/{id}");

app.MapControllerRoute(
    name: "comments.delete",
    pattern: "{controller=Comments}/{action=Delete}/{id}");

app.MapControllerRoute(
    name: "comments.edit",
    pattern: "{controller=Comments}/{action=Edit}/{id}");

app.MapControllerRoute(
    name: "questions.change-category",
    pattern: "{controller=Questions}/{action=ChangeCategory}/{questionId}/{categoryId}");

app.MapRazorPages();

app.Run();
