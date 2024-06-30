using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;

[ApiController]
[Route("files/")]
public class UploadController : ControllerBase
{
    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null // Ignore bad data
            }))
            {
                // Read the headers first
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                // Ensure headers is not null before proceeding
                if (headers == null)
                {
                    return BadRequest("No headers found in the CSV file");
                }
                // Read the records and ensure they are not null
                var records = csv.GetRecords<dynamic>().ToList();

                if (records == null)
                {
                    return BadRequest("No records found in the CSV file");
                }
                // Prepare JSON response
                var jsonResult = new
                {
                        contents = records.Select(record => {
                        var expando = (IDictionary<string, object>)record;
                        return headers.ToDictionary(header => header, header => expando.ContainsKey(header) ? expando[header] : null);
                    }).ToList()
                };

                return Ok(jsonResult);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
