# Configuração do Ambiente de Desenvolvimento PostgreSQL

Este guia irá ajudá-lo a configurar um ambiente de desenvolvimento PostgreSQL local para o projeto DeveloperStore.

## Descrição do Problema

Os testes de integração funcionam porque usam banco de dados SQLite em memória, mas a API de desenvolvimento falha com erros de autenticação PostgreSQL porque não há servidor PostgreSQL local configurado.

**Erro:** `28P01: autenticação do tipo senha falhou para o usuário "devstore_user"`

## Pré-requisitos

- Windows 10/11
- .NET 8 SDK
- Git Bash (recomendado para executar comandos)

## Opções de Solução

Escolha uma das seguintes abordagens:

### Opção 1: Docker PostgreSQL (Recomendado - Configuração Fácil)

Esta é a forma mais rápida de colocar o PostgreSQL rodando localmente.

#### 1.1 Instalar Docker Desktop
- Baixe e instale o [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Inicie o Docker Desktop

#### 1.2 Criar Container PostgreSQL
Abra o terminal e execute:

```bash
# Criar e iniciar container PostgreSQL
docker run --name developerstore-postgres \
  -e POSTGRES_DB=DeveloperStore_Dev \
  -e POSTGRES_USER=devstore_user \
  -e POSTGRES_PASSWORD=devstore_pass \
  -p 5432:5432 \
  -d postgres:15

# Verificar se o container está rodando
docker ps
```

#### 1.3 Criar Docker Compose (Opcional mas Recomendado)
Crie `docker-compose.yml` na raiz do projeto:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    container_name: developerstore-postgres
    environment:
      POSTGRES_DB: DeveloperStore_Dev
      POSTGRES_USER: devstore_user
      POSTGRES_PASSWORD: devstore_pass
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres_data:
```

Então execute:
```bash
# Iniciar com Docker Compose
docker-compose up -d

# Parar quando necessário
docker-compose down
```

---

### Opção 2: Instalação Nativa do PostgreSQL

#### 2.1 Baixar e Instalar PostgreSQL
1. Baixe o PostgreSQL do [site oficial](https://www.postgresql.org/download/windows/)
2. Execute o instalador
3. Durante a instalação:
   - Lembre-se da senha do superusuário (postgres)
   - Mantenha a porta padrão 5432
   - Selecione componentes: PostgreSQL Server, pgAdmin 4, Command Line Tools

#### 2.2 Configurar PostgreSQL
Abra o **pgAdmin 4** ou linha de comando **psql**:

```sql
-- Conecte como superusuário e crie o banco e usuário
CREATE DATABASE "DeveloperStore_Dev";
CREATE USER devstore_user WITH ENCRYPTED PASSWORD 'devstore_pass';
GRANT ALL PRIVILEGES ON DATABASE "DeveloperStore_Dev" TO devstore_user;

-- Conceder permissões adicionais para operações de schema
\c "DeveloperStore_Dev"
GRANT ALL ON SCHEMA public TO devstore_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO devstore_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO devstore_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO devstore_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO devstore_user;
```

---

## Configuração de Migração do Banco de Dados

Após o PostgreSQL estar rodando, configure o schema do banco de dados:

### Passo 1: Instalar EF Core Tools (se não instalado)
```bash
dotnet tool install --global dotnet-ef
```

### Passo 2: Navegar para o Diretório do Projeto
```bash
cd "c:\dotnet\Personal\mouts-project"
```

### Passo 3: Verificar Conexão
Teste a string de conexão verificando se o EF Core consegue conectar:

```bash
dotnet ef dbcontext info --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api
```

### Passo 4: Aplicar Migrações do Banco de Dados
```bash
# Criar e aplicar migrações
dotnet ef database update --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api
```

Se as migrações não existirem, crie-as:
```bash
# Criar migração inicial
dotnet ef migrations add InitialCreate --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api

# Aplicar migração
dotnet ef database update --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api
```

## Verificação da String de Conexão

Seu `appsettings.Development.json` deve ter:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DeveloperStore_Dev;Username=devstore_user;Password=devstore_pass;Port=5432"
  }
}
```

## Testando a Configuração

### Teste 1: Teste de Conexão
```bash
# Execute a aplicação
dotnet run --project DeveloperStore.Api

# Verifique os logs para inicialização bem-sucedida sem erros de banco de dados
```

### Teste 2: Teste de Endpoint da API
```bash
# Teste o endpoint de vendas
curl -X GET "http://localhost:5079/api/Sales" -H "accept: application/json"
```

### Teste 3: Teste do Swagger
Abra o navegador e navegue para: `http://localhost:5079/swagger`

## Solução de Problemas

### Problemas Comuns e Soluções

#### Problema 1: "Connection refused" ou "server doesn't exist"
**Solução:** Servidor PostgreSQL não está rodando
```bash
# Para Docker:
docker start developerstore-postgres

# Para instalação nativa:
# Inicie o serviço PostgreSQL do Windows Services ou:
net start postgresql-x64-15
```

#### Problema 2: "Authentication failed"
**Solução:** Usuário ou senha incorreta
```sql
-- Resetar senha do usuário
ALTER USER devstore_user WITH ENCRYPTED PASSWORD 'devstore_pass';
```

#### Problema 3: "Database does not exist"
**Solução:** Banco de dados não foi criado
```sql
CREATE DATABASE "DeveloperStore_Dev";
GRANT ALL PRIVILEGES ON DATABASE "DeveloperStore_Dev" TO devstore_user;
```

#### Problema 4: "Permission denied"
**Solução:** Usuário não tem permissões
```sql
\c "DeveloperStore_Dev"
GRANT ALL ON SCHEMA public TO devstore_user;
```

### Verificar Status do PostgreSQL

#### Docker:
```bash
# Verificar se o container está rodando
docker ps

# Verificar logs do container
docker logs developerstore-postgres

# Conectar ao PostgreSQL no container
docker exec -it developerstore-postgres psql -U devstore_user -d DeveloperStore_Dev
```

#### Instalação Nativa:
```bash
# Verificar status do serviço Windows
sc query postgresql-x64-15

# Conectar via psql
psql -h localhost -U devstore_user -d DeveloperStore_Dev
```

## Fluxo de Trabalho de Desenvolvimento

### Desenvolvimento Diário
```bash
# Iniciar PostgreSQL (Docker)
docker start developerstore-postgres

# Executar a API
dotnet run --project DeveloperStore.Api

# Parar PostgreSQL quando terminar (Docker)
docker stop developerstore-postgres
```

### Ao Adicionar Novas Migrações
```bash
# Criar migração
dotnet ef migrations add NomeDaMigracao --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api

# Aplicar migração
dotnet ef database update --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api
```

## Alternativa: Usar SQLite para Desenvolvimento

Se preferir evitar a configuração do PostgreSQL, você pode configurar o ambiente de desenvolvimento para usar SQLite como os testes:

### Modificar Program.cs
```csharp
// Substitua a seção de serviços de infraestrutura:
if (builder.Environment.IsDevelopment())
{
    // Usar SQLite para desenvolvimento
    builder.Services.AddDbContext<DeveloperStoreDbContext>(options =>
        options.UseSqlite("Data Source=DeveloperStore.db"));
    builder.Services.AddScoped<ISaleRepository, SaleRepository>();
}
else if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddInfrastructureDevelopment(builder.Configuration);
}
else
{
    builder.Services.AddInfrastructure(builder.Configuration);
}
```

## Comandos Úteis do Docker

### Gerenciamento do Container
```bash
# Listar todos os containers
docker ps -a

# Iniciar container parado
docker start developerstore-postgres

# Parar container rodando
docker stop developerstore-postgres

# Remover container (cuidado: apaga os dados!)
docker rm developerstore-postgres

# Ver logs do container
docker logs developerstore-postgres -f
```

### Backup e Restore
```bash
# Fazer backup do banco
docker exec developerstore-postgres pg_dump -U devstore_user DeveloperStore_Dev > backup.sql

# Restaurar backup
docker exec -i developerstore-postgres psql -U devstore_user -d DeveloperStore_Dev < backup.sql
```

## Dicas de Desenvolvimento

### 1. Variáveis de Ambiente
Considere usar variáveis de ambiente para configurações sensíveis:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST:-localhost};Database=${DB_NAME:-DeveloperStore_Dev};Username=${DB_USER:-devstore_user};Password=${DB_PASS:-devstore_pass};Port=${DB_PORT:-5432}"
  }
}
```

### 2. Scripts de Automação
Crie scripts para automatizar tarefas comuns:

**start-dev.bat:**
```batch
@echo off
echo Iniciando ambiente de desenvolvimento...
docker start developerstore-postgres
timeout /t 3
dotnet run --project DeveloperStore.Api
```

**stop-dev.bat:**
```batch
@echo off
echo Parando ambiente de desenvolvimento...
docker stop developerstore-postgres
```

### 3. Configuração de IDE
Configure sua IDE (Visual Studio/VS Code) para:
- Auto-iniciar containers Docker
- Variáveis de ambiente de desenvolvimento
- Tasks para comandos EF Core

## Validação Final

Após seguir este guia, verifique se tudo está funcionando:

### ✅ Lista de Verificação
- [ ] PostgreSQL rodando (Docker ou nativo)
- [ ] Banco `DeveloperStore_Dev` criado
- [ ] Usuário `devstore_user` configurado
- [ ] Migrações EF Core aplicadas
- [ ] API inicia sem erros de banco
- [ ] Endpoint `/api/Sales` responde (mesmo que vazio)
- [ ] Swagger carrega sem erros
- [ ] Testes de integração continuam passando

### 🧪 Teste Completo
```bash
# 1. Verificar PostgreSQL
docker ps | grep developerstore-postgres

# 2. Testar conexão
dotnet ef dbcontext info --project DeveloperStore.Infrastructure --startup-project DeveloperStore.Api

# 3. Iniciar API
dotnet run --project DeveloperStore.Api

# 4. Em outro terminal, testar endpoint
curl http://localhost:5079/api/Sales

# 5. Verificar Swagger
# Abrir http://localhost:5079/swagger no navegador
```

## Conclusão

Após seguir este guia:
1. ✅ PostgreSQL estará rodando localmente
2. ✅ Banco de dados e usuário estarão configurados
3. ✅ Endpoints da API funcionarão sem erros de autenticação
4. ✅ Swagger carregará com sucesso
5. ✅ Testes de integração continuarão funcionando como antes

A abordagem recomendada é **Docker PostgreSQL** pela simplicidade e consistência entre ambientes de desenvolvimento.

### 🎯 Próximos Passos
1. Escolha uma opção (Docker recomendado)
2. Siga os passos da configuração
3. Execute os testes de validação
4. Comece a desenvolver com confiança!

**Dica:** Mantenha este guia como referência para outros desenvolvedores da equipe!
