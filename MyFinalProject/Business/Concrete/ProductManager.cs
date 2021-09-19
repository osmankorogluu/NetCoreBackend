using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.CCS;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspect.Autofac.Caching;
using Core.Aspect.Validation;
using Core.CrossCuttingConcerns.Validator;
using Core.Utilities.Results;
using Core.Utilities.Results.Business;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entites.Concrete;
using Entites.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        IProductDal _ProductDal;
        ICategoryService _categoryService;


        public ProductManager(IProductDal productDal, ICategoryService categoryService)
        {
            _ProductDal = productDal;
            _categoryService = categoryService;

        }
       // [SecuredOperation("admin,editor")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult add(Product product)
        {
            IResult result = BusinessRules.Run(CheckIfProductNameExists(product.ProductName),
                CheckIfProductCountpfCategoryCorrect(product.CategoryId), CheckIfCategoryLimitExceded());
            if (result != null)
            {
                return result;
            }

            _ProductDal.Add(product);
            return new SuccessResult(Messages.ProductAdded);

        }
        [CacheAspect]
        public IDataResult<List<Product>> GetAll()
        {
            if (DateTime.Now.Hour == 10)
            {
                return new ErrorDataResult<List<Product>>(Messages.MaintenanceTime);
            }
            //eki çıktılar geliyot hocam
            return new SuccessDataResult<List<Product>>(_ProductDal.GetAll(), Messages.ProductsListed);
        }

        public IDataResult<List<Product>> GetAllByCategoryId(int id)
        {
            return new SuccessDataResult<List<Product>>(_ProductDal.GetAll(p => p.CategoryId == id));
        }
        [CacheAspect]
        
        public IDataResult<Product> GetById(int productId)
        {
            return new SuccessDataResult<Product>(_ProductDal.Get(p => p.ProductId == productId));
        }

        public IDataResult<List<Product>> GetByUnitPrice(decimal min, decimal max)
        {
            return new SuccessDataResult<List<Product>>(_ProductDal.GetAll(p => p.UnitPrice >= min && p.UnitPrice <= max));
        }

        public IDataResult<List<ProductDetailDto>> GetProductDetails()
        {
            return new SuccessDataResult<List<ProductDetailDto>>(_ProductDal.GetProductDetails());
        }
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult update(Product product)
        {
            var result = _ProductDal.GetAll(p => p.CategoryId == product.CategoryId).Count;
            if (result >= 10)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }
            throw new NotImplementedException();
        }
        IDataResult<List<ProductDetailDto>> IProductService.GetProductDetails()
        {
            throw new NotImplementedException();
        }
        private IResult CheckIfProductCountpfCategoryCorrect(int categoryID)
        {
            var result = _ProductDal.GetAll(p => p.CategoryId == categoryID).Count;
            if (result >= 10)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }
            return new SuccessResult();
        }

        private IResult CheckIfProductNameExists(string productName)
        {
            var result = _ProductDal.GetAll(p => p.ProductName == productName).Any();
            if (result)
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }
            return new SuccessResult();
        }
        private IResult CheckIfCategoryLimitExceded()
        {
            var result = _categoryService.GetAll();
            if (result.Data.Count > 15)
            {
                return new ErrorResult(Messages.CategoryLimitExceded);
            }
            return new SuccessResult();
        }

       
        
    }
    
}
