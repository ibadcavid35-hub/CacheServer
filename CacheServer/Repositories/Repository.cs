using CacheServer.Context;
using CacheServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer.Repositories;

public class Repository : IRepository
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<bool> AddAsync(Product product)
    {
        if (product == null) return false;
        await _context.Products.AddAsync(product);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deletedProduct = await _context.Products.FirstOrDefaultAsync(x=>x.Id == id);
        if (deletedProduct == null) return false;
        _context.Products.Remove(deletedProduct);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        if (product == null) return false;

        var updatedProduct = await _context.Products.FirstOrDefaultAsync(x=>x.Id == product.Id);

        if (updatedProduct == null) return false;

        updatedProduct.Name = product.Name;
        updatedProduct.Price = product.Price;
        updatedProduct.Stock = product.Stock;
        updatedProduct.ModifiedDate = DateTime.Now;

        _context.Products.Update(updatedProduct);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync(); 
    }
}
