﻿using Microsoft.AspNetCore.Mvc;
using TranscriereYouTube.Models;

[ApiController]
[Route("api/descarcare")]
public class DescarcareController : ControllerBase
{
    private readonly IVideoDownloader _videoDownloader;

    public DescarcareController(IVideoDownloader videoDownloader)
    {
        _videoDownloader = videoDownloader;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartDescarcare([FromBody] DescarcareRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoUrl))
            return BadRequest("⚠️ URL-ul videoclipului este necesar.");

        var rezultat = await _videoDownloader.DownloadVideoAsync(request.VideoUrl);

        if (!rezultat.Success)
            return BadRequest(new { Eroare = rezultat.ErrorMessage });

        return Ok(new { CaleFisier = rezultat.Data });
    }
}
