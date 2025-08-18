using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.ExeptionHandling;
using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Identity.Client;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Polly.Caching;

namespace EventApi.Services
{
    public class FileHandlingService : IFileHandlingService
    {
        IWebHostEnvironment _webHostEnvironment;
        public FileHandlingService(IWebHostEnvironment webHost)
        {
            _webHostEnvironment = webHost;
        }

        public async Task<Result<string>> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return Result<string>.Failure(FileErrors.NullOrEmpty);
            }
            try
            {
                var uniqueFileName = $"{Guid.NewGuid().ToString()}_{imageFile.FileName}";

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/event_images");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                var imageUrl = $"/uploads/event_images/{uniqueFileName}";
                return Result<string>.Success(imageUrl);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(FileErrors.SaveFailed);
            }
        }

        public async Task<Result<List<Attendee>>> ParseAttendeesAsync(IFormFile attendeeFile)
        {

            if (attendeeFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return await ParseXlsxFileAsync(attendeeFile);
            }
            else if (attendeeFile.ContentType == "text/csv")
            {
                return await ParseCsvFileAsync(attendeeFile);
            }
            else
            {
                return Result<List<Attendee>>.Failure(FileErrors.UnsupportedType);
            }
        }
        public async Task<Result<List<Attendee>>> ParseXlsxFileAsync(IFormFile attendeeFile)
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

                var nameAliases = new[] { "name", "full name", "attendee name" };
                var emailAliases = new[] { "email", "email address" };

                int nameColumnIndex = -1;
                int emailColumnIndex = -1;

                IRow headerRow = sheet.GetRow(sheet.FirstRowNum);

                if (headerRow == null)
                {
                    // Handle case where the file is completely empty
                    return Result<List<Attendee>>.Failure(FileErrors.InvalidHeaders);
                }

                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    var headerText = headerRow.GetCell(i)?.ToString()?.Trim().ToLower();

                    if (string.IsNullOrEmpty(headerText)) continue;
                    if (nameAliases.Contains(headerText))
                    {
                        nameColumnIndex = i;
                    }
                    if (emailAliases.Contains(headerText))
                    {
                        emailColumnIndex = i;
                    }
                }
                if (emailColumnIndex == -1 || nameColumnIndex == -1)
                {
                    var missingHeaders = new List<string>();
                    if (nameColumnIndex == -1) missingHeaders.Add("Name");
                    if (emailColumnIndex == -1) missingHeaders.Add("Email");

                    // This stops the process and sends an error message up the chain.
                    return Result<List<Attendee>>.Failure(FileErrors.InvalidHeaders);
                }

                // Loop through the rows, starting from row 1 to skip the header (NPOI is 0-indexed)
                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue; // Skip empty rows

                    // Assuming Name is in Column 1 (index 0) and Email is in Column 2 (index 1)
                    var name = row.GetCell(nameColumnIndex)?.ToString()?.Trim();
                    var email = row.GetCell(emailColumnIndex)?.ToString()?.Trim();

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
                if (attendees.Any())
                {
                    return Result<List<Attendee>>.Failure(FileErrors.NoAttendeeFound);
                }
            }

            // This is an async method, but NPOI's stream reading is synchronous.
            // We return a completed task to match the method signature.
            return Result<List<Attendee>>.Success(attendees);
        }

        public async Task<Result<List<Attendee>>> ParseCsvFileAsync(IFormFile attendeeFile)
        {
            var attendees = new List<Attendee>();

            using (var reader = new StreamReader(attendeeFile.OpenReadStream()))
            {
                var headerLine = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(headerLine))
                {
                    return Result<List<Attendee>>.Failure(FileErrors.InvalidHeaders);
                }

                var nameAliases = new[] { "name", "attendee name", "full name" };
                var emailAliases = new[] { "email", "email address" };

                int nameColumnIndex = -1;
                int emailColumnIndex = -1;

                var headers = headerLine.Split(',');

                for (int i = 0; i < headers.Count(); i++)
                {
                    var headerText = headers[i].Trim().ToLower();
                    if (!string.IsNullOrEmpty(headerText))
                    {
                        if (nameAliases.Contains(headerText))
                        {
                            nameColumnIndex = i;
                        }
                        if (emailAliases.Contains(headerText))
                        {
                            emailColumnIndex = i;
                        }
                    }
                }
                if (nameColumnIndex == -1 || emailColumnIndex == -1)
                {
                    var missingHeaders = new List<string>();
                    if (nameColumnIndex == -1) missingHeaders.Add("Name");
                    if (emailColumnIndex == -1) missingHeaders.Add("Email");

                    return Result<List<Attendee>>.Failure(FileErrors.InvalidHeaders);
                }

                while (!reader.EndOfStream)
                {
                    var dataLine = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(dataLine)) continue;

                    var dataParts = dataLine.Split(',');
                    if (dataParts.Length <= nameColumnIndex || dataParts.Length <= emailColumnIndex)
                    {
                        continue;
                    }

                    var name = dataParts[nameColumnIndex].Trim();
                    var email = dataParts[emailColumnIndex].Trim();

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                    {
                        attendees.Add(new Attendee
                        {
                            Name = name,
                            Email = email
                        });
                    }
                }

                if (attendees.Any())
                {
                    return Result<List<Attendee>>.Failure(FileErrors.NoAttendeeFound);
                }
            }

            return Result<List<Attendee>>.Success(attendees);
        }
    }
}