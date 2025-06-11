using System.ComponentModel.DataAnnotations;

namespace GloboClima.Application.DTOs.Response.Auth
{
    /// <summary>
    /// Resposta base para operações de autenticação
    /// </summary>
    public class BaseAuthResponseDto
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem descritiva do resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta do registro de usuário
    /// </summary>
    public class RegisterResponseDto : BaseAuthResponseDto
    {
        /// <summary>
        /// ID único do usuário no Cognito
        /// </summary>
        public string? UserSub { get; set; }
    }

    /// <summary>
    /// Resposta da confirmação de registro
    /// </summary>
    public class ConfirmSignUpResponseDto : BaseAuthResponseDto
    {
    }

    /// <summary>
    /// Resposta do reenvio de código
    /// </summary>
    public class ResendCodeResponseDto : BaseAuthResponseDto
    {
    }

    /// <summary>
    /// Informações do usuário
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// ID único do usuário
        /// </summary>
        public string Sub { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o email foi verificado
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Primeiro nome do usuário
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Sobrenome do usuário
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Telefone do usuário
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Data de nascimento
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Gênero do usuário
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Data de criação da conta
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Roles/permissões do usuário
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Resposta do login
    /// </summary>
    public class LoginResponseDto : BaseAuthResponseDto
    {
        /// <summary>
        /// Token de acesso JWT
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Token de identidade
        /// </summary>
        public string? IdToken { get; set; }

        /// <summary>
        /// Token para renovação
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Data de expiração do token
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Informações do usuário logado
        /// </summary>
        public UserInfoDto? UserInfo { get; set; }
    }

    /// <summary>
    /// Resposta da renovação de token
    /// </summary>
    public class RefreshTokenResponseDto : BaseAuthResponseDto
    {
        /// <summary>
        /// Novo token de acesso
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Novo token de identidade
        /// </summary>
        public string? IdToken { get; set; }

        /// <summary>
        /// Data de expiração do novo token
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Resposta da alteração de senha
    /// </summary>
    public class ChangePasswordResponseDto : BaseAuthResponseDto
    {
    }

    /// <summary>
    /// Resposta da solicitação de reset de senha
    /// </summary>
    public class ForgotPasswordResponseDto : BaseAuthResponseDto
    {
    }

    /// <summary>
    /// Resposta do reset de senha
    /// </summary>
    public class ResetPasswordResponseDto : BaseAuthResponseDto
    {
    }

    /// <summary>
    /// Resposta da atualização de perfil
    /// </summary>
    public class UpdateProfileResponseDto : BaseAuthResponseDto
    {
        /// <summary>
        /// Informações atualizadas do usuário
        /// </summary>
        public UserInfoDto? UserInfo { get; set; }
    }
}