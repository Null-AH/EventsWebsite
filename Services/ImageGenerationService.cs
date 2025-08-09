using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Data;
using EventApi.Interfaces;
using EventApi.Mappers;
using EventApi.Models;
using MathNet.Numerics.Interpolation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NPOI.Util;
using QRCoder;
using SkiaSharp;

namespace EventApi.Services
{
    public class ImageGenerationService : IImageGenerationService
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ImageGenerationService(AppDBContext context, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _context = context;
        }
        public async Task/*<string>*/ GenerateInvitationsForEventAsync(int eventId)
        {
            var eventInfo = await _context.Events
            .Include(e => e.Attendees).Include(e => e.TemplateElements)
            .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventInfo == null || !eventInfo.TemplateElements.Any() || string.IsNullOrEmpty(eventInfo.BackgroundImageUri))
            {
                throw new Exception("Event generation failed: Event data or template elements or background image is missing");
            }

            var templateNameElement = eventInfo.TemplateElements.FirstOrDefault(e => e.ElementType.ToLower() == "Name".ToLower());
            var templateQrElement = eventInfo.TemplateElements.FirstOrDefault(e => e.ElementType.ToLower() == "QR".ToLower());
            if (templateNameElement == null || templateQrElement == null)
            {
                throw new Exception("Event generation failed: Template is missing the required 'Element Type'");
            }

            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, eventInfo.BackgroundImageUri.TrimStart('/'));
            using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using var originalBitmap = SKBitmap.Decode(stream);

            using var typeface = SKTypeface.FromFamilyName(templateNameElement.FontTheme) ?? SKTypeface.Default;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = templateNameElement.FontColor == null ?
                        SKColors.Black
                        : SKColor.Parse(templateNameElement.FontColor) //just in case the font color was null i need it to be black as a default option,
            };

            var targetWidth = (float)templateNameElement.Width;
            var targetHeight = (float)templateNameElement.Height;

            var safeEventName = string.Join("_", eventInfo.Name.Split(Path.GetInvalidFileNameChars()));
            var downloadsFolder = Path.Combine(_hostEnvironment.WebRootPath, $"downloads/generated_invitations/{safeEventName}_{eventId}/");
            Directory.CreateDirectory(downloadsFolder);

            foreach (var attendee in eventInfo.Attendees)
            {
                using var backgroundBitmap = originalBitmap.Copy();

                using var surface = SKSurface.Create(backgroundBitmap.Info, backgroundBitmap.GetPixels());
                var canvas = surface.Canvas;

                float textSize = targetHeight; 
                SKRect textBounds = new SKRect();
                using var font = new SKFont(typeface);

                while (true)
                {
                    font.Size = textSize;

                    font.MeasureText(attendee.Name,out textBounds);

                    if (textBounds.Width <= targetWidth && textBounds.Height <= targetHeight) break;
                    textSize -= 1f;
                    if (textSize <= 5) break;
                }

                float baselineY = (float)templateNameElement.Y + (targetHeight - textBounds.Height) / 2 - textBounds.Top;
                canvas.DrawText(attendee.Name, (float)templateNameElement.X, baselineY,font ,paint);

                //var qrCodePayload = Guid.NewGuid().ToString();
                var qrCodePayload = attendee.Email;
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrCodePayload, QRCodeGenerator.ECCLevel.Q);

                using var pngQrCode = new PngByteQRCode(qrCodeData);

                var darkColor = new byte[] { 0, 0, 0, 255 };      // Black, fully opaque
                var lightColor = new byte[] { 255, 255, 255}; 
                byte[] qrCodeAsPngBytes = pngQrCode.GetGraphic(20,darkColor,lightColor,false);

                using var qrCodeBitmap = SKBitmap.Decode(qrCodeAsPngBytes);

                var destRect = new SKRect(
                    (float)templateQrElement.X,
                    (float)templateQrElement.Y,
                    (float)(templateQrElement.X + templateQrElement.Width),
                    (float)(templateQrElement.Y + templateQrElement.Height)
                );
                canvas.DrawBitmap(qrCodeBitmap, destRect);

                using var finalImage = surface.Snapshot();
                using var data = finalImage.Encode(SKEncodedImageFormat.Jpeg, 90);


                var safeAttendeeName = string.Join("_", attendee.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeAttendeeName}_{attendee.Id}.jpg";

                var GeneratedInvitationFullPath = Path.Combine(downloadsFolder, fileName);

                var newInvitation = new Invitation
                {
                    AttendeeId = attendee.Id,
                    //UniqueQRCode = qrCodePayload
                };
                await _context.Invitations.AddAsync(newInvitation);

                await using (var fileStream = new FileStream(GeneratedInvitationFullPath, FileMode.Create))
                {
                    await fileStream.WriteAsync(data.ToArray());
                }

            }

            var zipFileDirectory = Path.Combine(_hostEnvironment.WebRootPath, $"downloads/CompressedFiles");
            var zipFileFullPath = Path.Combine(zipFileDirectory, $"{safeEventName}_{eventId}.zip");
            var zipRelativeUrl = $"/downloads/CompressedFiles/{safeEventName}_{eventId}.zip";

            Directory.CreateDirectory(zipFileDirectory);

            ZipFile.CreateFromDirectory(downloadsFolder, zipFileFullPath);

            eventInfo.GeneratedInvitationsZipUri = zipRelativeUrl;

            await _context.SaveChangesAsync();

            Directory.Delete(downloadsFolder, true);

            //return zipFileFullPath;
        }
    }
}
