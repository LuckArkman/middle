# Intelligent Automation Platform

Uma plataforma SaaS poderosa para criação, orquestração e execução de agentes de automação inteligentes através de workflows visuais.

## 🚀 Sobre o Projeto

O **Intelligent Automation** permite que usuários criem fluxos de trabalho complexos de forma visual (drag-and-drop), conectando diferentes módulos para automatizar tarefas que vão desde simples requisições HTTP até operações avançadas de trading em exchanges como a Binance.

A plataforma foi desenhada com foco em **extensibilidade**, **performance** e **escalabilidade**, utilizando uma arquitetura moderna baseada em .NET e bancos de dados híbridos.

## 🛠️ Tecnologias Utilizadas

### Backend
- **Framework:** .NET 8/9
- **Persistência SQL:** Entity Framework Core
- **Persistência NoSQL:** MongoDB (para workflows e logs de execução)
- **Mensageria/Real-time:** SignalR
- **Bibliotecas Principais:**
  - Binance.Net: Integração com a API da Binance.
  - MercadoPago.Net: Processamento de pagamentos e assinaturas.

### Frontend
- **Framework:** Blazor (WebApp / BlazorApp)
- **Design de Workflows:** Blazor.Diagrams
- **Estilização:** CSS Moderno e Componentes customizados.

## 🏗️ Estrutura do Projeto

O projeto segue os princípios da **Clean Architecture**:

- \IntelligentAutomation.Domain\: Entidades de negócio, enums e a lógica de definição de workflows.
- \IntelligentAutomation.Application\: Serviços de aplicação, DTOs e interfaces.
- \IntelligentAutomation.Infrastructure\: Implementação de persistência (EF Core e MongoDB) e serviços externos.
- \IntelligentAutomation.AgentRuntime\: O motor (Workflow Engine) que executa os nós do agente.
- \IntelligentAutomation.Orchestrator\: API de controle e coordenação de execuções.
- \IntelligentAutomation.WebApp\: Interface web principal com o construtor visual de agentes.
- \IntelligentAutomation.ApiGateway\: Ponto de entrada unificado para os serviços.

## ✨ Funcionalidades Principais

- **Visual Agent Builder:** Interface intuitiva para desenhar o comportamento do agente conectando nós de gatilho e ação.
- **Motor de Execução de Workflows:** Engine resiliente que processa cada passo do fluxo de trabalho, mantendo o contexto entre os módulos.
- **Catálogo de Módulos:**
  - **HTTP Request:** Realize chamadas para qualquer API REST externa.
  - **Binance Trading:** Execute ordens de compra e venda (\Limit\, \Market\) automaticamente.
- **Gestão SaaS:** Sistema completo de planos, assinaturas e usuários integrado ao Mercado Pago.

## 🚀 Como Começar

### Pré-requisitos
- .NET 8.0 SDK ou superior.
- Docker (opcional, para rodar MongoDB e SQL).
- IDE (Visual Studio 2022 ou VS Code).

### Configuração
1.  Clone o repositório.
2.  Configure as variáveis de ambiente necessárias:
    -   \ConnectionStrings:DefaultConnection\: String de conexão SQL.
    -   \ConnectionStrings:MongoConnection\: String de conexão MongoDB.
    -   \BINANCE_API_KEY\ e \BINANCE_API_SECRET\ (para testes do módulo Binance).
    -   \MERCADOPAGO_ACCESS_TOKEN\ (para testes de pagamento).
3.  Execute as migrações do banco de dados:
    \\\ash
    dotnet ef database update --project IntelligentAutomation.Infrastructure
    \\\
4.  Inicie o projeto:
    \\\ash
    dotnet run --project IntelligentAutomation.WebApp
    \\\

## 🧩 Como adicionar novos módulos
