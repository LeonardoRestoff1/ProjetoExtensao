# Biblioteca Online — Documentação do projeto

Documento para apoio à entrega e apresentação (Projeto de Extensão — aplicação web em ASP.NET Core).

---

## 1. Visão geral

O **Biblioteca Online** é uma aplicação web para gestão de um acervo de livros e de empréstimos a usuários cadastrados. Permite:

- **Leitores**: consultar o acervo, solicitar empréstimos (com aprovação pelo administrador), acompanhar empréstimos e histórico, renovar e devolver quando aplicável.
- **Administradores**: manter livros e usuários leitores, operar empréstimos, aprovar ou rejeitar solicitações pendentes, consultar histórico e cadastrar outros administradores.

A interface segue o padrão **MVC** (Model — View — Controller) do ASP.NET Core, com páginas **Razor** e persistência via **Entity Framework Core**.

---

## 2. Tecnologias utilizadas

| Camada / recurso | Tecnologia |
|------------------|------------|
| Linguagem | C# |
| Framework web | **ASP.NET Core 8** (MVC) |
| Acesso a dados | **Entity Framework Core 8** (ORM) |
| Banco de dados | **SQLite** (arquivo local) |
| Interface | HTML, **Bootstrap 5**, CSS próprio, Bootstrap Icons |
| Autenticação simplificada | **Sessão** (cookie de sessão no servidor) |
| Containerização | **Docker** (Dockerfile multi-estágio) |
| Hospedagem | **Railway** (deploy via GitHub + Docker) |

---

## 3. Banco de dados

### 3.1 SQLite — banco embutido

- Banco **relacional embutido**: não exige servidor separado.
- Dados gravados em um arquivo: **`BibliotecaOnline.db`**.

### 3.2 Onde o arquivo fica

Na inicialização, o caminho é montado em código (`Program.cs`) usando `ContentRootPath`, sempre na pasta raiz do projeto. Em produção, pode ser sobrescrito pela variável de ambiente `BIBLIOTECA_DB_PATH` (usado no Docker/Railway para apontar para um volume persistente).

### 3.3 Como o esquema é criado

1. Na inicialização, `context.Database.Migrate()` aplica migrações pendentes.
2. Em seguida, `DbInitializer.Seed()` insere dados iniciais **somente se** ainda não existir administrador.
3. O arquivo `BibliotecaOnline.db` é criado automaticamente na primeira execução.

---

## 4. Organização do projeto

```
BibliotecaOnline/
├── Controllers/        → Ações HTTP, regras de acesso, orquestração
├── Models/             → Entidades (Usuario, Livro, Emprestimo, Administrador)
├── ViewModels/         → Modelos específicos de telas (dashboard, login)
├── Data/               → BibliotecaDbContext, DbInitializer (seed)
├── Migrations/         → Histórico de alterações do banco (EF Core)
├── Views/              → Razor (.cshtml) organizadas por controller
├── wwwroot/            → CSS, JS, imagens estáticas
├── Infrastructure/     → Constantes de sessão (SessionAuth.cs)
└── Program.cs          → Configuração do app, sessão, SQLite, migração, seed
```

---

## 5. Modelo de dados

### Entidades

- **Usuario** — leitor: nome, e-mail (login), telefone opcional, senha.
- **Livro** — título, autor, categoria, quantidade de exemplares (estoque lógico).
- **Emprestimo** — vínculo usuário + livro, datas de início/fim/devolução; flag `AguardandoAprovacao`.
- **Administrador** — nome, e-mail, senha; perfil separado do leitor, sem FK para empréstimos.

### Relacionamentos

- Um **Usuario** tem vários **Emprestimos** (1:N).
- Um **Livro** tem vários **Emprestimos** (1:N).
- Exclusão em cascata **restrita** (não apaga livro/usuário se houver empréstimo vinculado).

---

## 6. Autenticação e perfis

Autenticação por **sessão** (sem ASP.NET Identity ou JWT):

- Após login bem-sucedido, grava-se na sessão o **papel** (`User` ou `Admin`), nome e ID.
- Cada action controller verifica manualmente o papel antes de executar.
- Cookie de sessão: `HttpOnly`, timeout de 4 horas, `Secure` em produção.

