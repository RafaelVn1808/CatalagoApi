# Documentação do Projeto - Catálogo para Lojas

## 1. Visão Geral

Sistema de catálogo web para lojas físicas, permitindo que clientes consultem produtos, preços e disponibilidade antes de se deslocarem até o ponto físico. **Não é e-commerce** — sem pagamento online nem entrega. Foco em informação, organização e apoio às vendas presenciais.

---

## 2. Hospedagem e Infraestrutura

### 2.1 Resumo da Stack

| Componente | Serviço | Finalidade |
|------------|---------|------------|
| Frontend | **Vercel** | Site estático responsivo |
| Backend | **Render** ou **Railway** | API ASP.NET Core |
| Banco de Dados | **Supabase** | PostgreSQL + Storage (imagens) |

### 2.2 Vercel (Frontend)

| Plano | Valor | Adequação | Incluso |
|-------|-------|-----------|---------|
| **Hobby** | **Grátis** | ✅ Recomendado para início | 1M edge requests/mês, 100 GB transferência, domínio customizado, HTTPS, CDN global, 100 deploys/dia |
| Pro | $20/usuário/mês | Projetos com mais tráfego | 10M requests, 1 TB transferência, builds mais rápidos, cold start prevention |

**Plano sugerido:** Hobby (grátis) — suficiente para catálogo com tráfego moderado.

**Domínio customizado:** Configuração gratuita no Vercel. O custo é apenas da compra do domínio (Registro.br: ~R$ 40/ano para .com.br; provedores internacionais: $10-15/ano para .com).

---

### 2.3 Render (Backend - API)

| Plano/Instância | Valor | Adequação | Observações |
|-----------------|-------|-----------|-------------|
| **Free** | **$0** | ⚠️ Apenas testes | Serviço "dorme" após 15 min inatividade — delay de ~1 min na primeira requisição |
| **Starter** | **$7/mês** | ✅ Recomendado para produção | 512 MB RAM, 0.5 CPU — sempre ativo, adequado para API .NET |
| Standard | $25/mês | Escala maior | 2 GB RAM, 1 CPU |

**Plano sugerido:** Starter ($7/mês) — API sempre disponível.

---

### 2.4 Railway (Backend - Alternativa)

| Plano | Valor | Incluso | Observações |
|-------|-------|---------|-------------|
| Free | $0 | $1 em créditos | Limitado para testes |
| **Hobby** | **$5/mês** | $5 em créditos | Paga pelo uso; créditos cobrem API pequena |
| Pro | $20/mês | $20 em créditos | Para maior volume |

**Vantagens do Railway:** Modelo usage-based, fácil deploy, bom para .NET.  
**Desvantagem:** Menos previsível — uso acima dos créditos gera cobrança extra.

**Comparativo Render vs Railway:** Para API sempre ligada, **Render Starter ($7)** tende a ser mais previsível. Railway pode ser mais barato se o tráfego for bem baixo.

---

### 2.5 Supabase (Banco de Dados + Imagens)

#### Banco PostgreSQL

| Plano | Valor | Incluso | Adequação |
|-------|-------|---------|-----------|
| **Free** | **$0** | 500 MB DB, 1 GB file storage, 5 GB egress | ⚠️ Projetos pausam após 1 semana inatividade |
| **Pro** | **$25/mês** | 8 GB DB, 100 GB storage, 250 GB egress, **backups diários (7 dias)** | ✅ Recomendado para produção |

#### Supabase Storage (Imagens)

- Incluso no plano: 1 GB (Free) ou 100 GB (Pro)
- Excedente Pro: $0.021/GB
- CDN integrada (Cloudflare)
- URLs públicas ou assinadas

**Plano sugerido:** Pro ($25/mês) — backups automáticos, mais espaço e sem pausa por inatividade.

---

### 2.6 Estimativa de Custo Mensal

| Cenário | Frontend | Backend | Banco/Storage | Domínio | **Total/mês** |
|---------|----------|---------|---------------|---------|---------------|
| **Mínimo (início)** | $0 (Vercel Hobby) | $7 (Render Starter) | $25 (Supabase Pro) | ~R$ 4 (diluído/ano) | **~$32 (~R$ 180)** |
| **Alternativa** | $0 | $5 (Railway Hobby) | $25 | ~R$ 4 | **~$30 (~R$ 165)** |

