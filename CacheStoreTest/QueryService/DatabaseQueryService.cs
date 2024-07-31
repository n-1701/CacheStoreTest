using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheStoreTest.Caching;
using CacheStoreTest.Data;
using CacheStoreTest.Models;
using Microsoft.EntityFrameworkCore;

public class DatabaseQueryService
{
    private readonly InsuranceDbContext dbContext;
    private readonly ICacheService redisCacheService;
    private readonly IGarnetCacheService garnetCacheService;

    public DatabaseQueryService(InsuranceDbContext context, IRedisCacheService redisCacheService, IGarnetCacheService garnetCacheServcie)
    {
        dbContext = context;
        this.redisCacheService = redisCacheService;
        this.garnetCacheService = garnetCacheServcie;
        garnetCacheService.DeleteKey("SQL");
        garnetCacheService.DeleteKey("redis");
        garnetCacheService.DeleteKey("garnet");
    }

    public async Task<string> MeasureQueryTimeAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var data = await dbContext.Claims.ToListAsync();

        stopwatch.Stop();
        string message = $"Data retrieved from SQL database in {stopwatch.ElapsedMilliseconds} ms";
        await garnetCacheService.PushMessage("SQL", message);
        return message;
    }

    public async Task<string> MeasureQueryTimeWithRedisCacheAsync()
    {

        const string cacheKey = "claimsData";
        var stopwatch = new Stopwatch();
        try
        {
            stopwatch.Start();

            // Try to get the data from the cache
            var cachedData = await redisCacheService.GetAsync<List<Claim>>(cacheKey);

            stopwatch.Stop();
            if (cachedData != null)
            {
                string message = $"Data retrieved from redis cache in {stopwatch.ElapsedMilliseconds} ms";
                await garnetCacheService.PushMessage("redis", message);
                return message;
            }

            stopwatch.Reset();
            stopwatch.Start();
            // If not in cache, query the database
            var data = await dbContext.Claims.ToListAsync();
            stopwatch.Stop();
            var queryTime = $"Data retrieved from SQL database in {stopwatch.ElapsedMilliseconds} ms";
            await garnetCacheService.PushMessage("SQL", queryTime);
            // Store the data in the cache
            await redisCacheService.SetAsync(cacheKey, data);

            return queryTime;
        }
        catch (Exception ex)
        {
            return $"Exception occured. Message = {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<string> MeasureQueryTimeWithGarnetCacheAsync()
    {
        const string cacheKey = "claimsData";
        var stopwatch = new Stopwatch();
        try
        {
            stopwatch.Start();

            // Try to get the data from the cache
            var cachedData = await garnetCacheService.GetAsync<List<Claim>>(cacheKey);

            stopwatch.Stop();
            if (cachedData != null)
            {
                string message = $"Data retrieved from garnet cache in {stopwatch.ElapsedMilliseconds} ms";
                await garnetCacheService.PushMessage("garnet", message);
                return message;
            }

            stopwatch.Reset();
            stopwatch.Start();
            // If not in cache, query the database
            var data = await dbContext.Claims.ToListAsync();
            stopwatch.Stop();
            var queryTime = $"Data retrieved from SQL database in {stopwatch.ElapsedMilliseconds} ms";
            await garnetCacheService.PushMessage("SQL", queryTime);
            // Store the data in the cache
            await garnetCacheService.SetAsync(cacheKey, data);

            return queryTime;
        }
        catch (Exception ex)
        {
            return $"Exception occured. Message = {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
        }
    }
    public async Task<List<string>> GetAllQueryTimeAsync()
    {
        string[] sqlMessages = await garnetCacheService.GetAllMessages("SQL");
        string[] redisMessages = await garnetCacheService.GetAllMessages("redis");
        string[] garnetMessages = await garnetCacheService.GetAllMessages("garnet");
        List<string> messages = new List<string>(sqlMessages);
        messages.AddRange(redisMessages);
        messages.AddRange(garnetMessages);
        return messages;
    }
}

