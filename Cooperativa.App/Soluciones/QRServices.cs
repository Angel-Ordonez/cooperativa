using Cooperativa.App.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Soluciones
{
    public interface IQRServices
    {
        Task<byte[]> GenerarQR(string texto);

    }

    public class QRServices : IQRServices
    {


        public async Task<byte[]> GenerarQR(string texto)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrBitmap = qrCode.GetGraphic(20);

            byte[] qrBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                qrBytes = ms.ToArray();
            }

            return qrBytes;
        }
















    }



}
