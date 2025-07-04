﻿using SalesService.Infrastructure.Repositories;

namespace SalesService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IArticlePriceListRepository ArticlePriceListRepository { get; }

        ISalesQuoteRepository SalesQuoteRepository { get; }
        ISalesQuoteArticleRepository SalesQuoteArticleRepository { get; }
        ISaleRepository SaleRepository { get; }
        ISaleArticleRepository SaleArticleRepository { get; }

        IDeliveryNoteRepository DeliveryNoteRepository { get; }
        IDeliveryNoteArticleRepository DeliveryNoteArticleRepository { get; }



        Task SaveAsync();
    }
}
