using DevExpress.DevAV.ViewModels;
using DevExpress.Mvvm.DataModel;

namespace DevExpress.DevAV.Common {
    partial class CollectionViewModel<TEntity, TProjection, TPrimaryKey, TUnitOfWork> : ISupportFiltering<TEntity>
        where TEntity : class
        where TProjection : class
        where TUnitOfWork : IUnitOfWork {
    }
}