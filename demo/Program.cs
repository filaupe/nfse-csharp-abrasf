using abrasfV2_03.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using WebService;
using System.Text.Json;

var secretsPath = Path.Combine(AppContext.BaseDirectory, "secrets.json");
if (!File.Exists(secretsPath))
{
    Console.Error.WriteLine($"Arquivo de segredos não encontrado: {secretsPath}");
    return;
}

var secrets = JsonSerializer.Deserialize<Secrets>(File.ReadAllText(secretsPath));
if (secrets is null)
{
    Console.Error.WriteLine("Falha ao ler secrets.json.");
    return;
}

var cabecalho = new cabecalho
{
    versao = "2.03",
    versaoDados = "2.03",
};
var envio = new GerarNfseEnvio
{
    Rps = new tcDeclaracaoPrestacaoServico()
    {
        InfDeclaracaoPrestacaoServico = new tcInfDeclaracaoPrestacaoServico()
        {
            Rps = new tcInfRps()
            {
                IdentificacaoRps = new tcIdentificacaoRps
                {
                    Numero = "5",
                    Serie = "UNI",
                    Tipo = 1
                },
                DataEmissao = DateTime.Today,
                Status = 1,
            },

            Competencia = DateTime.Today,
            Prestador = new tcIdentificacaoPrestador
            {
                CpfCnpj = new tcCpfCnpj
                {
                    Item = secrets!.Cnpj,
                    ItemElementName = ItemChoiceType.Cnpj,
                },
                InscricaoMunicipal = secrets!.InscricaoMunicipal
            },

            Servico = new tcDadosServico
            { 
                Valores = new tcValoresDeclaracaoServico
                {
                    ValorServicos = 100.00m,

                    // ▶ Alíquota obrigatória:
                    Aliquota = 2m,           
                    AliquotaSpecified = true,

                    // ▶ Valor ISS – Barretos exige declarar mesmo para Simples.
                    //   Se seu contador disser que deve estar zerado, use 0m.
                    ValorIss = 2.00m,           // 100 × 2 %  (exemplo)
                    ValorIssSpecified = true,
                },

                IssRetido = 2,
                ItemListaServico = tsItemListaServico.Item0501,
                Discriminacao = "Serviço de consultoria em TI",
                CodigoMunicipio = 3550308,
                ExigibilidadeISS = 1
            },

            OptanteSimplesNacional = 1,        // 1=Sim, 2=Não
            IncentivoFiscal = 2,         // 1=Sim, 2=Não

            Tomador = new tcDadosTomador
            {
                IdentificacaoTomador = new tcIdentificacaoTomador
                {
                    CpfCnpj = new tcCpfCnpj
                    {
                        Item = "12345678000195",    // CNPJ do tomador
                        ItemElementName = ItemChoiceType.Cnpj
                    },
                },
                RazaoSocial = "Empresa Cliente Ltda",
                Endereco = new tcEndereco
                {
                    Endereco = "Rua das Flores",
                    Numero = "100",
                    Complemento = "Sala 45",
                    Bairro = "Centro",
                    CodigoMunicipio = 3550308,    // São Paulo
                    CodigoMunicipioSpecified = true,
                    Uf = tsUf.SP,
                    UfSpecified = true,
                    Cep = "01234567",
                    CodigoPais = "1058"    // Brasil
                },
                Contato = new tcContato
                {
                    Telefone = "11987654321",
                    Email = "contato@empresacliente.com.br"
                }
            },
        },
    }
};

string cabecMsg = SerializeToXml(cabecalho);
string dadosMsgSemAssinatura = SerializeToXml(envio);

var client = new nfseClient();
var body = new GerarNfseRequestBody(cabecMsg, dadosMsgSemAssinatura);
var request = new GerarNfseRequest(body);

var response = await client.GerarNfseAsync(request);
Console.WriteLine(response.Body.outputXML);

static string SerializeToXml<T>(T obj)
{
    var settings = new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(false),
        OmitXmlDeclaration = true,
        Indent = false
    };

    var ns = new XmlSerializerNamespaces();
    ns.Add(string.Empty, "http://www.abrasf.org.br/nfse.xsd");

    var serializer = new XmlSerializer(typeof(T));

    using var sb = new StringWriter();
    using var xw = XmlWriter.Create(sb, settings);
    serializer.Serialize(xw, obj, ns);

    return sb.ToString();
}

class Secrets
{
    public string Cnpj { get; set; } = string.Empty;
    public string InscricaoMunicipal { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public Certificado Certificado { get; set; } = new();
}

class Certificado
{
    public string Arquivo { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
