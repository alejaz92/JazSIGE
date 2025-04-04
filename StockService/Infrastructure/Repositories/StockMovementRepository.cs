﻿using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Repositories
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly StockDbContext _context;

        public StockMovementRepository(StockDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StockMovement stockMovement)
        {
            await _context.StockMovements.AddAsync(stockMovement);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByArticleAsync(int articleId) => await _context.StockMovements
                .Where(sm => sm.ArticleId == articleId)
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();

        public async Task<IEnumerable<StockMovement>> GetAllAsync() => await _context.StockMovements
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();


    }
}
