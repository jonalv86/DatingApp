using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "Debe especificar una contraseña entre 4 y 8 caracteres.")]
        public string Password { get; set; }
    }
}