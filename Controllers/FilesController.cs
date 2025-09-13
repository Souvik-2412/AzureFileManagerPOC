using Microsoft.AspNetCore.Mvc;
using AzureFileManager.Services; // Add this line or update with the correct namespace for BlobService

[ApiController]
[Route("api")]
public class FilesController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly BlobService _blobService;
    private readonly TableService _tableService;

    public FilesController(AuthService authService, BlobService blobService, TableService tableService)
    {
        _authService = authService;
        _blobService = blobService;
        _tableService = tableService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        try
        {
            if (_authService.Register(user))
                return Ok(new { message = "Registered" });
            return BadRequest(new { message = "Email exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Registration failed", error = ex.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            if (_authService.Login(request.Email, request.Password))
                return Ok(new { message = "Logged in" });
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Login failed", error = ex.Message });
        }
    }

    [HttpPost("files/upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string userId)
    {
        try
        {
            var url = await _blobService.UploadFileAsync(file);
            await _tableService.AddMetadataAsync(new FileMetadata { PartitionKey = userId, RowKey = Guid.NewGuid().ToString(), FileUrl = url });
            return Ok(new { fileUrl = url });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "File upload failed", error = ex.Message });
        }
    }

    [HttpGet("files/list/{userId}")]
    public async Task<IActionResult> ListFiles(string userId)
    {
        try
        {
            var files = await _tableService.GetFilesAsync(userId);
            return Ok(files);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to list files", error = ex.Message });
        }
    }

    [HttpDelete("files/delete")]
    public async Task<IActionResult> DeleteFile([FromBody] DeleteFileRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RowKey))
            {
                return BadRequest(new { message = "RowKey is required." });
            }
            if (string.IsNullOrEmpty(request.BlobName))
            {
                return BadRequest(new { message = "BlobName is required." });
            }
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { message = "UserId is required." });
            }
            await _tableService.DeleteMetadataAsync(request.UserId, request.RowKey);
            await _blobService.DeleteFileAsync(request.BlobName);
            return Ok(new { message = "Deleted" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "File deletion failed", error = ex.Message });
        }
    }
}