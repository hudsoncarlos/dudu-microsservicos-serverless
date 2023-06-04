using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SQS.Fornecedor
{
    class Fornecedor
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var client = new AmazonSQSClient(RegionEndpoint.USEast1);
            var request = new SendMessageRequest
            {
                QueueUrl = "https://sqs.us-east-1.amazonaws.com/810748663526/teste",
                MessageBody = "teste 123"
            };

            await client.SendMessageAsync(request);
        }
    }
}
