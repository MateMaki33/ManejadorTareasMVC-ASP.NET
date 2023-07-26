using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;
using TareasMVC.Servicios;

namespace TareasMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //creamos politica de autenticación de usuarios y la pasamos a MVC
            var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Add services to the container.
            //----------------------------------------------------------------------------------------
            builder.Services.AddControllersWithViews(opciones =>
            {
                opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));

                //Traduccion de vistas a otros lenguajes
            }).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)

            //Traduccion de datos, errores. Permite usar un archivo para traducir
            .AddDataAnnotationsLocalization(opciones =>
            {
                opciones.DataAnnotationLocalizerProvider = (_, factoria) => factoria.Create(typeof(RecursoCompartido));
            }).AddJsonOptions(opciones =>
            {
                opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            //Configuracion del servicio DbContext a traves de la clase ApplicationDbContext
            builder.Services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseSqlServer("name=DefaultConnection"));

            builder.Services.AddAuthentication().AddMicrosoftAccount(opciones =>

            {   //Servicios de autenticacion para login con cuenta Microsoft
                opciones.ClientId = builder.Configuration["MicrosoftClientId"];
                opciones.ClientSecret = builder.Configuration["MicrosoftSecretId"];
            });

            /*
             Ahora añadimos servicio de Identity. Tengo que pasarle la clase que representa a los usuarios 
            En este caso Utilizamos IdentityUser, que es una clase por defecto de Identity y IdentityRole que
            es una clase que representa un rol.
             */
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
            {
                opciones.SignIn.RequireConfirmedAccount = false;

                //Configuramos almacenamiento pasandole el contexto de nuestra BD

            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();


            //Para que EFC no utilice vistas por defecto de Login, configuramos para direccionar a donde queremos
            builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
                opciones =>
                {
                    opciones.LoginPath = "/usuarios/login";
                    opciones.AccessDeniedPath = "/usuarios/login"; //Si se deniega el acceso a una ruta devovemos al login
                });

            // Globalizacion. Configuramos tambien con la carpeta de recursos creada
            builder.Services.AddLocalization(opciones =>
            {
                opciones.ResourcesPath = "Recursos";
            });

            builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();

            //----------------------------------------------------------------------------------------------------------------
            var app = builder.Build();

            //Añadimos varios lenguajes para la localizacion

            
            app.UseRequestLocalization(opciones =>
            {
            opciones.DefaultRequestCulture = new RequestCulture("es");

            //pasamos una lista a partir del array creado a SupportedUICultures
            opciones.SupportedUICultures = Constantes.CulturasUISoportadas.Select(cultura=> new CultureInfo(cultura.Value)).ToList();
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication(); //Obtener datos del User
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}