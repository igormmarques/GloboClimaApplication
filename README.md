# GloboClimaApp 🌦️

GloboClimaApp é um aplicativo de previsão do tempo que oferece informações climáticas em tempo real para cidades ao redor do mundo. Os usuários podem pesquisar o clima de suas cidades favoritas, visualizar métricas detalhadas do clima e adicionar/remover cidades de sua lista de favoritos.

## Funcionalidades

- **Informações Climáticas**: Pesquise por uma cidade e obtenha informações detalhadas sobre o clima, como temperatura, umidade, velocidade do vento, nascer do sol, pôr do sol, e muito mais, fornecidas pela API OpenWeatherMap.
  
- **Gerenciamento de Favoritos**: Os usuários podem salvar suas cidades favoritas e visualizar os dados climáticos dessas cidades ao fazer login. As cidades favoritas podem ser removidas facilmente.

- **Autenticação de Usuários**: Login seguro com tokens JWT, que permitem que os usuários gerenciem suas sessões e favoritos de forma segura.

- **Design Responsivo**: O aplicativo foi desenvolvido para ser totalmente responsivo utilizando Bootstrap, garantindo uma experiência de usuário fluida em qualquer dispositivo.

- **Funcionalidade de Logout**: Os usuários podem sair de suas sessões de forma segura com a opção de logout.

## Estrutura do Projeto

### Backend (API)

O backend do GloboClimaApp é construído com ASP.NET Core e fornece APIs para lidar com a autenticação dos usuários e o gerenciamento de favoritos. Principais características:

- **Autenticação**: A API utiliza tokens JWT para autenticar os usuários. Esse token é enviado em todas as requisições para garantir acesso seguro.
  
- **Gerenciamento de Cidades Favoritas**: Uma API segura que permite aos usuários adicionar, remover e visualizar suas cidades favoritas.

### Frontend

O frontend foi desenvolvido com ASP.NET MVC devido à simplicidade e maior controle. As principais funcionalidades da interface incluem:

- **Barra de Pesquisa**: Insira o nome de uma cidade para obter suas condições climáticas atuais.

- **Favoritos**: O aplicativo exibe uma lista de cidades favoritas do usuário e permite a remoção dessas cidades da lista.

- **Informações do Usuário**: O nome do usuário é exibido após o login.

## Como Funciona

- **Login**: Os usuários fazem login no aplicativo com suas credenciais. Após a autenticação bem-sucedida, um token JWT é gerado e armazenado na sessão. Esse token é enviado no cabeçalho Authorization em todas as requisições subsequentes para a API.
  
- **Pesquisa de Clima**: Na página de clima, os usuários podem pesquisar por uma cidade e o aplicativo exibirá os dados climáticos atuais da cidade informada.

- **Adicionar aos Favoritos**: Após pesquisar uma cidade, os usuários podem clicar no botão "Adicionar aos Favoritos" para salvar a cidade. Isso envia uma solicitação para a API backend com o nome da cidade e o ID do usuário.
  
- **Visualizar e Gerenciar Favoritos**: Na dashboard, os usuários podem visualizar todas as suas cidades favoritas. O aplicativo recupera essa lista através da chamada à API `GetFavorites`. Os usuários também podem remover cidades dos favoritos clicando no botão "Remover".

## Endpoints da API

### API de Clima

- `GET /api/weather/{cityName}`: Busca dados climáticos para uma cidade específica.

### API de Favoritos

- `POST /api/favorites`: Adiciona uma cidade aos favoritos do usuário.

- `GET /api/favorites`: Obtém a lista de cidades favoritas do usuário.

- `DELETE /api/favorites/{cityName}`: Remove uma cidade dos favoritos do usuário.

## Configuração e Instalação

Clone o repositório:

```bash
git clone https://github.com/igormmarques/GloboClimaApplication.git

Navegue até o diretório do projeto:

```bash
cd GloboClimaApp
Configure o arquivo appsettings.json com suas chaves de API para o OpenWeatherMap e as credenciais de assinatura JWT.

Restaure os pacotes necessários:
dotnet restore

Execute o projeto:
dotnet run
```

## Tecnologias Utilizadas

- **Backend**: ASP.NET Core (API)
- **Frontend**: ASP.NET MVC
- **Autenticação**: JWT (JSON Web Tokens)
- **Banco de Dados**: Entity Framework Core com SQL Server
- **UI**: Bootstrap para design responsivo
- **Integração de API**: OpenWeatherMap API para dados climáticos

## Melhorias Futuras

- **Melhorias na UI**: Melhorar a interface para uma interação mais amigável com o usuário.
- **Histórico Climático**: Implementar uma funcionalidade para rastrear e exibir o histórico climático das cidades favoritas.
- **Atualizações em Tempo Real**: Adicionar suporte a WebSockets para fornecer atualizações de clima em tempo real.

## Licença

Este projeto está licenciado sob a licença MIT.

