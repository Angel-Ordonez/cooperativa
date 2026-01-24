using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Model.Configuraciones;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Domain.Model.People;

namespace Cooperativa.App.Engine
{
    public interface ICalculationService
    {
        //Task<CalculoSocioInversionVm> CalculoSocioInversionVm();
        Task<string> CalculoSocioInversionPrestamo(Guid prestamoId, decimal capitalAPagar);
        InteresDiario_CalcularInteresAndCapitalVm InteresDiario_CalcularInteresAndCapitalByTotalAPagar(Prestamo prestamo, ConfiguracionPrestamo configuracion, decimal totalAPagar, int diasDeGracias);
        InteresDiario_CalcularInteresAndCapitalVm InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(Prestamo prestamo, ConfiguracionPrestamo configuracion, DateTime fechaCotizacion, int diasDeGracias);
    }


    public class CalculationService: ICalculationService
    {
        private readonly CooperativaDbContext _context;

        public CalculationService(CooperativaDbContext context)
        {
            _context = context;
        }

        public async Task<string> CalculoSocioInversionPrestamo(Guid prestamoId, decimal capitalAPagar)
        {
            //Aqui voy a tener errores de decimales, con redondeos

            //Aqui podria ir calculando las ganacias cuando se vaya pagando el interes
            decimal letocaASocios = 0.0M;

            if (capitalAPagar > 0)
            {
                decimal suma = 0.0M;
                var detallesSociosInversiones = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted && x.Enabled)
                    .Include(s => s.SocioInversion)
                    .ToListAsync();

                var porcentajeSocios = detallesSociosInversiones.Sum(x => x.PorcentajeEnPrestamo);
                letocaASocios = Math.Round(capitalAPagar * (porcentajeSocios / 100), 2);

                decimal centavosQueNecesitoQuitar = 0.0M;
                foreach (var detalleS in detallesSociosInversiones)
                {
                    var socioInversion = detalleS.SocioInversion;

                    var pagoDelCapital = Math.Round(capitalAPagar * (detalleS.PorcentajeEnPrestamo / 100), 2);
                    detalleS.CantidadPagadaDePrestamo += pagoDelCapital;
                    detalleS.CantidadPagadaDePrestamo = Math.Round(detalleS.CantidadPagadaDePrestamo, 2);


                    var restaParaValidar = detalleS.SePresto - detalleS.CantidadPagadaDePrestamo;
                    if (restaParaValidar < 1 && restaParaValidar > 0 && detalleS.SePresto > detalleS.CantidadPagadaDePrestamo)   //Aqui seria si, hace falta centavos
                    {
                        detalleS.CantidadPagadaDePrestamo += restaParaValidar;
                        pagoDelCapital += restaParaValidar;
                        pagoDelCapital = Math.Round(pagoDelCapital, 2);

                        centavosQueNecesitoQuitar = restaParaValidar;
                    }
                    else if (centavosQueNecesitoQuitar > 0 && detalleS.CantidadPagadaDePrestamo > detalleS.SePresto)     //Aqui es cuando quito lo centavos de arriba
                    {
                        pagoDelCapital -= centavosQueNecesitoQuitar;
                        pagoDelCapital = Math.Round(pagoDelCapital, 2);

                        detalleS.CantidadPagadaDePrestamo -= centavosQueNecesitoQuitar; //Se lo quito al siguiente
                    }


                    //Podria tener problemas por el porcentaje al redondearse
                    if (detalleS.SePresto < detalleS.CantidadPagadaDePrestamo)
                    {
                        throw new Exception ($"ERROR: discripancia en cantidades de DetalleInversion {socioInversion.CodigoInversion}");
                    }

                    if (detalleS.SePresto == detalleS.CantidadPagadaDePrestamo)
                    {
                        detalleS.Enabled = false;
                    }


                    socioInversion.CantidadPrestada -= pagoDelCapital;
                    socioInversion.NoPrestado += pagoDelCapital;
                    socioInversion.PorcetajePrestado = (socioInversion.CantidadPrestada / socioInversion.Cantidad) * 100;

                    if (socioInversion.CantidadPrestada < 0 || socioInversion.PorcetajePrestado < 0)
                    {
                        throw new Exception ($"ERROR: discripancia en cantidades de DetalleInversion {socioInversion.CodigoInversion}");
                    }

                    if (socioInversion.PorcetajePrestado < 100)
                    {
                        socioInversion.Estado = EstadoInversion.EnCaja;
                        socioInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)socioInversion.Estado);
                    }

                    suma += pagoDelCapital;
                }

