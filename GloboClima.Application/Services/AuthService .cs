using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using GloboClima.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using GloboClima.Application.DTOs.Request.Auth;
using GloboClima.Application.DTOs.Response.Auth;
using System.IdentityModel.Tokens.Jwt;

namespace GloboClima.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly IConfiguration _configuration;
        private readonly string _userPoolId;
        private readonly string _clientId;

        public AuthService(IAmazonCognitoIdentityProvider cognitoClient, IConfiguration configuration)
        {
            _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userPoolId = _configuration["AWS:Cognito:UserPoolId"] ?? throw new InvalidOperationException("UserPoolId not configured");
            _clientId = _configuration["AWS:Cognito:AppClientId"] ?? throw new InvalidOperationException("AppClientId not configured");
        }

        #region Autenticação Básica

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                var userAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = request.Email },
                    new AttributeType { Name = "given_name", Value = request.FirstName },
                    new AttributeType { Name = "family_name", Value = request.LastName }
                };

                var signUpRequest = new SignUpRequest
                {
                    ClientId = _clientId,
                    Username = request.Email,
                    Password = request.Password,
                    UserAttributes = userAttributes
                };

                var response = await _cognitoClient.SignUpAsync(signUpRequest);

                return new RegisterResponseDto
                {
                    Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK,
                    Message = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                        ? "Registration successful. Please check your email for verification code."
                        : $"Registration failed. Status: {response.HttpStatusCode}",
                    UserSub = response.UserSub
                };
            }
            catch (UsernameExistsException)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "A user with this email already exists"
                };
            }
            catch (InvalidPasswordException ex)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"Invalid password: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<ConfirmSignUpResponseDto> ConfirmSignUpAsync(ConfirmSignUpRequestDto request)
        {
            try
            {
                var confirmRequest = new ConfirmSignUpRequest
                {
                    ClientId = _clientId,
                    Username = request.Email,
                    ConfirmationCode = request.ConfirmationCode
                };

                var response = await _cognitoClient.ConfirmSignUpAsync(confirmRequest);

                return new ConfirmSignUpResponseDto
                {
                    Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK,
                    Message = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                        ? "Email confirmed successfully. You can now login."
                        : $"Email confirmation failed. Status: {response.HttpStatusCode}"
                };
            }
            catch (CodeMismatchException)
            {
                return new ConfirmSignUpResponseDto
                {
                    Success = false,
                    Message = "Invalid confirmation code. Please check the code and try again."
                };
            }
            catch (ExpiredCodeException)
            {
                return new ConfirmSignUpResponseDto
                {
                    Success = false,
                    Message = "Confirmation code has expired. Please request a new code."
                };
            }
            catch (Exception ex)
            {
                return new ConfirmSignUpResponseDto
                {
                    Success = false,
                    Message = $"Email confirmation failed: {ex.Message}"
                };
            }
        }

        public async Task<ResendCodeResponseDto> ResendConfirmationCodeAsync(ResendCodeRequestDto request)
        {
            try
            {
                var resendRequest = new ResendConfirmationCodeRequest
                {
                    ClientId = _clientId,
                    Username = request.Email
                };

                var response = await _cognitoClient.ResendConfirmationCodeAsync(resendRequest);

                return new ResendCodeResponseDto
                {
                    Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK,
                    Message = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                        ? "Confirmation code resent successfully. Please check your email."
                        : $"Failed to resend confirmation code. Status: {response.HttpStatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new ResendCodeResponseDto
                {
                    Success = false,
                    Message = $"Failed to resend confirmation code: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var authRequest = new InitiateAuthRequest
                {
                    ClientId = _clientId,
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", request.Email },
                        { "PASSWORD", request.Password }
                    }
                };

                var response = await _cognitoClient.InitiateAuthAsync(authRequest);

                if (response.AuthenticationResult?.IdToken != null)
                {
                    var userInfo = ExtractUserInfoFromToken(response.AuthenticationResult.IdToken);

                    return new LoginResponseDto
                    {
                        Success = true,
                        Message = "Login successful",
                        AccessToken = response.AuthenticationResult.AccessToken,
                        IdToken = response.AuthenticationResult.IdToken,
                        RefreshToken = response.AuthenticationResult.RefreshToken,
                        ExpiresAt = DateTime.UtcNow.AddSeconds((double)response.AuthenticationResult.ExpiresIn),
                        UserInfo = userInfo
                    };
                }

                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials or user not confirmed"
                };
            }
            catch (UserNotConfirmedException)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "User not confirmed. Please check your email and confirm your account first."
                };
            }
            catch (NotAuthorizedException)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            try
            {
                var refreshRequest = new InitiateAuthRequest
                {
                    ClientId = _clientId,
                    AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "REFRESH_TOKEN", request.RefreshToken }
                    }
                };

                var response = await _cognitoClient.InitiateAuthAsync(refreshRequest);

                if (response.AuthenticationResult != null)
                {
                    return new RefreshTokenResponseDto
                    {
                        Success = true,
                        Message = "Token refreshed successfully",
                        AccessToken = response.AuthenticationResult.AccessToken,
                        IdToken = response.AuthenticationResult.IdToken,
                        ExpiresAt = DateTime.UtcNow.AddSeconds((double)response.AuthenticationResult.ExpiresIn)
                    };
                }

                return new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = "Failed to refresh token"
                };
            }
            catch (NotAuthorizedException)
            {
                return new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = "Refresh token is invalid or expired"
                };
            }
            catch (Exception ex)
            {
                return new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = $"Token refresh failed: {ex.Message}"
                };
            }
        }

        #endregion

        #region Gerenciamento de Senha

        public async Task<ChangePasswordResponseDto> ChangePasswordAsync(string email, ChangePasswordRequestDto request)
        {
            try
            {
                // Primeiro, fazer login para obter o access token
                var loginRequest = new InitiateAuthRequest
                {
                    ClientId = _clientId,
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", email },
                        { "PASSWORD", request.CurrentPassword }
                    }
                };

                var loginResponse = await _cognitoClient.InitiateAuthAsync(loginRequest);

                if (loginResponse.AuthenticationResult?.AccessToken == null)
                {
                    return new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Alterar a senha
                var changePasswordRequest = new ChangePasswordRequest
                {
                    AccessToken = loginResponse.AuthenticationResult.AccessToken,
                    PreviousPassword = request.CurrentPassword,
                    ProposedPassword = request.NewPassword
                };

                var response = await _cognitoClient.ChangePasswordAsync(changePasswordRequest);

                return new ChangePasswordResponseDto
                {
                    Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK,
                    Message = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                        ? "Password changed successfully"
                        : "Failed to change password"
                };
            }
            catch (NotAuthorizedException)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "Current password is incorrect"
                };
            }
            catch (InvalidPasswordException ex)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = $"New password is invalid: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = $"Password change failed: {ex.Message}"
                };
            }
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            try
            {
                var forgotPasswordRequest = new ForgotPasswordRequest
                {
                    ClientId = _clientId,
                    Username = request.Email
                };

                var response = await _cognitoClient.ForgotPasswordAsync(forgotPasswordRequest);

                return new ForgotPasswordResponseDto
                {
                    Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK,
                    Message = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                        ? "Password reset code sent to your email"
                        : "Failed to send password reset code"
                };
            }
            catch (UserNotFoundException)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }
            catch (Exception ex)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = $"Password reset request failed: {ex.Message}"
                };
            }
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                var resetPasswordRequest = new ConfirmForgotPasswordRequest
                {
                    ClientId = _clientId,
                    Username = request.Email,
                    ConfirmationCode = request.ConfirmationCode,
                    Password = request.NewPassword
                };

                var response = await _cognitoClient.ConfirmForgotPasswordAsync(resetPasswordRequest);

                return new ResetPasswordResponseDto
                {
                    Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK,
                    Message = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                        ? "Password reset successfully"
                        : "Failed to reset password"
                };
            }
            catch (CodeMismatchException)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid confirmation code"
                };
            }
            catch (ExpiredCodeException)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "Confirmation code has expired"
                };
            }
            catch (Exception ex)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = $"Password reset failed: {ex.Message}"
                };
            }
        }

        #endregion

        #region Gerenciamento de Perfil
        public async Task<GetUserResponse> GetUserAsync(GetUserRequest request)
        {
            try
            {
                return await _cognitoClient.GetUserAsync(request);

            }
            catch (UserNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UserInfoDto?> GetUserProfileAsync(string email)
        {
            try
            {
                var getUserRequest = new AdminGetUserRequest
                {
                    UserPoolId = _userPoolId,
                    Username = email
                };

                var response = await _cognitoClient.AdminGetUserAsync(getUserRequest);


                return MapCognitoUserToUserInfo(response);
            }
            catch (UserNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UpdateProfileResponseDto> UpdateProfileAsync(string email, UpdateProfileRequestDto request)
        {
            try
            {
                var userAttributes = new List<AttributeType>();

                if (!string.IsNullOrEmpty(request.FirstName))
                    userAttributes.Add(new AttributeType { Name = "given_name", Value = request.FirstName });

                if (!string.IsNullOrEmpty(request.LastName))
                    userAttributes.Add(new AttributeType { Name = "family_name", Value = request.LastName });

                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    userAttributes.Add(new AttributeType { Name = "phone_number", Value = request.PhoneNumber });

                if (request.DateOfBirth.HasValue)
                    userAttributes.Add(new AttributeType { Name = "birthdate", Value = request.DateOfBirth.Value.ToString("yyyy-MM-dd") });

                if (!string.IsNullOrEmpty(request.Gender))
                    userAttributes.Add(new AttributeType { Name = "gender", Value = request.Gender });

                if (userAttributes.Any())
                {
                    var updateRequest = new AdminUpdateUserAttributesRequest
                    {
                        UserPoolId = _userPoolId,
                        Username = email,
                        UserAttributes = userAttributes
                    };

                    var response = await _cognitoClient.AdminUpdateUserAttributesAsync(updateRequest);

                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var updatedUserInfo = await GetUserProfileAsync(email);
                        return new UpdateProfileResponseDto
                        {
                            Success = true,
                            Message = "Profile updated successfully",
                            UserInfo = updatedUserInfo
                        };
                    }
                }

                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "No changes were made to the profile"
                };
            }
            catch (Exception ex)
            {
                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = $"Profile update failed: {ex.Message}"
                };
            }
        }

        #endregion
              
        #region Métodos Auxiliares

        private UserInfoDto ExtractUserInfoFromToken(string idToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(idToken);

                var userInfo = new UserInfoDto
                {
                    Sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "",
                    Email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                    EmailVerified = bool.Parse(token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value ?? "false"),
                    FirstName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value,
                    LastName = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value,
                    PhoneNumber = token.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value
                };

                // Extrair data de nascimento se disponível
                var birthdateClaim = token.Claims.FirstOrDefault(c => c.Type == "birthdate")?.Value;
                if (!string.IsNullOrEmpty(birthdateClaim) && DateTime.TryParse(birthdateClaim, out var birthdate))
                {
                    userInfo.DateOfBirth = birthdate;
                }

                // Extrair gênero se disponível
                userInfo.Gender = token.Claims.FirstOrDefault(c => c.Type == "gender")?.Value;

                // Extrair roles
                var rolesClaims = token.Claims.Where(c => c.Type == "cognito:groups").Select(c => c.Value).ToList();
                userInfo.Roles = rolesClaims;

                return userInfo;
            }
            catch
            {
                return new UserInfoDto();
            }
        }

        private UserInfoDto MapCognitoUserToUserInfo(AdminGetUserResponse cognitoUser)
        {
            var userInfo = new UserInfoDto
            {
                Sub = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "sub")?.Value ?? "",
                Email = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? "",
                EmailVerified = bool.Parse(cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "email_verified")?.Value ?? "false"),
                FirstName = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "given_name")?.Value,
                LastName = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "family_name")?.Value,
                PhoneNumber = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "phone_number")?.Value,
                Gender = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "gender")?.Value,
                CreatedAt = cognitoUser.UserCreateDate,
                UpdatedAt = cognitoUser.UserLastModifiedDate
            };

            // Extrair data de nascimento
            var birthdateAttr = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "birthdate")?.Value;
            if (!string.IsNullOrEmpty(birthdateAttr) && DateTime.TryParse(birthdateAttr, out var birthdate))
            {
                userInfo.DateOfBirth = birthdate;
            }

            return userInfo;
        }

        #endregion
    }
}