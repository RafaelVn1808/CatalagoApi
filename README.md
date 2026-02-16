# CatalagoApi

API do sistema de catálogo para lojas físicas. Desenvolvida em ASP.NET Core com PostgreSQL.

## Requisitos

- .NET 10 SDK
- PostgreSQL 16+ (ou Supabase)

## Configuração

1. **Banco de dados**: Configure a connection string em `appsettings.json` ou use variáveis de ambiente:
   ```
   ConnectionStrings__DefaultConnection=Host=...;Database=catalago;Username=...;Password=...
   ```

2. **JWT**: Para produção, altere a chave em `appsettings.json`:
   ```
   Jwt__Key=sua-chave-secreta-longa-minimo-32-caracteres
   ```

3. **Supabase** (para upload de imagens): Configure em `appsettings.json` ou variáveis:
   ```
   Supabase__Url=https://seu-projeto.supabase.co
   Supabase__ServiceKey=sua-service-key
   ```
   Crie o bucket `imagens-produtos` no Supabase Storage (público para leitura).

## Executar

```bash
dotnet run --project CatalagoApi
```

A API estará em `https://localhost:7xxx` (verifique a porta no console).

## Usuário inicial (desenvolvimento)

- **Email:** admin@catalago.com
- **Senha:** Admin@123

## Endpoints principais

| Método | Rota | Autenticação | Descrição |
|--------|------|--------------|-----------|
| POST | /api/auth/login | Não | Login (retorna JWT) |
| GET | /api/produtos | Não | Listar produtos (busca, categoria, paginação) |
| GET | /api/produtos/{id} | Não | Detalhe do produto |
| POST | /api/produtos | Sim | Criar produto |
| PUT | /api/produtos/{id} | Sim | Atualizar produto |
| DELETE | /api/produtos/{id} | Sim | Excluir produto |
| PUT | /api/produtos/{id}/estoque | Sim | Atualizar estoque por loja |
| GET | /api/categorias | Não | Listar categorias |
| POST | /api/categorias | Sim | Criar categoria |
| GET | /api/lojas | Não | Listar lojas |
| POST | /api/lojas | Sim | Criar loja |
| POST | /api/upload/produto | Sim | Upload de imagem (form-data: file) |
| GET | /api/estoque | Sim | Listar estoque (query: lojaId, apenasComEstoque) |
| GET | /api/estoque/loja/{id} | Sim | Estoque de uma loja |
| POST | /api/importacao/csv | Sim | Importar produtos e estoque via CSV |

## Documentação OpenAPI

Em desenvolvimento: `https://localhost:7xxx/openapi/v1.json`
