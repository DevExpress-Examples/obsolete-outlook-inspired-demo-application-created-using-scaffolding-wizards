using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.DevAV.DevAVDbDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.ViewModel;
using DevExpress.Mvvm.DataAnnotations;

namespace DevExpress.DevAV.ViewModels {
    /// <summary>
    /// Represents the root POCO view model for the DevAVDb data model.
    /// </summary>
    public partial class DevAVDbViewModel : DocumentsViewModel<DevAVDbModuleDescription, IDevAVDbUnitOfWork> {

        const string TablesGroup = "Tables";

        INavigationService NavigationService { get { return this.GetService<INavigationService>(); } }

        /// <summary>
        /// Creates a new instance of DevAVDbViewModel as a POCO view model.
        /// </summary>
        public static DevAVDbViewModel Create() {
            return ViewModelSource.Create(() => new DevAVDbViewModel());
        }

        static DevAVDbViewModel() {
            MetadataLocator.Default = MetadataLocator.Create().AddMetadata<DevAVDbMetadataProvider>();
        }
        /// <summary>
        /// Initializes a new instance of the DevAVDbViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the DevAVDbViewModel type without the POCO proxy factory.
        /// </summary>
        protected DevAVDbViewModel()
            : base(UnitOfWorkSource.GetUnitOfWorkFactory()) {
            NavigationPaneVisibility = NavigationPaneVisibility.Normal;
        }

        protected override DevAVDbModuleDescription[] CreateModules() {
            return new DevAVDbModuleDescription[] {
                new DevAVDbModuleDescription("Customers", "CustomerCollectionView", TablesGroup, FiltersSettings.GetCustomersFilterTree(this), GetPeekCollectionViewModelFactory(x => x.Customers)),
                new DevAVDbModuleDescription("Orders", "OrderCollectionView", TablesGroup, FiltersSettings.GetSalesFilterTree(this)),
                new DevAVDbModuleDescription("Employees", "EmployeeCollectionView", TablesGroup, FiltersSettings.GetEmployeesFilterTree(this), GetPeekCollectionViewModelFactory(x => x.Employees)),
                new DevAVDbModuleDescription("Tasks", "EmployeeTaskCollectionView", TablesGroup, null, GetPeekCollectionViewModelFactory(x => x.Tasks)),
                new DevAVDbModuleDescription("Opportunities", "QuoteCollectionView", TablesGroup, FiltersSettings.GetOpportunitiesFilterTree(this)),
                new DevAVDbModuleDescription("Products", "ProductCollectionView", TablesGroup, FiltersSettings.GetProductsFilterTree(this), GetPeekCollectionViewModelFactory(x => x.Products)),
            };
        }

        protected override void OnActiveModuleChanged(DevAVDbModuleDescription oldModule) {
            if(ActiveModule != null) {
                NavigationService.ClearNavigationHistory();
            }
            base.OnActiveModuleChanged(oldModule);
            if(ActiveModule != null && ActiveModule.FilterTreeViewModel != null)
                ActiveModule.FilterTreeViewModel.SetViewModel(DocumentManagerService.ActiveDocument.Content);
        }

        IDocumentManagerService SignleObjectDocumentManagerService { get { return this.GetService<IDocumentManagerService>("SignleObjectDocumentManagerService"); } }

        long maxTaskId;

        public override void OnLoaded(DevAVDbModuleDescription module) {
            base.OnLoaded(module);
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
            jumpListService.Items.AddOrReplace("NewEmployee", GetJumpListIcon("icon-new-employee-16"), () => SignleObjectDocumentManagerService.ShowNewEntityDocument<Employee>(this));
            jumpListService.Items.AddOrReplace("Customers", GetJumpListIcon("Modules/icon-nav-customers-32"), () => Show(Modules.Where(m => m.DocumentType == "CustomerCollectionView").First()));
            jumpListService.Items.AddOrReplace("Opportunities", GetJumpListIcon("Modules/icon-nav-opportunities-32"), () => Show(Modules.Where(m => m.DocumentType == "QuoteCollectionView").First()));
            jumpListService.Apply();
        }

        ImageSource GetJumpListIcon(string iconName) {
            return new BitmapImage(new Uri(string.Format("pack://application:,,,/DevExpress.OutlookInspiredApp;component/Resources/{0}.png", iconName)));
        }

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
}