                suma = Math.Round(suma, 2);
                if (suma != letocaASocios)
                {
                    throw new Exception("ERROR suma no concuerda en calculos de socios");
                }

            }








            return "";
        }












        public InteresDiario_CalcularInteresAndCapitalVm InteresDiario_CalcularInteresAndCapitalByTotalAPagar(Prestamo prestamo, ConfiguracionPrestamo configuracion, decimal totalAPagar, int diasDeGracias)
        {
            var response = new InteresDiario_CalcularInteresAndCapitalVm { };


            decimal interesAlDia = 0;
            decimal capitalActual = 0;
            decimal interesAPagar = 0;
            decimal capitalAPagar = 0;
            decimal restaCapital = 0;
            decimal proximoPago = 0;
            decimal deudaTotal = 0;
            var obsevacion = "";
            DateTime hastaFecha = DateTime.Now;

            decimal interes = 0;

            if (prestamo != null)
            {
                if (prestamo.RestaCapital > 0)
                {
                    capitalActual = prestamo.RestaCapital;
                }
                else
                {
                    capitalActual = prestamo.CantidadInicial;
                }


                var ultimaPagoFecha = DateTime.MinValue;
                if (prestamo.FechaUltimoPago != DateTime.MinValue && prestamo.FechaUltimoPago != null)
                {
                    ultimaPagoFecha = prestamo.FechaUltimoPago.Date;
                }
                else
                {
                    ultimaPagoFecha = prestamo.FechaEntragado.Date;
                }

                //TimeSpan restarFechas = DateTime.Now.Date - prestamo.FechaUltimoPago.Date;                     // Restar las fechas: Desde el ultimo pago hasta la actualida
                TimeSpan restarFechas = DateTime.Now.Date - ultimaPagoFecha;
                int cantidadDias = restarFechas.Days;                                                // Obtener la cantidad de días que se generaron interes
                var ultimoDetalle = prestamo.PrestamoDetalles.Where(x => !x.IsSoftDeleted).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                interes = prestamo.InteresPorcentaje;
                if (interes > 1)
                {
                    interes = interes / 100;
                }

                if (((double)interes) < 0.05 || ((double)interes) > 0.1)
                {
                    response.Error = true;
                    response.ErrorDescripcion = "Interes no valido. No cumple entre 5% y 10%";
                    return response;
                    //return AppResult.New(false, $"Interes no valido. No cumple entre 5% y 10%");
                }

                var interesMes = prestamo.RestaCapital * interes;
                interesMes = Math.Round(interesMes, 2);

                if (prestamo.FechaUltimoPago == DateTime.MinValue || prestamo.FechaUltimoPago == null)  //Si no han pagado ningun cuota
                {
                    if(prestamo.FechaEntragado == null || prestamo.FechaEntragado == DateTime.MinValue)
                    {
                        interesAPagar = 0;
                    }
                    else
                    {
                        restarFechas = DateTime.Now - prestamo.FechaEntragado;
                        cantidadDias = restarFechas.Days;

                        if (configuracion.PrimerMesInteresObligatorio && cantidadDias <= 30)
                        {
                            interesAPagar = interesMes;
                        }
                        else   //Pero que pasa cuando es febrero? solo tiene 28 o 29 dias
                        {
                            var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;
                            interesAlDia = interesMes / Convert.ToDecimal(restaConproximoMes.TotalDays);
                            //interesAlDia = Math.Round(interesAlDia, 2);
                            interesAPagar = interesAlDia * cantidadDias;
                        }

                        hastaFecha = prestamo.FechaEntragado.AddDays(cantidadDias);
                    }

                }
                else if (ultimoDetalle != null)
                {
                    //var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;      //No contaba los dias excatos entre fechas de meses
                    var restaConproximoMes = ultimoDetalle.FechaProximoPago - ultimoDetalle.FechaPago;

                    interesMes = ultimoDetalle.ProximoPago;

                    //Calcular el interes a pagar por dias

                    var interesBaseMes = ultimoDetalle.ProximoPago;
                    //var interesAlDia = interesBaseMes / 30;     //30 dias
                    interesAlDia = interesBaseMes / Convert.ToDecimal(restaConproximoMes.TotalDays);
                    //interesAlDia = Math.Round(interesAlDia, 2);
                    interesAPagar = interesAlDia * cantidadDias;

                    hastaFecha = ultimoDetalle.FechaPago.AddDays(cantidadDias);
                }
                interesAPagar = Math.Round(interesAPagar, 2);


                response.InteresMes = interesMes;
                response.DiasGenerados = cantidadDias;
                response.InteresGenerado = Math.Round(interesAPagar, 2);
                response.InteresDiario = Math.Round(interesAlDia, 2);

                totalAPagar = Math.Round(totalAPagar, 2);




                if (totalAPagar < interesAPagar && totalAPagar >= interesMes)
                {
                    interesAPagar = interesMes;

                    var diasApagar = interesAPagar / response.InteresDiario;
                    diasApagar = Math.Round(diasApagar);
                    response.DiasAPagar = (int)diasApagar;

                    if (ultimoDetalle != null)
                    {
                        hastaFecha = ultimoDetalle.FechaPago.AddMonths(1);
                    }
                    else
                    {
                        hastaFecha = prestamo.FechaEntragado.AddMonths(1);
                    }
                }

                deudaTotal = response.InteresGenerado + capitalActual;
                deudaTotal = Math.Round(deudaTotal, 2);

                if (totalAPagar < interesAPagar)
                {
                    response.Error = true;
                    response.ErrorDescripcion = $"Cantidad es insuficiente: | Interes Mes: {interesMes} {prestamo.Moneda} | Interes Generado: {interesAPagar.ToString("N2")} {prestamo.Moneda} | Capital: {capitalActual.ToString("N2")} {prestamo.Moneda} ";
                    return response;

                    //return AppResult.New(false, $"Cantidad es insuficiente: | Interes Mes: {interesMes} {prestamo.Moneda} | Interes Generado: {interesAPagar.ToString("N2")} {prestamo.Moneda} | Capital: {capitalActual.ToString("N2")} {prestamo.Moneda} |");
                }

                var sobrante = totalAPagar - interesAPagar;
                sobrante = Math.Round(sobrante, 2);

                if (interesAPagar >= response.InteresGenerado)
                {
                    capitalAPagar = Math.Round(sobrante, 2);
                    interesAPagar = Math.Round(interesAPagar, 2);
                    response.DiasAPagar = response.DiasGenerados;
                }
                else
                {
                    //Ojo aveces sera mas del mes, aveces llevan 39.8 dias, entonces podria pagar 39 dias y el otro 0.8 se va al capital

                    var interesAPagarPrevio = interesAPagar + sobrante;

                    #region Validar si es necesario redondear los dias (solo si el resiudo da 0.99)

                    decimal valor = interesAPagarPrevio / response.InteresDiario;

                    int diasEnteros = (int)valor;
                    decimal diferenciaDecimal = valor - diasEnteros;

                    int diasTotales = diasEnteros;

                    if (diferenciaDecimal >= 0.99m)
                    {
                        diasTotales++;
                    }

                    #endregion


                    var diferenciaDias = diasTotales - response.DiasAPagar;
                    var diferenciaDiasEntero = (int)diferenciaDias;

                    var diferenciaParaCapital = diferenciaDias - diferenciaDiasEntero;

                    var agregarInteres = diferenciaDiasEntero * response.InteresDiario;


                    var decimalesBasuras = totalAPagar - (interesAPagar + agregarInteres);  //Si hay entonces seran decimales en negativo
                    if(decimalesBasuras < 0)
                    {
                        agregarInteres += decimalesBasuras;
                    }


                    //var diasTotalesAntes = interesAPagarPrevio / response.InteresDiario;
                    //if (totalAPagar >= interesAPagar + agregarInteres)
                    //{
                    //    interesAPagar += agregarInteres;
                    //}

                    if (totalAPagar >= (interesAPagar + agregarInteres + decimalesBasuras))
                    {
                        interesAPagar += agregarInteres;
                    }

                    if (interesAPagar >= response.InteresGenerado)
                    {
                        var paraCapital = diferenciaParaCapital * response.InteresDiario;
                        paraCapital = Math.Round(paraCapital, 2);
                        capitalAPagar = paraCapital;
                    }

                    hastaFecha = hastaFecha.AddDays(diferenciaDiasEntero);
                    response.DiasAPagar += diferenciaDiasEntero;

                    interesAPagar = Math.Round(interesAPagar, 2);

                    var suma = interesAPagar + capitalAPagar;
                    var perdidaDecimales = Math.Abs(suma - totalAPagar);    //Siempre un resultado en español

                    if (suma != totalAPagar)
                    {

                        var diferenciaConInteresPagar = interesAPagar - perdidaDecimales;

                        if (interesAPagar == totalAPagar || diferenciaConInteresPagar == totalAPagar)
                        {
                            //Por calculos decimales se agrego un poco mas
                            if(perdidaDecimales < 0.50M)
                            {
                                //totalAPagar -= capitalAPagar;
                                //interesAPagar -= capitalAPagar;
                                totalAPagar -= perdidaDecimales;
                                interesAPagar -= perdidaDecimales;
                                capitalAPagar = 0.0M;
                            }
                            else
                            {
                                response.Error = true;
                                response.ErrorDescripcion = $"Ha ocurrido un error en calculos, porfavor pongase en contacto con el desarrollador";
                                return response;
                            }
                        }
                        else if(interesAPagar < totalAPagar && perdidaDecimales < response.InteresDiario)           //Cuando no alcanza para otro dia de interes
                        {
                            totalAPagar -= perdidaDecimales;
                            capitalAPagar = 0.0M;
                        }
                        else
                        {
                            response.Error = true;
                            response.ErrorDescripcion = $"Ha ocurrido un error en calculos, porfavor pongase en contacto con el desarrollador";
                            return response;
                        }
                    }

                }



                if (totalAPagar > deudaTotal)
                {
                    response.Error = true;
                    response.ErrorDescripcion = $"El total a pagar es mayor a la deuda total: {deudaTotal.ToString("N2")} {prestamo.Moneda}";
                    return response;
                    //return AppResult.New(false, $"El total a pagar es mayor a la deuda total: {deudaTotal.ToString("N2")} {prestamo.Moneda}");
                }

                restaCapital = capitalActual - capitalAPagar;
                restaCapital = Math.Round(restaCapital, 2);


                #region Asi calculaba el ProximoPago, pero como ahora estoy redondeando y calculando por dia
                //if (prestamo.InteresPorcentaje > 1)
                //{
                //    proximoPago = restaCapital * (prestamo.InteresPorcentaje / 100);
                //}
                //else
                //{
                //    proximoPago = restaCapital * prestamo.InteresPorcentaje;
                //}
                #endregion

                var hastaFechaMas1Mes = hastaFecha.AddMonths(1);
                TimeSpan restarFechas2 = hastaFechaMas1Mes.Date - hastaFecha.Date;
                int cuantosDiasParProximoPago = restarFechas2.Days;
                var proximoInteresPrevio = restaCapital * interes;
                var proximoInteresDiario = proximoInteresPrevio / cuantosDiasParProximoPago;
                //proximoInteresDiario = Math.Round(proximoInteresDiario, 2);

                proximoPago = proximoInteresDiario * cuantosDiasParProximoPago;

                proximoPago = Math.Round(proximoPago, 2);
            }



            response.TotalAPagar = totalAPagar;
            response.CapitalActual = capitalActual;
            response.InteresAPagar = interesAPagar;
            response.CapitalAPagar = capitalAPagar;
            response.RestaCapital = restaCapital;
            response.ProximoPago = proximoPago;
            response.DeudaTotal = deudaTotal;
            response.Observacion = obsevacion;
            response.HastaFecha = hastaFecha;
            response.PrestamoId = prestamo.Id;

            if (interesAlDia > 0)
            {
                var cantidadDiasMesInteres = response.InteresMes / interesAlDia;
                cantidadDiasMesInteres = Math.Round(cantidadDiasMesInteres, 2);
                response.DiasDelPrimerMesDeInteres = (int)cantidadDiasMesInteres;
            }


            if (diasDeGracias > 0)
            {

                if(response.InteresGenerado >= totalAPagar || response.InteresGenerado >= interesAPagar)
                {
                    if (response.DiasGenerados > diasDeGracias )
                    {
                        var totalDias = response.DiasAPagar - diasDeGracias;
                        var interesPagar = interesAlDia * totalDias;
                        var capitalPagarGracias = totalAPagar - interesPagar;

                        if(capitalPagarGracias > capitalActual)
                        {
                            capitalPagarGracias = capitalActual;
                        }

                        if (capitalPagarGracias < 0)
                        {
                            capitalPagarGracias = 0;
                        }

                        //var hastaFechaGracias = hastaFecha.AddDays(-diasDeGracias);
                        var hastaFechaGracias = hastaFecha;         //NO le debo quitar dias porque son dias de gracias

                        var deudaTotalGracias = capitalActual + interesPagar;
                        var restaCapitalGracias = capitalActual - capitalPagarGracias;


                        var hastaFechaMas1Mes = hastaFechaGracias.AddMonths(1);
                        TimeSpan restarFechas2 = hastaFechaMas1Mes.Date - hastaFechaGracias.Date;
                        int cuantosDiasParProximoPago = restarFechas2.Days;
                        var proximoInteresPrevio = restaCapitalGracias * interes;
                        var proximoInteresDiario = proximoInteresPrevio / cuantosDiasParProximoPago;
                        //proximoInteresDiario = Math.Round(proximoInteresDiario, 2);

                        var proximoPagoGracias = proximoInteresDiario * cuantosDiasParProximoPago;
                        proximoPagoGracias = Math.Round(proximoPagoGracias, 2);





                        if (capitalPagarGracias > response.CapitalActual)
                        {
                            capitalPagarGracias = response.CapitalActual;
                        }

                        var newCalculoDiasDeGracia = new CalculoConDiasDeGraciasVm
                        {
                            CapitalAPagar = capitalPagarGracias,
                            DiasAPagar = totalDias,
                            HastaFecha = hastaFechaGracias,
                            InteresAPagar = interesPagar,
                            TotalAPagar = capitalPagarGracias + interesPagar,
                            DeudaTotal = deudaTotalGracias,
                            ProximoPago = proximoPagoGracias,
                            RestaCapital = restaCapitalGracias
                        };
                        response.CalculoConDiasDeGracias = newCalculoDiasDeGracia;
                    }
                    else
                    {
                        response.ErrorDescripcion = "Dias de Gracias es mayor a Dias generados";
                    }
                }
                else
                {
                    //response.ErrorDescripcion = $"No es posible aplicar Dias de Gracias porque quedan {diasFlotando} dias flotando";
                    response.ErrorDescripcion = $"Para poder aplicar los Días de gracia, El total a pagar a cotizar debe ser el interés total generado ({response.InteresGenerado}) ";

                }

            }


            return response;
        }





        //Este basicamente solo sirve para calcular dias de intereses, capital sera raro o imposible
        public InteresDiario_CalcularInteresAndCapitalVm InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(Prestamo prestamo, ConfiguracionPrestamo configuracion, DateTime fechaCotizacion, int diasDeGracias)
        {
            var response = new InteresDiario_CalcularInteresAndCapitalVm { };

            decimal totalAPagar = 0;

            decimal interesAlDia = 0;
            decimal capitalActual = 0;
            decimal interesAPagar = 0;
            decimal capitalAPagar = 0;
            decimal restaCapital = 0;
            decimal proximoPago = 0;
            decimal deudaTotal = 0;
            var obsevacion = "";
            DateTime hastaFecha = DateTime.Now;

            decimal interes = 0;

            if (prestamo != null)
            {
                if (prestamo.RestaCapital > 0)
                {
                    capitalActual = prestamo.RestaCapital;
                }
                else if(prestamo.Estado == EstadoPrestamo.Pagado)
                {
                    capitalActual = 0;  //Porque el prestamo ya esta pagado (Bueno le agregue esta capacidad porque utilizo este engine para generar pdf, que tambien aplica para prestamos ya pagados)
                }
                else
                {
                    capitalActual = prestamo.CantidadInicial;
                }

                var ultimaPagoFecha = DateTime.MinValue;
                if(prestamo.FechaUltimoPago != DateTime.MinValue && prestamo.FechaUltimoPago != null)
                {
                    ultimaPagoFecha = prestamo.FechaUltimoPago.Date;
                }
                else
                {
                    ultimaPagoFecha = prestamo.FechaEntragado.Date;
                }


                //TimeSpan restarFechaCotizacion = fechaCotizacion.Date - prestamo.FechaUltimoPago.Date;
                TimeSpan restarFechaCotizacion = fechaCotizacion.Date - ultimaPagoFecha;
                int cantidadDiasAcalcular = restarFechaCotizacion.Days;


                TimeSpan restarFechas = DateTime.Now.Date - prestamo.FechaUltimoPago.Date;                  // Restar las fechas: Desde el ultimo pago hasta la actualida
                int cantidadDias = restarFechas.Days;                                                // Obtener la cantidad de días que se generaron interes
                var ultimoDetalle = prestamo.PrestamoDetalles.Where(x => !x.IsSoftDeleted).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                interes = prestamo.InteresPorcentaje;
                if (interes > 1)
                {
                    interes = interes / 100;
                }

                if (((double)interes) < 0.05 || ((double)interes) > 0.1)
                {
                    response.Error = true;
                    response.ErrorDescripcion = "Interes no valido. No cumple entre 5% y 10%";
                    return response;
                    //return AppResult.New(false, $"Interes no valido. No cumple entre 5% y 10%");
                }

                var interesMes = prestamo.RestaCapital * interes;
                interesMes = Math.Round(interesMes, 2);

                if (prestamo.FechaUltimoPago == DateTime.MinValue || prestamo.FechaUltimoPago == null)  //Si no han pagado ningun cuota
                {
                    if (prestamo.FechaEntragado == null || prestamo.FechaEntragado == DateTime.MinValue)
                    {
                        interesAPagar = 0;
                    }
                    else
                    {
                        restarFechas = DateTime.Now - prestamo.FechaEntragado;
                        cantidadDias = restarFechas.Days;

                        if (configuracion.PrimerMesInteresObligatorio && cantidadDias <= 30)
                        {
                            interesAPagar = interesMes;
                        }
                        else   //Pero que pasa cuando es febrero? solo tiene 28 o 29 dias
                        {
                            var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;
                            interesAlDia = interesMes / Convert.ToDecimal(restaConproximoMes.TotalDays);
                            //interesAlDia = Math.Round(interesAlDia, 2);
                            interesAPagar = interesAlDia * cantidadDias;
                        }

                        hastaFecha = prestamo.FechaEntragado.AddDays(cantidadDias);

                    }

                }
                else if (ultimoDetalle != null)
                {
                    //var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;      //No contaba los dias excatos entre fechas de meses
                    var restaConproximoMes = ultimoDetalle.FechaProximoPago - ultimoDetalle.FechaPago;

                    interesMes = ultimoDetalle.ProximoPago;

                    //Calcular el interes a pagar por dias

                    var interesBaseMes = ultimoDetalle.ProximoPago;
                    interesAlDia = interesBaseMes / Convert.ToDecimal(restaConproximoMes.TotalDays);
                    //interesAlDia = Math.Round(interesAlDia, 2);
                    interesAPagar = interesAlDia * cantidadDias;
                    hastaFecha = ultimoDetalle.FechaPago.AddDays(cantidadDias);
                }

                interesAPagar = Math.Round(interesAPagar, 2);

                response.InteresMes = interesMes;
                response.DiasGenerados = cantidadDias;
                response.InteresGenerado = Math.Round(interesAPagar, 2);
                response.InteresDiario = Math.Round(interesAlDia, 2);

                totalAPagar = cantidadDiasAcalcular * interesAlDia;
                totalAPagar = Math.Round(totalAPagar, 2);




                if (totalAPagar < interesAPagar && totalAPagar >= interesMes)
                {
                    interesAPagar = interesMes;

                    var diasApagar = interesAPagar / response.InteresDiario;
                    response.DiasAPagar = (int)diasApagar;

                    if (ultimoDetalle != null)
                    {
                        hastaFecha = ultimoDetalle.FechaPago.AddMonths(1);
                    }
                    else
                    {
                        hastaFecha = prestamo.FechaEntragado.AddMonths(1);
                    }
                }


                deudaTotal = response.InteresGenerado + capitalActual;
                deudaTotal = Math.Round(deudaTotal, 2);

                if (totalAPagar < interesAPagar)
                {
                    response.Error = true;
                    response.ErrorDescripcion = $"Cantidad es insuficiente: | Interes Mes: {interesMes} {prestamo.Moneda} | Interes Generado: {interesAPagar.ToString("N2")} {prestamo.Moneda} | Capital: {capitalActual.ToString("N2")} {prestamo.Moneda} ";
                    return response;
                }

                var sobrante = totalAPagar - interesAPagar;
                sobrante = Math.Round(sobrante, 2);

                if (interesAPagar >= response.InteresGenerado)
                {
                    //capitalAPagar = sobrante;
                    capitalAPagar = Math.Round(sobrante, 2);
                    interesAPagar = Math.Round(interesAPagar, 2);
                    response.DiasAPagar = response.DiasGenerados;
                }
                else
                {
                    //Ojo aveces sera mas del mes, aveces llevan 39.8 dias, entonces podria pagar 39 dias y el otro 0.8 se va al capital

                    var interesAPagarPrevio = interesAPagar + sobrante;



                    #region Validar si es necesario redondear los dias (solo si el resiudo da 0.99)

                    decimal valor = interesAPagarPrevio / response.InteresDiario;

                    int diasEnteros = (int)valor;
                    decimal diferenciaDecimal = valor - diasEnteros;

                    int diasTotales = diasEnteros;

                    if (diferenciaDecimal >= 0.99m)
                    {
                        diasTotales++;
                    }

                    #endregion


                    var diferenciaDias = diasTotales - response.DiasAPagar;
                    var diferenciaDiasEntero = (int)diferenciaDias;

                    var diferenciaParaCapital = diferenciaDias - diferenciaDiasEntero;

                    var agregarInteres = diferenciaDiasEntero * response.InteresDiario;


                    var decimalesBasuras = totalAPagar - (interesAPagar + agregarInteres);  //Si hay entonces seran decimales en negativo
                    if (decimalesBasuras < 0)
                    {
                        agregarInteres += decimalesBasuras;
                    }




                    //var diasTotales = interesAPagarPrevio / response.InteresDiario;
                    //var diferenciaDias = diasTotales - response.DiasAPagar;
                    //var diferenciaDiasEntero = (int)diferenciaDias;

                    //var diferenciaParaCapital = diferenciaDias - diferenciaDiasEntero;

                    //var agregarInteres = diferenciaDiasEntero * response.InteresDiario;
                    var paraCapital = diferenciaParaCapital * response.InteresDiario;
                    paraCapital = Math.Round(paraCapital, 2);


                    interesAPagar += agregarInteres;
                    capitalAPagar = paraCapital;
                    hastaFecha = hastaFecha.AddDays(diferenciaDiasEntero);
                    response.DiasAPagar += diferenciaDiasEntero;

                    interesAPagar = Math.Round(interesAPagar, 2);

                    var suma = interesAPagar + capitalAPagar;
                    var perdidaDecimales = Math.Abs(suma - totalAPagar);    //Siempre un resultado en español
                    if (suma != totalAPagar && perdidaDecimales > 0.50M)
                    {
                        response.Error = true;
                        response.ErrorDescripcion = $"Ha ocurrido un error en calculos, porfavor pongase en contacto con el desarrollador";
                        return response;
                    }
                    else
                    {
                        interesAPagar -= perdidaDecimales;      //Esa perdida decimal seria lo que iria para Capital pero este metodo especofico; InteresDiario_CalcularInteresAndCapitalByFechaCotizacion solo sirve para calcular intereses hasta tal dia
                        //en vez de perdidaDecimales pude aver puesto capitalAPagar
                    }

                }






                if (totalAPagar > deudaTotal)
                {
                    response.Error = true;
                    response.ErrorDescripcion = $"El total a pagar es mayor a la deuda total: {deudaTotal.ToString("N2")} {prestamo.Moneda}";
                    return response;
                }

                restaCapital = capitalActual - capitalAPagar;
                restaCapital = Math.Round(restaCapital, 2);




                #region Asi calculaba el ProximoPago, pero como ahora estoy redondeando y calculando por dia
                //if (prestamo.InteresPorcentaje > 1)
                //{
                //    proximoPago = restaCapital * (prestamo.InteresPorcentaje / 100);
                //}
                //else
                //{
                //    proximoPago = restaCapital * prestamo.InteresPorcentaje;
                //}
                #endregion

                var hastaFechaMas1Mes = hastaFecha.AddMonths(1);
                TimeSpan restarFechas2 = hastaFechaMas1Mes.Date - hastaFecha.Date;
                int cuantosDiasParProximoPago = restarFechas2.Days;
                var proximoInteresPrevio = restaCapital * interes;
                var proximoInteresDiario = proximoInteresPrevio / cuantosDiasParProximoPago;
                //proximoInteresDiario = Math.Round(proximoInteresDiario, 2);

                proximoPago = proximoInteresDiario * cuantosDiasParProximoPago;

                proximoPago = Math.Round(proximoPago, 2);
            }


            response.TotalAPagar = totalAPagar;
            response.CapitalActual = capitalActual;
            response.InteresAPagar = interesAPagar;
            response.CapitalAPagar = capitalAPagar;
            response.RestaCapital = restaCapital;
            response.ProximoPago = proximoPago;
            response.DeudaTotal = deudaTotal;
            response.Observacion = obsevacion;
            response.HastaFecha = hastaFecha;
            response.PrestamoId = prestamo.Id;

            if (interesAlDia > 0)
            {
                var cantidadDiasMesInteres = response.InteresMes / interesAlDia;
                cantidadDiasMesInteres = Math.Round(cantidadDiasMesInteres, 2);
                response.DiasDelPrimerMesDeInteres = (int)cantidadDiasMesInteres;
            }



            if (diasDeGracias > 0)
            {

                if (response.InteresGenerado >= totalAPagar || response.InteresGenerado >= interesAPagar)
                {
                    if (response.DiasGenerados > diasDeGracias)
                    {
                        var totalDias = response.DiasAPagar - diasDeGracias;
                        var interesPagar = interesAlDia * totalDias;
                        var capitalPagarGracias = totalAPagar - interesPagar;


                        if (capitalPagarGracias > capitalActual)
                        {
                            capitalPagarGracias = capitalActual;
                        }

                        if (capitalPagarGracias < 0)
                        {
                            capitalPagarGracias = 0;
                        }

                        //var hastaFechaGracias = hastaFecha.AddDays(-diasDeGracias);
                        var hastaFechaGracias = hastaFecha;         //NO le debo quitar dias porque son dias de gracias

                        var deudaTotalGracias = capitalActual + interesPagar;
                        var restaCapitalGracias = capitalActual - capitalPagarGracias;


                        var hastaFechaMas1Mes = hastaFechaGracias.AddMonths(1);
                        TimeSpan restarFechas2 = hastaFechaMas1Mes.Date - hastaFechaGracias.Date;
                        int cuantosDiasParProximoPago = restarFechas2.Days;
                        var proximoInteresPrevio = restaCapitalGracias * interes;
                        var proximoInteresDiario = proximoInteresPrevio / cuantosDiasParProximoPago;
                        //proximoInteresDiario = Math.Round(proximoInteresDiario, 2);

                        var proximoPagoGracias = proximoInteresDiario * cuantosDiasParProximoPago;
                        proximoPagoGracias = Math.Round(proximoPagoGracias, 2);





                        if (capitalPagarGracias > response.CapitalActual)
                        {
                            capitalPagarGracias = response.CapitalActual;
                        }

                        var newCalculoDiasDeGracia = new CalculoConDiasDeGraciasVm
                        {
                            CapitalAPagar = capitalPagarGracias,
                            DiasAPagar = totalDias,
                            HastaFecha = hastaFechaGracias,
                            InteresAPagar = interesPagar,
                            TotalAPagar = capitalPagarGracias + interesPagar,
                            DeudaTotal = deudaTotalGracias,
                            ProximoPago = proximoPagoGracias,
                            RestaCapital = restaCapitalGracias
                        };
                        response.CalculoConDiasDeGracias = newCalculoDiasDeGracia;
                    }
                    else
                    {
                        response.ErrorDescripcion = "Dias de Gracias es mayor a Dias generados";
                    }
                }
                else
                {
                    //response.ErrorDescripcion = $"No es posible aplicar Dias de Gracias porque quedan {diasFlotando} dias flotando";
                    response.ErrorDescripcion = $"Para poder aplicar los Días de gracia, El total a pagar a cotizar debe ser el interés total generado ({response.InteresGenerado}) ";

                }

            }




            return response;
        }








        //public async Task< InteresDiario_CalcularInteresAndCapitalVm> CrearEgresoCaja(Caja? caja, Guid? prestamoId, Guid? socioInversionId, Guid? retiroId, ConfiguracionPrestamo configuracion, DateTime fechaCotizacion)
        //public async Task<InteresDiario_CalcularInteresAndCapitalVm> CrearEgresoCaja(Caja caja, PrestamoDetalle? prestamoDetalle, Prestamo? prestamo, SocioInversion? socioInversion, Retiro? retiro, ConfiguracionPrestamo configuracion, DateTime fechaCotizacion, string obs, Guid? usuarioId)
        //{
        //    //OJO: Despues de esto debe traer todo filtrado por Cliente (Organizacion)
        //    var egresosCantidad = await _context.Egreso.CountAsync();

        //    string correlativo = "";
        //    decimal monto = 0m;

        //    if(prestamo != null)
        //    {
        //        //correlativo = prestamo.CodigoPrestamo;
        //        //monto = prestamo.CantidadInicial;

        //        //var newEgreso = Egreso.New(egresosCantidad + 1, correlativo, monto, "Nuevo Prestamo", $"Nuevo Prestamo {cliente.Nombre} {cliente.Apellido} por {Math.Round(command.CantidadInicial, 2)} HNL ", newPrestamo.Id, usuarioId);
        //        //await _context.Egreso.AddAsync(newEgreso);
        //    }
        //    else if(prestamoDetalle != null)
        //    {
        //        //var newEgreso = Egreso.New(cantidadEgresos + 1, ingresoDetalle.Correlativo, prestamoDetalle.TotalAPagar, $"Se elimino Cuota con # de Ingreso: {ingresoDetalle.Correlativo} de Prestamo {prestamo.CodigoPrestamo}", command.Observacion, prestamo.Id, modifiedBy);
        //        //await _context.Egreso.AddAsync(newEgreso);
        //    }
        //    else if (socioInversion != null)
        //    {
        //        //correlativo = socioInversion.CodigoInversion;
        //        //monto = socioInversion.Cantidad;                //Tengo duda, creo que seria PrestamoDetalle

        //        //if(obs.Length < 1) obs = $"Se elimino Cuota con # de Ingreso: {ingresoDetalle.Correlativo} de Prestamo {prestamo.CodigoPrestamo}"

        //        //var newEgreso = Egreso.New(egresosCantidad + 1, correlativo, monto, "Nuevo Prestamo", $"Nuevo Prestamo {cliente.Nombre} {cliente.Apellido} por {Math.Round(command.CantidadInicial, 2)} HNL ", newPrestamo.Id, usuarioId);
        //        //await _context.Egreso.AddAsync(newEgreso);
        //    }
        //    else if (retiro != null)
        //    {
        //        correlativo = retiro.NumeroRetiro;
        //        monto = retiro.Monto;

        //        var newEgreso = Egreso.NewPorRetiro(egresosCantidad + 1, correlativo, monto, );
        //        await _context.Egreso.AddAsync(newEgreso);
        //    }
        //    else
        //    {
        //        correlativo = "Sin referencia";
        //    }




        //    var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();
        //    var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, correlativo, monto, usuarioId);
        //    newTransaccion.EgresoId = newEgreso.Id;



        //    await _context.Transaccion.AddAsync(newTransaccion);
        //    //await _context.SaveChangesAsync();


        //    var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

        //    if (ultimoRegistroCaja != null)
        //    {
        //        string saldoEnMoneda = Math.Round(ultimoRegistroCaja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

        //        if (ultimoRegistroCaja.SaldoActual < command.CantidadInicial)
        //        {
        //            return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {saldoEnMoneda} ");
        //        }

        //        var nuevoSaldo = ultimoRegistroCaja.SaldoActual - newEgreso.Monto;

        //        var agregarACaja = Caja.NewEgreso(createdBy, codigoPrestamo, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newEgreso.Id, createdBy);

        //        ultimoRegistroCaja.Enabled = false;
        //        ultimoRegistroCaja.ModifiedDate = DateTime.Now;
        //        ultimoRegistroCaja.ModifiedBy = createdBy;


        //        await _context.Caja.AddAsync(agregarACaja);
        //        await _context.SaveChangesAsync();
        //        transaction.Commit();

        //    }






        //    var response = new InteresDiario_CalcularInteresAndCapitalVm { };

        //    return response;
        //}
























    }


}


