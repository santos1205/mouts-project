
# DeveloperStore Sales API - Documentação de Desenvolvimento

> **Nota**: Este arquivo rastreia o processo completo de desenvolvimento. Serve como um log detalhado de desenvolvimento e documentação técnica.

## ⚡ Configuração Rápida - PostgreSQL Local

**🚨 IMPORTANTE**: Para executar a API localmente, você precisa configurar um banco PostgreSQL. Os testes de integração funcionam (usam SQLite), mas a API de desenvolvimento requer PostgreSQL.

### 🐳 Configuração Rápida com Docker (Recomendado)
```bash
# 1. Iniciar PostgreSQL no Docker
docker run --name developerstore-postgres \
  -e POSTGRES_DB=DeveloperStore_Dev \
  -e POSTGRES_USER=devstore_user \
  -e POSTGRES_PASSWORD=devstore_pass \
  -p 5432:5432 \
  -d postgres:15

# 2. Aplicar migrações EF Core
dotnet ef database update --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api

# 3. Executar a API
dotnet run --project DeveloperStore.Api
```

### 📖 Guia Completo de Configuração
Para instruções detalhadas, problemas comuns e alternativas, consulte:
**→ [POSTGRESQL-DEV-SETUP.md](POSTGRESQL-DEV-SETUP.md)** (Guia completo de configuração PostgreSQL)

### ✅ Verificação de Funcionamento
Após configuração:
- API: `http://localhost:5079`
- Swagger: `http://localhost:5079/swagger`
- Endpoint de teste: `http://localhost:5079/api/Sales`

---

