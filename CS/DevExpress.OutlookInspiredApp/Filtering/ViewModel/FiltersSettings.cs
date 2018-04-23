using DevExpress.DevAV.DevAVDbDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.OutlookInspiredApp.Properties;
using DevExpress.Mvvm.ViewModel;
using System;

namespace DevExpress.DevAV.ViewModels {
    internal static class FiltersSettings {
        public static FilterTreeViewModel<Employee, long> GetEmployeesFilterTree(object parentViewModel) {
            return FilterTreeViewModel<Employee, long>.Create(
                new FilterTreeModelPageSpecificSettings<Settings>(Settings.Default, "Status", x => x.EmployeesStaticFilters, x => x.EmployeesCustomFilters, null,
                    new[] {
                        BindableBase.GetPropertyName(() => new Employee().FullName),
                        BindableBase.GetPropertyName(() => new Employee().Id),
                    }),
                CreateUnitOfWork().Employees, (recipient, handler) => RegisterEntityChangedMessageHandler<Employee, long>(recipient, handler)
            ).SetParentViewModel(parentViewModel);
        }
        public static FilterTreeViewModel<Customer, long> GetCustomersFilterTree(object parentViewModel) {
            return FilterTreeViewModel<Customer, long>.Create(
                new FilterTreeModelPageSpecificSettings<Settings>(Settings.Default, "Favorites", x => x.CustomersStaticFilters, x => x.CustomersCustomFilters, null,
                    new[] {
                        BindableBase.GetPropertyName(() => new Customer().Id),
                    }),
                CreateUnitOfWork().Customers, (recipient, handler) => RegisterEntityChangedMessageHandler<Customer, long>(recipient, handler)
            ).SetParentViewModel(parentViewModel);
        }
        public static FilterTreeViewModel<Product, long> GetProductsFilterTree(object parentViewModel) {
            return FilterTreeViewModel<Product, long>.Create(
                new FilterTreeModelPageSpecificSettings<Settings>(Settings.Default, "Category", x => x.ProductsStaticFilters, x => x.ProductsCustomFilters,
                    new[] {
                        BindableBase.GetPropertyName(() => new Product().Id),
                        BindableBase.GetPropertyName(() => new Product().EngineerId),
                        BindableBase.GetPropertyName(() => new Product().SupportId),
                        BindableBase.GetPropertyName(() => new Product().Support),
                    }),
                CreateUnitOfWork().Products, (recipient, handler) => RegisterEntityChangedMessageHandler<Product, long>(recipient, handler)
            ).SetParentViewModel(parentViewModel);
        }
        public static FilterTreeViewModel<Order, long> GetSalesFilterTree(object parentViewModel) {
            return FilterTreeViewModel<Order, long>.Create(
                new FilterTreeModelPageSpecificSettings<Settings>(Settings.Default, "Category", x => x.OrdersStaticFilters, x => x.OrdersCustomFilters,
                    new[] {
                        BindableBase.GetPropertyName(() => new Order().Id),
                        BindableBase.GetPropertyName(() => new Order().CustomerId),
                        BindableBase.GetPropertyName(() => new Order().EmployeeId),
                        BindableBase.GetPropertyName(() => new Order().StoreId),
                    },
                    new[] {
                        BindableBase.GetPropertyName(() => new Order().Customer) + "." + BindableBase.GetPropertyName(() => new Customer().Name),
                    }),
                CreateUnitOfWork().Orders, (recipient, handler) => RegisterEntityChangedMessageHandler<Order, long>(recipient, handler)
            ).SetParentViewModel(parentViewModel);
        }
        public static FilterTreeViewModel<Quote, long> GetOpportunitiesFilterTree(object parentViewModel) {
            return FilterTreeViewModel<Quote, long>.Create(
                new FilterTreeModelPageSpecificSettings<Settings>(Settings.Default, "Category", x => x.QuotesStaticFilters, null, null),
                CreateUnitOfWork().Quotes, (recipient, handler) => RegisterEntityChangedMessageHandler<Quote, long>(recipient, handler)
            ).SetParentViewModel(parentViewModel);
        }

        static IDevAVDbUnitOfWork CreateUnitOfWork() {
            return UnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        }

        static void RegisterEntityChangedMessageHandler<TEntity, TPrimaryKey>(object recipient, Action handler) {
            Messenger.Default.Register<EntityMessage<TEntity, TPrimaryKey>>(recipient, message => handler());
        }

    }
}