using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mosquitto.Tests
{
    [TestFixture]
    public class ClientTests 
    {

        private static string _pwd = Environment.CurrentDirectory;
        private static string _slnPath = _pwd.Substring(0, _pwd.IndexOf("Mosquitto-CSharp", StringComparison.InvariantCulture)) + "Mosquitto-CSharp/";

        private static string _mosquittoTestSslPath = _slnPath + "third-party/mosquitto/src/test/ssl";

#if DEBUG
        private static string _mosquittoExePath = _slnPath + "third-party/mosquitto/build/src/Debug/mosquitto.exe";
#else
        private static string _mosquittoExePath = _slnPath + "third-party/mosquitto/build/src/Release/mosquitto.exe";
#endif

        private static byte[] _testPayload = new byte[] { 0, 1, 2, 3, 4, 5 };

        private static int _port = 1883;

        #region Helper Methods

        private class MosquittoServer : IDisposable
        {
            private readonly Process _process;

            public MosquittoServer(int port)
            {
                _process = Process.Start(new ProcessStartInfo(_mosquittoExePath, $"-p {port}")
                {
                  CreateNoWindow = true,
                  UseShellExecute = false
                });
            }

            public void Dispose()
            {
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
            }

        }

        private static CancellationToken CancelAfter(TimeSpan timeSpan)
        {
            return new CancellationTokenSource(timeSpan).Token;
        }

        private static string SslPath(string fileName)
        {
            return Path.Combine(_mosquittoTestSslPath, fileName);
        }
        #endregion

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _port = 1883;
        }

        [SetUp]
        public void Setup()
        {
            // increment the port for each test
            _port++;
        }


        [Test]
        public void CreateClient()
        {
            using (var client = new Client("test")) { }
        }

        [Test]
        public void SetLogCallback()
        {
            using (var client = new Client("test"))
            {
                void LogCallback(LogLevel level, string message)
                {
                    System.Console.WriteLine($"[{level}] {message}");
                }

                client.onLogEvent += LogCallback;
                client.onLogEvent -= LogCallback;
            }
        }

        [Test]
        public void SetUserNameAndPassword()
        {
            using (var client = new Client("test"))
            {
                Assert.AreEqual(Error.Success, client.SetUsernameAndPassword("name", "password"));
            }
        }

        [Test]
        public void SetStringOption()
        {
            using (var client = new Client("test"))
            {
                Assert.AreEqual(Error.Success, client.SetOption(Option.BindAddress, "127.0.0.1"));
                Assert.AreEqual(Error.Inval, client.SetOption(Option.SendMaximum, "127.0.0.1"));
            }
        }

        [Test]
        public void SetIntOption()
        {
            using (var client = new Client("test"))
            {
                Assert.AreEqual(Error.Success, client.SetOption(Option.SendMaximum, 0));
                Assert.AreEqual(Error.Inval, client.SetOption(Option.BindAddress, 0));
            }
        }

        [Test]
        public void SetWill()
        {
            using (var client = new Client("test"))
            {

                Assert.AreEqual(Error.Success, client.SetWill("will", _testPayload, _testPayload.Length, QualityOfService.AtLeastOnce, true));
                Assert.AreEqual(Error.Success, client.ClearWill());
            }
        }

        [Test]
        public void SetCaFile()
        {
            using (var client = new Client("test"))
            {
                Assert.AreEqual(Error.Success, client.SetTls(cafile: SslPath("all-ca.crt")));
            }
        }

        [Test]
        public void SetCaFPath()
        {
            using (var client = new Client("test"))
            {
                Assert.AreEqual(Error.Success, client.SetTls(capath: SslPath("rootCA")));
            }
        }

        [Test]
        public void SetCertificateAndKeyPlain()
        {
            using (var client = new Client("test"))
            {
                Assert.AreEqual(Error.Success, client.SetTls(cafile: SslPath("test-root-ca.crt"), certfile: SslPath("client.crt"), keyfile: SslPath("client.key")));
            }
        }

        
        // Disable this test until the server is set up with TLS
        //[Test]
        public void SetCertificateAndKeyEncrypted()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                byte[] GetPassword() => System.Text.Encoding.Default.GetBytes("password");
                Assert.AreEqual(Error.Success, client.SetTls(cafile: SslPath("test-root-ca.crt"), certfile: SslPath("client-encrypted.crt"), keyfile: SslPath("client-encrypted.key"), getPassword: GetPassword));

                int connectCount = 0;
                void OnConnect()
                {
                    connectCount++;
                }

                int connectFailedCount = 0;
                void OnConnectFailed(Error err, ConnectFailedReason reason)
                {
                    connectFailedCount++;
                    Assert.AreNotEqual(Error.Tls, err);
                }
                client.Connect("127.0.0.1", port, onConnected: OnConnect, onConnectFailed: OnConnectFailed);
                for (int i = 0; i < 100 && connectCount == 0 && connectFailedCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, connectCount, "Expected onConnected to be invoked");
                Assert.AreEqual(0, connectFailedCount, "Expected onConnectFailed not to be invoked");
            }
        }

        // Disable this test until the server is set up with TLS
        //[Test]
        public void SetCertificateAndKeyEncryptedNoPassword()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                Assert.AreEqual(Error.Success, client.SetTls(cafile: SslPath("test-root-ca.crt"), certfile: SslPath("client-encrypted.crt"), keyfile: SslPath("client-encrypted.key")));
                int connectCount = 0;
                void OnConnect()
                {
                    connectCount++;
                }

                int connectFailedCount = 0;
                void OnConnectFailed(Error err, ConnectFailedReason reason)
                {
                    connectFailedCount++;
                    Assert.AreEqual(Error.Tls, err);
                }
                client.Connect("127.0.0.1", port, onConnected: OnConnect, onConnectFailed: OnConnectFailed);
                for (int i = 0; i < 100 && connectCount == 0 && connectFailedCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(0, connectCount, "Expected onConnected not to be invoked");
                Assert.AreEqual(1, connectFailedCount, "Expected onConnectFailed to be invoked");
            }
        }

        // Disable this test until the server is set up with TLS
        //[Test]
        public void SetCertificateAndKeyEncryptedBadPassword()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                byte[] GetIncorrectPassword() => System.Text.Encoding.Default.GetBytes("incorrect");
                Assert.AreEqual(Error.Success, client.SetTls(cafile: SslPath("test-root-ca.crt"), certfile: SslPath("client-encrypted.crt"), keyfile: SslPath("client-encrypted.key"), getPassword: GetIncorrectPassword));
                int connectCount = 0;
                void OnConnect()
                {
                    connectCount++;
                }

                int connectFailedCount = 0;
                void OnConnectFailed(Error err, ConnectFailedReason reason)
                {
                    connectFailedCount++;
                    Assert.AreEqual(Error.Tls, err);
                }
                client.Connect("127.0.0.1", port, onConnected: OnConnect, onConnectFailed: OnConnectFailed);
                for (int i = 0; i < 100 && connectCount == 0 && connectFailedCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(0, connectCount, "Expected onConnected not to be invoked");
                Assert.AreEqual(1, connectFailedCount, "Expected onConnectFailed to be invoked");
            }
        }

        [Test]
        public void Connect()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                int connectCount = 0;
                void OnConnected()
                {
                    connectCount++;
                }
                int connectFailedCount = 0;
                void OnConnectFailed(Error error, ConnectFailedReason reason)
                {
                    connectFailedCount++;
                }

                client.Connect("127.0.0.1", port, onConnected: OnConnected, onConnectFailed: OnConnectFailed);

                for(int i = 0; i < 100 && connectCount == 0 && connectFailedCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, connectCount, "Expected onConnected to be invoked");
                Assert.AreEqual(0, connectFailedCount, "Expected onConnectFailed not to be invoked");
            }
        }

        [Test]
        public async Task ConnectAsync()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public void ConnectTimeout()
        {
            int port = _port;
            using (var client = new Client("test"))
            {
                int connectCount = 0;
                void OnConnected()
                {
                    connectCount++;
                }

                int connectFailedCount = 0;
                void OnConnectFailed(Error error, ConnectFailedReason reason)
                {
                    connectFailedCount++;
                }

                int disconnectedCount = 0;
                void OnDisconnected(Error error)
                {
                    disconnectedCount++;
                }

                client.onDisconnectedEvent += OnDisconnected;

                client.Connect("127.0.0.1", port, keepalive: 10, onConnected: OnConnected, onConnectFailed: OnConnectFailed);

                // Wait to see if we get any event callback for 10 seconds
                for (int i = 0; i < 1000 && connectCount == 0 && connectFailedCount == 0 && disconnectedCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(0, connectCount, "Expected onConnected not to be invoked");
                Assert.AreEqual(1, connectFailedCount, "Expected onConnectFailed to be invoked");
                Assert.AreEqual(0, disconnectedCount, "Expected onDisconnected not to be invoked");
            }
        }

        [Test]
        public async Task ConnectTimeoutAsync()
        {
            int port = _port;
            using (var client = new Client("test"))
            {
                try
                {
                    await client.ConnectAsync("127.0.0.1", port, keepalive: 10, cancellationToken: CancelAfter(TimeSpan.FromSeconds(10)));

                    Assert.Fail("Expected ConnectAsync to result in exception");
                }
                catch(Exception e)
                {
                    Assert.IsAssignableFrom(typeof(ErrorException), e);
                }
            }
        }

        [Test]
        public async Task ReconnectAsync()
        {
            int port = _port;
            using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                await client.DisconnectAsync(cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                await client.ReconnectAsync(cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public async Task RemoteDisconnect()
        {
            int port = _port;
            using (var client = new Client("test"))
            {
                int disconnectCount = 0;
                void OnDisconnected(Error err)
                {
                    disconnectCount++;
                    Assert.AreNotEqual(Error.Success, err);
                }

                client.onDisconnectedEvent += OnDisconnected;

                using (new MosquittoServer(port))
                {
                    await client.ConnectAsync("127.0.0.1", port, keepalive: 10, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
                }

                for (int i = 0; i < 100 && disconnectCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, disconnectCount, "Expected OnDisconnect to be called");
            }
        }

        [Test, TestCase(true), TestCase(false)]
        public async Task SubscribeAfterAutoReconnect(bool clearSession)
        {
            int port = _port;
            using (var client = new Client("test", clearSession, reconnectSettings: new ReconnectSettings(reconnectAutomatically: true, maximumReconnectDelay: 1)))
            {
                int disconnectCount = 0;
                void OnDisconnected(Error err)
                {
                    disconnectCount++;
                }

                client.onDisconnectedEvent += OnDisconnected;

                using (new MosquittoServer(port))
                {
                    await client.ConnectAsync("127.0.0.1", port, keepalive: 10, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
                }

                for (int i = 0; i < 100 && disconnectCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(0, disconnectCount, "Expected OnDisconnect not to be called");


                int subscribeCount = 0;
                void OnSubscribed(QualityOfService qos)
                {
                    subscribeCount++;
                }

                using (new MosquittoServer(port))
                {
                    client.Subscribe("test", QualityOfService.AtLeastOnce, OnSubscribed);

                    for (int i = 0; i < 500 && subscribeCount == 0; i++)
                    {
                        Thread.Sleep(10);
                    }
                }

                Assert.AreEqual(1, subscribeCount, "Expected OnSubscribe to be called exactly once");
                Assert.AreEqual(0, disconnectCount, "Expected OnDisconnect not to be called");
            }
        }

        [Test, TestCase(true), TestCase(false)]
        public async Task SubscribeAfterManualReconnect(bool clearSession)
        {
            int port = _port;
            using (var client = new Client("test", clearSession))
            {
                int disconnectCount = 0;
                void OnDisconnected(Error err)
                {
                    disconnectCount++;
                }

                client.onDisconnectedEvent += OnDisconnected;

                using (new MosquittoServer(port))
                {
                    await client.ConnectAsync("127.0.0.1", port, keepalive: 10, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
                }

                for (int i = 0; i < 100 && disconnectCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, disconnectCount, "Expected OnDisconnect to be called");

                int subscribeCount = 0;
                void OnSubscribed(QualityOfService qos)
                {
                    subscribeCount++;
                }

                using (new MosquittoServer(port))
                {
                    await client.ReconnectAsync(cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                    client.Subscribe("test", QualityOfService.AtLeastOnce, OnSubscribed);

                    for (int i = 0; i < 500 && subscribeCount == 0; i++)
                    {
                        Thread.Sleep(10);
                    }
                }

                Assert.AreEqual(1, subscribeCount, "Expected OnSubscribe to be called exactly once");
            }
        }

        [Test]
        public async Task Subscribe()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(10)));

                int subscribeCount = 0;
                QualityOfService subscribedQos = (QualityOfService)(-1);
                void OnSubscribed(QualityOfService qos)
                {
                    subscribeCount++;
                    subscribedQos = qos;
                }

                Assert.AreEqual(Error.Success, client.Subscribe("test", QualityOfService.AtLeastOnce, OnSubscribed));

                for (int i = 0; i < 100 && subscribeCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, subscribeCount);
                Assert.AreEqual(QualityOfService.AtLeastOnce, subscribedQos);
            }
        }

        [Test]
        public async Task SubscribeAsync()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                var qos = await client.SubscribeAsync("test", QualityOfService.AtLeastOnce, CancelAfter(TimeSpan.FromSeconds(1)));

                Assert.AreEqual(QualityOfService.AtLeastOnce, qos);
            }
        }

        [Test]
        public async Task SubscribeMultiple()
        {
            int port = _port;
            using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(10)));

                int subscribeCount = 0;
                QualityOfService[] subscribedQos = null;
                void OnSubscribedMultiple(QualityOfService[] qos)
                {
                    subscribeCount++;
                    subscribedQos = qos;
                }

                Assert.AreEqual(Error.Success, client.SubscribeMultiple(new string[] { "test", "test2" }, QualityOfService.AtLeastOnce, OnSubscribedMultiple));

                for (int i = 0; i < 100 && subscribeCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, subscribeCount);
                Assert.NotNull(subscribedQos);
                Assert.AreEqual(2, subscribedQos.Length);
                Assert.AreEqual(QualityOfService.AtLeastOnce, subscribedQos[0]);
                Assert.AreEqual(QualityOfService.AtLeastOnce, subscribedQos[1]);
            }
        }

        [Test]
        public async Task SubscribeMultipleAsync()
        {
            int port = _port;
            using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                var qos = await client.SubscribeMultipleAsync(new string[] { "test", "test2" }, QualityOfService.AtLeastOnce, CancelAfter(TimeSpan.FromSeconds(1)));

                Assert.NotNull(qos);
                Assert.AreEqual(2, qos.Length);
                Assert.AreEqual(QualityOfService.AtLeastOnce, qos[0]);
                Assert.AreEqual(QualityOfService.AtLeastOnce, qos[1]);
            }
        }

        [Test]
        public async Task Unsubscribe()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
                await client.SubscribeAsync("test", QualityOfService.AtLeastOnce, CancelAfter(TimeSpan.FromSeconds(1)));

                int unsubscribeCount = 0;
                void OnUnsubscribed()
                {
                    unsubscribeCount++;
                }

                Assert.AreEqual(Error.Success, client.Unsubscribe("test", OnUnsubscribed));

                for (int i = 0; i < 100 && unsubscribeCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, unsubscribeCount);
            }
        }

        [Test]
        public async Task UnsubscribeAsync()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));
                await client.SubscribeAsync("test", QualityOfService.AtLeastOnce, CancelAfter(TimeSpan.FromSeconds(1)));

                await client.UnsubscribeAsync("test", CancelAfter(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public async Task UnsubscribeWithoutSubscribe()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                int unsubscribeCount = 0;
                void OnUnsubscribed()
                {
                    unsubscribeCount++;
                }

                Assert.AreEqual(Error.Success, client.Unsubscribe("test", OnUnsubscribed));

                for (int i = 0; i < 100 && unsubscribeCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, unsubscribeCount);
            }
        }

        [Test]
        public async Task UnsubscribeWithoutSubscribeAsync()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                await client.UnsubscribeAsync("test", CancelAfter(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public async Task Publish()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                int publishCount = 0;
                void OnPublished()
                {
                    publishCount++;
                }

                Assert.AreEqual(Error.Success, client.Publish("test", _testPayload, _testPayload.Length, QualityOfService.AtLeastOnce, false, OnPublished));

                for (int i = 0; i < 100 && publishCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.AreEqual(1, publishCount);
            }
        }

        [Test]
        public async Task PublishAsync()
        {
            int port = _port;
			using (var client = new Client("test"))
            using (new MosquittoServer(port))
            {
                await client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1)));

                await client.PublishAsync("test", _testPayload, _testPayload.Length, QualityOfService.AtLeastOnce, false, CancelAfter(TimeSpan.FromSeconds(1)));
            }
        }


        [Test]
        public async Task SubscribeAndPublishAsync()
        {
            int port = _port;
            using (var client = new Client("test"))
            using (var client2 = new Client("test2"))
            using (new MosquittoServer(port))
            {
                await Task.WhenAll(client.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1))),
                                   client2.ConnectAsync("127.0.0.1", port, cancellationToken: CancelAfter(TimeSpan.FromSeconds(1))));


                int messageReceivedCount = 0;
                void OnReceivedMessage(Message message)
                {
                    messageReceivedCount++;
                    Assert.AreEqual("test_topic", message.topic);
                    Assert.AreEqual(_testPayload, message.payload);
                    Assert.AreEqual(QualityOfService.AtLeastOnce, message.qos);
                }

                client.onMessageReceivedEvent += OnReceivedMessage;
                await client.SubscribeAsync("test_topic", QualityOfService.AtLeastOnce, CancelAfter(TimeSpan.FromSeconds(1)));

                await client2.PublishAsync("test_topic", _testPayload, _testPayload.Length, QualityOfService.AtLeastOnce, false, CancelAfter(TimeSpan.FromSeconds(1)));

                for(int i = 0; i < 100 && messageReceivedCount == 0; i++)
                {
                    Thread.Sleep(10);
                }

                Assert.LessOrEqual(1, messageReceivedCount, "Expected to receive message at least once");
            }
        }
    }
}
