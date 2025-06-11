# ============================================================================
# AWS Cognito Configuration for GloboClima API
# ============================================================================

terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# ============================================================================
# Variables
# ============================================================================

variable "project_name" {
  description = "Nome do projeto"
  type        = string
  default     = "globo-clima-app"
}

variable "environment" {
  description = "Ambiente (dev, staging, prod)"
  type        = string
  default     = "prod"
}

variable "app_domain" {
  description = "Domínio da aplicação"
  type        = string
  default     = "xferatech.com"
}

variable "admin_email" {
  description = "Email do administrador para notificações"
  type        = string
  default     = "admin@xferatech.com"
}

# ============================================================================
# Data Sources
# ============================================================================

data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

# ============================================================================
# Locals
# ============================================================================

locals {
  common_tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "terraform"
    Owner       = "GloboClima Team"
  }
  
  pool_name = "${var.project_name}-${var.environment}-user-pool"
  client_name = "${var.project_name}-${var.environment}-app-client"
}

# ============================================================================
# SNS Topic for Admin Notifications
# ============================================================================

resource "aws_sns_topic" "admin_notifications" {
  name = "${var.project_name}-${var.environment}-admin-notifications"
  
  tags = merge(local.common_tags, {
    Name = "Admin Notifications Topic"
  })
}

resource "aws_sns_topic_subscription" "admin_email" {
  topic_arn = aws_sns_topic.admin_notifications.arn
  protocol  = "email"
  endpoint  = var.admin_email
}

# ============================================================================
# Cognito User Pool
# ============================================================================

resource "aws_cognito_user_pool" "main" {
  name = local.pool_name

  # Configurações de login - usando apenas email
  auto_verified_attributes = ["email"]
  username_attributes      = ["email"]

  # Configurações de política de senha
  password_policy {
    minimum_length                   = 8
    require_uppercase                = true
    require_lowercase                = true
    require_numbers                  = true
    require_symbols                  = true
    temporary_password_validity_days = 1
  }

  # Configurações de MFA
  mfa_configuration = "OPTIONAL"
  
  software_token_mfa_configuration {
    enabled = true
  }

  # Schema de atributos personalizados
  schema {
    attribute_data_type      = "String"
    name                     = "email"
    required                 = true
    developer_only_attribute = false
    mutable                  = true
  }

  schema {
    attribute_data_type      = "String"
    name                     = "given_name"
    required                 = true
    developer_only_attribute = false
    mutable                  = true
  }

  schema {
    attribute_data_type      = "String"
    name                     = "family_name"
    required                 = true
    developer_only_attribute = false
    mutable                  = true
  }

  schema {
    attribute_data_type      = "String"
    name                     = "phone_number"
    required                 = false
    developer_only_attribute = false
    mutable                  = true
  }

  # Atributos personalizados
  schema {
    attribute_data_type      = "String"
    name                     = "user_type"
    developer_only_attribute = false
    mutable                  = true
    
    string_attribute_constraints {
      min_length = 1
      max_length = 50
    }
  }

  schema {
    attribute_data_type      = "String"
    name                     = "subscription_plan"
    developer_only_attribute = false
    mutable                  = true
    
    string_attribute_constraints {
      min_length = 1
      max_length = 20
    }
  }

  schema {
    attribute_data_type      = "DateTime"
    name                     = "last_login"
    developer_only_attribute = false
    mutable                  = true
  }

  schema {
    attribute_data_type      = "Boolean"
    name                     = "marketing_consent"
    developer_only_attribute = false
    mutable                  = true
  }

  # Configurações de verificação
  verification_message_template {
    default_email_option = "CONFIRM_WITH_CODE"
    email_subject        = "GloboClima - Código de Verificação"
    email_message        = "Seu código de verificação é: {####}"
  }

  # Configurações de recuperação de conta
  account_recovery_setting {
    recovery_mechanism {
      name     = "verified_email"
      priority = 1
    }
  }

  # Configurações de administração
  admin_create_user_config {
    allow_admin_create_user_only = false
    
    invite_message_template {
      email_subject = "GloboClima - Convite para acessar a plataforma"
      email_message = "Você foi convidado para acessar o GloboClima. Seu nome de usuário temporário é {username} e sua senha temporária é {####}"
      sms_message   = "GloboClima: Usuário {username}, senha temporária {####}"
    }
  }

  # Configurações de dispositivos
  device_configuration {
    challenge_required_on_new_device      = true
    device_only_remembered_on_user_prompt = true
  }

  # Configurações de email - usando padrão AWS
  email_configuration {
    email_sending_account = "COGNITO_DEFAULT"
  }

  # Configurações de usuário
  user_pool_add_ons {
    advanced_security_mode = "ENFORCED"
  }

  tags = merge(local.common_tags, {
    Name = "GloboClima User Pool"
  })

}

# ============================================================================
# Cognito User Pool Domain
# ============================================================================

resource "aws_cognito_user_pool_domain" "main" {
  domain       = "${var.project_name}-${var.environment}-auth"
  user_pool_id = aws_cognito_user_pool.main.id

  depends_on = [aws_cognito_user_pool.main]
}

