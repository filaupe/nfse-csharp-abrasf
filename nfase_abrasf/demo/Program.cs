using abrasfV2_03.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using WebService;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;

string idDec = "Dec_" + 1.ToString();
string idRps = "Rps_" + 1.ToString();

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
            Id = idDec,
            Rps = new tcInfRps()
            {
                Id = idRps,
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
    }
};

string cabecMsg = SerializeToXml(cabecalho);
string dadosMsgSemAssinatura = SerializeToXml(envio);

var cert = new X509Certificate2(@"C:\temp\certificado-a1.pfx", "SenhaForte1234!", 
    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

string dadosMsgAssinado = SignXml(dadosMsgSemAssinatura, cert, idDec);

var client = new nfseClient();
var body = new GerarNfseRequestBody(cabecMsg, dadosMsgAssinado);
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

static string SignXml(string xml, X509Certificate2 cert, string idToSign)
{
    var xmlDoc = new XmlDocument { PreserveWhitespace = true };
    xmlDoc.LoadXml(xml);

    var ns = new XmlNamespaceManager(xmlDoc.NameTable);
    ns.AddNamespace("ns", "http://www.abrasf.org.br/nfse.xsd");

    // A referência será o InfDeclaracaoPrestacaoServico
    XmlElement elementToSign = (XmlElement?)xmlDoc.SelectSingleNode("//ns:InfDeclaracaoPrestacaoServico", ns)
        ?? throw new Exception("Elemento InfDeclaracaoPrestacaoServico não encontrado.");

    // Mas o local de inserção será dentro de <Rps>
    XmlElement rpsNode = (XmlElement?)xmlDoc.SelectSingleNode("//ns:Rps", ns)
        ?? throw new Exception("Elemento Rps não encontrado.");

    var signedXml = new SignedXml(xmlDoc)
    {
        SigningKey = cert.GetRSAPrivateKey()
    };

    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
    signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;

    var reference = new Reference($"#{idToSign}")
    {
        DigestMethod = SignedXml.XmlDsigSHA1Url
    };
    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
    reference.AddTransform(new XmlDsigC14NTransform());

    signedXml.AddReference(reference);

    var keyInfo = new KeyInfo();
    keyInfo.AddClause(new KeyInfoX509Data(cert));
    signedXml.KeyInfo = keyInfo;

    signedXml.ComputeSignature();
    XmlElement xmlSignature = signedXml.GetXml();

    // Insere dentro do nó <Rps> após <InfDeclaracaoPrestacaoServico>
    rpsNode.AppendChild(xmlDoc.ImportNode(xmlSignature, true));

    return xmlDoc.OuterXml;
}
