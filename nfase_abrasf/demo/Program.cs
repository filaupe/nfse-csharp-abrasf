using abrasfV2_03.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using WebService;

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
            Id = "Dec_1",
            Rps = new tcInfRps()
            {
                Id = "RPS_1",
                IdentificacaoRps = new tcIdentificacaoRps
                {
                    Numero = "1",
                    Serie = "UNI",
                    Tipo = 1
                },
                DataEmissao = DateTime.Today,
                Status = 1
            },

            Competencia = DateTime.Today,
            Prestador = new tcIdentificacaoPrestador
            {
                CpfCnpj = new tcCpfCnpj
                {
                    Item = "01234567000189",
                    ItemElementName = ItemChoiceType.Cnpj
                },
                InscricaoMunicipal = "123456"
            },

            Servico = new tcDadosServico
            {
                Valores = new tcValoresDeclaracaoServico
                {
                    ValorServicos = 100.00m
                },
                IssRetido = 2,        // 1=Sim, 2=Não
                ItemListaServico = tsItemListaServico.Item0107,
                Discriminacao = "Serviço de consultoria em TI",
                CodigoMunicipio = 3550308, // São Paulo / IBGE
                ExigibilidadeISS = 1         // 1 = Exigível
            },

            OptanteSimplesNacional = 2,        // 1=Sim, 2=Não
            IncentivoFiscal = 2,         // 1=Sim, 2=Não
        },
        Signature = null
    }
};

string cabecMsg = SerializeToXml(cabecalho);
string dadosMsg = SerializeToXml(envio);

var client = new nfseClient();

var body = new GerarNfseRequestBody(cabecMsg, dadosMsg);
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