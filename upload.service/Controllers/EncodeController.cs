using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace upload.service.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class EncodeController : ControllerBase
    {
        [HttpPost("Gbk")]
        public FileResult GbkEncode([FromBody]Dictionary<string, string> data)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var gbk = Encoding.GetEncoding("gbk");
            var utf16 = Encoding.Unicode;
            var bytes = utf16.GetBytes(data.GetValueOrDefault("text"));
            var res = Encoding.Convert(utf16, gbk, bytes);
            return File(res, "text/plain", "data.txt");
        }
    }
}