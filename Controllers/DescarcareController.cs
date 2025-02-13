using Microsoft.AspNetCore.Mvc;
using TranscriereYouTube.Interfaces;

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
        public IActionResult StartDescarcare([FromBody] string videoUrl)
        {
            var rezultat = _descarcatorService.Descarca(videoUrl);
            return Ok(rezultat);
        }
    }
}