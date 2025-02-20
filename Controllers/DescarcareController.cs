using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> StartDescarcare([FromBody] DescarcareRequest request)
        {
            if (string.IsNullOrEmpty(request.VideoUrl))
                return BadRequest("⚠️ URL-ul videoclipului este necesar.");

            var rezultat = await _descarcatorService.DescarcaVideoAsync(request.VideoUrl);

            if (!rezultat.Success)
                return BadRequest(new { Eroare = rezultat.ErrorMessage });

            return Ok(new { CaleFisier = rezultat.Data });
        }
    }
}
