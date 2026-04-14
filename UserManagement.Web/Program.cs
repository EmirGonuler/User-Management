using UserManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// Register HttpClient for UserApiService
// BaseAddress is read from appsettings.json — no hardcoded URLs
builder.Services.AddHttpClient<UserApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException(
                      "ApiSettings:BaseUrl is not configured in appsettings.json");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── App Pipeline ────────────────────────────────────────
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

// Default route — go straight to Users list
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}");

app.Run();
