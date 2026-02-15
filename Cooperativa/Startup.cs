using Cooperativa.App.Domain.Data;
using Cooperativa.App.Engine;
using Cooperativa.App.Soluciones;
using Cooperativa.App.Soluciones.Pdf;
using Cooperativa.App.Utilidades;
using Hangfire;
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

namespace Cooperativa
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
                    Title = "Coopez Version 1",
                    Description = "Sistema de Cooperativa",
                    Version = "v1"
                });
            });
            services.AddSwaggerGen();
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.FullName);  //Esto me permite usar el Command y CommandHadler repetido en todos mis metodos
            });

            services.AddDbContextPool<CooperativaDbContext>(options =>
                                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //Configuración de Hangfire (Dejar activiades trabajando en segundo plano)
            services.AddHangfire(config => config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();

            //Engine
            services.AddTransient<IExchangeratesService, ExchangeratesService>();
            services.AddTransient<ICalculationService, CalculationService>();
            services.AddTransient<IQRServices, QRServices>();
            services.AddTransient<IUtilidadesBase, UtilidadesBase>();
            services.AddTransient<IPdfPimService, PdfPimService>();
            services.AddTransient<INotificacionesEngine, NotificacionesEngine>();

            services.AddMediatR(typeof(Cooperativa.App.Startup));

            services.AddCors(options => options.AddPolicy("AllowWebApp",
                builder => builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                ));

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

            app.UseCors("AllowWebApp");

            app.UseHttpsRedirection();
          //  app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseHangfireDashboard(); //https://localhost:5000/hangfire: activa el “Dashboard” web de Hangfire, que es una interfaz visual donde puedes ver y administrar todos los trabajos en segundo plano.

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
