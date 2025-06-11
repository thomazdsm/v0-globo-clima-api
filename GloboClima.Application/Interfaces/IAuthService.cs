using Amazon.CognitoIdentityProvider.Model;
using GloboClima.Application.DTOs.Request.Auth;
using GloboClima.Application.DTOs.Response.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.Interfaces
{
    public interface IAuthService
    {
        // Autenticação Básica
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<ConfirmSignUpResponseDto> ConfirmSignUpAsync(ConfirmSignUpRequestDto request);
        Task<ResendCodeResponseDto> ResendConfirmationCodeAsync(ResendCodeRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        // Gerenciamento de Senha
        Task<GetUserResponse> GetUserAsync(GetUserRequest request);
        Task<ChangePasswordResponseDto> ChangePasswordAsync(string email, ChangePasswordRequestDto request);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);

        // Gerenciamento de Perfil
        Task<UserInfoDto?> GetUserProfileAsync(string email);
        Task<UpdateProfileResponseDto> UpdateProfileAsync(string email, UpdateProfileRequestDto request);
    }
}
