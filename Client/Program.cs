// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine(Environment.OSVersion);
        
        var cert = X509Certificate.CreateFromCertFile("domain.crt");
        var options = new QuicClientConnectionOptions()
        {
            DefaultCloseErrorCode = 1,
            DefaultStreamErrorCode = 2,
            IdleTimeout = TimeSpan.FromMinutes(1),
            MaxInboundBidirectionalStreams = 0,
            MaxInboundUnidirectionalStreams = 0,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { { SslApplicationProtocol.Http3 }},
                ClientCertificates = new X509CertificateCollection() {{cert}},
                RemoteCertificateValidationCallback = ((sender, certificate, chain, errors) => true),
                LocalCertificateSelectionCallback = ((sender, host, certificates, certificate, issuers) => cert),
                EnabledSslProtocols = SslProtocols.Tls,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                TargetHost = "127.0.0.1",
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                AllowRenegotiation = true
            },
            RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5432),
        };
        var connection = await QuicConnection.ConnectAsync(options);
    }
}