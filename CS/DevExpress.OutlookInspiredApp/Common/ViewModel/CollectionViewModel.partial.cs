using DevExpress.DevAV.Common.DataModel;
using DevExpress.DevAV.ViewModels;

namespace DevExpress.DevAV.Common.ViewModel {
    partial class CollectionViewModel<TEntity, TProjection, TPrimaryKey, TUnitOfWork> : ISupportFiltering<TEntity>
        where TEntity : class
        where TProjection : class
        where TUnitOfWork : IUnitOfWork {
    }
}