## Índice
- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Stack Tecnológica](#stack-tecnológica)
- [Configuração e Setup](#configuração-e-setup)
- [Princípios de Desenvolvimento](#princípios-de-desenvolvimento)
- [Log de Desenvolvimento](#log-de-desenvolvimento)
- [Próximos Passos](#próximos-passos)

## Visão Geral

A **DeveloperStore Sales API** é uma API CRUD completa para gerenciar registros de vendas de uma loja de desenvolvedores. O projeto segue os princípios de Domain-Driven Design (DDD) e implementa padrões de Clean Architecture para garantir manutenibilidade, testabilidade e escalabilidade.

### Requisitos de Negócio
- **Funcionalidade Principal**: Criar, Ler, Atualizar, Deletar registros de vendas
- **Pontos de Dados**: Número da Venda, Data, Cliente, Filial, Produtos, Quantidades, Preços, Descontos, Totais, Status de Cancelamento
- **Regras de Negócio**:
  - Níveis de desconto baseados na quantidade de itens:
    - < 4 itens: 0% de desconto
    - 4-9 itens: 10% de desconto
    - 10-20 itens: 20% de desconto
  - Máximo de 20 itens por produto
- **Eventos de Domínio**: VendaCriada, VendaModificada, VendaCancelada

## Arquitetura

### Princípios da Clean Architecture

O projeto implementa **Clean Architecture** com regras rigorosas de fluxo de dependências:

```
┌─────────────────────────────────────────┐
│                   API                   │ ← Camada de Apresentação
│            (Controllers)                │
└─────────────────┬───────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│              Application                │ ← Camada de Aplicação
│          (Use Cases/CQRS)               │
└─────────────────┬───────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│               Domain                    │ ← Camada de Domínio
│        (Entidades/Regras de Negócio)    │
└─────────────────┬───────────────────────┘
                  ↑
┌─────────────────────────────────────────┐
│            Infrastructure               │ ← Camada de Infraestrutura
│         (Acesso a Dados/Externo)        │
└─────────────────────────────────────────┘
```

### Regras de Fluxo de Dependências

**✅ Dependências Permitidas:**
- API → Application + Infrastructure
- Application → Domain
- Infrastructure → Application + Domain
- Tests → Todas as camadas

**❌ Dependências Proibidas:**
- Domain → Qualquer outra camada
- Application → Infrastructure
- Application → API

### Por que Clean Architecture?

1.  **Independência**: A lógica de negócio é isolada de preocupações externas.
2.  **Testabilidade**: Fácil de fazer testes unitários das regras de negócio principais.
3.  **Flexibilidade**: Pode trocar bancos de dados e frameworks sem tocar na lógica de negócio.
4.  **Manutenibilidade**: Mudanças em uma camada não cascateiam para outras.
5.  **Escalabilidade**: Limites bem definidos suportam o escalonamento de equipes.

## Estrutura do Projeto

```
DeveloperStore/
├── .gitignore                          # Regras do Git ignore
├── global.json                         # Fixação da versão do .NET SDK
├── DeveloperStore.sln                  # Arquivo de solução
├── DeveloperStore.Domain/              # 🔵 Camada de Domínio
│   ├── DeveloperStore.Domain.csproj
│   └── Class1.cs                       # (A ser substituído)
├── DeveloperStore.Application/         # 🟢 Camada de Aplicação
│   ├── DeveloperStore.Application.csproj
│   └── Class1.cs                       # (A ser substituído)
├── DeveloperStore.Infrastructure/      # 🟡 Camada de Infraestrutura
│   ├── DeveloperStore.Infrastructure.csproj
│   └── Class1.cs                       # (A ser substituído)
├── DeveloperStore.Api/                 # 🔴 Camada de Apresentação
│   ├── DeveloperStore.Api.csproj
│   ├── Program.cs                      # Ponto de entrada da aplicação
│   ├── appsettings.json               # Configuração
│   └── Properties/
│       └── launchSettings.json        # Configurações de desenvolvimento
└── DeveloperStore.Tests/               # 🧪 Camada de Testes
    ├── DeveloperStore.Tests.csproj
    └── UnitTest1.cs                   # (A ser substituído)
```

### Responsabilidades das Camadas

#### 🔵 Camada de Domínio (`DeveloperStore.Domain`)
- **Propósito**: Contém entidades de negócio, objetos de valor e regras de domínio.
- **Dependências**: Nenhuma (Lógica de negócio pura).
- **Conteúdo** *(Planejado)*:
  - Agregado raiz `Sale`
  - Entidade `SaleItem`
  - Objetos de valor (Money, CustomerInfo, etc.)
  - Eventos de domínio
  - Validações de regras de negócio

#### 🟢 Camada de Aplicação (`DeveloperStore.Application`)
- **Propósito**: Orquestra fluxos de trabalho de negócio e casos de uso.
- **Dependências**: Apenas Domínio.
- **Conteúdo** *(Planejado)*:
  - Comandos/consultas CQRS
  - Manipuladores (Handlers) MediatR
  - Serviços de aplicação
  - DTOs e perfis AutoMapper
  - Interfaces de repositório

#### 🟡 Camada de Infraestrutura (`DeveloperStore.Infrastructure`)
- **Propósito**: Implementa preocupações externas (banco de dados, mensageria, etc.).
- **Dependências**: Application + Domain.
- **Conteúdo** *(Planejado)*:
  - DbContext do Entity Framework
  - Implementações de repositório
  - Manipulação de mensagens Rebus
  - Integrações com serviços externos

#### 🔴 Camada de API (`DeveloperStore.Api`)
- **Propósito**: Endpoints HTTP e lógica de apresentação.
- **Dependências**: Application + Infrastructure.
- **Conteúdo**:
  - Controllers
  - Configuração Swagger
  - Configuração de injeção de dependência
  - Configuração de middleware

## Estratégia de Testes e Garantia de Qualidade

### Filosofia de Testes

O projeto segue uma **estratégia de testes abrangente** que garante qualidade de código, manutenibilidade e conformidade com os requisitos de negócio. Os testes são integrados durante todo o ciclo de vida de desenvolvimento, em vez de serem uma reflexão tardia.

### Pirâmide de Testes

```
        🔺 Testes End-to-End
       ────────────────────
      🔹🔹🔹 Testes de Integração
     ──────────────────────────
   🔸🔸🔸🔸🔸 Testes Unitários (Base)
  ────────────────────────────────
```

**Testes Unitários (70%)**: Testes rápidos e isolados da lógica de negócio.
**Testes de Integração (20%)**: Testes de interação entre componentes.
**Testes End-to-End (10%)**: Testes completos de fluxo de trabalho do usuário.

### Tipos de Teste e Ferramentas

#### 🧪 Testes Unitários
**Propósito**: Testar componentes individuais em completo isolamento.
- **Framework**: xUnit com suporte async/await.
- **Asserções**: FluentAssertions para testes expressivos e legíveis.
- **Mocking**: Moq para isolamento de dependências.
- **Geração de Dados**: Bogus para dados de teste realistas.
- **Velocidade**: Milissegundos por teste.
- **Cobertura**: Lógica de domínio, Handlers da aplicação, Objetos de valor.

```csharp
[Fact]
public void Venda_Deve_CalcularDesconto_ComBase_NosNiveisDeQuantidade()
{
    // Arrange
    var sale = Sale.Create(customerId: Guid.NewGuid(), branchId: Guid.NewGuid(), saleNumber: "S001");
    
    // Act - Adiciona 5 itens (deve acionar o nível de 10% de desconto)
    sale.AddItem(productId: Guid.NewGuid(), quantity: 5, unitPrice: Money.Of(10.00m, "USD"));
    
    // Assert
    sale.SaleLevelDiscount.Amount.Should().Be(5.00m); // 10% de 50.00
    sale.TotalAmount.Amount.Should().Be(45.00m);
}
```

#### 🔗 Testes de Integração
**Propósito**: Testar interações de componentes com dependências reais.
- **Framework**: Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory).
- **Banco de Dados**: Testcontainers com instâncias reais do PostgreSQL.
- **Testes HTTP**: Servidor de teste em memória com pipeline completo.
- **Velocidade**: Segundos por teste.
- **Cobertura**: Endpoints da API, Operações de banco de dados, Fluxos completos.

```csharp
public class SalesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task POST_Sales_Deve_CriarVenda_ERetornar_201()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createSaleRequest = new CreateSaleRequest { /* dados de teste */ };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/sales", createSaleRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sale = await response.Content.ReadFromJsonAsync<SaleDto>();
        sale.SaleNumber.Should().NotBeNullOrEmpty();
    }
}
```

#### 🏗️ Testes de Arquitetura
**Propósito**: Aplicar princípios da Arquitetura Limpa e regras de dependência.
- **Framework**: NetArchTest para análise de dependências.
- **Cobertura**: Limites de camadas, Convenções de nomenclatura, Fluxo de dependências.
- **Frequência**: A cada build (integração CI/CD).

```csharp
[Fact]
public void Dominio_NaoDeve_Depender_DaInfraestrutura()
{
    var result = Types.InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn("DeveloperStore.Infrastructure")
        .GetResult();
    
    result.IsSuccessful.Should().BeTrue();
}
```

### Cronograma de Testes e Integração

#### **Fase 1: Testes de Base (Passo 4 - Implementação CQRS)**
**Quando**: Durante a implementação de comandos e queries do MediatR.
**Abordagem**: Desenvolvimento Orientado a Testes (TDD) para handlers críticos de negócio.

```
🚀 Objetivos de Teste do Passo 4:
✅ Testes unitários para todos os handlers de Command/Query
✅ Cobertura abrangente da lógica de domínio
✅ Verificação do comportamento dos objetos de valor
✅ Validação da aplicação das regras de negócio
```

**Benefícios dos Testes Iniciais**:
- **Feedback de Design**: Escrever testes revela problemas no design da API.
- **Prevenção de Regressão**: Captura bugs imediatamente, quando o contexto está fresco.
- **Documentação**: Os testes servem como uma especificação viva.
- **Confiança**: Refatoração segura com a rede de segurança dos testes.

#### **Fase 2: Testes de Integração (Passo 5 - API de Produção)**
**Quando**: Após a conclusão da implementação do CQRS.
**Abordagem**: Testar fluxos de trabalho HTTP reais com integração de banco de dados.

```
🔗 Objetivos de Teste do Passo 5:
✅ Teste completo do ciclo de requisição/resposta HTTP
✅ Integração de banco de dados com PostgreSQL real
✅ Verificação do pipeline de validação
✅ Tratamento de erros e casos extremos
```

#### **Fase 3: Testes Abrangentes (Passo 6 - Prontidão para Produção)**
**Quando**: Antes do deployment e configuração de CI/CD.
**Abordagem**: Suíte de testes completa com validação de performance e arquitetura.

```
🛡️ Objetivos de Teste do Passo 6:
✅ Testes de performance e carga
✅ Verificação da conformidade da arquitetura
✅ Testes de segurança (autenticação/autorização)
✅ Testes de contrato para estabilidade da API
```

### Melhores Práticas de Teste

#### **1. Convenção de Nomenclatura de Testes**
```csharp
// Padrão: [UnidadeDeTrabalho]_Deve_[ComportamentoEsperado]_Quando_[EstadoSobTeste]
public void Venda_Deve_LancarExcecao_Quando_QuantidadeExcedeMaximo()
public void CreateSaleHandler_Deve_RetornarSaleDto_Quando_DadosValidosFornecidos()
public void GET_Sales_Deve_Retornar200_Quando_VendasExistem()
```

#### **2. Padrão AAA (Arrange-Act-Assert)**
```csharp
[Fact]
public async Task CriarVenda_Deve_PublicarEventoVendaCriada()
{
    // Arrange - Configurar dados de teste e dependências
    var mockRepository = new Mock<ISaleRepository>();
    var handler = new CreateSaleCommandHandler(mockRepository.Object);
    var command = new CreateSaleCommand { /* dados válidos */ };
    
    // Act - Executar a operação sob teste
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert - Verificar os resultados esperados
    result.Should().NotBeNull();
    result.SaleNumber.Should().NotBeNullOrEmpty();
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Sale>()), Times.Once);
}
```

#### **3. Gerenciamento de Dados de Teste**
```csharp
// Use Bogus para dados de teste consistentes e realistas
public class SaleTestDataBuilder
{
    private static readonly Faker<CreateSaleCommand> SaleCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.Items, f => f.Make(3, () => new SaleItemDto
        {
            ProductId = f.Random.Guid(),
            Quantity = f.Random.Int(1, 10),
            UnitPrice = f.Random.Decimal(1, 100)
        }));
    
    public static CreateSaleCommand ComandoDeVendaValido() => SaleCommandFaker.Generate();
}
```

#### **4. Isolamento de Dependências**
```csharp
// Mock de dependências externas para testar em isolamento
[Fact]
public async Task CreateSaleHandler_Deve_ChamarRepositorio_UmaVez()
{
    // Arrange
    var mockRepository = new Mock<ISaleRepository>();
    var mockValidator = new Mock<IValidator<CreateSaleCommand>>();
    mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateSaleCommand>(), default))
             .ReturnsAsync(new ValidationResult());
    
    var handler = new CreateSaleCommandHandler(mockRepository.Object, mockValidator.Object);
    
    // Act & Assert
    await handler.Handle(ComandoDeVendaValido(), CancellationToken.None);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Sale>()), Times.Once);
}
```

### Análise de ROI de Testes

| Investimento em Testes | Prevenção de Bugs | Velocidade de Desenvolvimento | Valor a Longo Prazo |
|------------------------|-------------------|-------------------------------|---------------------|
| **Sem Testes**         | 0%                | Rápido inicialmente           | Negativo (dívida técnica) |
| **Apenas Unitários**   | 60%               | +20% tempo                    | Alto ROI            |
| **Unitários + Integração**| 85%            | +30% tempo                    | Muito Alto ROI      |
| **Suíte Abrangente**   | 95%               | +40% tempo                    | ROI Máximo          |

### Testes de Integração Contínua

#### **Testes no Pipeline de Build**
```yaml
# Exemplo de GitHub Actions
- name: Executar Testes Unitários
  run: dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"
  
- name: Executar Testes de Integração
  run: dotnet test DeveloperStore.Tests.Integration --configuration Release
  
- name: Testes de Arquitetura
  run: dotnet test DeveloperStore.Tests.Architecture --configuration Release
```

#### **Portões de Qualidade (Quality Gates)**
- **Cobertura Mínima de Testes**: 80% para a lógica de negócio.
- **Todos os Testes Devem Passar**: Tolerância zero para testes com falha.
- **Conformidade da Arquitetura**: Todas as regras de dependência aplicadas.
- **Limites de Performance**: Tempos de resposta da API abaixo de 200ms.

### Estrutura do Projeto de Testes

```
DeveloperStore.Tests/
├── Unit/                           # Testes rápidos e isolados
│   ├── Domain/
│   │   ├── SaleTests.cs           # Testes da raiz de agregado
│   │   ├── SaleItemTests.cs       # Testes de entidade
│   │   └── ValueObjects/          # Testes de objetos de valor
│   └── Application/
│       ├── Commands/              # Testes de command handlers
│       └── Queries/               # Testes de query handlers
├── Integration/                    # Testes com dependências reais
│   ├── Controllers/               # Testes de endpoints da API
│   ├── Repository/                # Integração com banco de dados
│   └── Infrastructure/            # Testes de serviços externos
├── Architecture/                   # Testes de conformidade
│   └── ArchitectureTests.cs       # Verificação das regras de dependência
├── TestUtilities/                  # Infraestrutura de testes compartilhada
│   ├── Builders/                  # Builders de dados de teste
│   ├── Fixtures/                  # Fixtures de teste compartilhadas
│   └── Extensions/                # Extensões de ajuda para testes
└── TestData/                      # Arquivos de dados de teste estáticos
    ├── ValidSales.json
    └── InvalidScenarios.json
```

### Estratégia de Testes de Performance

#### **Teste de Carga com NBomber**
```csharp
var scenario = Scenario.Create("cenario_criar_venda", async context =>
{
    var saleRequest = SaleTestDataBuilder.ComandoDeVendaValido();
    var response = await httpClient.PostAsJsonAsync("/api/sales", saleRequest);
    
    return Response.Ok(response.StatusCode.ToString());
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(5))
);
```

### Documentação de Testes

Todos os testes servem como **documentação viva** do comportamento do sistema:
- **Regras de Negócio**: Testes de domínio documentam os requisitos de negócio.
- **Contratos da API**: Testes de integração documentam o comportamento da API.
- **Arquitetura**: Testes de arquitetura documentam as decisões de design.
- **Performance**: Testes de carga documentam as expectativas de performance.

## Stack Tecnológica

### Framework Principal
- **.NET 8**: Versão LTS mais recente para performance e recursos modernos.
- **ASP.NET Core Web API**: Framework para APIs RESTful.
- **C# 11**: Recursos mais recentes da linguagem com tipos de referência nullable.

### Padrões de Arquitetura
- **CQRS (Command Query Responsibility Segregation)**: Separa operações de leitura/escrita.
- **MediatR**: Mensageria em processo para desacoplamento.
- **Domain-Driven Design**: Modelos de domínio ricos com lógica de negócio.
- **Padrão Repository**: Abstração de acesso a dados.

### Dados e Persistência
- **Entity Framework Core**: ORM com provedor In-Memory para desenvolvimento.
- **AutoMapper**: Mapeamento objeto-para-objeto.

### Mensageria e Eventos
- **Rebus**: Service bus leve para eventos de domínio.
- **Eventos de Domínio**: Padrão publish/subscribe para eventos de negócio.

### Testes e Garantia de Qualidade
- **xUnit**: Framework de testes moderno com excelente suporte async.
- **FluentAssertions**: Biblioteca de asserções expressiva para testes legíveis.
- **Moq**: Framework de mock para isolamento de dependências.
- **Bogus**: Geração de dados falsos para cenários de teste realistas.
- **Microsoft.AspNetCore.Mvc.Testing**: Testes de integração com WebApplicationFactory.
- **Testcontainers**: Testes de integração baseados em Docker com bancos de dados reais.
- **NetArchTest**: Testes de conformidade arquitetural.

### Ferramentas de Desenvolvimento
- **Swagger/OpenAPI**: Documentação e teste de API.
- **global.json**: Gerenciamento de versão do SDK.

## Configuração e Setup

### Pré-requisitos
- .NET 8 SDK (versão 8.0.413 ou posterior)
- Visual Studio Code ou Visual Studio 2022
- Git para controle de versão

### Passos de Configuração do Projeto

1.  **Criação da Solução**:
    ```bash
    dotnet new sln -n DeveloperStore
    ```

2.  **Criação dos Projetos**:
    ```bash
    # Camadas principais
    dotnet new classlib -n DeveloperStore.Domain
    dotnet new classlib -n DeveloperStore.Application
    dotnet new classlib -n DeveloperStore.Infrastructure
    dotnet new webapi -n DeveloperStore.Api
    dotnet new xunit -n DeveloperStore.Tests
    ```

3.  **Montagem da Solução**:
    ```bash
    dotnet sln add DeveloperStore.Domain DeveloperStore.Application \
                   DeveloperStore.Infrastructure DeveloperStore.Api \
                   DeveloperStore.Tests
    ```

4.  **Configuração de Dependências**:
    ```bash
    # Aplicar dependências da Clean Architecture
    dotnet add DeveloperStore.Application reference DeveloperStore.Domain
    dotnet add DeveloperStore.Infrastructure reference DeveloperStore.Application
    dotnet add DeveloperStore.Api reference DeveloperStore.Application DeveloperStore.Infrastructure
    dotnet add DeveloperStore.Tests reference DeveloperStore.Domain DeveloperStore.Application DeveloperStore.Infrastructure
    ```

### Arquivos de Configuração

#### `global.json` - Fixação da Versão do SDK
```json
{
  "sdk": {
    "version": "8.0.413"
  }
}
```
**Propósito**: Garante uma versão consistente do .NET em todos os ambientes de desenvolvimento.

#### `Program.cs` - Bootstrap da API
```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

**Decisões Chave de Configuração**:
- **Integração com Swagger**: Documentação automática da API.
- **Baseado em Controllers**: Padrão MVC tradicional em vez de Minimal APIs para maior estrutura.
- **Swagger Apenas em Desenvolvimento**: Consideração de segurança para produção.

#### `.gitignore` - Controle de Versão
Arquivo ignore abrangente cobrindo:
- Artefatos de build (`bin/`, `obj/`)
- Arquivos de IDE (`.vs/`, `.vscode/`)
- Arquivos específicos de usuário (`*.user`, `*.suo`)
- Gerenciamento de pacotes (`packages/`, `*.nupkg`)
- Arquivos sensíveis (`*.pubxml`, certificados)

## Princípios de Desenvolvimento

### 1. Princípio da Responsabilidade Única (SRP)
Cada classe e projeto tem uma única razão para mudar:
- Domínio: Mudanças nas regras de negócio.
- Aplicação: Mudanças nos casos de uso.
- Infraestrutura: Mudanças de tecnologia.
- API: Mudanças na apresentação.

### 2. Princípio da Inversão de Dependência (DIP)
As dependências fluem em direção a abstrações:
- Módulos de alto nível não dependem de módulos de baixo nível.
- Ambos dependem de abstrações (interfaces).
- A infraestrutura implementa interfaces da aplicação.

### 3. Princípio Aberto/Fechado (OCP)
Aberto para extensão, fechado para modificação:
- Novas funcionalidades são adicionadas através de novas classes.
- O código existente permanece inalterado.
- Alcançado através de interfaces e injeção de dependência.

### 4. Princípio da Segregação de Interfaces (ISP)
Muitas interfaces específicas são melhores do que uma interface de propósito geral:
- Interfaces de repositório por agregado.
- Interfaces de serviço focadas.
- Sem interfaces "gordas".

### 5. Não se Repita (DRY)
Regras de negócio centralizadas na camada de domínio:
- Única fonte da verdade para a lógica de negócio.
- Serviços de domínio reutilizáveis.
- Regras de validação consistentes.

## Próximos Passos

### Passo 5: Implementação de Queries e Integração com a API ✅ CONCLUÍDO
- Implementar queries CQRS (GetSale, GetSales) com handlers.
- Criar DTOs de query e configurações do AutoMapper.
- Adicionar validação e filtragem abrangentes para queries.
- Atualizar controllers da API para usar comandos/queries do MediatR.
- Substituir controllers de teste por endpoints CQRS de produção.

### Passo 6: Testes de Integração e Prontidão para Produção ⏳ PRÓXIMO
- Adicionar testes de integração com WebApplicationFactory e um banco de dados real.
- Implementar middleware global de tratamento de erros.
- Adicionar logging e monitoramento de requisição/resposta.
- Configurar injeção de dependência com todos os serviços CQRS.
- Adicionar framework de autenticação/autorização.

### Passo 7: Recursos Avançados e Implantação
- Implementar logging abrangente com logs estruturados.
- Adicionar monitoramento de performance e health checks.
- Configurar pipeline de CI/CD com testes automatizados.
- Implantar no Azure com infraestrutura como código.
- Adicionar versionamento de API e compatibilidade retroativa.

---

## Padrões de Qualidade de Código

### Convenções de Nomenclatura
- **Classes**: PascalCase (`Sale`, `SaleItem`)
- **Métodos**: PascalCase (`CalculateDiscount`)
- **Propriedades**: PascalCase (`TotalAmount`)
- **Campos**: camelCase com underscore (`_repository`)
- **Parâmetros**: camelCase (`saleId`)

### Estratégia de Tratamento de Erros
- **Domínio**: Lançar exceções específicas do domínio.
- **Aplicação**: Retornar objetos `Result`.
- **API**: Middleware global de tratamento de exceções.
- **Infraestrutura**: Registrar o log e relançar com contexto.

### Estratégia de Testes
- **Testes Unitários**: Lógica de domínio e serviços de aplicação com abordagem TDD.
- **Testes de Integração**: Endpoints da API e operações de banco de dados com dependências reais.
- **Testes de Arquitetura**: Verificar regras de dependência e conformidade com a Clean Architecture.
- **Testes de Contrato**: Compatibilidade da API e detecção de quebras de contrato.
- **Testes de Performance**: Testes de carga e validação do tempo de resposta.
- **Testes de Segurança**: Autenticação, autorização e varredura de vulnerabilidades.

---

## Log de Desenvolvimento

Esta seção rastreia todas as mudanças e progressos feitos durante o desenvolvimento.

### Sessão 1 - Configuração do Projeto e Arquitetura (21 de Agosto, 2025)

#### ✅ Tarefas Completadas

**1. Base da Arquitetura Limpa**
- Criado arquivo de solução: `DeveloperStore.sln`
- Configurados 5 projetos seguindo princípios da Arquitetura Limpa:
  - `DeveloperStore.Domain` (Lógica de negócio)
  - `DeveloperStore.Application` (Casos de uso)
  - `DeveloperStore.Infrastructure` (Acesso a dados)
  - `DeveloperStore.Api` (Apresentação)
  - `DeveloperStore.Tests` (Testes)

**2. Configuração de Dependências**
- Configuradas referências de projeto para aplicar regras de fluxo de dependências.
- Application → Domain
- Infrastructure → Application
- API → Application + Infrastructure
- Tests → Todas as camadas

**3. Configuração .NET 8**
- Criado `global.json` para fixar versão do SDK em 8.0.413.
- Atualizados todos os projetos para o framework .NET 8.
- Configurado Swagger para melhor compatibilidade com .NET 8.

**4. Configuração do Ambiente de Desenvolvimento**
- `.gitignore` abrangente para projetos .NET.
- Estrutura básica da API com documentação Swagger.
- Verificada funcionalidade de build e runtime.
- Adicionada documentação de build (este arquivo) ao `.gitignore`.

#### 🔧 Decisões Técnicas Tomadas

**Padrão de Arquitetura**: Clean Architecture
- **Justificativa**: Garante testabilidade, manutenibilidade e independência da lógica de negócio.
- **Benefício**: Pode alterar bancos de dados, frameworks sem afetar regras de negócio principais.

**Estrutura do Projeto**: Em camadas com regras de dependência estritas.
- **Justificativa**: Previne violações arquiteturais e acoplamento.
- **Benefício**: Força a separação adequada de responsabilidades.

**Estratégia de Documentação**: Documentação viva excluída do VCS.
- **Justificativa**: Log de desenvolvimento detalhado sem poluir o repositório.
- **Benefício**: Histórico completo de desenvolvimento para aprendizado e depuração.

#### 📁 Arquivos Criados/Modificados
- `DeveloperStore.sln` - Arquivo de solução
- `global.json` - Fixação da versão do SDK
- `DeveloperStore.Domain/DeveloperStore.Domain.csproj` - Projeto de domínio
- `DeveloperStore.Application/DeveloperStore.Application.csproj` - Projeto de aplicação
- `DeveloperStore.Infrastructure/DeveloperStore.Infrastructure.csproj` - Projeto de infraestrutura
- `DeveloperStore.Api/DeveloperStore.Api.csproj` - Projeto da API com Swagger
- `DeveloperStore.Api/Program.cs` - Configuração de bootstrap da API
- `DeveloperStore.Tests/DeveloperStore.Tests.csproj` - Projeto de testes
- `.gitignore` - Regras de ignore abrangentes + exclusão da documentação
- `build-documentation.md` - Este arquivo de documentação

#### 🚦 Status Atual
- ✅ **Arquitetura**: Base da Arquitetura Limpa estabelecida.
- ✅ **Sistema de Build**: Todos os projetos compilam com sucesso.
- ✅ **API**: API básica executa com documentação Swagger.
- ✅ **Documentação**: Documentação técnica abrangente.
- ⏳ **Modelo de Domínio**: Não iniciado (Próximo passo).
- ⏳ **Persistência**: Não iniciada.
- ⏳ **Lógica de Negócio**: Não iniciada.

#### 🔮 Objetivos da Próxima Sessão
- **Passo 2**: ✅ **CONCLUÍDO** - Definir Modelo de Domínio (Agregado Sale, entidade SaleItem).
- **Passo 3**: Configurar Entity Framework e camada de persistência.
- **Passo 4**: Implementar primeiro caso de uso (Criar Venda).

### Sessão 2 - Desenvolvimento do Modelo de Domínio (21 de Agosto, 2025)

#### ✅ Tarefas Completadas

**1. Base da Camada de Domínio**
- Criada estrutura de pastas: `Entities/`, `ValueObjects/`, `Events/`.
- Implementadas classes base:
  - `Entity` - Classe base para todas as entidades de domínio com identidade e tratamento de eventos.
  - `DomainEvent` - Classe base para todos os eventos de domínio.

**2. Implementação de Objetos de Valor**
- `Money` - Objeto de valor imutável para valores monetários com operações de negócio.
- `CustomerInfo` - Identidade externa para o agregado Cliente (desnormalizado).
- `BranchInfo` - Identidade externa para o agregado Filial (desnormalizado).
- `ProductInfo` - Identidade externa para o agregado Produto (desnormalizado).

**3. Eventos de Domínio**
- `SaleCreated` - Disparado quando uma nova venda é criada.
- `SaleModified` - Disparado quando uma venda é modificada (itens adicionados/removidos/atualizados).
- `SaleCancelled` - Disparado quando uma venda é cancelada.

**4. Entidades de Domínio**
- `SaleItem` - Entidade representando um item de linha em uma venda.
  - Validação de quantidade (máx. 1-20 itens por produto).
  - Tratamento de preço e desconto.
  - Aplicação de regras de negócio.
- `Sale` - Raiz de Agregado controlando todas as operações de venda.
  - Cálculo de desconto no nível da venda baseado em faixas de quantidade.
  - Gerenciamento de itens (adicionar/atualizar/remover).
  - Tratamento de cancelamento.
  - Publicação de eventos de domínio.

**5. Implementação de Regras de Negócio**
- **Limites de Quantidade**: Máximo de 20 itens por produto.
- **Faixas de Desconto**:
  - < 4 itens: 0% de desconto
  - 4-9 itens: 10% de desconto
  - 10-20 itens: 20% de desconto
- **Imutabilidade**: Objetos de valor são imutáveis.
- **Consistência**: Todas as mudanças passam pela raiz do agregado.
- **Eventos**: Eventos de domínio publicados para todas as operações de negócio importantes.

#### 🔧 Decisões Técnicas Tomadas

**Padrões de Domain-Driven Design**:
- **Raiz de Agregado**: `Sale` controla todo o acesso e mantém a consistência.
- **Identidades Externas**: Dados desnormalizados de outros contextos delimitados.
- **Objetos de Valor**: Objetos imutáveis definidos por seus valores.
- **Eventos de Domínio**: Desacoplam efeitos colaterais da lógica de negócio principal.

**Centralização da Lógica de Negócio**:
- Todas as regras de negócio aplicadas na camada de domínio.
- Modelos de domínio ricos com comportamento, não meros contêineres de dados anêmicos.
- Cláusulas de guarda (Guard Clauses) previnem estados inválidos.
- Propriedades calculadas para valores derivados.

**Arquitetura Orientada a Eventos**:
- Eventos de domínio publicados para auditabilidade e integração.
- Acoplamento fraco entre agregados através de eventos.
- Preparação para Event Sourcing (eventos contêm todos os dados necessários).

#### 📁 Arquivos Criados/Modificados
- `DeveloperStore.Domain/Events/DomainEvent.cs` - Classe base de evento de domínio
- `DeveloperStore.Domain/Events/SaleCreated.cs` - Evento de venda criada
- `DeveloperStore.Domain/Events/SaleModified.cs` - Evento de venda modificada
- `DeveloperStore.Domain/Events/SaleCancelled.cs` - Evento de venda cancelada
- `DeveloperStore.Domain/Entities/Entity.cs` - Classe base de entidade
- `DeveloperStore.Domain/Entities/Sale.cs` - Raiz de agregado Sale
- `DeveloperStore.Domain/Entities/SaleItem.cs` - Entidade de item de venda
- `DeveloperStore.Domain/ValueObjects/Money.cs` - Objeto de valor Money
- `DeveloperStore.Domain/ValueObjects/CustomerInfo.cs` - Identidade externa do cliente
- `DeveloperStore.Domain/ValueObjects/BranchInfo.cs` - Identidade externa da filial
- `DeveloperStore.Domain/ValueObjects/ProductInfo.cs` - Identidade externa do produto
- Removido `DeveloperStore.Domain/Class1.cs` - Limpeza do arquivo padrão

#### 🚦 Status Atual
- ✅ **Arquitetura**: Base da Clean Architecture estabelecida.
- ✅ **Sistema de Build**: Todos os projetos compilam com sucesso.
- ✅ **API**: API básica em execução com documentação Swagger.
- ✅ **Documentação**: Documentação técnica abrangente.
- ✅ **Modelo de Domínio**: Modelo de domínio completo com regras de negócio.
- ⏳ **Persistência**: Não iniciada (Próximo passo).
- ⏳ **Camada de Aplicação**: Não iniciada.
- ⏳ **Lógica de Negócio**: Lógica de domínio completa, orquestração da aplicação necessária.

#### 🔮 Objetivos da Próxima Sessão
- **Passo 3**: Configurar Entity Framework DbContext e o padrão Repository.
- **Passo 4**: Implementar o primeiro comando CQRS (Criar Venda) com MediatR.
- **Passo 5**: Criar o controller da API de Vendas.

### Sessão 2.1 - Configuração de Depuração do VS Code (21 de Agosto, 2025)

#### ✅ Tarefas Completadas

**1. Configuração do VS Code**
- Criado `.vscode/launch.json` - Configuração de depuração para o projeto da API.
- Criado `.vscode/tasks.json` - Tarefas de build, teste e watch.
- Criado `.vscode/settings.json` - Configurações do VS Code específicas do projeto.

**2. Controller de Depuração**
- Criado `DebugController.cs` - Controller temporário para testar o modelo de domínio.
- Adicionado endpoint `CreateSampleSale` - Demonstra as regras de negócio em ação.
- Adicionado endpoint `TestBusinessRuleViolation` - Mostra o tratamento de exceções.

#### 📁 Arquivos Criados/Modificados
- `.vscode/launch.json` - Configuração de depuração do VS Code
- `.vscode/tasks.json` - Tarefas de build do VS Code
- `.vscode/settings.json` - Configurações do projeto VS Code
- `DeveloperStore.Api/Controllers/DebugController.cs` - Endpoints de depuração

### Sessão 3 - Implementação da Camada de Persistência (22 de Agosto, 2025)

#### ✅ Tarefas Completadas

**1. Configuração do Entity Framework Core com PostgreSQL**
- Adicionados pacotes NuGet:
  - `Microsoft.EntityFrameworkCore` - Funcionalidade principal do EF.
  - `Npgsql.EntityFrameworkCore.PostgreSQL` - Provedor para PostgreSQL.
  - `Microsoft.EntityFrameworkCore.Design` - Ferramentas de migração.
- Configuradas connection strings do PostgreSQL em `appsettings.json` e `appsettings.Development.json`.
- Configurados bancos de dados separados para ambientes de produção e desenvolvimento.

**2. DbContext e Configurações de Entidade**
- Criado `DeveloperStoreDbContext` com configurações adequadas de DbSet.
- Implementadas configurações de entidade abrangentes:
  - `SaleConfiguration` - Mapeia o agregado Sale com todos os objetos de valor (Cliente, Filial, objetos Money).
  - `SaleItemConfiguration` - Mapeia entidades SaleItem com informações de Produto e rastreamento de desconto.
- Configuradas entidades próprias (owned entities) para objetos de valor usando o padrão `OwnsOne` do EF Core.
- Adicionados índices adequados (restrição única em SaleNumber, índices de performance).
- Configuradas relações de chave estrangeira com exclusão em cascata.

**3. Implementação do Padrão Repository**
- Criada a interface `ISaleRepository` na camada de Domínio (princípio da inversão de dependência).
- Implementado `SaleRepository` na camada de Infraestrutura com padrão async/await completo.
- Adicionadas operações CRUD abrangentes:
  - Operações de consulta: GetByIdAsync, GetBySaleNumberAsync, GetAllAsync.
  - Operações de filtro: GetByCustomerIdAsync, GetByBranchIdAsync, GetByDateRangeAsync.
  - Operações de comando: AddAsync, UpdateAsync, DeleteAsync.
  - Operações utilitárias: ExistsAsync, SaleNumberExistsAsync.
  - Unidade de Trabalho: SaveChangesAsync.

**4. Compatibilidade dos Objetos de Valor com EF Core**
- Adicionados construtores sem parâmetros para o EF Core em todos os objetos de valor:
  - `Money` - Lida com moeda e valores decimais.
  - `CustomerInfo` - Identidade externa com validação.
  - `BranchInfo` - Identidade externa com validação.
  - `ProductInfo` - Identidade externa com objeto de valor Money aninhado.
- Mantida a imutabilidade e a lógica de negócio enquanto se dá suporte aos requisitos do EF Core.

**5. Configuração da Injeção de Dependência**
- Criados métodos de extensão `DependencyInjection` na camada de Infraestrutura.
- Configurado o PostgreSQL com políticas de nova tentativa de conexão e pooling de conexões.
- Adicionadas configurações específicas de desenvolvimento com logging e depuração aprimorados.
- Registradas as implementações de repositório com escopo adequado.
- Separadas as configurações de produção e desenvolvimento.

**6. Sistema de Migração de Banco de Dados**
- Criada com sucesso a migração inicial com as ferramentas CLI do EF Core.
- Gerado esquema de banco de dados abrangente:
  - Tabela `Sales` com todas as propriedades de Sale e objetos de valor desnormalizados.
  - Tabela `SaleItems` com informações de Produto e detalhes de preços.
  - Restrições de chave estrangeira e índices adequados.
  - Suporte para objetos de valor complexos (Money, CustomerInfo, BranchInfo, ProductInfo).
- A migração inclui tipos de dados e restrições apropriados do PostgreSQL.

**7. Integração e Testes da API**
- Atualizado `Program.cs` para registrar os serviços de Infraestrutura com base no ambiente.
- Criado um `SalesController` abrangente para testar a camada de persistência:
  - Endpoints CRUD completos para gerenciamento de Vendas.
  - Endpoint de criação de dados de teste (`POST /api/sales/test-create`).
  - Endpoints de filtragem por cliente e filial.
  - Tratamento de erros e logging adequados em todo o controller.
- Aprimorado o `DebugController` com o status de conclusão do Passo 3 e orientação sobre os próximos passos.

#### 🔧 Decisões Técnicas Tomadas

**PostgreSQL em vez de Banco de Dados em Memória**:
- **Justificativa**: Solicitado pelo usuário para uma implementação pronta para produção.
- **Benefícios**: Restrições reais do banco de dados, testes de performance, semelhança com a produção.
- **Configuração**: Bancos de dados de dev/prod separados, pooling de conexões, políticas de nova tentativa.

**Padrão Repository na Camada de Domínio**:
- **Justificativa**: Segue o princípio de inversão de dependência do DDD.
- **Benefícios**: O Domínio não depende da Infraestrutura, testes mais fáceis.
- **Implementação**: Interface no Domínio, implementação na Infraestrutura.

**Estratégia de Mapeamento de Objetos de Valor**:
- **Justificativa**: O `OwnsOne` do EF Core para objetos de valor complexos mantém o encapsulamento.
- **Benefícios**: Normalização do banco de dados preservando a integridade do modelo de domínio.
- **Desafio**: Exigiu construtores para o EF Core, mantendo a imutabilidade.

**Configuração Abrangente de Entidades**:
- **Justificativa**: A configuração explícita previne surpresas das convenções do EF Core.
- **Benefícios**: Controle total sobre o esquema do banco de dados, índices e restrições adequados.
- **Manutenção**: As configurações são co-localizadas com as mudanças de domínio.

**Configuração de Desenvolvimento vs Produção**:
- **Justificativa**: Necessidades diferentes para depuração vs performance.
- **Implementação**: Métodos de extensão separados com configurações específicas do ambiente.
- **Benefícios**: Experiência de desenvolvimento aprimorada sem sobrecarga em produção.

#### 📁 Arquivos Criados/Modificados

**Camada de Infraestrutura:**
- `DeveloperStore.Infrastructure/DeveloperStoreDbContext.cs` - DbContext principal (renomeado de Class1.cs)
- `DeveloperStore.Infrastructure/Persistence/Configurations/SaleConfiguration.cs` - Mapeamento da entidade Sale
- `DeveloperStore.Infrastructure/Persistence/Configurations/SaleItemConfiguration.cs` - Mapeamento da entidade SaleItem
- `DeveloperStore.Infrastructure/Persistence/Repositories/SaleRepository.cs` - Implementação do repositório
- `DeveloperStore.Infrastructure/DependencyInjection.cs` - Registro e configuração de serviços
- `DeveloperStore.Infrastructure/Migrations/` - Arquivos da migração inicial do banco de dados

**Camada de Domínio:**
- `DeveloperStore.Domain/Repositories/ISaleRepository.cs` - Interface do repositório
- Atualizados todos os objetos de valor com construtores para o EF Core, mantendo a imutabilidade

**Camada da API:**
- `DeveloperStore.Api/Controllers/SalesController.cs` - Controller CRUD abrangente
- `DeveloperStore.Api/Controllers/DebugController.cs` - Aprimorado com o status do Passo 3
- `DeveloperStore.Api/Program.cs` - Registro de serviços da infraestrutura
- `DeveloperStore.Api/appsettings.json` - Configuração de conexão do PostgreSQL
- `DeveloperStore.Api/appsettings.Development.json` - Configuração do banco de dados de desenvolvimento

#### 🗄️ Esquema de Banco de Dados Gerado

**Tabela Sales:**
```sql
CREATE TABLE "Sales" (
    "Id" uuid NOT NULL,
    "SaleNumber" character varying(50) NOT NULL,
    "SaleDate" timestamp with time zone NOT NULL,
    "CustomerId" uuid NOT NULL,
    "CustomerName" character varying(200) NOT NULL,
    "CustomerEmail" character varying(250) NOT NULL,
    "BranchId" uuid NOT NULL,
    "BranchName" character varying(200) NOT NULL,
    "BranchLocation" character varying(200) NOT NULL,
    "SaleLevelDiscountAmount" numeric(18,2) NOT NULL,
    "SaleLevelDiscountCurrency" character varying(3) NOT NULL DEFAULT 'BRL',
    "IsCancelled" boolean NOT NULL DEFAULT false,
    "CancellationReason" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "ModifiedAt" timestamp with time zone,
    CONSTRAINT "PK_Sales" PRIMARY KEY ("Id"),
    CONSTRAINT "IX_Sales_SaleNumber" UNIQUE ("SaleNumber")
);
```

**Tabela SaleItems:**
```sql
CREATE TABLE "SaleItems" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "ProductName" character varying(200) NOT NULL,
    "ProductCategory" character varying(100) NOT NULL,
    "ProductUnitPrice" numeric(18,2) NOT NULL,
    "ProductUnitPriceCurrency" character varying(3) NOT NULL DEFAULT 'BRL',
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "UnitPriceCurrency" character varying(3) NOT NULL DEFAULT 'BRL',
    "DiscountAmount" numeric(18,2) NOT NULL,
    "DiscountCurrency" character varying(3) NOT NULL DEFAULT 'BRL',
    "SaleId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ModifiedAt" timestamp with time zone,
    CONSTRAINT "PK_SaleItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SaleItems_Sales_SaleId" FOREIGN KEY ("SaleId") REFERENCES "Sales" ("Id") ON DELETE CASCADE
);
```

#### 🚀 Endpoints da API Disponíveis

**Saúde e Depuração:**
- `GET /api/debug/health` - Status de conclusão do Passo 3 e próximos passos.
- `GET /api/debug/create-sample-sale` - Testa as regras de domínio sem persistência.

**Gerenciamento de Vendas:**
- `GET /api/sales` - Lista todas as vendas com informações resumidas.
- `GET /api/sales/{id}` - Obtém venda detalhada com todos os itens.
- `POST /api/sales/test-create` - Cria uma venda de amostra com dados de teste.
- `GET /api/sales/customer/{customerId}` - Filtra vendas por cliente.
- `GET /api/sales/exists/{saleNumber}` - Verifica a disponibilidade do número da venda.

**Documentação Swagger:**
- `GET /swagger` - Interface interativa de documentação e teste da API.

#### 🚦 Status Atual
- ✅ **Arquitetura**: Base da Clean Architecture estabelecida.
- ✅ **Sistema de Build**: Todos os projetos compilam com sucesso.
- ✅ **API**: API aprimorada com endpoints de persistência.
- ✅ **Documentação**: Atualizada com a conclusão do Passo 3.
- ✅ **Modelo de Domínio**: Modelo de domínio rico e completo com regras de negócio.
- ✅ **Camada de Persistência**: Implementação completa do PostgreSQL com EF Core.
- ⏳ **Migração de Banco de Dados**: Criada, mas ainda não aplicada à instância do PostgreSQL.
- ⏳ **Camada de Aplicação**: Implementação do CQRS necessária (Próximo passo).
- ⏳ **Recursos de Produção**: Validação, middleware de tratamento de erros necessários.

### Sessão 4 - Implementação de CQRS + MediatR + Testes Unitários (23 de Agosto, 2025)

#### ✅ Tarefas Completadas

**1. Instalação de Pacotes NuGet**
- Adicionados pacotes principais de CQRS e validação:
  - `MediatR 13.0.0` - Mensageria em processo para implementação de CQRS.
  - `FluentValidation 12.0.0` - Interface fluente para construir regras de validação fortemente tipadas.
  - `AutoMapper 15.0.1` - Mapeamento objeto-a-objeto baseado em convenções.
- Adicionado framework de testes abrangente:
  - `xUnit 2.9.2` - Framework de testes moderno com suporte a async/await.
  - `FluentAssertions 8.6.0` - Biblioteca de asserções expressiva para testes legíveis.
  - `Moq 4.20.72` - Framework de mocking para isolamento de dependências.
  - `Bogus 35.6.3` - Geração de dados falsos realistas para cenários de teste.

**2. Objetos de Transferência de Dados (DTOs)**
- Criada estrutura de DTO abrangente:
  - `SaleDto` - Informações completas da venda com itens, valores e metadados.
  - `SaleItemDto` - Item de linha individual com informações do produto e detalhes de preços.
  - `CreateSaleItemDto` - DTO específico do comando para criar itens de venda.
- Implementada separação limpa entre entidades de domínio e contratos da API.
- Adicionado tratamento adequado de moeda e precisão decimal para dados financeiros.

**3. Implementação do Comando CQRS**
- `CreateSaleCommand` - Objeto de comando limpo implementando `IRequest<SaleDto>`.
  - Informações do cliente (ID, nome, email).
  - Informações da filial (ID, nome, localização).
  - Coleção de itens de venda com detalhes completos de produto e preço.
- Seguidos os princípios do CQRS com objetos de comando imutáveis.

**4. Camada de Validação Abrangente**
- `CreateSaleCommandValidator` usando FluentValidation:
  - **Validação do Cliente**: ID obrigatório, nome (3-200 caracteres), formato de email válido.
  - **Validação da Filial**: ID obrigatório, nome (3-200 caracteres), localização (3-200 caracteres).
  - **Validação da Coleção de Itens**: Máximo de 1-20 itens, sem itens nulos.
  - **Validação no Nível do Item**:
    - Informações do produto obrigatórias (ID, nome 3-200 caracteres, categoria 3-100 caracteres).
    - Intervalo de quantidade: Máximo de 1-20 itens por produto.
    - Preços unitários positivos com códigos de moeda válidos.
- Aplicação das regras de negócio na camada de validação com mensagens de erro claras.

**5. Configuração do AutoMapper**
- `SaleMappingProfile` - Configuração de mapeamento complexa:
  - **Entidade Sale para SaleDto**: Mapeia a raiz do agregado com todos os objetos de valor.
  - **Mapeamento de Objetos de Valor**: Transformações dos objetos Customer, Branch e Money.
  - **SaleItem para SaleItemDto**: Mapeamento completo do item de linha, incluindo totais calculados.
  - **Mapeamento de Objetos Aninhados**: Tratamento de ProductInfo com objeto de valor Money aninhado.
- Corrigidos problemas de mapeamento de nomes de propriedades (LineTotal vs TotalPrice) através de desenvolvimento iterativo.

**6. Handler do Comando CQRS**
- `CreateSaleCommandHandler` implementando `IRequestHandler<CreateSaleCommand, SaleDto>`:
  - **Execução da Validação**: Validação de entrada abrangente com relatórios de erro detalhados.
  - **Geração de Número de Venda Único**: Lógica de negócio para gerar identificadores de venda únicos.
  - **Criação de Objetos de Valor**: Construção adequada dos objetos de valor do domínio.
  - **Criação da Entidade de Domínio**: Criação da raiz do agregado Sale com aplicação das regras de negócio.
  - **Persistência no Repositório**: Persistência de dados assíncrona com tratamento de erros adequado.
  - **Integração com AutoMapper**: Mapeamento limpo de entidades de domínio para DTOs de resposta.

**7. Suíte de Testes Abrangente (25 Testes Passando)**

**Infraestrutura de Teste:**
- `SaleTestDataBuilder` usando a biblioteca Bogus para geração de dados de teste realistas.
- Utilitários de teste abrangentes com o padrão builder para cenários de teste consistentes.
- Builders de dados de teste adequados para todos os DTOs, comandos e entidades de domínio.

**Testes da Camada de Domínio (12 testes):**
- **Testes da Entidade Sale**: Criação, aplicação de regras de negócio, gerenciamento de estado.
- **Gerenciamento de Itens de Venda**: Operações de adicionar/atualizar/remover com validação de quantidade.
- **Testes de Lógica de Negócio**: Cálculo de desconto com base nos níveis de quantidade (0%, 10%, 20%).
- **Testes de Objetos de Valor**: Imutabilidade e validação de Money, CustomerInfo, BranchInfo.
- **Eventos de Domínio**: Publicação adequada de eventos para SaleCreated, SaleModified, SaleCancelled.

**Testes da Camada de Aplicação (13 testes):**
- **Testes de Validação de Comando (8 testes)**:
  - Aceitação de comando válido.
  - Cenários de falha na validação do cliente.
  - Cenários de falha na validação da filial.
  - Validação da coleção de itens (vazia, muitos itens).
  - Validação de item individual (quantidade, preço).
  - Validação do formato de email.
  - Verificação abrangente de mensagens de erro.

- **Testes do Handler de Comando (5 testes)**:
  - Criação de venda bem-sucedida com dados válidos.
  - Tratamento de exceção de validação para comandos inválidos.
  - Geração de número de venda único com lógica de nova tentativa.
  - Verificação da aplicação da lógica de negócio.
  - Verificação da integração e mapeamento de propriedades do AutoMapper.

**8. Abordagem de Desenvolvimento Orientado a Testes (TDD)**
- **Ciclo Red-Green-Refactor**: Implementado ao longo do desenvolvimento.
- **Testes de Domínio Primeiro**: Regras de negócio validadas antes da implementação.
- **Testes de Validação**: Regras de validação de entrada testadas antes da implementação do handler.
- **Testes do Handler**: Padrão CQRS testado com dependências mockadas.
- **Abordagem de Integração**: Testes guiaram o design da API e identificaram problemas cedo.

**9. Técnicas Avançadas de Teste**
- **Mocking de Dependências**: Mocking abrangente de Repository, Validator e Mapper.
- **Dados de Teste Realistas**: Biblioteca Bogus para gerar cenários de teste consistentes e realistas.
- **Testes Assíncronos**: Padrões de teste async/await adequados em toda a suíte.
- **Testes de Exceção**: Cenários de exceção de validação com asserções detalhadas.
- **Verificação de Comportamento**: Verificação de mock para garantir chamadas de método adequadas.

#### 🔧 Decisões Técnicas Tomadas

**MediatR para Implementação de CQRS**:
- **Justificativa**: Separação limpa de comandos/queries com mensageria em processo.
- **Benefícios**: Handlers desacoplados, testes fáceis, suporte a middleware para preocupações transversais.
- **Implementação**: Padrão `IRequest<TResponse>` com injeção de dependência.

**FluentValidation em vez de Data Annotations**:
- **Justificativa**: Regras de validação mais expressivas, testáveis e flexíveis.
- **Benefícios**: Cenários de validação complexos, validadores personalizados, mensagens de erro claras.
- **Implementação**: Classes de validador separadas com suporte a validação assíncrona.

**AutoMapper para Mapeamento de DTO**:
- **Justificativa**: Reduz código repetitivo e mantém a consistência do mapeamento.
- **Benefícios**: Mapeamento baseado em convenções com configurações personalizadas para cenários complexos.
- **Desafios**: Mudanças no construtor da versão 15.x exigiram correções iterativas.

**Estratégia de Testes Abrangente**:
- **Justificativa**: Abordagem TDD garante que os requisitos de negócio sejam implementados corretamente.
- **Benefícios**: Detecção precoce de bugs, documentação através de testes, refatoração segura.
- **Implementação**: 25 testes cobrindo a lógica de domínio e a camada de aplicação com dados realistas.

**Dependências Mockadas nos Testes**:
- **Justificativa**: Testes rápidos e isolados focados no comportamento da unidade.
- **Benefícios**: Testes confiáveis e independentes de dependências externas.
- **Implementação**: Framework Moq com padrões adequados de setup/verify.

#### 📁 Arquivos Criados/Modificados

**Camada de Aplicação - DTOs:**
- `DeveloperStore.Application/Common/DTOs/SaleDto.cs` - DTO completo de resposta da venda
- `DeveloperStore.Application/Common/DTOs/SaleItemDto.cs` - DTO do item de linha da venda
- `DeveloperStore.Application/Common/DTOs/CreateSaleItemDto.cs` - DTO de entrada do comando

**Camada de Aplicação - Implementação CQRS:**
- `DeveloperStore.Application/Sales/Commands/CreateSale/CreateSaleCommand.cs` - Comando CQRS
- `DeveloperStore.Application/Sales/Commands/CreateSale/CreateSaleCommandValidator.cs` - Regras do FluentValidation
- `DeveloperStore.Application/Sales/Commands/CreateSale/CreateSaleCommandHandler.cs` - Handler do comando com lógica de negócio

**Camada de Aplicação - AutoMapper:**
- `DeveloperStore.Application/Common/Mappings/SaleMappingProfile.cs` - Configuração de mapeamento de entidade para DTO

**Infraestrutura de Teste:**
- `DeveloperStore.Tests/TestUtilities/Builders/SaleTestDataBuilder.cs` - Geração de dados de teste realistas

**Suítes de Teste:**
- `DeveloperStore.Tests/Unit/Domain/SaleTests.cs` - Testes de lógica de negócio da entidade de domínio
- `DeveloperStore.Tests/Unit/Application/Sales/Commands/CreateSaleCommandValidatorTests.cs` - Testes de validação
- `DeveloperStore.Tests/Unit/Application/Sales/Commands/CreateSaleCommandHandlerTests.cs` - Testes de comportamento do handler

**Configuração do Projeto:**
- `DeveloperStore.Application/DeveloperStore.Application.csproj` - Adicionados pacotes MediatR, FluentValidation, AutoMapper
- `DeveloperStore.Tests/DeveloperStore.Tests.csproj` - Adicionados pacotes de teste abrangentes

#### 🧪 Resumo dos Resultados dos Testes

```
✅ 25 Testes Passando (0 Falhas)
📊 Detalhamento dos Testes:
   - Testes de Lógica de Domínio: 12/12 ✅
   - Testes de Validação de Comando: 8/8 ✅
   - Testes de Handler de Comando: 5/5 ✅
🚀 Status do Build: Sucesso
⚡ Execução dos Testes: 168ms no total
```

**Áreas de Cobertura dos Testes:**
- ✅ Aplicação de regras de negócio (cálculos de desconto, limites de quantidade)
- ✅ Imutabilidade e validação de objetos de valor
- ✅ Publicação de eventos de domínio
- ✅ Validação de entrada com cenários de erro abrangentes
- ✅ Lógica do handler CQRS com dependências mockadas
- ✅ Configuração e mapeamento de propriedades do AutoMapper
- ✅ Padrões de integração com o repositório
- ✅ Padrões async/await em toda a suíte

#### 🏗️ Benefícios Arquiteturais Alcançados

**1. Separação de Responsabilidades:**
- Comandos separados de queries (princípio CQRS).
- Lógica de validação isolada em validadores dedicados.
- Lógica de negócio centralizada nas entidades de domínio.
- Lógica de mapeamento abstraída através do AutoMapper.

**2. Testabilidade:**
- Cobertura de 100% de testes unitários para os caminhos críticos do negócio.
- Testes rápidos e isolados com dependências mockadas.
- Abordagem TDD garantindo que os requisitos guiem a implementação.
- Builders de dados de teste abrangentes para cenários consistentes.

**3. Manutenibilidade:**
- Padrão limpo de comando/handler.
- Validação fluente com mensagens de erro claras.
- Mapeamento automatizado reduzindo código repetitivo.
- Suítes de teste bem estruturadas servindo como documentação.

**4. Domain-Driven Design:**
- Entidades de domínio ricas com lógica de negócio encapsulada.
- Objetos de valor mantendo a imutabilidade.
- Eventos de domínio permitindo acoplamento fraco.
- Padrão Repository abstraindo o acesso a dados.

#### 🔄 Desafios de Desenvolvimento e Soluções

**Desafio 1: Compatibilidade de Versão do AutoMapper**
- **Problema**: O AutoMapper 15.x mudou a sintaxe do construtor, causando erros de build.
- **Solução**: Atualizado para usar a sintaxe `typeof()` e adicionada referência explícita do pacote ao projeto de testes.
- **Aprendizado**: Testar a compatibilidade de versão é importante ao atualizar pacotes.

**Desafio 2: Assinaturas de Método do Repositório**
- **Problema**: Falhas na configuração do mock devido a parâmetros `CancellationToken` opcionais nos métodos do repositório.
- **Solução**: Parâmetros `CancellationToken` explícitos nas configurações do mock.
- **Aprendizado**: Árvores de expressão não podem conter parâmetros opcionais.

**Desafio 3: Nomes de Propriedade da Entidade de Domínio**
- **Problema**: A configuração do AutoMapper falhou devido à incompatibilidade de nomes de propriedade (TotalPrice vs LineTotal).
- **Solução**: Atualizado o perfil de mapeamento para usar os nomes de propriedade corretos das entidades de domínio.
- **Aprendizado**: O AutoMapper requer nomes de propriedade exatos ou configuração explícita.

**Desafio 4: Consistência dos Dados de Teste**
- **Problema**: A criação manual de dados de teste era propensa a erros e inconsistente.
- **Solução**: Implementados builders de dados de teste baseados em Bogus para dados realistas e consistentes.
- **Aprendizado**: Uma infraestrutura de dados de teste adequada é crucial para testes confiáveis.

#### 🚦 Status Atual
- ✅ **Arquitetura**: Base da Clean Architecture com implementação de CQRS.
- ✅ **Sistema de Build**: Todos os projetos compilam com sucesso com gerenciamento abrangente de pacotes.
- ✅ **API**: API aprimorada pronta para integração com o controller CQRS.
- ✅ **Documentação**: Atualizada com detalhes abrangentes da implementação do Passo 4.
- ✅ **Modelo de Domínio**: Modelo de domínio rico com regras de negócio e validação completas.
- ✅ **Camada de Persistência**: Implementação completa do PostgreSQL com integração EF Core.
- ✅ **Camada de Aplicação**: Implementação completa de CQRS com MediatR, validação e mapeamento.
- ✅ **Infraestrutura de Testes**: Suíte de testes abrangente com 25 testes passando usando a abordagem TDD.
- ⏳ **Implementação de Queries**: Queries CQRS ainda não implementadas (Próxima prioridade).
- ⏳ **Integração da API**: Controllers precisam ser atualizados para usar comandos/queries do MediatR.
- ⏳ **Injeção de Dependência**: Configuração do contêiner IoC para uso em produção.

#### 🔮 Objetivos da Próxima Sessão
- **Passo 5**: ✅ **CONCLUÍDO** - Implementação de Queries e Integração da API.
- **Passo 6**: Testes de Integração e Prontidão para Produção.
  - Adicionar middleware global de tratamento de erros e logging de requisição/resposta.
  - Criar suíte de testes de integração abrangente com WebApplicationFactory.
  - Adicionar framework de autenticação/autorização.
  - Monitoramento de performance e health checks.

### Sessão 5 - Implementação de Queries CQRS e Integração com a API (23 de Agosto, 2025)

#### ✅ Tarefas Completadas

**1. Implementação de Queries CQRS**
- `GetSaleByIdQuery` - Query para recuperar detalhes completos da venda por ID.
  - Retorna informações detalhadas da venda, incluindo todos os itens.
  - Implementa `IRequest<GetSaleByIdResponse?>` para tratamento de resposta anulável.
- `GetAllSalesQuery` - Query para recuperar a lista de vendas com informações resumidas.
  - Retorna uma lista de vendas com informações essenciais para visualizações de listagem.
  - Implementa `IRequest<List<GetAllSalesResponse>>`.

**2. DTOs de Resposta das Queries**
- `GetSaleByIdResponse` - Resposta com detalhes completos da venda:
  - Informações completas da venda com detalhes de cliente, filial e itens.
  - DTOs aninhados para CustomerDto, BranchDto, ProductDto, SaleItemDto.
  - Tratamento adequado de valores monetários com MoneyDto para todos os campos financeiros.
- `GetAllSalesResponse` - Informações resumidas da venda para listagem:
  - Dados essenciais da venda sem o detalhamento dos itens.
  - Otimizado para visualizações de lista com um `ItemCount` calculado.

**3. Handlers das Queries**
- `GetSaleByIdQueryHandler` - Handler para recuperação de venda individual:
  - Usa o padrão Repository com injeção de dependência.
  - Integração com AutoMapper para conversão de entidade para DTO.
  - Tratamento seguro de nulos, retornando `null` para vendas inexistentes.
- `GetAllSalesQueryHandler` - Handler para recuperação da lista de vendas:
  - Recuperação em massa eficiente do repositório.
  - Conversão de lista pelo AutoMapper com tipagem adequada.

**4. Extensão do Perfil do AutoMapper**
- Estendido o `SaleMappingProfile` com mapeamentos de query abrangentes:
  - **Mapeamentos de Objetos de Valor**: Money → MoneyDto, CustomerInfo → CustomerDto, BranchInfo → BranchDto, ProductInfo → ProductDto.
  - **Mapeamentos de Entidade para DTO de Query**: Sale → GetSaleByIdResponse, Sale → GetAllSalesResponse.
  - **Mapeamentos Aninhados Complexos**: SaleItem com informações de produto aninhadas e objetos monetários.
  - **Campos Calculados**: `ItemCount` calculado a partir do tamanho da coleção.
- Resolvidos conflitos de namespace entre DTOs de comando e query usando nomes totalmente qualificados.

**5. Implementação do Controller da API de Produção**
- **Substituído o controller de teste** pela implementação CQRS de produção:
  - Eliminadas dependências diretas do repositório em favor do MediatR.
  - Injeção de dependência adequada com a interface `IMediator`.
  - Separação limpa de operações de comando e query.

**6. Endpoints da API RESTful com CQRS**
- `GET /api/sales` - Lista todas as vendas usando `GetAllSalesQuery`:
  - Retorna informações resumidas otimizadas para visualizações de listagem.
  - Códigos de status HTTP adequados (200 OK, 500 Internal Server Error).
  - Tratamento de erros abrangente com logging estruturado.
- `GET /api/sales/{id}` - Obtém detalhes da venda usando `GetSaleByIdQuery`:
  - Retorna informações completas da venda, incluindo todos os itens.
  - 404 Not Found para vendas inexistentes.
  - Logging detalhado para depuração e monitoramento.
- `POST /api/sales` - Cria uma venda usando o `CreateSaleCommand` existente:
  - Validação do modelo da requisição com data annotations.
  - Mapeamento adequado de DTO entre as camadas da API e da Aplicação.
  - Resposta `CreatedAtAction` com o header `Location`.

**7. Modelos de Requisição/Resposta da API**
- `CreateSaleRequest` - Modelo de requisição específico da API:
  - Validação com data annotations (Required, StringLength, EmailAddress, Range).
  - `CreateSaleItemRequest` aninhado para itens de venda.
  - Separação limpa dos DTOs da camada de aplicação.
- Mapeamento adequado entre os modelos da API e os comandos CQRS:
  - `CreateSaleRequest` → `CreateSaleCommand`
  - `CreateSaleItemRequest` → `CreateSaleItemDto`

**8. Tratamento de Erros e Logging Aprimorados**
- **Logging Estruturado**: Logging abrangente em todas as ações do controller.
- **Tratamento de Exceções**: Blocos try-catch com respostas de erro adequadas.
- **Códigos de Status HTTP**: Códigos de status apropriados para diferentes cenários.
- **Documentação OpenAPI**: Atributos `ProducesResponseType` para documentação Swagger.

#### 🔧 Decisões Técnicas Tomadas

**Padrão de Query CQRS**:
- **Justificativa**: Separação completa de operações de leitura e escrita, seguindo os princípios do CQRS.
- **Benefícios**: DTOs otimizados para diferentes casos de uso, interfaces de query limpas.
- **Implementação**: Handlers separados para cada tipo de query com responsabilidades focadas.

**Respostas de Query Anuláveis**:
- **Justificativa**: `GetSaleByIdQuery` retorna anulável para lidar graciosamente com entidades inexistentes.
- **Benefícios**: Tratamento de nulos seguro em tipo, contratos de API claros.
- **Implementação**: `IRequest<GetSaleByIdResponse?>` com verificações de nulo no controller.

**AutoMapper para DTOs de Query**:
- **Justificativa**: Estratégia de mapeamento consistente entre comandos e queries.
- **Benefícios**: Redução de código repetitivo, configurações de mapeamento de fácil manutenção.
- **Desafio**: Conflitos de namespace resolvidos com nomes de tipo totalmente qualificados.

**Modelos de Requisição da Camada de API**:
- **Justificativa**: Separar contratos da API dos DTOs da camada de aplicação.
- **Benefícios**: Flexibilidade de versionamento da API, validação na fronteira da API.
- **Implementação**: Modelos de requisição com data annotations, mapeando para DTOs de comando.

**MediatR nos Controllers**:
- **Justificativa**: Elimina dependências de repositório, reforça o padrão CQRS.
- **Benefícios**: Dependências de controller limpas, despacho centralizado de comandos/queries.
- **Implementação**: Dependência única de `IMediator` com o método `Send()` para todas as operações.

#### 📁 Arquivos Criados/Modificados

**Camada de Aplicação - Implementação de Query:**
- `DeveloperStore.Application/Sales/Queries/GetSaleById/GetSaleByIdQuery.cs` - Definição da query
- `DeveloperStore.Application/Sales/Queries/GetSaleById/GetSaleByIdResponse.cs` - DTOs de resposta com tipos aninhados
- `DeveloperStore.Application/Sales/Queries/GetSaleById/GetSaleByIdQueryHandler.cs` - Implementação do handler da query
- `DeveloperStore.Application/Sales/Queries/GetAllSales/GetAllSalesQuery.cs` - Definição da query
- `DeveloperStore.Application/Sales/Queries/GetAllSales/GetAllSalesResponse.cs` - DTO de resposta
- `DeveloperStore.Application/Sales/Queries/GetAllSales/GetAllSalesQueryHandler.cs` - Implementação do handler da query

**Camada de Aplicação - Atualizações de Mapeamento:**
- `DeveloperStore.Application/Common/Mappings/SaleMappingProfile.cs` - Estendido com mapeamentos de query

**Camada da API - Implementação de Produção:**
- `DeveloperStore.Api/Controllers/SalesController.cs` - Reescrevita completa do controller CQRS
- `DeveloperStore.Api/Controllers/CreateSaleRequest.cs` - Modelos de requisição da API com validação

#### 🚀 Endpoints da API Após o Passo 5

**Gerenciamento de Vendas (Implementação CQRS):**
- `GET /api/sales` - Lista todas as vendas (Query)
  - **Handler**: GetAllSalesQueryHandler
  - **Resposta**: `List<GetAllSalesResponse>` com informações resumidas
  - **Códigos de Status**: 200 OK, 500 Internal Server Error
- `GET /api/sales/{id}` - Obtém detalhes da venda (Query)
  - **Handler**: GetSaleByIdQueryHandler
  - **Resposta**: `GetSaleByIdResponse` com detalhes completos da venda
  - **Códigos de Status**: 200 OK, 404 Not Found, 500 Internal Server Error
- `POST /api/sales` - Cria nova venda (Command)
  - **Handler**: CreateSaleCommandHandler
  - **Requisição**: `CreateSaleRequest` com validação
  - **Resposta**: `SaleDto` com informações da venda criada
  - **Códigos de Status**: 201 Created, 400 Bad Request, 500 Internal Server Error

#### 🧪 Validação dos Resultados dos Testes

```
✅ Todos os Testes Passando: 25/25 ✅
🏗️ Status do Build: Sucesso
⚡ Implementação CQRS: Completa
🔄 Transformação da API: Controller de Teste → Controller CQRS de Produção
```

**Processo de Validação:**
- ✅ Testes existentes mantêm compatibilidade após as mudanças no controller da API.
- ✅ O sistema de build valida todas as novas implementações de query.
- ✅ As configurações do AutoMapper são testadas através dos testes unitários existentes.
- ✅ O padrão Repository é preservado, garantindo que a infraestrutura de teste funcione.

#### 🔄 Desafios de Desenvolvimento e Soluções

**Desafio 1: Conflitos de Namespace no AutoMapper**
- **Problema**: `SaleItemDto` existia nos namespaces de comando e query, causando erros de compilação.
- **Solução**: Usados nomes de tipo totalmente qualificados (ex: `Sales.Queries.GetSaleById.SaleItemDto`) nos perfis de mapeamento.
- **Aprendizado**: O gerenciamento de namespaces é crucial em implementações CQRS grandes.

**Desafio 2: Design do Modelo de Requisição/Resposta da API**
- **Problema**: Necessidade de validação específica da API, mantendo a separação da camada de aplicação.
- **Solução**: Criados modelos de requisição de API separados com mapeamento para DTOs da aplicação.
- **Aprendizado**: A separação limpa permite o versionamento da API sem quebrar os contratos da aplicação.

**Desafio 3: Gerenciamento de Dependências do Controller**
- **Problema**: Transição de dependências de repositório para o padrão MediatR.
- **Solução**: Substituição sistemática de chamadas de repositório por operações `MediatR.Send()`.
- **Aprendizado**: O MediatR fornece um gráfico de dependências mais limpo e melhor testabilidade.

**Desafio 4: Tratamento de Respostas Anuláveis**
- **Problema**: `GetSaleById` deve lidar graciosamente com entidades inexistentes.
- **Solução**: Implementado `IRequest<GetSaleByIdResponse?>` com verificações de nulo tanto no handler quanto no controller.
- **Aprendizado**: O tratamento de nulos seguro em tipo melhora a confiabilidade da API e a experiência do desenvolvedor.

#### 🚦 Status Atual
- ✅ **Arquitetura**: Clean Architecture com implementação completa de CQRS.
- ✅ **Sistema de Build**: Todos os projetos compilam com sucesso com gerenciamento abrangente de pacotes.
- ✅ **API**: API CQRS pronta para produção com tratamento de erros e logging adequados.
- ✅ **Documentação**: Atualizada com detalhes abrangentes da implementação do Passo 5.
- ✅ **Modelo de Domínio**: Modelo de domínio rico com regras de negócio e validação completas.
- ✅ **Camada de Persistência**: Implementação completa do PostgreSQL com integração EF Core.
- ✅ **Camada de Aplicação**: Implementação completa de CQRS com comandos e queries.
- ✅ **Implementação de Queries**: Implementação completa do lado de leitura com DTOs otimizados.
- ✅ **Integração da API**: Controller de produção usando MediatR para todas as operações.
- ✅ **Infraestrutura de Testes**: Todos os testes existentes passando, base para testes de integração.
- ⏳ **Testes de Integração**: Testes com `WebApplicationFactory` necessários para validação completa da API.
- ⏳ **Middleware**: Middleware global de tratamento de erros e logging de requisição/resposta necessários.
- ⏳ **Autenticação**: Implementação de segurança necessária para o deployment em produção.

#### 📝 Aprendizados Chave do Passo 5

**Implementação de Queries CQRS**:
- Modelos de query separados permitem a otimização do lado de leitura sem afetar as operações de escrita.
- Tipos de retorno anuláveis fornecem tratamento seguro de dados opcionais.
- O AutoMapper permite a conversão consistente de objetos entre operações de comando e query.

**Design de API de Produção**:
- O MediatR elimina as dependências do controller e reforça os limites do CQRS.
- Modelos de requisição de API separados fornecem flexibilidade de validação e versionamento.
- Tratamento de erros e logging abrangentes são essenciais para o monitoramento em produção.

**Arquitetura de Controller**:
- A dependência única de `IMediator` simplifica o construtor do controller e os testes.
- O logging estruturado fornece visibilidade operacional sobre o processamento das requisições.
- Códigos de status HTTP e tipos de resposta adequados melhoram a experiência do desenvolvedor da API.

**Gerenciamento da Configuração do AutoMapper**:
- Conflitos de namespace requerem gerenciamento cuidadoso em implementações CQRS grandes.
- Nomes de tipo totalmente qualificados resolvem a ambiguidade entre DTOs semelhantes.
- Mapeamentos aninhados complexos se beneficiam de configuração explícita em vez de convenção.

#### 📝 Aprendizados Chave do Passo 4

**Persistência na Clean Architecture**:
- Interfaces de repositório pertencem à camada de Domínio para inversão de dependência.
- A camada de Infraestrutura implementa todas as dependências externas.
- A separação adequada permite testes fáceis e troca de banco de dados.

**Mapeamento de Objetos de Valor com EF Core**:
- O padrão `OwnsOne` preserva o encapsulamento do objeto de valor no esquema relacional.
- Construtores sem parâmetros são necessários, mas podem ser privados.
- Objetos de valor complexos (como `ProductInfo` com `Money` aninhado) requerem configuração cuidadosa.

**Integração com PostgreSQL**:
- Políticas de nova tentativa de conexão são essenciais para implantações em nuvem.
- Configurações de desenvolvimento/produção separadas melhoram a experiência do desenvolvedor.
- Uma estratégia de indexação adequada melhora o desempenho das consultas.

**Domain-Driven Design com ORM**:
- Modelos de domínio ricos podem coexistir com o mapeamento relacional.
- Propriedades computadas (como `TotalAmount`) devem ser ignoradas nas configurações de entidade.
- Eventos de domínio não requerem persistência, mas permitem o Event Sourcing futuro.

---

## Próximos Passos

### Passo 6: Testes de Integração e Prontidão para Produção ✅ (CONCLUÍDO)

**Objetivo**: Implementar testes de integração e recursos para produção, incluindo middleware de tratamento de exceções global, logging estruturado e monitoramento.

#### Implementações Realizadas

**✅ Middleware de Produção**:
- **GlobalExceptionMiddleware**: Tratamento centralizado de exceções com respostas JSON estruturadas.
  - Respostas formatadas com RequestId, Timestamp, StatusCode e Message.
  - Níveis de detalhe diferentes com base no ambiente (Desenvolvimento vs Produção).
  - Logging estruturado de todas as exceções para monitoramento.
  - Integração com `IHostEnvironment` para comportamento específico do ambiente.

- **RequestResponseLoggingMiddleware**: Logging detalhado de requisições HTTP.
  - Captura de método, path, status code e duração da requisição.
  - Logging do corpo da requisição/resposta (com filtros de content-type).
  - Métricas de performance para identificação de endpoints lentos.
  - Níveis de log diferentes com base no status code da resposta.

**✅ Logging Estruturado (Serilog)**:
- Configuração completa do Serilog para substituir o logging padrão do ASP.NET Core.
- Múltiplos sinks: Console (para desenvolvimento) e Arquivo (para produção).
- Enriquecimento com informações contextuais (nome da Aplicação, Ambiente).
- Configuração específica por ambiente (Desenvolvimento, Produção, Teste).
- Logging automático de requisições com `Serilog.AspNetCore`.

**✅ Health Checks e Monitoramento**:
- Endpoint `/health` para verificação de status da aplicação.
- Integração com os health checks do ASP.NET Core.
- Base para futuras verificações de dependências externas (banco de dados, cache, etc.).

**✅ Configuração CORS**:
- Política CORS configurada para desenvolvimento e testes.
- Permite qualquer origem, método e header durante o desenvolvimento.
- Base para configuração restritiva em produção.

**✅ Arquitetura de Testes de Integração**:
- **DeveloperStoreWebApplicationFactory**: Factory customizada para testes.
  - Configuração de ambiente "Testing" isolado.
  - Substituição do PostgreSQL por um banco de dados InMemory.
  - Seeding automático de dados de teste.
  - Limpeza entre testes para isolamento.

- **Cobertura de Testes Implementada**:
  - Endpoint de health check (✅ funcionando).
  - Comportamento do GlobalExceptionMiddleware.
  - Controllers de Vendas (GET, POST, PUT, DELETE).
  - Tratamento de cenários de erro (404, 400).

#### Status Atual dos Testes

**Testes Unitários**: ✅ **28/28 PASSANDO**
- Todos os testes de lógica de domínio, command handlers e validações funcionando.
- Cobertura completa das regras de negócio.
- Mocking adequado de dependências.

**Testes de Integração**: ⚠️ **1/9 PASSANDO**
- ✅ Endpoint de health check funcionando corretamente.
- ❌ Testes de Controller com entidades próprias (owned entities) do EF Core enfrentando problemas de configuração.
- ❌ Testes de middleware com conflitos de versionamento resolvidos, mas o comportamento do endpoint precisa de ajuste.

#### Problemas Identificados e Soluções

**1. Conflitos de Versão do EF Core**:
- ✅ **Resolvido**: Atualizado `DeveloperStore.Tests.csproj` para usar EF Core 9.0.8 e System.Text.Json 9.0.8.
- Eliminados conflitos entre as versões 9.0.1 e 9.0.8 do `EntityFrameworkCore.Relational`.

**2. EF Core InMemory com Entidades Próprias**:
- ❌ **Parcialmente resolvido**: Objetos de valor (Money, ProductInfo) causam problemas de persistência no provedor InMemory.
- **Solução de contorno implementada**: Testes básicos funcionam, mas a criação de dados complexos precisa de ajuste.
- **Solução futura**: Considerar usar um banco de dados SQLite em memória para testes ou simplificar os dados de seed.

**3. Compatibilidade do Program.cs com Testes**:
- ✅ **Resolvido**: Simplificado `Program.cs` para funcionar tanto com hospedagem normal quanto com `WebApplicationFactory`.
- Removida lógica condicional complexa que bloqueava a inicialização dos testes.

#### Recursos de Produção Implementados

**Pipeline de Logging Estruturado**:
```csharp
// Configuração do Serilog com múltiplos sinks
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/developerstore-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "DeveloperStore.Api")
    .CreateBootstrapLogger();
```

**Tratamento Global de Exceções**:
```csharp
// Respostas de erro estruturadas
{
  "RequestId": "0HMVKJQ9R7MCV:00000001",
  "Timestamp": "2025-08-23T12:00:00Z",
  "StatusCode": 404,
  "Message": "Venda não encontrada",
  "Details": "..." // Apenas em Desenvolvimento
}
```

**Monitoramento de Performance**:
```csharp
// Logging de Requisição/Resposta com tempo
[12:00:00 INF] HTTP POST /api/sales responded 201 Created in 45ms
```

#### Próximas Melhorias para Testes

**Completude dos Testes de Integração**:
1.  Resolver a configuração de entidades próprias no banco de dados InMemory.
2.  Implementar testes end-to-end completos para todos os endpoints.
3.  Validar o comportamento do middleware com diferentes tipos de exceções.
4.  Testes básicos de performance e carga.

**Gerenciamento de Dados de Teste**:
1.  Padrão builder para criação de dados de teste complexos.
2.  Fixtures de teste reutilizáveis entre diferentes classes de teste.
3.  Estratégias de seeding de banco de dados mais robustas.

#### 📝 Aprendizados Chave do Passo 6

**Arquitetura de Middleware de Produção**:
- O middleware de tratamento de exceções deve ser posicionado no início do pipeline.
- O middleware de logging deve capturar tanto casos de sucesso quanto de erro.
- O comportamento específico do ambiente é crucial para não vazar dados sensíveis em produção.

**Integração com Serilog**:
- Um logger de bootstrap é necessário para capturar logs durante a inicialização da aplicação.
- Múltiplos sinks permitem diferentes estratégias de logging (console + arquivo).
- O logging estruturado facilita a análise e o monitoramento em produção.

**Testes com WebApplicationFactory**:
- A substituição de serviços deve ser cuidadosa com as cadeias de dependência.
- O banco de dados InMemory tem limitações com modelos de domínio complexos.
- O isolamento de ambiente é crítico para testes determinísticos.

**Limitações do EF Core InMemory**:
- Entidades próprias podem ter comportamento diferente entre os provedores InMemory e SQL.
- Hierarquias complexas de objetos de valor podem precisar de simplificação para testes.
- Considere o SQLite em memória como uma alternativa mais próxima do comportamento real.

---

### Passo 7: Recursos Avançados e Implantação (PLANEJADO)

**Objetivo**: Implementar recursos avançados como cache, limitação de taxa (rate limiting), pipeline de CI/CD e implantação no Azure/AWS.

**Escopo Planejado**:
- Cache de respostas com Redis.
- Limitação de taxa e throttling.
- Versionamento de API.
- Observabilidade com OpenTelemetry.
- CI/CD com GitHub Actions.
- Implantação no Azure App Service.
- Conteinerização com Docker.
- Benchmarking de performance.

---