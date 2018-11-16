using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace upload.service.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly string path;
        private readonly string requestPath;
        public UploadController(IOptions<Settings> setting)
        {
            path = setting.Value.UploadPath;
            requestPath = setting.Value.RequestPath;
        }

        [HttpPost]
        public async Task<ActionResult<dynamic>> Upload([FromForm]IFormFile file)
        {
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    return new {
                        Success = true,
                        data = $"{Request.Scheme}://{Request.Host}{requestPath}/{file.FileName}"
                    };
                }
            }
            return new
            {
                Success = false
            };
        }
    }
}
