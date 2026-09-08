# Guia de Deploy — Biblioteca Online

> **Por que não Cloudflare Pages?**
> O Cloudflare Pages executa apenas sites estáticos (HTML/CSS/JS) ou funções JavaScript/TypeScript.
> Uma aplicação ASP.NET Core precisa de um servidor que rode o runtime do .NET — o Cloudflare Pages
> não oferece isso. As alternativas gratuitas mais simples são **Railway** e **Render**.
> Se quiser manter um domínio no Cloudflare, é possível usar o **Cloudflare Tunnel** como proxy
> na frente de qualquer servidor — explicado na opção 3.

---

## Pré-requisito único: o código no GitHub

Todos os serviços abaixo leem direto do GitHub. Antes de qualquer deploy:

1. Crie uma conta em [github.com](https://github.com) se ainda não tiver.
2. Crie um repositório **privado** chamado `BibliotecaOnline`.
3. No terminal, dentro da pasta `BibliotecaOnline\BibliotecaOnline` (onde está o `.gitignore`):

```powershell
git init
git add .
git commit -m "primeiro commit"
git remote add origin https://github.com/SEU_USUARIO/BibliotecaOnline.git
git push -u origin main
```

> O `.gitignore` já está configurado para **não enviar** o arquivo `*.db` (banco de dados local).
> O banco será criado automaticamente no servidor na primeira execução.

---

## Opção 1 — Railway (recomendado para iniciantes)

**O que é:** plataforma que detecta automaticamente o `Dockerfile` e faz o deploy com poucos cliques.
**Plano gratuito:** 500 horas/mês (suficiente para demonstração acadêmica).

### Passo a passo

1. Acesse [railway.app](https://railway.app) e faça login com sua conta do GitHub.

2. Clique em **"New Project"** → **"Deploy from GitHub repo"**.

3. Selecione o repositório `BibliotecaOnline`.

4. O Railway detecta o `Dockerfile` automaticamente. Clique em **"Deploy Now"**.

5. Aguarde o build (2–4 minutos). Quando aparecer **"Active"**, clique em **"Settings"** → **"Domains"** → **"Generate Domain"**.
   Você receberá uma URL pública tipo `biblioteca-online-producao.up.railway.app`.

6. **Configurar o banco de dados persistente:**
   - No painel do serviço, vá em **"Variables"** e adicione:
     ```
     BIBLIOTECA_DB_PATH = /data/BibliotecaOnline.db
     ```
   - Vá em **"Volumes"** → **"Add Volume"** → Monte em `/data`.
   - Isso garante que o banco não apague entre deploys.

7. Acesse a URL gerada. Na primeira visita, o banco é criado e o seed roda automaticamente.
   Login padrão: `admin@biblioteca.com` / `admin123` — **troque a senha após o primeiro acesso.**

### Atualizar o deploy depois

Basta fazer `git push` normalmente. O Railway detecta o novo commit e refaz o deploy automaticamente.

---

## Opção 2 — Render

**O que é:** alternativa ao Railway, também suporta Docker, plano gratuito disponível.
**Diferença principal:** no plano gratuito, o serviço "dorme" após 15 minutos sem acesso
(a primeira requisição depois do sleep demora ~30 segundos para acordar).

### Passo a passo

1. Acesse [render.com](https://render.com) e faça login com GitHub.

2. Clique em **"New +"** → **"Web Service"**.

3. Conecte o repositório `BibliotecaOnline`.

4. Configure:
   | Campo | Valor |
   |-------|-------|
   | Environment | `Docker` |
   | Branch | `main` |
   | Region | `Ohio (US East)` ou `Frankfurt` |
   | Instance Type | `Free` |

5. Em **"Environment Variables"**, adicione:
   ```
   BIBLIOTECA_DB_PATH = /data/BibliotecaOnline.db
   ```

6. Em **"Disks"** (aba avançada), adicione um disco:
   - Mount Path: `/data`
   - Size: `1 GB` (mínimo disponível)

   > ⚠️ Discos no Render são pagos (~$0.25/mês por GB). Para demonstração sem custo,
   > omita o volume — o banco será recriado a cada deploy (dados não persistem, mas o seed
   > reinsere os dados de exemplo automaticamente).

7. Clique em **"Create Web Service"**. A URL será gerada após o build.

---

## Opção 3 — Cloudflare Tunnel (domínio CF + servidor próprio)

**Quando usar:** você tem um servidor ou VPS (mesmo local, com IP dinâmico) e quer que
o site apareça com um domínio Cloudflare (ex.: `biblioteca.seudominio.com`).

**O que o Cloudflare Tunnel faz:** cria um túnel criptografado entre o seu servidor e
a rede do Cloudflare, sem precisar abrir portas no roteador.

### Passo a passo resumido

1. Tenha um domínio no Cloudflare (ou transfira um existente).

2. No servidor onde a aplicação rodará, instale o `cloudflared`:
   ```bash
   # Linux (Debian/Ubuntu)
   curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb
   sudo dpkg -i cloudflared.deb
   ```

3. Autentique:
   ```bash
   cloudflared tunnel login
   ```

4. Crie o túnel:
   ```bash
   cloudflared tunnel create biblioteca-online
   ```

5. Configure o arquivo `~/.cloudflared/config.yml`:
   ```yaml
   tunnel: <ID-DO-TUNEL>
   credentials-file: /root/.cloudflared/<ID-DO-TUNEL>.json

   ingress:
     - hostname: biblioteca.seudominio.com
       service: http://localhost:8080
     - service: http_status:404
   ```

6. Aponte o DNS no painel do Cloudflare:
   ```bash
   cloudflared tunnel route dns biblioteca-online biblioteca.seudominio.com
   ```

7. Suba a aplicação localmente com Docker:
   ```bash
   docker run -d -p 8080:8080 \
     -v biblioteca-data:/data \
     --name biblioteca \
     biblioteca-online
   ```

8. Inicie o túnel:
   ```bash
   cloudflared tunnel run biblioteca-online
   ```

O site ficará acessível em `https://biblioteca.seudominio.com` via rede Cloudflare.

---

## Resumo comparativo

| | Railway | Render | CF Tunnel |
|--|---------|--------|-----------|
| Facilidade | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| Custo | Gratuito (500h/mês) | Gratuito (dorme) | Gratuito (precisa de servidor) |
| Banco persistente | Sim (volume gratuito) | Sim (disco pago) | Sim (arquivo local) |
| Deploy automático | Sim (git push) | Sim (git push) | Manual |
| Domínio customizado | Sim (plano pago) | Sim (gratuito) | Sim (via Cloudflare) |
| **Melhor para** | Demonstração acadêmica | Demonstração acadêmica | Quem já tem servidor |

---

## Credenciais padrão (seed)

Criadas automaticamente na primeira execução:

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | `admin@biblioteca.com` | `admin123` |
| Usuário teste | `joao@email.com` | `senha123` |
| Usuário teste | `ana@email.com` | `senha456` |

> ⚠️ Troque as senhas logo após o primeiro deploy.
