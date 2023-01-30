using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mosquitto
{
    public static class ClientAsyncUtility
    {
        private class CaptureState<T> : IDisposable
        {
            private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();
            private readonly CancellationToken _token;
            private readonly Client _client;

            private CancellationTokenRegistration _ctr;

            public CaptureState(Client client, CancellationToken cancellationToken)
            {
                _client = client;
                _token = cancellationToken;
                _ctr = cancellationToken.Register(OnCancel);
            }

            public Task ConnectAsync(string host, int port, int keepalive, string bindAddress)
            {
                _client.onDisconnectedEvent += OnDisconnected;

                _client.Connect(host, port, keepalive, bindAddress, onConnected: OnConnected, onConnectFailed: OnConnectFailed);

                return _tcs.Task;
            }

            public Task<T> SubscribeAsync(string topic, QualityOfService qos)
            {
                _client.onDisconnectedEvent += OnDisconnected;

                DisposeOnError(_client.Subscribe(topic, qos, onSubscribed: OnSubscribed));

                return _tcs.Task;
            }
            public Task<T> SubscribeMultipleAsync(string[] topics, QualityOfService qos)
            {
                _client.onDisconnectedEvent += OnDisconnected;

                DisposeOnError(_client.SubscribeMultiple(topics, qos, onSubscribed: OnSubscribedMultiple));

                return _tcs.Task;
            }

            public Task<T> UnsubscribeAsync(string topic)
            {
                _client.onDisconnectedEvent += OnDisconnected;

                DisposeOnError(_client.Unsubscribe(topic, onUnsubscribed: OnUnsubscribed));

                return _tcs.Task;
            }

            public Task<T> PublishAsync(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain)
            {
                _client.onDisconnectedEvent += OnDisconnected;

                DisposeOnError(_client.Publish(topic, payload, payloadLength, qos, retain, onPublished: OnPublished));

                return _tcs.Task;
            }

            public void Dispose()
            {
                _ctr.Dispose();
                _client.onDisconnectedEvent -= OnDisconnected;
            }

            void DisposeOnError(Error error)
            {
                if (error == Error.Success)
                {
                    return;
                }

                _tcs.TrySetException(new ErrorException(error));
                Dispose();
            }

            void OnConnected()
            {
                _tcs.TrySetResult(default);
                Dispose();
            }

            void OnConnectFailed(Error error, ConnectFailedReason reason)
            {
                if (error == Error.Success)
                {
                    _tcs.TrySetException(new ConnectFailedException(reason));
                }
                else
                {
                    _tcs.TrySetException(new ErrorException(error));
                }
                Dispose();
            }

            void OnSubscribed(QualityOfService qos)
            {
                if (_tcs is TaskCompletionSource<QualityOfService> tcsqos)
                {
                    tcsqos.TrySetResult(qos);
                }
                else
                {
                    _tcs.TrySetResult(default);
                }
                Dispose();
            }

            void OnSubscribedMultiple(QualityOfService[] qosList)
            {
                if (_tcs is TaskCompletionSource<QualityOfService[]> tcsqos)
                {
                    tcsqos.TrySetResult(qosList);
                }
                else
                {
                    _tcs.TrySetResult(default);
                }
                Dispose();
            }

            void OnUnsubscribed()
            {
                _tcs.TrySetResult(default);
                Dispose();
            }

            void OnPublished()
            {
                _tcs.TrySetResult(default);
                Dispose();
            }

            void OnDisconnected(Error error)
            {
                _tcs.TrySetException(new ErrorException(error));
                Dispose();
            }

            void OnCancel()
            {
                _tcs.TrySetCanceled(_token);
                Dispose();
            }
        }

        public static Task ConnectAsync(this Client client, string host, int port = 1883, int keepalive = 60,
            string bindAddress = null, CancellationToken cancellationToken = default)
        {
            return new CaptureState<bool>(client, cancellationToken).ConnectAsync(host, port, keepalive, bindAddress);
        }

        public static Task<QualityOfService> SubscribeAsync(this Client client, string topic,
            QualityOfService qos, CancellationToken cancellationToken = default)
        {
            return new CaptureState<QualityOfService>(client, cancellationToken).SubscribeAsync(topic, qos);
        }

        public static Task<QualityOfService[]> SubscribeMultipleAsync(this Client client, string[] topics,
            QualityOfService qos, CancellationToken cancellationToken = default)
        {
            return new CaptureState<QualityOfService[]>(client, cancellationToken).SubscribeMultipleAsync(topics, qos);
        }

        public static Task UnsubscribeAsync(this Client client, string topic,
            CancellationToken cancellationToken = default)
        {
            return new CaptureState<bool>(client, cancellationToken).UnsubscribeAsync(topic);
        }

        public static Task PublishAsync(this Client client, string topic, byte[] payload, int payloadLength,
            QualityOfService qos, bool retain, CancellationToken cancellationToken = default)
        {
            return new CaptureState<bool>(client, cancellationToken)
                .PublishAsync(topic, payload, payloadLength, qos, retain);
        }

    }
}