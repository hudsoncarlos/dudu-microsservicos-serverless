using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Compartilhado;
using Compartilhado.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Coletor
{
    public class Function
    {
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            foreach (var record in dynamoEvent.Records)
            {
                if (record.EventName == "INSERT")
                {
                    var pedido = record.Dynamodb.NewImage.ToObject<Pedido>();
                    pedido.Status = StatusDoPedido.Coletado;

                    try
                    {
                        await ProcessarValorDoPedido(pedido);
                        context.Logger.LogLine($"Sucesso na coleta do pedido: '{pedido.Id}'");
                        // Adicionar na fila de pedido.
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"Erro: '{ex.Message}'");
                        pedido.JustificativaDeCancelamento = ex.Message;
                        pedido.Cancelado = true;
                        // Adicionar à fila de falha.
                    }

                    await pedido.SalvarAsync();
                }
            }
        }

        private async Task ProcessarValorDoPedido(Pedido pedido)
        {
            foreach (var produto in pedido.Produtos)
            {
                var produtoDoEstoque = await ObterProdutoDoDynamoDBAsync(produto.Id);
                if (produtoDoEstoque == null) throw new InvalidOperationException($"Produto não encontrado na tabela estoque. {produto.Id}");

                produto.Valor = produtoDoEstoque.Valor;
                produto.Nome = produtoDoEstoque.Nome;
            }

            var valorTotal = pedido.Produtos.Sum(x => x.Valor * x.Quantidade);
            if (pedido.ValorTotal != 0 && pedido.ValorTotal != valorTotal)
                throw new InvalidOperationException($"O valor esperado do pedido é de R$ {pedido.ValorTotal} e o valor verdadeiro é R$ {valorTotal}");

            pedido.ValorTotal = valorTotal;
        }

        private async Task<Produto> ObterProdutoDoDynamoDBAsync(string id)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
            var request = new QueryRequest
            {
                TableName = "estoque",
                KeyConditionExpression = "Id = :v_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":v_id", new AttributeValue { S = id } } }
            };

            var response = await client.QueryAsync(request);
            var item = response.Items.FirstOrDefault();
            if (item == null) return null;
            return item.ToObject<Produto>();
        }
    }
}