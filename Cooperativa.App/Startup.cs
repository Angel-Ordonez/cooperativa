using Cooperativa.App.Domain.Data;
using Cooperativa.App.Engine;
using Cooperativa.App.Soluciones;
using Cooperativa.App.Soluciones.Pdf;
using Cooperativa.App.Utilidades;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cooperativa.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Este método es llamado por el tiempo de ejecución. Utilice este método para agregar servicios al contenedor.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cooperativa Version 1",
                    Description = "Sistema de Cooperativa",
                    Version = "v1"
                });
            });
            services.AddSwaggerGen();

            services.AddDbContextPool<CooperativaDbContext>(options =>
                                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IExchangeratesService, ExchangeratesService>();
            services.AddTransient<ICalculationService, CalculationService>();
            services.AddTransient<IQRServices, QRServices>();
            services.AddTransient<IUtilidadesBase, UtilidadesBase>();
            services.AddTransient<IPdfPimService, PdfPimService>();

            #region  Para no registrar los services/Engine asi, ChatGpt me recomienda que instale la libreria Scrutor

            //// Registrar controladores y demás
            //services.AddControllers();

            //// Escaneo y registro automático de todos los servicios que implementen interfaces
            //services.Scan(scan => scan
            //    .FromAssemblyOf<IUtilidadesBase>() // o de cualquier clase del ensamblado
            //    .AddClasses()                       // busca todas las clases públicas
            //    .AsImplementedInterfaces()          // registra cada clase con su interfaz
            //    .WithTransientLifetime());          // usa AddTransient
            #endregion



            services.AddMediatR(typeof(Startup));

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.FullName);  // O cualquier lógica que garantice unicidad
            });

            //services.AddRazorPages();
        }

        // Este método es llamado por el tiempo de ejecución. Utilice este método para configurar la canalización de solicitudes HTTP.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cooperativav1"));

                //app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
          //  app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
