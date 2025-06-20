# nfse-csharp-abrasf

Biblioteca e demonstração em C# para emissão de NFSe segundo o layout ABRASF 2.03.

> Ambiente atual: **Homologação** (sandbox de teste).

## Estrutura do repositório

| Diretório/arquivo | Descrição |
|-------------------|-----------|
| `abrasfV2_03/` | Classes geradas a partir do XSD oficial 2.03. |
| `demo/` | Projeto console de demonstração que consome as classes. |
| `demo/secrets.json` | **NÃO VAI PARA O GIT** – armazena dados sensíveis. |
| `demo/secrets.example.json` | Exemplo de como preencher os segredos. |

## Configuração rápida

1. Copie o arquivo de exemplo:
   ```bash
   cp demo/secrets.example.json demo/secrets.json
   ```
2. Preencha em `demo/secrets.json` seus dados reais:
   * `Cnpj` – CNPJ do prestador.
   * `InscricaoMunicipal` – Inscrição municipal do prestador.
   * `RazaoSocial` – Razão social da empresa.
   * `Certificado.Arquivo` – Caminho para o seu certificado `*.pfx`.
   * `Certificado.Senha` – Senha do certificado.
3. **Não commit** o arquivo `secrets.json`. Ele já está listado no `.gitignore`.

## Executando a demo

```bash
# Na raiz do repositório
dotnet restore
cd demo
dotnet run
```
A aplicação gera uma requisição `GerarNfse` com dados fictícios e imprime a resposta XML da prefeitura em homologação.

## Observações importantes

* A biblioteca **não** está preparada para produção – use somente para testes/homologação.
* Caso deseje apontar para produção, verifique URLs, certificados e políticas da sua prefeitura.
* Pull requests são bem-vindos!
