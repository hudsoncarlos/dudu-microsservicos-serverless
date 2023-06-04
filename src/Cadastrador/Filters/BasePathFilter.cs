using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cadastrador.Filters
{
    /// <summary>
    /// O filtro de documento BasePath define a propriedade BasePath do Swagger e a remove dos caminhos de URL individuais
    /// </summary>
    public class BasePathFilter : IDocumentFilter
    {
        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="basePath">BasePath para remover das operações</param>
        public BasePathFilter(string basePath)
        {
            BasePath = basePath;
        }

        /// <summary>
        /// Obtém o BasePath do Swagger Doc
        /// </summary>
        /// <returns>O BasePath do Swagger Doc</returns>
        public string BasePath { get; }

        /// <summary>
        /// Aplique o filtro
        /// </summary>
        /// <param name="swaggerDoc">OpenApiDocument</param>
        /// <param name="context">FilterContext</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Servers.Add(new OpenApiServer() { Url = this.BasePath });

            var pathsToModify = swaggerDoc.Paths.Where(p => p.Key.StartsWith(this.BasePath)).ToList();

            foreach (var path in pathsToModify)
            {
                if (path.Key.StartsWith(this.BasePath))
                {
                    string newKey = Regex.Replace(path.Key, $"^{this.BasePath}", string.Empty);
                    swaggerDoc.Paths.Remove(path.Key);
                    swaggerDoc.Paths.Add(newKey, path.Value);
                }
            }
        }
    }
}
