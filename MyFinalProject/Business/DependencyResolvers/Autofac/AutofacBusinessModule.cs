using Autofac;
using Business.Concrete;
using Business.Abstract;
using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.Concrete.EntityFramework;
using DataAccess.Abstract;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;

using static Core.Utilities.Interceptors.MethodInterception;
using Business.CCS;
using static Core.Utilities.Security.JWT.JWT;
using Core.Utilities.Security.JWT;
using Microsoft.AspNetCore.Http;

namespace Business.DependencyResolvers.Autofac
{
    public class AutofacBusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProductManager>().As<IProductService>().SingleInstance();
            builder.RegisterType<EfProductDal>().As<IProductDal>().SingleInstance();
            // bu satır doğru mu? CategoryManager ICategoryDal'dan mı türeyecek?
            // hazır aldım doğru çalıştır düzelecek 
            builder.RegisterType<CategoryManager>().As<ICategoryService>().SingleInstance();
            builder.RegisterType<EfCategoryDal>().As<ICategoryDal>().SingleInstance();

            builder.RegisterType<UserManager>().As<IUserService>();
            builder.RegisterType<EfUserDal>().As<IUserDal>();

            builder.RegisterType<AuthManager>().As<IAuthService>();
            builder.RegisterType<JwtHelper>().As<ITokenHelper>();

          

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();
        }
    }
}
