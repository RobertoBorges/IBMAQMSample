using IBM.WMQ;
using System.Collections;

IDictionary<string, object> properties = null;

/// <summary>
/// Fill the following with your information
/// </summary>
string MQ_QUEUE_NAME = "DEV.QUEUE.1";
string MQ_HOST_NAME = "127.0.0.1";
string MQ_PORT_PROPERTY = "1414";
string MQ_CHANNEL_PROPERTY = "DEV.APP.SVRCONN";
string MQ_SSL_CERT_STORE_PROPERTY = "";
string MQ_SSL_CIPHER_SPEC_PROPERTY = "";
string MQ_SSL_PEER_NAME_PROPERTY = "";
Int32 MQ_SSL_RESET_COUNT_PROPERTY = 0;
bool MQ_SSL_REVOCATION_CHECK = false;

Console.WriteLine("Start of SampleIBMMQ Application\n");
try
{
    properties = new Dictionary<string, object>();

    FillMQProperties();
    GetMessages();
}
catch (ArgumentException ex)
{
    Console.WriteLine("Invalid arguments!\n{0}", ex);
}
catch (Exception ex)
{
    Console.WriteLine("Exception caught: {0}", ex);
    Console.WriteLine("Sample execution FAILED!");
}

Console.WriteLine("End of SampleIBMMQ Application");


bool FillMQProperties()
{
    try
    {
        // set the properties
        properties.Add(MQC.HOST_NAME_PROPERTY, (MQ_HOST_NAME != string.Empty) ? MQ_HOST_NAME : "localhost");
        properties.Add(MQC.PORT_PROPERTY, (MQ_PORT_PROPERTY != string.Empty) ? Convert.ToInt32(MQ_PORT_PROPERTY) : 1414);
        properties.Add(MQC.CHANNEL_PROPERTY, (MQ_CHANNEL_PROPERTY != string.Empty) ? MQ_CHANNEL_PROPERTY : "SYSTEM.DEF.SVRCONN");
        properties.Add(MQC.SSL_CERT_STORE_PROPERTY, (MQ_SSL_CERT_STORE_PROPERTY != string.Empty) ? MQ_SSL_CERT_STORE_PROPERTY : "");
        properties.Add(MQC.SSL_CIPHER_SPEC_PROPERTY, (MQ_SSL_CIPHER_SPEC_PROPERTY != string.Empty) ? MQ_SSL_CIPHER_SPEC_PROPERTY : "");
        properties.Add(MQC.SSL_PEER_NAME_PROPERTY, (MQ_SSL_PEER_NAME_PROPERTY != string.Empty) ? MQ_SSL_PEER_NAME_PROPERTY : "");
        properties.Add(MQC.SSL_RESET_COUNT_PROPERTY, MQ_SSL_RESET_COUNT_PROPERTY);
        properties.Add("QueueName", MQ_QUEUE_NAME);
        properties.Add("sslCertRevocationCheck", MQ_SSL_REVOCATION_CHECK);
    }
    catch (Exception e)
    {
        Console.WriteLine("Exeption caught while parsing command line arguments: " + e.Message);
        Console.WriteLine(e.StackTrace);
        return false;
    }

    return true;
}
void DisplayMQProperties()
{
    // display all details
    Console.WriteLine("MQ Parameters");
    Console.WriteLine("1) queueName = " + properties["QueueName"]);
    Console.WriteLine("2) keyRepository = " + properties[MQC.SSL_CERT_STORE_PROPERTY]);
    Console.WriteLine("3) cipherSpec = " + properties[MQC.SSL_CIPHER_SPEC_PROPERTY]);
    Console.WriteLine("4) host = " + properties[MQC.HOST_NAME_PROPERTY]);
    Console.WriteLine("5) port = " + properties[MQC.PORT_PROPERTY]);
    Console.WriteLine("6) channel = " + properties[MQC.CHANNEL_PROPERTY]);
    Console.WriteLine("7) sslPeerName = " + properties[MQC.SSL_PEER_NAME_PROPERTY]);
    Console.WriteLine("8) keyResetCount = " + properties[MQC.SSL_RESET_COUNT_PROPERTY]);
    Console.WriteLine("9) sslCertRevocationCheck = " + properties["sslCertRevocationCheck"]);
    Console.WriteLine("");
}
MQQueueManager CreateQMgrConnection()
{
    // mq properties
    var connectionProperties = new Hashtable {
                    { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED},
                    { MQC.HOST_NAME_PROPERTY, properties[MQC.HOST_NAME_PROPERTY] },
                    { MQC.PORT_PROPERTY, properties[MQC.PORT_PROPERTY] },
                    { MQC.CHANNEL_PROPERTY, properties[MQC.CHANNEL_PROPERTY] }  };

    if ((string)properties[MQC.SSL_CERT_STORE_PROPERTY] != "") connectionProperties.Add(MQC.SSL_CERT_STORE_PROPERTY, properties[MQC.SSL_CERT_STORE_PROPERTY]);
    if ((string)properties[MQC.SSL_CIPHER_SPEC_PROPERTY] != "") connectionProperties.Add(MQC.SSL_CIPHER_SPEC_PROPERTY, properties[MQC.SSL_CIPHER_SPEC_PROPERTY]);
    if ((string)properties[MQC.SSL_PEER_NAME_PROPERTY] != "") connectionProperties.Add(MQC.SSL_PEER_NAME_PROPERTY, properties[MQC.SSL_PEER_NAME_PROPERTY]);
    if ((int)properties[MQC.SSL_RESET_COUNT_PROPERTY] != 0) connectionProperties.Add(MQC.SSL_RESET_COUNT_PROPERTY, properties[MQC.SSL_RESET_COUNT_PROPERTY]);
    if ((bool)properties["sslCertRevocationCheck"] != false) MQEnvironment.SSLCertRevocationCheck = true;

    // Queue Manager name can be passed instead of "" in the MQQueueManager constructor
    return new MQQueueManager("", connectionProperties);
}
void GetMessages()
{
    try
    {
        String queueName = Convert.ToString(properties["QueueName"]);

        DisplayMQProperties();

        // create connection
        Console.Write("Connecting to queue manager.. ");
        using (var queueManager = CreateQMgrConnection())
        {
            Console.WriteLine("done");

            // accessing queue
            Console.Write("Accessing queue " + queueName + ".. ");
            var queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_OUTPUT);
            Console.WriteLine("done");

            Console.WriteLine("Sending a message ");

            var msg = new MQMessage();
            msg.WriteString($"Hello message sent at: {DateTime.Now.ToString()}");
            queue.Put(msg);
            Console.WriteLine("Message sent, press any key to get the message from the queue");
            Console.ReadKey();

            // creating a message object
            var message = new MQMessage();
            try
            {
                queue.Get(message);
                Console.WriteLine($"Message Received. Data = {message.ReadString(message.MessageLength)}");
            }
            catch (MQException mqe)
            {
                if (mqe.ReasonCode == 2033)
                    Console.WriteLine("No messages available at queue");
                else
                    Console.WriteLine("MQException received. Details: {0} - {1}", mqe.ReasonCode, mqe.Message);
            }

            // closing queue
            Console.Write("Closing queue.. ");
            queue.Close();
            Console.WriteLine("done");

            // disconnecting queue manager
            Console.Write("Disconnecting queue manager.. ");
        }
        Console.WriteLine("done");
    }
    catch (MQException mqe)
    {
        Console.WriteLine("");
        Console.WriteLine("MQException received. Details: {0} - {1}", mqe.ReasonCode, mqe.Message);
        Console.WriteLine(mqe.StackTrace);
    }
}
