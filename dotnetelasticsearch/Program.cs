using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .Authentication(new BasicAuthentication("elastic", "Password&1234"));
var elasticsearchClient = new ElasticsearchClient(settings);
builder.Services.AddSingleton(elasticsearchClient);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/create-index", async (ElasticsearchClient client, string indexName) =>
{
    var response = await client.Indices.CreateAsync(indexName);
    return response;
});
app.MapPost("/index", async (ElasticsearchClient client, string indexName, WeatherForecast document) =>
{
    var response = await client.IndexAsync(document, i => i.Index(indexName));
    return response;
});
app.Run();

public record WeatherForecast
{
    public string Name { get; set; } = null!;
    
    public double Temperature { get; set; }
}