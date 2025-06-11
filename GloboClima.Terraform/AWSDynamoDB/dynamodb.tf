terraform {
  required_version = ">= 1.0"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.0"
    }
  }
}

# Variables para configuração
variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "project_name" {
  description = "Project name"
  type        = string
  default     = "globo-clima-app"
}

variable "table_name" {
  description = "DynamoDB table name"
  type        = string
  default     = "globo-clima-app-user-favorites"
}

# Locals para padronização
locals {
  table_name = var.table_name != "" ? var.table_name : "${var.project_name}-user-favorites-${var.environment}"
  
  common_tags = {
    Environment = var.environment
    Project     = var.project_name
    ManagedBy   = "terraform"
    Service     = "user-favorites"
  }
}

# DynamoDB Table para armazenar cidades favoritas dos usuários
resource "aws_dynamodb_table" "user_favorite_cities" {
  name           = local.table_name
  billing_mode   = var.environment == "prod" ? "ON_DEMAND" : "PROVISIONED"
  
  # Capacidade provisionada apenas para ambientes não-prod
  read_capacity  = var.environment != "prod" ? 5 : null
  write_capacity = var.environment != "prod" ? 5 : null
  
  hash_key  = "UserId"
  range_key = "LocationId"

  # Atributos principais
  attribute {
    name = "UserId"
    type = "S"
  }

  attribute {
    name = "LocationId"
    type = "S"
  }

  # Atributo para GSI por país
  attribute {
    name = "CountryCode"
    type = "S"
  }

  # Atributo para busca por data de criação
  attribute {
    name = "CreatedAt"
    type = "S"
  }

  # Global Secondary Index para buscar por país
  global_secondary_index {
    name            = "CountryCodeIndex"
    hash_key        = "CountryCode"
    range_key       = "CreatedAt"
    projection_type = "ALL"
    
    read_capacity  = var.environment != "prod" ? 2 : null
    write_capacity = var.environment != "prod" ? 2 : null
  }

  # Global Secondary Index para buscar todas as cidades de um usuário ordenadas por data
  global_secondary_index {
    name            = "UserCreatedAtIndex"
    hash_key        = "UserId"
    range_key       = "CreatedAt"
    projection_type = "ALL"
    
    read_capacity  = var.environment != "prod" ? 2 : null
    write_capacity = var.environment != "prod" ? 2 : null
  }

  # TTL para limpeza automática (opcional)
  ttl {
    attribute_name = "ExpiresAt"
    enabled        = true
  }

  # Point-in-time recovery
  point_in_time_recovery {
    enabled = var.environment == "prod" ? true : false
  }

  # Deletion protection para produção
  deletion_protection_enabled = var.environment == "prod" ? true : false

  tags = merge(local.common_tags, {
    Name = local.table_name
  })
}

# KMS Key para criptografia em produção
resource "aws_kms_key" "dynamodb_key" {
  count = var.environment == "prod" ? 1 : 0
  
  description             = "KMS key for DynamoDB encryption - ${local.table_name}"
  deletion_window_in_days = 7
  
  tags = merge(local.common_tags, {
    Name = "${local.table_name}-kms-key"
  })
}

resource "aws_kms_alias" "dynamodb_key_alias" {
  count = var.environment == "prod" ? 1 : 0
  
  name          = "alias/${local.table_name}-key"
  target_key_id = aws_kms_key.dynamodb_key[0].key_id
}

# IAM Policy para acesso à tabela
resource "aws_iam_policy" "dynamodb_policy" {
  name        = "${local.table_name}-access-policy"
  description = "Policy for accessing ${local.table_name} DynamoDB table"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:PutItem",
          "dynamodb:GetItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan"
        ]
        Resource = [
          aws_dynamodb_table.user_favorite_cities.arn,
          "${aws_dynamodb_table.user_favorite_cities.arn}/index/*"
        ]
      }
    ]
  })

  tags = local.common_tags
}

# CloudWatch Alarms para monitoramento
resource "aws_cloudwatch_metric_alarm" "read_throttle_alarm" {
  count = var.environment == "prod" ? 1 : 0
  
  alarm_name          = "${local.table_name}-read-throttle"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "ReadThrottledEvents"
  namespace           = "AWS/DynamoDB"
  period              = "300"
  statistic           = "Sum"
  threshold           = "0"
  alarm_description   = "This metric monitors read throttle events"

  dimensions = {
    TableName = aws_dynamodb_table.user_favorite_cities.name
  }

  tags = local.common_tags
}

resource "aws_cloudwatch_metric_alarm" "write_throttle_alarm" {
  count = var.environment == "prod" ? 1 : 0
  
  alarm_name          = "${local.table_name}-write-throttle"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "WriteThrottledEvents"
  namespace           = "AWS/DynamoDB"
  period              = "300"
  statistic           = "Sum"
  threshold           = "0"
  alarm_description   = "This metric monitors write throttle events"

  dimensions = {
    TableName = aws_dynamodb_table.user_favorite_cities.name
  }

  tags = local.common_tags
}

# Outputs
output "dynamodb_table_name" {
  description = "Name of the DynamoDB table"
  value       = aws_dynamodb_table.user_favorite_cities.name
}

output "dynamodb_table_arn" {
  description = "ARN of the DynamoDB table"
  value       = aws_dynamodb_table.user_favorite_cities.arn
}

output "dynamodb_policy_arn" {
  description = "ARN of the IAM policy for DynamoDB access"
  value       = aws_iam_policy.dynamodb_policy.arn
}

output "kms_key_arn" {
  description = "ARN of the KMS key (prod only)"
  value       = var.environment == "prod" ? aws_kms_key.dynamodb_key[0].arn : null
}