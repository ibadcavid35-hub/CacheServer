using CacheServer.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer.Repositories;

public class CachedRepository : IRepository
{
    private readonly IRepository _repository;
    private readonly IMemoryCache _memoryCache;
    private const string _SingleProductPrefix = "product_";
    private const string AllProductsKey = "products_all";
    private readonly TimeSpan _duration = TimeSpan.FromMinutes(10);

    public CachedRepository(IRepository repository,IMemoryCache cache)
    {
        _repository = repository;
        _memoryCache = cache;
    }

    public async Task<bool> AddAsync(Product product)
    {
        bool isAdded = await _repository.AddAsync(product);
        if (isAdded)
        {
            _memoryCache.Remove(AllProductsKey);
            Console.WriteLine("New product added.");
        }
        return isAdded;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        bool isDeleted = await _repository.DeleteAsync(id);
        if (isDeleted)
        {
            _memoryCache.Remove(AllProductsKey);
            _memoryCache.Remove($"{_SingleProductPrefix}{id}");
            Console.WriteLine($"Product {id} was deleted.");
        }
        return isDeleted;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        if (!_memoryCache.TryGetValue(AllProductsKey, out List<Product> products))
        {
            Console.WriteLine("[DB] The entire list was not found in the cache. Reading from the database...");
            products = await _repository.GetAllAsync();

            if (products != null)
            {
                _memoryCache.Set(AllProductsKey, products, _duration);
            }
        }
        else
        {
            Console.WriteLine("[CACHE] The entire list was retrieved from the cache.");
        }
        return products;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        string cacheKey = $"{_SingleProductPrefix}{id}";
        if (!_memoryCache.TryGetValue(cacheKey, out Product product))
        {
            Console.WriteLine($"[DB LOG] {id} not found in cache. Reading from the database...");

            product = await _repository.GetByIdAsync(id);

            if (product != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_duration);

                _memoryCache.Set(cacheKey, product, cacheOptions);
            }
        }
        else
        {
            Console.WriteLine($"[CACHE LOG] {id} was retrieved from the cache.");
        }

        return product;
    }



    public async Task<bool> UpdateAsync(Product product)
    {
        bool isUpdated = await _repository.UpdateAsync(product);
        if (isUpdated)
        {
            _memoryCache.Remove(AllProductsKey);
            _memoryCache.Remove($"{_SingleProductPrefix}{product.Id}");
            Console.WriteLine($"Product {product.Id} has been updated.");
        }
        return isUpdated;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _repository.SaveChangesAsync();
    }
}
