using CutUsage;

var builder = WebApplication.CreateBuilder(args);

// register your two repos so they can be injected
builder.Services.AddScoped<LayRepository>();
builder.Services.AddScoped<MarkerRepository>();
builder.Services.AddScoped<MarkerPlanRepository>();

builder.Services.AddControllersWithViews();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Docket}/{action=SelectDocket}/{id?}");

app.Run();

