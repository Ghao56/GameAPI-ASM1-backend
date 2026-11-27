using System.ComponentModel.DataAnnotations;

namespace GameApi.DTOs;

public sealed record CreateCharacterRequest(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [Required, StringLength(30, MinimumLength = 1)] string Class);

public sealed record UpdateCharacterRequest(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [Required, StringLength(30, MinimumLength = 1)] string Class,
    [Required, Range(1, 100)] int? Level,
    [Required, Range(0, 100000)] int? Hp);

public sealed record CharacterResponse(int Id, string Name, string Class, int Level, int Hp, int AccountId);
