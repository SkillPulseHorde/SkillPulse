using UserService.Domain.Entities;

namespace UserService.Application.Dto;

public record SubordinatesDto(
    List<UserDto> Subordinates);