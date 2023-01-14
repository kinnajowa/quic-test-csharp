// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Server;

public static class Program
{
    public static async Task Main()
    {
        var cert = X509Certificate.CreateFromCertFile("domain.crt");
        var options = new QuicListenerOptions()
        {
            ListenEndPoint = new IPEndPoint(IPAddress.Any, 5432),
            ApplicationProtocols = new List<SslApplicationProtocol>() {{SslApplicationProtocol.Http3}},
            ConnectionOptionsCallback = (async (quicConnection, info, arg3) =>
            {
                var options = new QuicServerConnectionOptions()
                {
                    DefaultCloseErrorCode = 1,
                    DefaultStreamErrorCode = 2,
                    IdleTimeout = TimeSpan.FromMinutes(1),
                    MaxInboundBidirectionalStreams = 1,
                    MaxInboundUnidirectionalStreams = 0,
                    ServerAuthenticationOptions = new SslServerAuthenticationOptions()
                    {
                        ApplicationProtocols = new List<SslApplicationProtocol>() { { SslApplicationProtocol.Http3 } },
                        ClientCertificateRequired = true,
                        RemoteCertificateValidationCallback = ((sender, certificate, chain, errors) => true),
                        EnabledSslProtocols = SslProtocols.Tls,
                        ServerCertificate = cert,
                        ServerCertificateSelectionCallback = ((sender, name) => cert),
                        EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                        AllowRenegotiation = true
                    }
                };
                return options;
            })
        };
        var listener = await QuicListener.ListenAsync(options);
        var connection = await listener.AcceptConnectionAsync();
        Console.WriteLine("new connection");
    }
}