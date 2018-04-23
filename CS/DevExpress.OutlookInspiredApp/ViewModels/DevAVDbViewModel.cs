#region #Lesson8
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.DevAV.Common.ViewModel;
using DevExpress.DevAV.DevAVDbDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

namespace DevExpress.DevAV.ViewModels {
    /// <summary>
    /// Represents the root POCO view model for the DevAVDb data model.
    /// </summary>
    public partial class DevAVDbViewModel : DocumentsViewModel<DevAVDbModuleDescription, IDevAVDbUnitOfWork> {
        const string TablesGroup = "Tables";

        /// <summary>
        /// Creates a new instance of DevAVDbViewModel as a POCO view model.
        /// </summary>
        public static DevAVDbViewModel Create() {
            return ViewModelSource.Create(() => new DevAVDbViewModel());
        }

        /// <summary>
        /// Initializes a new instance of the DevAVDbViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the DevAVDbViewModel type without the POCO proxy factory.
        /// </summary>
        protected DevAVDbViewModel()
            : base(UnitOfWorkSource.GetUnitOfWorkFactory()) {
        }

        protected override DevAVDbModuleDescription[] CreateModules() {
            return new DevAVDbModuleDescription[] {
                new DevAVDbModuleDescription("Employees", "EmployeeCollectionView", TablesGroup, FiltersSettings.GetEmployeesFilterTree(this), GetPeekCollectionViewModelFactory(x => x.Employees)),
                new DevAVDbModuleDescription("Customers", "CustomerCollectionView", TablesGroup, FiltersSettings.GetCustomersFilterTree(this), GetPeekCollectionViewModelFactory(x => x.Customers)),
                new DevAVDbModuleDescription("Products", "ProductCollectionView", TablesGroup, FiltersSettings.GetProductsFilterTree(this), GetPeekCollectionViewModelFactory(x => x.Products)),
                new DevAVDbModuleDescription("Sales", "OrderCollectionView", TablesGroup, FiltersSettings.GetSalesFilterTree(this)),
                new DevAVDbModuleDescription("Opportunities", "QuoteCollectionView", TablesGroup, FiltersSettings.GetOpportunitiesFilterTree(this)),
            };
        }

        public virtual FilterPaneVisibility FilterPaneVisibility { get; set; }

        protected override void OnActiveModuleChanged(DevAVDbModuleDescription oldModule) {
            base.OnActiveModuleChanged(oldModule);
            if(ActiveModule != null && ActiveModule.FilterTreeViewModel != null)
                ActiveModule.FilterTreeViewModel.SetViewModel(DocumentManagerService.ActiveDocument.Content);
        }

        IDocumentManagerService SignleObjectDocumentManagerService { get { return this.GetService<IDocumentManagerService>("SignleObjectDocumentManagerService"); } }

        long maxTaskId;

        public override void OnLoaded() {
            base.OnLoaded();
            RegisterJumpList();
            Messenger.Default.Register<EntityMessage<EmployeeTask, long>>(this, OnEmployeeTaskMessage);
            maxTaskId = GetLastTaskId();
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        void RegisterJumpList() {
            IApplicationJumpListService jumpListService = this.GetService<IApplicationJumpListService>();
            jumpListService.Items.AddOrReplace("NewEmployee", NewEmployeeIcon, () => SignleObjectDocumentManagerService.ShowNewEntityDocument<Employee>(this));
            jumpListService.Items.AddOrReplace("Customers", CustomersIcon, () => Show(Modules.Where(m => m.DocumentType == "CustomerCollectionView").First()));
            jumpListService.Items.AddOrReplace("Opportunities", OpportunitiesIcon, () => Show(Modules.Where(m => m.DocumentType == "QuoteCollectionView").First()));
            jumpListService.Apply();
        }
        ImageSource NewEmployeeIcon { get { return new BitmapImage(new Uri("pack://application:,,,/DevExpress.OutlookInspiredApp;component/Resources/icon-new-employee-16.png")); } }
        ImageSource CustomersIcon { get { return new BitmapImage(new Uri("pack://application:,,,/DevExpress.OutlookInspiredApp;component/Resources/Modules/icon-nav-customers-32.png")); } }
        ImageSource OpportunitiesIcon { get { return new BitmapImage(new Uri("pack://application:,,,/DevExpress.OutlookInspiredApp;component/Resources/Modules/icon-nav-opportunities-32.png")); } }

        void OnEmployeeTaskMessage(EntityMessage<EmployeeTask, long> message) {
            if(message.MessageType == EntityMessageType.Added)
                maxTaskId = message.PrimaryKey;
        }

        static long GetLastTaskId() {
            return UnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork().Tasks.Max(x => x.Id);
        }

        void OnTimerTick(object sender, EventArgs e) {
            long currentMaxTaskId = GetLastTaskId();
            if(currentMaxTaskId != maxTaskId) {
                maxTaskId = currentMaxTaskId;
                var task = UnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork().Tasks.FirstOrDefault(x => x.Id == maxTaskId);
                if(task != null) {
                    var notificationService = this.GetRequiredService<INotificationService>();
                    var notification = notificationService.CreatePredefinedNotification(string.Format("New task assigned to {0}", task.AssignedEmployee.FullName), task.Subject, string.Empty);
                    notification
                        .ShowAsync()
                        .ContinueWith(t =>
                    {
                        if(t.Result == NotificationResult.Activated)
                            SignleObjectDocumentManagerService.ShowExistingEntityDocument<EmployeeTask, long>(this, maxTaskId);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }

    public partial class DevAVDbModuleDescription : ModuleDescription<DevAVDbModuleDescription> {
        public DevAVDbModuleDescription(string title, string documentType, string group, IFilterTreeViewModel filterTreeViewModel, Func<DevAVDbModuleDescription, object> peekCollectionViewModelFactory = null)
            : base(title, documentType, group, peekCollectionViewModelFactory) {
            FilterTreeViewModel = filterTreeViewModel;
        }

        public IFilterTreeViewModel FilterTreeViewModel { get; private set; }
    }
    public enum FilterPaneVisibility {
        Minimized,
        Normal,
        Off
    }
}
#endregion #Lesson8