*Conversão aproximada: 1 USD ≈ R$ 5,60 (fev/2025)*

**Observação:** O domínio é custo anual único (R$ 40-80 para .com.br ou $10-15 para .com).

---

## 3. Regras de Negócio

### 3.1 Conversar com o Vendedor (não Reserva)

- **Não há** fluxo de reserva de produtos
- Botão: **"Conversar sobre este produto"** ou **"Falar com o vendedor"**
- Ao clicar: abre WhatsApp com mensagem pré-preenchida (ex.: "Olá! Tenho interesse no produto [Nome do Produto], código [Código]")
- Número do WhatsApp configurável por loja
- Painel admin pode exibir **"Contatos via site"** ou **"Leads por produto"** (opcional), não "reservas"

### 3.2 Multi-loja

- Sistema suporta **múltiplas lojas**
- Cada produto pode existir em **várias lojas**, com estoque independente
- Modelo de dados:
  - **Produto** (cadastro único)
  - **Loja** (endereço, horário, contato, WhatsApp)
  - **Estoque** ou **ProdutoLoja** (ProdutoId, LojaId, Quantidade)
- Na listagem: exibir **em quais lojas** o produto está disponível
- Exemplos: "Disponível em: Loja Centro (2 un), Loja Norte (1 un)" ou "Disponível em 2 lojas"

### 3.3 Armazenamento de Imagens

- Imagens **não** são armazenadas no PostgreSQL
- Uso de **Supabase Storage** (S3-like)
- Fluxo:
  1. Upload da imagem pelo painel admin
  2. Imagem enviada ao Supabase Storage
  3. URL da imagem salva no banco (ex.: `imagem_url` na tabela Produto)
- Benefícios: banco mais leve, CDN para imagens, melhor performance

### 3.4 Autenticação

- **JWT** para o painel administrativo
- Fluxo:
  1. Login (email/senha) → API valida → retorna JWT
  2. Frontend envia `Authorization: Bearer <token>` nas requisições
  3. API valida token em rotas protegidas
- JWT pode conter: `UserId`, `LojaId` (ou lista), `Role` (Admin, GerenteLoja, etc.)
- Rotas públicas (catálogo): sem autenticação

### 3.5 Domínio Customizado

- Domínio próprio para o frontend (ex.: `catalogo.minhaloja.com.br`)
- Configuração no Vercel: adicionar domínio no projeto → apontar DNS (CNAME ou A)
- HTTPS automático via Let's Encrypt
- CORS na API: permitir origem do domínio customizado

---

## 4. Arquitetura do Projeto

### 4.1 Diagrama de Arquitetura

```
                    ┌─────────────────────────────────────────┐
                    │              CLIENTE (Navegador)         │
                    └─────────────────────┬───────────────────┘
                                          │
                                          ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         VERCEL (Frontend)                                │
│  • Site estático (React/Vue/HTML)                                        │
│  • Domínio customizado                                                   │
│  • CDN global                                                            │
└─────────────────────────────────────┬───────────────────────────────────┘
                                      │ HTTPS (API calls)
                                      ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    RENDER ou RAILWAY (Backend)                           │
│  • API ASP.NET Core                                                      │
│  • Controllers: Produtos, Categorias, Lojas, Auth, Upload                │
│  • JWT validation                                                        │
│  • Regras de negócio                                                     │
└───────────┬─────────────────────────────────────┬───────────────────────┘
            │                                     │
            │ Connection String                   │ Supabase Storage API
            ▼                                     ▼
┌───────────────────────────────┐    ┌───────────────────────────────────┐
│     SUPABASE (PostgreSQL)     │    │     SUPABASE STORAGE               │
│  • Produtos, Categorias       │    │  • Bucket: imagens-produtos        │
│  • Lojas, Estoque             │    │  • URLs públicas ou assinadas      │
│  • Usuários (admin)           │    └───────────────────────────────────┘
└───────────────────────────────┘
```

### 4.2 Estrutura da API (ASP.NET Core)