# ============================================================================
# Cognito App Client
# ============================================================================

resource "aws_cognito_user_pool_client" "web_client" {
  name         = "${local.client_name}-web"
  user_pool_id = aws_cognito_user_pool.main.id

  # Configurações OAuth
  allowed_oauth_flows_user_pool_client = true
  generate_secret                      = false
  allowed_oauth_scopes                 = ["openid", "email", "profile", "aws.cognito.signin.user.admin"]
  allowed_oauth_flows                  = ["code", "implicit"]

  # URLs de callback
  callback_urls = [
    "https://${var.app_domain}/auth/callback",
    "https://${var.app_domain}/",
    "http://localhost:3000/auth/callback",
    "http://localhost:3000/"
  ]

  logout_urls = [
    "https://${var.app_domain}/auth/logout",
    "https://${var.app_domain}/",
    "http://localhost:3000/"
  ]

  # Configurações de token
  access_token_validity                = 1     # 1 hora
  id_token_validity                    = 1     # 1 hora
  refresh_token_validity               = 30    # 30 dias
  token_validity_units {
    access_token  = "hours"
    id_token      = "hours"
    refresh_token = "days"
  }

  # Fluxos de autenticação permitidos
  explicit_auth_flows = [
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_CUSTOM_AUTH"
  ]

  # Configurações de leitura/escrita
  read_attributes = [
    "email",
    "email_verified",
    "given_name",
    "family_name",
    "phone_number",
    "phone_number_verified",
    "custom:user_type",
    "custom:subscription_plan",
    "custom:last_login",
    "custom:marketing_consent"
  ]

  write_attributes = [
    "email",
    "given_name",
    "family_name",
    "phone_number",
    "custom:user_type",
    "custom:subscription_plan",
    "custom:marketing_consent"
  ]

  # Prevenção de ataques de refresh token
  enable_token_revocation = true
  
  # Configurações avançadas
  prevent_user_existence_errors = "ENABLED"
}

# ============================================================================
# Cognito Identity Pool
# ============================================================================

resource "aws_cognito_identity_pool" "main" {
  identity_pool_name               = "${var.project_name}-${var.environment}-identity-pool"
  allow_unauthenticated_identities = false

  cognito_identity_providers {
    client_id               = aws_cognito_user_pool_client.web_client.id
    provider_name           = aws_cognito_user_pool.main.endpoint
    server_side_token_check = false
  }

  tags = local.common_tags
}

# ============================================================================
# IAM Roles for Identity Pool
# ============================================================================

resource "aws_iam_role" "authenticated" {
  name = "${var.project_name}-${var.environment}-cognito-authenticated"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Federated = "cognito-identity.amazonaws.com"
        }
        Action = "sts:AssumeRoleWithWebIdentity"
        Condition = {
          StringEquals = {
            "cognito-identity.amazonaws.com:aud" = aws_cognito_identity_pool.main.id
          }
          "ForAnyValue:StringLike" = {
            "cognito-identity.amazonaws.com:amr" = "authenticated"
          }
        }
      }
    ]
  })

  tags = local.common_tags
}

resource "aws_iam_role_policy" "authenticated" {
  name = "authenticated_policy"
  role = aws_iam_role.authenticated.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "cognito-sync:*",
          "cognito-identity:*"
        ]
        Resource = "*"
      }
    ]
  })
}

resource "aws_cognito_identity_pool_roles_attachment" "main" {
  identity_pool_id = aws_cognito_identity_pool.main.id

  roles = {
    authenticated = aws_iam_role.authenticated.arn
  }
}

# ============================================================================
# CloudWatch Log Groups
# ============================================================================

resource "aws_cloudwatch_log_group" "cognito_logs" {
  name              = "/aws/cognito/${local.pool_name}"
  retention_in_days = 30

  tags = local.common_tags
}

# ============================================================================
# Outputs
# ============================================================================

output "user_pool_id" {
  description = "ID do Cognito User Pool"
  value       = aws_cognito_user_pool.main.id
}

output "user_pool_arn" {
  description = "ARN do Cognito User Pool"
  value       = aws_cognito_user_pool.main.arn
}

output "user_pool_endpoint" {
  description = "Endpoint do Cognito User Pool"
  value       = aws_cognito_user_pool.main.endpoint
}

output "user_pool_domain" {
  description = "Domínio do Cognito User Pool"
  value       = aws_cognito_user_pool_domain.main.domain
}

output "web_client_id" {
  description = "ID do App Client Web"
  value       = aws_cognito_user_pool_client.web_client.id
}

output "identity_pool_id" {
  description = "ID do Cognito Identity Pool"
  value       = aws_cognito_identity_pool.main.id
}

output "cognito_domain_url" {
  description = "URL completa do domínio Cognito"
  value       = "https://${aws_cognito_user_pool_domain.main.domain}.auth.${data.aws_region.current.name}.amazoncognito.com"
}

output "aws_region" {
  description = "Região AWS atual"
  value       = data.aws_region.current.name
}