using System;
using System.IO;
using System.Security.Cryptography;
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
                string filename;

                using (var md5 = MD5.Create())
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var hash = md5.ComputeHash(stream);
                        filename = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
                filename = (filename ?? Path.GetFileNameWithoutExtension(file.FileName)) + Path.GetExtension(file.FileName);
                if(System.IO.File.Exists(Path.Combine(path, filename)))
                {
                    return new {
                        Success = true,
                        data = $"{Request.Scheme}://{Request.Host}{requestPath}/{filename}"
                    };
                }
                using (var fileStream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                {
                    
                    await file.CopyToAsync(fileStream);
                    return new {
                        Success = true,
                        data = $"{Request.Scheme}://{Request.Host}{requestPath}/{filename}"
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
