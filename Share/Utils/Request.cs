using Microsoft.Extensions.Logging;
using Mzr.Share.Interfaces;
using Mzr.Share.Models.ProxyPool;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mzr.Share.Utils
{
    public class RequestStatus
    {
        public string Url { get; set; } = null!;
        public int FailedCount { get; set; } = 1;
        public string LastError { get; set; } = string.Empty;
        public bool Dead { get; set; } = false;
        public DateTime AddTime { get; set; } = DateTime.UtcNow;
        public DateTime LastTime { get; set; } = DateTime.UtcNow;
    }
    public class Request : IDisposable
    {
        private readonly IProxyPool proxyPool;
        private readonly ILogger logger;

        public static readonly ConcurrentDictionary<string, RequestStatus> FailedRequests = new();

        public Request(IProxyPool proxyPool, ILogger<Request> logger)
        {
            this.logger = logger;
            this.proxyPool = proxyPool;
        }

        public async Task<HttpResponseMessage?> GetAsync(string url, Dictionary<string, string>? headers = null, int retryCount = 100, int timeout = 10, bool autoHttps = false)
        {
            var count = 0;
            var logLevel = LogLevel.Debug;
            var success = false;
            var urlKey = url.Contains("https://") ? url : url.Replace("http://", "https://");
            string error = string.Empty;
            do
            {
                if (count > 100)
                    logLevel = LogLevel.Warning;

                Proxy? proxy = null;
                try
                {
                    proxy = await proxyPool.GetProxy(60);
                    if (proxy == null)
                    {
                        error = $"Proxy is null.";
                    }
                    else
                    {

                        var httpHandler = new HttpClientHandler()
                        {
                            Proxy = new WebProxy(proxy.Url),
                            UseProxy = true
                        };
                        if (proxy.Headers.Count > 0)
                        {
                            if (headers == null)
                                headers = proxy.Headers;
                            else
                                foreach (var key in proxy.Headers.Keys)
                                    headers.Add(key, proxy.Headers[key]);
                        }
                        using var httpClient = new HttpClient(httpHandler) { Timeout = new TimeSpan(hours: 0, minutes: 0, seconds: timeout) };
                        AddHeader(httpClient, headers);
                        if (proxy.IsHttps && autoHttps)
                            url = url.Replace("http://", "https://");
                        var result = await httpClient.GetAsync(url);
                        if (result != null)
                        {
                            success = true;
                            return result;
                        }
                        else
                            error = $"[{count}/{retryCount}][{proxy.Url}] GetAsync request is null.";
                    }
                }
                catch (TaskCanceledException)
                {
                    error = $"[{count}/{retryCount}][{proxy?.Url}] Get request cancelled.";
                    logger.Log(logLevel, error);
                    await Task.Delay(1000);
                }
                catch (HttpRequestException)
                {
                    error = $"[{count}/{retryCount}][{proxy?.Url}] Get request failed by HttpRequestException.";
                    logger.Log(logLevel, error);
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    error = $"[{count}/{retryCount}][{proxy?.Url}] Get request failed by {ex.GetType()}. {ex}";
                    logger.Log(logLevel, error);
                    await Task.Delay(1000);
                }
                finally
                {
                    count++;
                    if (success == true)
                    {
                        if (count > 1)
                            FailedRequests.TryRemove(urlKey, out RequestStatus _);
                    }
                    else
                    {
                        if (FailedRequests.TryGetValue(urlKey, out RequestStatus? value) && value != null)
                        {
                            value.FailedCount++;
                            value.LastError = error;
                            value.LastTime = DateTime.UtcNow;
                        }
                        else
                            FailedRequests.TryAdd(urlKey, new RequestStatus() { FailedCount = count, LastError = error, Url = urlKey });
                    }
                }
            } while (count < retryCount);

            logger.LogCritical("Failed to Get {url} within {retryCount} tries.", url, retryCount);
            if (FailedRequests.TryGetValue(urlKey, out RequestStatus? requestStatus) && requestStatus != null)
            {
                requestStatus.Dead = true;
                requestStatus.LastTime = DateTime.UtcNow;
            }
            else
                FailedRequests.TryAdd(urlKey, new RequestStatus() { Dead = true, LastError = error, Url = url });

            return null;
        }

        public async Task<string?> GetStringAsync(string url, Dictionary<string, string>? headers = null, int retryCount = 100, int timeout = 10, bool autoHttps = false)
        {
            var response = await GetAsync(url: url, headers: headers, retryCount: retryCount, timeout: timeout, autoHttps: autoHttps);
            if (response == null)
            {
                logger.LogError("GetStringAsync return null response for url: {url}.", url);
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<Stream?> GetStreamAsync(string url, Dictionary<string, string>? headers = null, int retryCount = 100, int timeout = 10, bool autoHttps = false)
        {
            var response = await GetAsync(url: url, headers: headers, retryCount: retryCount, timeout: timeout, autoHttps: autoHttps);
            if (response == null)
            {
                logger.LogError("GetStreamAsync return null response for url: {url}.", url);
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<TValue?> GetFromJsonAsync<TValue>(string url, Dictionary<string, string>? headers = null, int retryCount = 1000, int timeout = 10, Func<Stream, string>? responseFunc = null, bool autoHttps = false) where TValue : class
        {
            var count = 0;
            var logLevel = LogLevel.Debug;
            do
            {
                if (count > 100)
                    logLevel = LogLevel.Warning;
                try
                {
                    if (await GetStreamAsync(url, headers, retryCount, timeout, autoHttps: autoHttps) is Stream data)
                    {
                        if (responseFunc != null)
                            data = new MemoryStream(Encoding.UTF8.GetBytes(responseFunc(data)));
                        var result = await JsonSerializer.DeserializeAsync<TValue>(data);
                        if (result == null)
                            throw new Exception("GetFromJsonAsync parse result is null.");
                        return result;
                    }
                    else
                        throw new Exception("GetFromJsonAsync response is null.");
                }
                catch (Exception ex)
                {
                    logger.Log(logLevel, "[{count}/{retryCount}] Failed to GetFromJsonAsync {url}. {ex}", count, retryCount, url, ex.Message);
                    await Task.Delay(1000);
                }
                finally { count++; }
            } while (count < retryCount);

            logger.LogCritical("Failed to GetFromJsonAsync {url} within {retryCount} tries.", url, retryCount);
            return null;
        }

        public void Dispose()
        {
        }

        private void AddHeader(HttpClient client, Dictionary<string, string>? headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }
    }
}
