using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using hrms_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

[ApiController]
[Route("api/[controller]")]
public class QrCodeController : ControllerBase
{
    private readonly HrmsDbContext _dbContext;
    private readonly string BaseURL; // Update this field to store the base URL

    public QrCodeController(HrmsDbContext dbContext)
    {
        _dbContext = dbContext;

        // Set the base URL here. Update this based on your deployment environment.
        BaseURL = "http://10.0.0.183/hrms";
    }

    [HttpPost("generatecompanyqr/{companyId}")]
    public IActionResult GenerateCompanyQrCode(int companyId, [FromBody] QrCodeRequestModel request)
    {
        try
        {
            // Fetch the company details from the database
            var company = _dbContext.Companies.Include(c => c.Employees).FirstOrDefault(c => c.CompanyId == companyId);

            if (company == null)
            {
                return NotFound("Company not found");
            }

            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

            // Check if EmployeeId is provided
            if (employeeIdClaim == null || string.IsNullOrEmpty(employeeIdClaim.Value))
            {
                return BadRequest("Token Is missing");
            }


            // Check if QR code already exists for the company
            var officeLocation = _dbContext.OffLocations
                .Where(ol => ol.Fkcompanyid == companyId)
                .OrderByDescending(ol => ol.OfficeLocationId)
                .FirstOrDefault();

            if (officeLocation != null)
            {
                // Update existing QR code details
                officeLocation.Geofencing = request.Location;
                officeLocation.Latitude = request.Latitude;
                officeLocation.Longitude = request.Longitude;
                _dbContext.SaveChanges();

                // Construct the URL for the QR code image
                var imageUrl = $"{BaseURL}/qrcodes/getimagebycompany/{company.CompanyId}";

                // Return the URL of the updated QR code image
                return Ok(new { Message = "QR code successfully updated.", ImageUrl = imageUrl });
            }

            // Create QR Code with only company ID
            var encodedData = $"Company ID: {company.CompanyId}, Latitude: {request.Latitude}, Longitude: {request.Longitude}";

            var barcodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300
                }
            };

            var pixelData = barcodeWriter.Write(encodedData);

            // Convert the QR code to a byte array
            var imageBytes = PixelDataToByteArray(pixelData, pixelData.Width, pixelData.Height);

            // Save QR code image bytes to the wwwroot/qrcodes folder
            var imageId = Guid.NewGuid().ToString();
            var imagePath = Path.Combine("wwwroot", "qrcodes", $"{imageId}.png");
            System.IO.File.WriteAllBytes(imagePath, imageBytes);

            // Save QR code image details to the OffLocation table with geofencing information
            var newOfficeLocation = new OffLocation
            {
                OfficeName = company.CompanyName,
                Qrcode = imageBytes,
                Qrcodeimage = $"{BaseURL}/qrcodes/{imageId}.png", // Save the URL to the database
                Geofencing = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Fkcompanyid = company.CompanyId,
                ModifiedDate=DateTime.Now,
                IsActive=true,
                CreatedBy = employeeIdClaim.Value,
                ModifiedBy = employeeIdClaim.Value,
            };

            // Add the new office location to the database
            _dbContext.OffLocations.Add(newOfficeLocation);
            _dbContext.SaveChanges();

            // Construct the URL for the QR code image
            var newImageUrl = $"{BaseURL}/qrcodes/getimagebycompany/{company.CompanyId}";

            // Return the URL of the newly generated QR code image
            return Ok(new { Message = "QR code successfully generated.", ImageUrl = newImageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("getimagebycompany/{companyId}")]
    public IActionResult GetQrCodeImageByCompany(int companyId)
    {
        try
        {
            // Retrieve the office location details from the database, including the related company details
            var officeLocation = _dbContext.OffLocations
                .Include(ol => ol.Fkcompany)
                .Where(ol => ol.Fkcompanyid == companyId)
                .OrderByDescending(ol => ol.OfficeLocationId) // Use the primary key or another property that increases monotonically
                .FirstOrDefault();

            if (officeLocation == null || string.IsNullOrEmpty(officeLocation.Qrcodeimage))
            {
                return NotFound("QR code not found for the given Company ID");
            }

            // Return the direct URL to the QR code image file
            return Ok(new { ImageUrl = officeLocation.Qrcodeimage });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    [HttpGet("getimagecompany/{companyId}")]
    public IActionResult GetQrImageByCompany(int companyId)
    {
        try
        {
            var officeLocation = _dbContext.OffLocations
                .Where(ol => ol.Fkcompanyid == companyId)
                .OrderByDescending(ol => ol.OfficeLocationId)
                .FirstOrDefault();

            if (officeLocation == null || officeLocation.Qrcode == null)
            {
                return NotFound("QR code image not found for the given Company ID");
            }

            var stream = new MemoryStream(officeLocation.Qrcode);

            return File(stream, "image/png"); // Adjust the content type based on your image format
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    private byte[] PixelDataToByteArray(PixelData pixelData, int width, int height)
    {
        // Convert PixelData to Bitmap
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
        bitmap.UnlockBits(bitmapData);

        // Convert Bitmap to byte array
        using (var stream = new System.IO.MemoryStream())
        {
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
    }

    // Define a request model to receive data in the POST request body
    public class QrCodeRequestModel
    {
        public string Location { get; set; }
        public double Latitude { get; set; } // Latitude property
        public double Longitude { get; set; } // Longitude property
    }
}
