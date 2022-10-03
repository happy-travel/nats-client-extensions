using System.Linq;
using NATS.Client.JetStream;

namespace NATS.Client
{
    internal class NatsClientConnectionFactoryDecorator : INatsClientConnectionFactory
    {
        private readonly ConnectionFactory _connectionFactory;

        public NatsClientConnectionFactoryDecorator(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IJetStream CreateJetStreamContext(Options options, params string[] streamNames)
        {
            var connection = _connectionFactory.CreateConnection(options);
            var jm = connection.CreateJetStreamManagementContext();
            var existedStreams = jm.GetStreamNames();
            
            foreach(var streamName in streamNames.Except(existedStreams))
                CreateStream(jm, streamName);

            return connection.CreateJetStreamContext();
        }


        private void CreateStream(IJetStreamManagement streamManagement, string streamName)
        {
            streamManagement.AddStream(new StreamConfiguration.StreamConfigurationBuilder()
                .WithName(streamName)
                .WithStorageType(StorageType.File)
                .AddSubjects(streamName)
                .Build());
        }
    }
}