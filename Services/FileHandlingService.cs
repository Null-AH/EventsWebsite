using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Interfaces;
using EventApi.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace EventApi.Services
{
    public class FileHandlingService : IFileHandlingService
    {
        IWebHostEnvironment _webHostEnvironment;
        public FileHandlingService(IWebHostEnvironment webHost)
        {
            _webHostEnvironment = webHost;
        }
        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // 1. Create a unique filename to prevent overwriting files with the same name.
            var uniqueFileName = $"{Guid.NewGuid().ToString()}_{imageFile.FileName}";

            // 2. Define the path where the images will be stored.
            //    This gets the absolute path to your wwwroot folder.
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/event_images");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 3. Ensure the directory exists.
            Directory.CreateDirectory(uploadsFolder);

            // 4. Save the file to the server.
            //    'using' ensures the stream is properly closed and disposed of.
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // 5. Return the publicly accessible URL path to the image.
            //    We don't want to store the full C:\... path in the database.
            return $"/uploads/event_images/{uniqueFileName}";
        }

        public async Task<List<Attendee>> ParseAttendeesAsync(IFormFile attendeeFile)
        {
            var attendees = new List<Attendee>();
            
            // NPOI works directly with the stream. No license is needed.
            using (var stream = attendeeFile.OpenReadStream())
            {
                stream.Position = 0; // Ensure the stream is at the beginning
                
                // XSSFWorkbook is for modern .xlsx files
                IWorkbook workbook = new XSSFWorkbook(stream);
                
                // Get the first sheet
                ISheet sheet = workbook.GetSheetAt(0);

                // Loop through the rows, starting from row 1 to skip the header (NPOI is 0-indexed)
                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue; // Skip empty rows

                    // Assuming Name is in Column 1 (index 0) and Email is in Column 2 (index 1)
                    var name = row.GetCell(0)?.ToString()?.Trim();
                    var email = row.GetCell(1)?.ToString()?.Trim();
                    
                    // Only add the attendee if a name & email exists
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                    {
                        attendees.Add(new Attendee
                        {
                            Name = name,
                            Email = email
                        });
                    }
                }
            }
            
            // This is an async method, but NPOI's stream reading is synchronous.
            // We return a completed task to match the method signature.
            return await Task.FromResult(attendees);
        }
    }
}