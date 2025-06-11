using Microsoft.AspNetCore.Mvc;
using GloboClima.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using GloboClima.Application.DTOs.Request.Auth;
using GloboClima.Application.DTOs.Response.Auth;
using Amazon.CognitoIdentityProvider.Model;

namespace GloboClima.WebAPI.Controllers
{
    /// <summary>
    /// Controller responsável pela autenticação e gerenciamento de usuários via AWS Cognito
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("Autenticação")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region Autenticação Básica

        /// <summary>
        /// Registra um novo usuário no sistema
        /// </summary>
        /// <param name="request">Dados para registro do usuário</param>
        /// <returns>Resultado do registro</returns>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/auth/register
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "password": "MinhaSenh@123",
        ///         "firstName": "João",
        ///         "lastName": "Silva"
        ///     }
        /// 
        /// Após o registro, um código de confirmação será enviado por email.
        /// </remarks>
        /// <response code="201">Usuário registrado com sucesso</response>
        /// <response code="400">Dados inválidos ou usuário já existe</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);

            if (result.Success)
                return CreatedAtAction(nameof(Register), result);

            return BadRequest(result);
        }

        /// <summary>
        /// Confirma o registro do usuário com o código recebido por email
        /// </summary>
        /// <param name="request">Email e código de confirmação</param>
        /// <returns>Resultado da confirmação</returns>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/auth/verify-email
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "confirmationCode": "123456"
        ///     }
        /// </remarks>
        /// <response code="200">Email confirmado com sucesso</response>
        /// <response code="400">Código inválido ou expirado</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ConfirmSignUpResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ConfirmSignUpResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConfirmSignUpResponseDto>> VerifyEmail([FromBody] ConfirmSignUpRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ConfirmSignUpAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Reenvia o código de confirmação para o email do usuário
        /// </summary>
        /// <param name="request">Email do usuário</param>
        /// <returns>Resultado do reenvio</returns>
        /// <response code="200">Código reenviado com sucesso</response>
        /// <response code="400">Email não encontrado ou erro no reenvio</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ResendCodeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResendCodeResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResendCodeResponseDto>> ResendVerification([FromBody] ResendCodeRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResendConfirmationCodeAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Realiza o login do usuário
        /// </summary>
        /// <param name="request">Credenciais do usuário</param>
        /// <returns>Tokens de autenticação</returns>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "password": "MinhaSenh@123"
        ///     }
        ///     
        /// A resposta incluirá os tokens JWT para autenticação nas próximas requisições.
        /// </remarks>
        /// <response code="200">Login realizado com sucesso</response>
        /// <response code="400">Credenciais inválidas ou usuário não confirmado</response>
        /// <response code="401">Credenciais inválidas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            if (result.Success)
                return Ok(result);

            // Retorna 401 para credenciais inválidas, 400 para outros erros
            if (result.Message.Contains("Invalid email or password") || result.Message.Contains("credentials"))
                return Unauthorized(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Renova o token de acesso usando o refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>Novos tokens de autenticação</returns>
        /// <response code="200">Token renovado com sucesso</response>
        /// <response code="400">Refresh token inválido ou expirado</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Gerenciamento de Senha

        /// <summary>
        /// Altera a senha do usuário autenticado
        /// </summary>
        /// <param name="request">Senha atual e nova senha</param>
        /// <returns>Resultado da alteração</returns>
        /// <response code="200">Senha alterada com sucesso</response>
        /// <response code="400">Senha atual incorreta ou nova senha inválida</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ChangePasswordResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = await GetUserEmailAsync();
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { success = false, message = "Usuário não encontrado" });

            var result = await _authService.ChangePasswordAsync(email, request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Solicita reset de senha via email
        /// </summary>
        /// <param name="request">Email do usuário</param>
        /// <returns>Resultado da solicitação</returns>
        /// <response code="200">Email de reset enviado com sucesso</response>
        /// <response code="400">Email não encontrado</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ForgotPasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ForgotPasswordResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Redefine a senha usando código de confirmação
        /// </summary>
        /// <param name="request">Email, código e nova senha</param>
        /// <returns>Resultado da redefinição</returns>
        /// <response code="200">Senha redefinida com sucesso</response>
        /// <response code="400">Código inválido ou expirado</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResetPasswordResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Gerenciamento de Perfil

        /// <summary>
        /// Obtém informações do usuário logado
        /// </summary>
        /// <returns>Dados do usuário autenticado</returns>
        /// <remarks>
        /// Endpoint protegido que retorna as informações do usuário autenticado.
        /// 
        /// Exemplo de uso:
        /// 
        ///     GET /api/auth/me
        ///     Authorization: Bearer {seu-jwt-token}
        /// </remarks>
        /// <response code="200">Informações do usuário retornadas com sucesso</response>
        /// <response code="401">Token inválido ou usuário não autenticado</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserInfoDto>> GetProfile()
        {
            try
            {
                var email = await GetUserEmailAsync();
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { success = false, message = "Usuário não encontrado" });

                var userInfoDto = _authService.GetUserProfileAsync(email);

                return Ok(new { success = true, data = userInfoDto });

            }
            catch (NotAuthorizedException)
            {
                return Unauthorized(new { success = false, message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao obter perfil: {ex.Message}" });
            }
        }

        /// <summary>
        /// Atualiza informações do perfil do usuário
        /// </summary>
        /// <param name="request">Dados para atualização</param>
        /// <returns>Resultado da atualização</returns>
        /// <response code="200">Perfil atualizado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpPut("update-profile")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UpdateProfileResponseDto>> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = await GetUserEmailAsync();
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { success = false, message = "Usuário não encontrado" });

            var result = await _authService.UpdateProfileAsync(email, request);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Métodos Auxiliares

        private async Task<string> GetUserEmailAsync()
        {
            var username = User.FindFirst("username")?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var getUserRequest = new UserRequestDto
            {
                Username = username
            };

            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var accessToken = authHeader.Substring("Bearer ".Length).Trim();
                var userRequest = new GetUserRequest
                {
                    AccessToken = authHeader.Substring("Bearer ".Length).Trim()
                };

                var cognitoUser = await _authService.GetUserAsync(userRequest);

                return cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? null;
            }
            return null;
        }

        #endregion
    }
}