**Conta inicial (seed):** criada automaticamente na primeira execução.

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | `admin@biblioteca.com` | `admin123` |
| Usuário teste | `joao@email.com` | `senha123` |
| Usuário teste | `ana@email.com` | `senha456` |

> Troque as senhas após o primeiro acesso em produção.

---

## 7. Fluxos principais

### Fluxo do leitor

1. **Cadastro** (`/Account/Register`) → cria conta, inicia sessão automaticamente.
2. **Login** (`/Account/Login`) → valida e-mail + senha.
3. **Consulta** (`/Livros`) → busca por título/autor/categoria, filtros de categoria e disponibilidade.
4. **Solicitação** → cria `Emprestimo` com `AguardandoAprovacao = true`. Estoque **não** é decrementado ainda.
5. **Meus empréstimos** → vê status (Pendente / Ativo / Atrasado), pode renovar ou devolver.

### Fluxo do administrador

1. **Login** com conta de `Administradores` → redireciona ao painel.
2. **Painel** (`/Admin`) → KPIs (acervo, ativos, atrasos) + tabela de solicitações pendentes.
3. **Aprovar** → decrementa estoque do livro, limpa `AguardandoAprovacao`.
4. **Rejeitar** → remove o registro de solicitação.
5. **Livros / Usuários / Empréstimos** → telas operacionais (CRUD completo).

---

## 8. Como executar localmente

**Pré-requisito:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

No terminal, dentro da pasta onde está o `.csproj`:

```powershell
dotnet restore
dotnet build
dotnet run
```

Acesse `http://localhost:5282`. Na primeira execução o banco é criado e o seed inserido automaticamente.

Para gerar os artefatos de produção localmente:

```powershell
.\publish.ps1
```

Os arquivos ficam na pasta `publish/`.

---

## 9. Testes realizados

Os testes foram realizados manualmente cobrindo os fluxos principais da aplicação. Não há framework de testes automatizados (xUnit/MSTest) neste projeto — os testes são funcionais e de usabilidade.

### 9.1 Testes funcionais — Perfil Leitor

| Cenário | Resultado |
|---------|-----------|
| Cadastro com dados válidos | ✅ Conta criada, sessão iniciada automaticamente |
| Cadastro com e-mail já existente | ✅ Mensagem de erro exibida |
| Cadastro com senha menor que 4 caracteres | ✅ Bloqueado com mensagem |
| Login com credenciais válidas | ✅ Redireciona para Home |
| Login com credenciais inválidas | ✅ Mensagem de erro exibida |
| Busca no acervo por título | ✅ Resultados filtrados corretamente |
| Busca no acervo por categoria | ✅ Filtro de categoria funciona |
| Filtro "Somente disponíveis" | ✅ Exibe apenas livros com estoque > 0 |
| Solicitar empréstimo de livro disponível | ✅ Criado com status Pendente, estoque não decrementado |
| Solicitar empréstimo de livro sem estoque | ✅ Botão não exibido |
| Solicitar empréstimo já existente (pendente) | ✅ Bloqueado pelo controller |
| Renovar empréstimo ativo (não atrasado) | ✅ DataFim estendida em 7 dias |
| Renovar empréstimo atrasado | ✅ Bloqueado com mensagem |
| Devolver empréstimo | ✅ DataDevolucaoReal preenchida, estoque incrementado |
| Visualizar histórico | ✅ Empréstimos devolvidos aparecem na seção Histórico |

### 9.2 Testes funcionais — Perfil Administrador

