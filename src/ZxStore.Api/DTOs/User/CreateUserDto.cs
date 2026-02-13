using System.ComponentModel.DataAnnotations;

namespace ZxStore.Api.DTOs.User;

public record CreateUserDto(

    [Required]
    [StringLength(18, MinimumLength = 3)]
    string Username,

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    string Password,

    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [DataType(DataType.EmailAddress)]
    string Email
    );
