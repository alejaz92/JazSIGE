using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Article;
using CatalogService.Business.Models.Brand;
using CatalogService.Business.Models.GrossIncomeType;
using CatalogService.Business.Models.Line;
using CatalogService.Business.Models.Unit;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class ArticleService : GenericService<Article, ArticleDTO, ArticleCreateDTO>, 
        IArticleService
    {
        private readonly IArticleRepository _repository;
        private readonly IStockServiceClient _stockServiceClient;
        private readonly ILineService _lineService;
        private readonly IBrandService _brandService;
        private readonly IUnitService _unitService;
        private readonly IGrossIncomeTypeService _grossIncomeTypeService;
        public ArticleService(
            IArticleRepository repository, 
            IStockServiceClient stockServiceClient,
            ILineService lineService,
            IBrandService brandService,
            IUnitService unitService,
            IGrossIncomeTypeService grossIncomeTypeService
            ) : base(repository) 
        {
            _repository = repository;
            _lineService = lineService;
            _brandService = brandService;
            _unitService = unitService;
            _grossIncomeTypeService = grossIncomeTypeService;
            _stockServiceClient = stockServiceClient;
        }

        protected override ArticleDTO MapToDTO(Article entity)
        {
            return new ArticleDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                SKU = entity.SKU,
                BrandId = entity.BrandId,
                Brand = entity.Brand.Description,
                LineId = entity.LineId,
                Line = entity.Line.Description,
                LineGroupId = entity.Line.LineGroupId,
                LineGroup = entity.Line.LineGroup.Description,
                UnitId = entity.UnitId,
                Unit = entity.Unit.Description,
                IsTaxed = entity.IsTaxed,
                IVAPercentage = entity.IVAPercentage,
                GrossIncomeTypeId = entity.GrossIncomeTypeId,
                GrossIncomeType = entity.GrossIncomeType.Description,
                Warranty = entity.Warranty,
                IsVisible = entity.IsVisible,
                isActive = entity.IsActive
            };
        }
        protected override Article MapToDomain(ArticleCreateDTO dto)
        {
            return new Article
            {
                Description = dto.Description,
                SKU = dto.SKU,
                BrandId = dto.BrandId,
                LineId = dto.LineId,
                UnitId = dto.UnitId,
                IsTaxed = dto.IsTaxed,
                IVAPercentage = dto.IVAPercentage,
                GrossIncomeTypeId = dto.GrossIncomeTypeId,
                Warranty = dto.Warranty,
                IsVisible = dto.IsVisible
            };
        }
        protected override void UpdateDomain(Article entity, ArticleCreateDTO dto)
        {
            entity.Description = dto.Description;
            entity.SKU = dto.SKU;
            entity.BrandId = dto.BrandId;
            entity.LineId = dto.LineId;
            entity.UnitId = dto.UnitId;
            entity.IsTaxed = dto.IsTaxed;
            entity.IVAPercentage = dto.IVAPercentage;
            entity.GrossIncomeTypeId = dto.GrossIncomeTypeId;
            entity.Warranty = dto.Warranty;
            entity.IsVisible = dto.IsVisible;
        }
        public async Task<bool> IsArticleDescriptionUnique(string Description)
        {
            var articles = await _repository.FindAsync(a => a.Description == Description);
            return !articles.Any();
        }
        public override async Task<string?> ValidateBeforeSave(ArticleCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Article description is mandatory.";
            var isUnique = await IsArticleDescriptionUnique(model.Description);
            if (!isUnique)
                return "Article already exists.";
            return null;
        }
        public async Task<ArticleDTO?> UpdateVisibilityAsync(int id)
        {
            var article = await GetWithIncludes(id);
            if (article == null)
                return null;
            article.IsVisible = !article.IsVisible;
            _repository.Update(article);
            await _repository.SaveChangesAsync();
            return MapToDTO(article);
        }
        protected override Task<IEnumerable<Article>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(
            a => a.Brand,
            a => a.Line,
            a => a.Line.LineGroup,
            a => a.Unit,
            a => a.GrossIncomeType
            );
        protected override Task<Article> GetWithIncludes(int id) => _repository.GetIncludingAsync(
            id,
            a => a.Brand,
            a => a.Line,
            a => a.Line.LineGroup,
            a => a.Unit,
            a => a.GrossIncomeType
            );
        public async Task<bool> ArticleExistsByDescription(string description)
        {
            var articles = await _repository.FindAsync(a => a.Description == description);
            return articles.Any();
        }
        public async Task<bool> ArticleExistsBySKU(string sku)
        {
            var articles = await _repository.FindAsync(a => a.SKU == sku);
            return articles.Any();
        }
        protected override async Task<bool> IsInUseAsync(int id)
        {

            return await _stockServiceClient.HasStockByArticleAsync(id);

        }
        protected override async Task EnsureHierarchyActivationAsync(Article entity)
        {
            // check if line is activated
            LineDTO line = await _lineService.GetByIdAsync(entity.LineId);

            if (!line.IsActive)
                await _lineService.UpdateStatusAsync(line.Id, true);


            BrandDTO brand = await _brandService.GetByIdAsync(entity.BrandId);

            if (!brand.IsActive)
                await _brandService.UpdateStatusAsync(brand.Id, true);


            UnitDTO unit = await _unitService.GetByIdAsync(entity.UnitId);

            if(!unit.IsActive)
                await _unitService.UpdateStatusAsync(unit.Id, true);
           
            GrossIncomeTypeDTO grossIncomeType = await _grossIncomeTypeService.GetByIdAsync(entity.GrossIncomeTypeId);

            if (!grossIncomeType.IsActive)
                await _grossIncomeTypeService.UpdateStatusAsync(grossIncomeType.Id, true);


        }

    }
}