| Cenário | Resultado |
|---------|-----------|
| Login como administrador | ✅ Redireciona ao painel de administração |
| Painel exibe KPIs corretos | ✅ Total de livros, ativos, atrasos calculados |
| Solicitações pendentes aparecem no painel | ✅ Tabela com usuário, livro e data |
| Aprovar solicitação | ✅ Estoque decrementado, empréstimo ativado |
| Rejeitar solicitação | ✅ Registro removido |
| Cadastrar novo livro | ✅ Aparece no acervo imediatamente |
| Editar livro existente | ✅ Alterações persistidas |
| Excluir livro sem empréstimos | ✅ Removido com sucesso |
| Excluir livro com empréstimos | ✅ Bloqueado (integridade referencial) |
| Cadastrar novo usuário | ✅ Usuário aparece na lista |
| Excluir usuário sem empréstimos | ✅ Removido com sucesso |
| Excluir usuário com empréstimos | ✅ Bloqueado com mensagem |
| Criar empréstimo diretamente | ✅ Aprovado imediatamente, estoque decrementado |
| Registrar devolução pela lista de empréstimos | ✅ Funciona corretamente |
| Cadastrar novo administrador | ✅ Aparece na lista |
| Excluir o único administrador | ✅ Bloqueado (regra: não pode ficar sem admin) |
| Excluir a si mesmo | ✅ Bloqueado |

### 9.3 Testes de usabilidade

| Aspecto avaliado | Observação |
|------------------|------------|
| Navegação entre seções | Clara — navbar com seções ativas destacadas |
| Feedback de ações (sucesso/erro) | Alertas verdes/vermelhos via TempData em todas as ações |
| Status dos empréstimos | Badges coloridos (verde = ativo, amarelo = pendente, vermelho = atrasado, cinza = devolvido) |
| Responsividade mobile | Layout Bootstrap 5 adaptável; tabelas com data-label em telas pequenas |
| Estado vazio (sem dados) | Empty state com ícone e mensagem em todas as listagens |
| Acessibilidade básica | Labels com `for`, `aria-label` nos botões de toggle, `role="alert"` nas mensagens |
| Formulários de login/cadastro | Toggle mostrar/ocultar senha implementado |

---

## 10. Melhorias implementadas (entrega final)

Em relação ao protótipo inicial, as seguintes melhorias foram aplicadas:

### Interface (UI/UX)
- **Hero section** na página inicial com chamada para ação
- **KPI tiles coloridos** no painel admin (borda verde para ativos, vermelha para atrasos)
- **Badges de status semânticos** em todos os empréstimos (4 estados com cores distintas)
- **Toggle mostrar/ocultar senha** nos formulários de login e cadastro
- **Empty state padronizado** em todas as listagens (ícone + mensagem + ação sugerida)
- **Capa visual** na página de detalhes do livro
- **Alerta contextual** para visitantes não logados na página do livro
- **Ícones Bootstrap Icons** em botões, labels e cabeçalhos
- **Responsividade mobile** melhorada com `table-responsive-stack` e `data-label`
- **Breadcrumb** na página de detalhes do livro

### Código e produção
- `Program.cs` preparado para produção: cookie seguro, variáveis de ambiente `PORT` e `BIBLIOTECA_DB_PATH`
- `appsettings.Production.json` com nível de log mínimo
- `Dockerfile` multi-estágio (build com SDK alpine → runtime alpine mínimo)
- `.dockerignore` para build limpo
- `publish.ps1` para gerar artefatos de forma automatizada

---

## 11. Limitações conhecidas

- Senhas armazenadas **em texto simples** (adequado apenas para demonstração; em produção usar hash com `BCrypt` ou ASP.NET Identity).
- Autenticação por **sessão em memória** — sem JWT/OAuth, sem suporte a múltiplas instâncias simultâneas.
- Sem paginação nas listagens de histórico (pode degradar com muitos registros).
- Sem envio de e-mails (notificações, recuperação de senha).

---

## 12. Hospedagem no Railway (passo a passo)

O Railway detecta o `Dockerfile` automaticamente e faz o deploy a partir do repositório GitHub.

### 12.1 Pré-requisito: subir o código no GitHub

Se ainda não tiver o projeto no GitHub:

