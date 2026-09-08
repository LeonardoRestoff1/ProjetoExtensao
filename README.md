# Biblioteca Online

Sistema de gerenciamento de biblioteca comunitária desenvolvido em C# com ASP.NET Core.

## Descrição

Aplicação web desenvolvida para auxiliar bibliotecas escolares e comunitárias no gerenciamento eficiente do acervo e dos empréstimos de livros. O sistema oferece uma solução simples, gratuita e funcional para melhorar o acesso à leitura e promover a organização.

## Tecnologias Utilizadas

- **C#** - Linguagem de programação
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM para acesso a dados
- **SQLite** - Banco de dados
- **Bootstrap 5** - Framework CSS para interface
- **Razor Pages** - Engine de views

## Funcionalidades

### 1. Cadastro de Livros
- Inserir dados como título, autor, categoria e quantidade disponível
- Editar informações dos livros cadastrados
- Excluir livros do acervo
- Visualizar detalhes de cada livro

### 2. Cadastro de Usuários
- Armazenar informações como nome, CPF, telefone e e-mail
- Editar dados dos usuários
- Excluir usuários
- Visualizar empréstimos ativos por usuário

### 3. Empréstimo de Livros
- Registrar empréstimos com data de retirada e previsão de devolução
- Controle automático de estoque (reduz quantidade ao emprestar)
- Validação de disponibilidade de exemplares
- Visualização de empréstimos ativos e atrasados

### 4. Devolução de Livros
- Registrar devoluções de livros
- Atualização automática do estoque (aumenta quantidade ao devolver)
- Controle de prazos e identificação de atrasos

### 5. Consulta de Acervo (Livros Disponíveis)
- Visualizar todos os livros cadastrados
- Filtros por título, autor ou categoria
- Indicação visual de disponibilidade (badges coloridos)

### 6. Consulta de Livros Emprestados por Usuário
- Visualizar todos os empréstimos ativos de um usuário específico
- Identificação de empréstimos atrasados
- Informações detalhadas de cada empréstimo

### 7. Histórico de Empréstimos
- Registro completo de todas as movimentações
- Visualização de empréstimos devolvidos e ativos
- Filtros e buscas

## Estrutura do Projeto

```
BibliotecaOnline/
├── Controllers/
│   ├── HomeController.cs
│   ├── LivrosController.cs
│   ├── UsuariosController.cs
│   └── EmprestimosController.cs
├── Data/
│   └── BibliotecaDbContext.cs
├── Models/
│   ├── Administrador.cs
│   ├── Emprestimo.cs
│   ├── Livro.cs
│   └── Usuario.cs
├── Views/
│   ├── Home/
│   ├── Livros/
│   ├── Usuarios/
│   └── Emprestimos/
└── wwwroot/
```

## Modelo de Dados

### Entidades

- **Usuario**: ID, Nome, Email, CPF, Telefone
- **Livro**: ID, Título, Autor, Categoria, Quantidade
- **Emprestimo**: ID, DataInicio, DataFim, DataDevolucaoReal, IdUsuario (FK), IdLivro (FK)
- **Administrador**: ID, Nome, Email, Senha

### Relacionamentos

- Usuario 1:N Emprestimo (um usuário pode ter vários empréstimos)
- Livro 1:N Emprestimo (um livro pode ter vários empréstimos)

## Como Executar

### Pré-requisitos

- .NET 8.0 SDK ou superior
- Visual Studio 2022 ou VS Code (opcional)

### Passos

1. Clone ou baixe o repositório
2. Abra o terminal na pasta do projeto
3. Execute o comando:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
4. Acesse `https://localhost:5001` ou `http://localhost:5000` no navegador

### Primeira Execução

O banco de dados SQLite será criado automaticamente na primeira execução. O arquivo `BibliotecaOnline.db` será gerado na pasta raiz do projeto.

## Funcionalidades de Controle

- **Controle de Estoque**: A quantidade de livros é automaticamente atualizada ao realizar empréstimos e devoluções
- **Validações**: Campos obrigatórios são validados tanto no cliente quanto no servidor
- **Mensagens de Feedback**: Sistema de mensagens de sucesso e erro para melhor experiência do usuário
- **Interface Responsiva**: Layout adaptável para diferentes tamanhos de tela

## Melhorias Futuras

- Sistema de autenticação e autorização
- Relatórios em PDF
- Notificações de empréstimos próximos ao vencimento
- Sistema de multas por atraso
- Exportação de dados
- API REST para integração com outros sistemas

## Desenvolvedor

Projeto desenvolvido como parte do Projeto de Extensão I - Aplicação Web.

## Licença

Este projeto é de código aberto e está disponível para uso educacional e comunitário.






