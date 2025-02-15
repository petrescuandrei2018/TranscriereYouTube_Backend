using Microsoft.AspNetCore.Mvc;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Models;

namespace TranscriereYouTube.Controllers
{
    [ApiController]
    [Route("api/descarcare")]
    public class DescarcareController : ControllerBase
    {
        private readonly IDescarcatorService _descarcatorService;

        public DescarcareController(IDescarcatorService descarcatorService)
        {
            _descarcatorService = descarcatorService;
        }

        [HttpPost("start")]
        public IActionResult StartDescarcare([FromBody] DescarcareRequest request)
        {
            var rezultat = _descarcatorService.Descarca(request.VideoUrl);
            return Ok(new { CaleFisier = rezultat });
        }
    }
}
