using System.Security.Claims;
using GameApi.DTOs;
using GameApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize]
public class CharactersController : ControllerBase
{
    private readonly CharacterService _characterService;

    public CharactersController(CharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCharacterRequest request)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var character = await _characterService.CreateAsync(accountId, request);
        return StatusCode(StatusCodes.Status201Created, character);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCharacterRequest request)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var character = await _characterService.UpdateAsync(accountId, id, request);
        if (character is null)
        {
            return NotFound(new { message = "Không tìm thấy nhân vật của bạn" });
        }

        return Ok(character);
    }
}