1. Crie uma conta em [github.com](https://github.com).
2. Crie um repositório **privado** chamado `BibliotecaOnline`.
3. No terminal, na pasta raiz onde estão `Dockerfile` e `BibliotecaOnline/`:

```powershell
git init
git add .
git commit -m "versão final - entrega"
git remote add origin https://github.com/SEU_USUARIO/BibliotecaOnline.git
git branch -M main
git push -u origin main
```

> O `.gitignore` já está configurado para **não enviar** o arquivo `*.db`.
> O banco é criado automaticamente no servidor na primeira execução.

### 12.2 Deploy no Railway

1. Acesse [railway.app](https://railway.app) e clique em **"Login with GitHub"**.

2. Clique em **"New Project"** → **"Deploy from GitHub repo"**.

3. Selecione o repositório `BibliotecaOnline`.
   - O Railway detecta o `Dockerfile` e configura o build automaticamente.

4. Clique em **"Deploy Now"** e aguarde (2–4 minutos).

5. Quando o status virar **"Active"**, vá em:
   **Settings → Networking → Generate Domain**
   Você receberá uma URL tipo `biblioteca-online-xxx.up.railway.app`.

6. **Configurar variáveis de ambiente** (aba "Variables"):
   ```
   BIBLIOTECA_DB_PATH  =  /data/BibliotecaOnline.db
   ```

7. **Configurar volume persistente** (aba "Volumes"):
   - Clique em "Add Volume"
   - Mount Path: `/data`
   - Isso garante que o banco de dados **não seja apagado** entre deploys.

8. Acesse a URL. O banco é criado e o seed executado automaticamente.
   **Acesse com:** `admin@biblioteca.com` / `admin123` e troque a senha.

### 12.3 Atualizar após mudanças

```powershell
git add .
git commit -m "descrição da mudança"
git push
```

O Railway detecta o novo commit e refaz o deploy automaticamente.

### 12.4 Plano gratuito do Railway

- **500 horas/mês** de execução (suficiente para demonstração acadêmica contínua).
- **1 GB** de volume incluído.
- Sem necessidade de cartão de crédito para o plano hobby.
- Limite pode ser consultado em [railway.app/pricing](https://railway.app/pricing).

---

## 13. GitHub Pages — página de apresentação do projeto

A pasta `docs/` contém uma página HTML estática com os diagramas e informações do projeto, publicável via GitHub Pages.

### Ativar o GitHub Pages

1. No repositório GitHub, vá em **Settings → Pages**.
2. Em **"Source"**, selecione: `Deploy from a branch`.
3. Branch: `main`, pasta: `/docs`.
4. Clique em **Save**.
5. Aguarde 1–2 minutos. A URL será gerada no formato:
   `https://SEU_USUARIO.github.io/BibliotecaOnline/`

A página contém:
- Apresentação do projeto e tecnologias
- Diagrama Entidade-Relacionamento (ER)
- Diagrama de arquitetura MVC
- Funcionalidades dos dois perfis (leitor e administrador)
- Fluxo de uso passo a passo
- Instruções de execução local

---

## 14. Estrutura de arquivos do repositório

```
BibliotecaOnline/          ← raiz do repositório
├── BibliotecaOnline/      ← projeto ASP.NET Core
│   ├── Controllers/
│   ├── Data/
│   ├── Infrastructure/
│   ├── Migrations/
│   ├── Models/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   ├── Program.cs
│   └── BibliotecaOnline.csproj
├── docs/                  ← GitHub Pages
│   ├── index.html
│   └── _config.yml
├── Dockerfile             ← build e deploy via Docker
├── .dockerignore
├── publish.ps1            ← script de publicação local
├── DEPLOY.md              ← guia detalhado de hospedagem
├── DOCUMENTACAO.md        ← este arquivo
└── .gitignore
```

---

## 15. O que destacar na apresentação

- **SQLite** como banco relacional local — ideal para projeto acadêmico, sem instalar servidor.
- **Entity Framework Core** com **migrações** versionando o esquema automaticamente.
- Separação em **MVC**, dois perfis (leitor × administrador), fluxo de **solicitação → aprovação**.
- **Deploy no Railway** via GitHub + Docker — aplicação acessível publicamente na internet.
- **GitHub Pages** com diagrama ER e arquitetura do sistema.
- Melhorias de **UX**: status com cores semânticas, empty states, responsividade mobile.

---

*Documento gerado para fins de documentação e apresentação da entrega do sistema Biblioteca Online.*
