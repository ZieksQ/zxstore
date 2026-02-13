using System.ComponentModel.DataAnnotations;

namespace ZxStore.Api.DTOs.User;

// NOTE: This is just the first version of update user dto 
// TODO: Create a separate dto for different services such as: 
// Update User Password (must have jwt auth to do this)
// Update User Username 
// Update User Info (needed user profile entity)
public record UpdateUserDto(

        [StringLength(100, MinimumLength = 3)]
        string? Username,

        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        string? Password,

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DataType(DataType.EmailAddress)]
        string? Email
    );
