using System.ComponentModel.DataAnnotations;

namespace GloboClima.Application.DTOs.Request.Auth
{
    /// <summary>
    /// Dados para login do usuário
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        /// <example>usuario@exemplo.com</example>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        /// <example>MinhaSenh@123</example>
        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para confirmação de registro
    /// </summary>
    public class ConfirmSignUpRequestDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Código de confirmação recebido por email
        /// </summary>
        [Required(ErrorMessage = "Código de confirmação é obrigatório")]
        public string ConfirmationCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para reenvio de código de confirmação
    /// </summary>
    public class ResendCodeRequestDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados necessários para registro de um novo usuário
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Email do usuário (será usado como username)
        /// </summary>
        /// <example>usuario@exemplo.com</example>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário (mínimo 8 caracteres)
        /// </summary>
        /// <example>MinhaSenh@123</example>
        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(8, ErrorMessage = "Senha deve ter pelo menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Primeiro nome do usuário
        /// </summary>
        /// <example>João</example>
        [Required(ErrorMessage = "Primeiro nome é obrigatório")]
        [MaxLength(50, ErrorMessage = "Primeiro nome não pode ter mais de 50 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Sobrenome do usuário
        /// </summary>
        /// <example>Silva</example>
        [Required(ErrorMessage = "Sobrenome é obrigatório")]
        [MaxLength(50, ErrorMessage = "Sobrenome não pode ter mais de 50 caracteres")]
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para renovação de token
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Refresh token para renovação
        /// </summary>
        [Required(ErrorMessage = "Refresh token é obrigatório")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para alteração de senha
    /// </summary>
    public class ChangePasswordRequestDto
    {
        /// <summary>
        /// Senha atual do usuário
        /// </summary>
        [Required(ErrorMessage = "Senha atual é obrigatória")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha do usuário
        /// </summary>
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(8, ErrorMessage = "Nova senha deve ter pelo menos 8 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da nova senha
        /// </summary>
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para solicitação de reset de senha
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para reset de senha
    /// </summary>
    public class ResetPasswordRequestDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Código de confirmação recebido por email
        /// </summary>
        [Required(ErrorMessage = "Código de confirmação é obrigatório")]
        public string ConfirmationCode { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha
        /// </summary>
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(8, ErrorMessage = "Nova senha deve ter pelo menos 8 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da nova senha
        /// </summary>
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para atualização de perfil
    /// </summary>
    public class UpdateProfileRequestDto
    {
        /// <summary>
        /// Primeiro nome do usuário
        /// </summary>
        [MaxLength(50, ErrorMessage = "Primeiro nome não pode ter mais de 50 caracteres")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Sobrenome do usuário
        /// </summary>
        [MaxLength(50, ErrorMessage = "Sobrenome não pode ter mais de 50 caracteres")]
        public string? LastName { get; set; }

        /// <summary>
        /// Telefone do usuário
        /// </summary>
        [Phone(ErrorMessage = "Telefone deve ter um formato válido")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Data de nascimento
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Gênero do usuário
        /// </summary>
        [MaxLength(20, ErrorMessage = "Gênero não pode ter mais de 20 caracteres")]
        public string? Gender { get; set; }
    }

    public class UserRequestDto
    {
        public string Username { get; set; } = string.Empty;
    }
}