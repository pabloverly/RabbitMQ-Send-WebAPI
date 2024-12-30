using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using WebApplication1.Domain;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public IActionResult InsertOrder(Order order)
        {
            try
            {
                #region Gravar na Fila


                var factory = new ConnectionFactory
                {
                    HostName = "IP_SERVIDOR",
                    Port = 5672, // Porta padrão para conexões AMQP (não HTTP)
                    UserName = "USUARIO", // Substitua pelo seu usuário do RabbitMQ
                    Password = "SENHA" // Substitua pela sua senha do RabbitMQ
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declarar a fila
                channel.QueueDeclare(queue: "hello",
                                     durable: false, //1º true par a fila deve ser persistente (armazerzada em disco)
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // Mensagem a ser enviada
                string message = JsonSerializer.Serialize(order);
                var body = Encoding.UTF8.GetBytes(message);

                //2º Persistir a mensagem (fazer primeiro passo acima durable = true) fila fica 20% mais lenta 
                // var basicProp = channel.CreateBasicProperties();
                // basicProp.Persistent = true;

                // Publicar a mensagem
                channel.BasicPublish(exchange: string.Empty,
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine($" [x] Sent {message}");

                Console.WriteLine(" Press [enter] to exit.");
                //Console.ReadLine();
                #endregion
                return Accepted(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting order");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


    }
}
