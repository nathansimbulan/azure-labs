#r "Newtonsoft.Json"
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
 
public static async Task<IActionResult> Run(HttpRequest request, ILogger log)
{
    log.LogInformation("DetermineLanguage function processed a request.");    
    string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
    dynamic bodyData = JsonConvert.DeserializeObject(requestBody);
    using (HttpClient client = new HttpClient())
    {
        client.BaseAddress = new Uri(System.Environment.GetEnvironmentVariable("EndpointUrl", EnvironmentVariableTarget.Process) + "/", UriKind.Absolute);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", System.Environment.GetEnvironmentVariable("EndpointKey", EnvironmentVariableTarget.Process));
        string postBody = JsonConvert.SerializeObject(
            new
            {
                Documents = new[]
                {
                    new
                    {
                        Id = Guid.NewGuid().ToString().ToLower(),
                        Text = bodyData.text
                    }
                }
            }
        );
        HttpResponseMessage response = await client.PostAsync($"languages", new StringContent(postBody, Encoding.UTF8, "application/json"));
        dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
        string detectedLanguage = result.documents[0].detectedLanguages[0].iso6391Name.ToObject<string>();
        return new OkObjectResult(
            new 
            {
                language = detectedLanguage,
                text = bodyData.text
            }
        );
    }
}