public class CalculoSocioInversionVm
{

}


public class InteresDiario_CalcularInteresAndCapitalVm
{
    public Guid PrestamoId { get; set; }
    public decimal TotalAPagar { get; set; }
    public decimal InteresMes { get; set; }
    public int DiasDelPrimerMesDeInteres { get; set; }
    public decimal InteresDiario { get; set; }
    public int DiasGenerados { get; set; }
    public int DiasAPagar { get; set; }
    public decimal InteresGenerado { get; set; }
    public decimal CapitalActual { get; set; }
    public decimal InteresAPagar { get; set; }
    public decimal CapitalAPagar { get; set; }
    public DateTime HastaFecha { get; set; }
    public decimal RestaCapital { get; set; }
    public decimal ProximoPago { get; set; }
    public decimal DeudaTotal { get; set; }
    public string Observacion { get; set; }
    public bool Error { get; set; }
    public string ErrorDescripcion { get; set; }
    public CalculoConDiasDeGraciasVm CalculoConDiasDeGracias { get;set; }
}

public class CalculoConDiasDeGraciasVm
{
    public decimal TotalAPagar { get; set; }
    public decimal InteresAPagar { get; set; }
    public decimal CapitalAPagar { get; set; }
    public int DiasAPagar { get; set; }
    public DateTime HastaFecha { get; set; }
    public decimal RestaCapital { get; set; }
    public decimal ProximoPago { get; set; }
    public decimal DeudaTotal { get; set; }
}