```
CatalagoApi/
├── Controllers/
│   ├── AuthController.cs          # Login, JWT
│   ├── ProdutosController.cs      # CRUD + listagem pública
│   ├── CategoriasController.cs    # CRUD + listagem pública
│   ├── LojasController.cs         # CRUD + info loja
│   ├── EstoqueController.cs       # Controle por loja
│   └── UploadController.cs        # Upload de imagens → Supabase Storage
├── Models/
│   ├── Produto.cs
│   ├── Categoria.cs
│   ├── Loja.cs
│   ├── Estoque.cs (ou ProdutoLoja)
│   ├── Usuario.cs
│   └── DTOs/
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Services/
│   ├── AuthService.cs             # Geração e validação JWT
│   ├── SupabaseStorageService.cs  # Upload de imagens
│   └── ImportacaoCsvService.cs    # Importação planilha
├── Middleware/                     # Tratamento de erros, logging
└── appsettings.json
```

### 4.3 Modelo de Dados (Entidades principais)

```
┌──────────────┐     ┌─────────────────┐     ┌──────────────┐
│   Categoria  │────<│    Produto      │>────│  ProdutoLoja │
├──────────────┤     ├─────────────────┤     │  (Estoque)   │
│ Id           │     │ Id              │     ├──────────────┤
│ Nome         │     │ Nome            │     │ ProdutoId    │
│ Descricao    │     │ Descricao       │     │ LojaId       │
└──────────────┘     │ Preco           │     │ Quantidade   │
                     │ ImagemUrl       │     └──────┬───────┘
                     │ CategoriaId     │            │
                     │ Ativo           │            │
                     └─────────────────┘     ┌──────▼───────┐
                                             │     Loja     │
                     ┌──────────────┐        ├──────────────┤
                     │   Usuario    │        │ Id           │
                     ├──────────────┤        │ Nome         │
                     │ Id           │        │ Endereco     │
                     │ Email        │        │ Telefone     │
                     │ SenhaHash    │        │ WhatsApp     │
                     │ LojaId?      │        │ Horario      │
                     │ Role         │        └──────────────┘
                     └──────────────┘
```

### 4.4 Fluxos Principais

#### Público (catálogo)
1. Listar produtos (com filtro por categoria, busca, paginação)
2. Ver detalhe do produto (incluindo lojas com disponibilidade)
3. Clicar em "Conversar com o vendedor" → redireciona para WhatsApp

#### Admin
1. Login → recebe JWT
2. CRUD de produtos, categorias, lojas
3. Controle de estoque por loja
4. Upload de imagens (→ Supabase Storage)
5. Importação CSV (produtos e estoque)

---

## 5. Configurações Necessárias

### 5.1 Variáveis de Ambiente (API)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `ConnectionStrings__DefaultConnection` | Connection string PostgreSQL (Supabase) | `Host=...;Database=...;Username=...;Password=...` |
| `Jwt__Key` | Chave secreta para assinar JWT | String longa e aleatória |
| `Jwt__Issuer` | Emissor do token | `CatalagoApi` |
| `Jwt__ExpirationMinutes` | Tempo de validade | `60` |
| `Supabase__Url` | URL do projeto Supabase | `https://xxx.supabase.co` |
| `Supabase__ServiceKey` | Chave de serviço (upload) | `eyJ...` |
| `Cors__AllowedOrigins` | Origens permitidas (frontend) | `https://catalogo.seudominio.com.br` |

### 5.2 CORS

- Permitir apenas o domínio do frontend em produção
- Em desenvolvimento: permitir `http://localhost:3000` (ou porta do frontend)

### 5.3 Políticas de Storage (Supabase)

- Bucket `imagens-produtos`: público para leitura (para exibir no catálogo)
- Upload: apenas via API com `ServiceKey` (rotas autenticadas)

---

## 6. Itens Fora do Escopo (Conforme Proposta Original)

- Pagamento online
- Carrinho de compras
- Entrega de produtos
- Cadastro de clientes
- Aplicativo mobile
- Integração fiscal ou emissão de notas

---

## 7. Checklist de Implantação

- [ ] Criar conta Supabase, projeto e obter connection string
- [ ] Configurar bucket de imagens no Supabase Storage
- [ ] Criar conta Vercel e conectar repositório do frontend
- [ ] Criar conta Render (ou Railway) e conectar repositório da API
- [ ] Configurar variáveis de ambiente em cada serviço
- [ ] Comprar domínio e configurar DNS no Vercel
- [ ] Configurar CORS na API com domínio real
- [ ] Realizar deploy e testes end-to-end

---

*Documento gerado em fev/2025. Valores sujeitos a alteração pelos provedores.*
