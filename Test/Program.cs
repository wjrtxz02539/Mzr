// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;


var handler = new HttpClientHandler()
{
    Proxy = new WebProxy("http://89.237.34.190:37647"),
    UseProxy = true
};
try
{
    var client = new HttpClient(handler) { Timeout = new TimeSpan(0, 0, 30) };
    var response = await client.GetStringAsync("http://httpbin.org");
    Console.WriteLine(response);
}
catch (TaskCanceledException ex)
{
    Console.WriteLine(ex.Message);
}
catch (HttpRequestException ex)
{
    Console.WriteLine(ex);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
