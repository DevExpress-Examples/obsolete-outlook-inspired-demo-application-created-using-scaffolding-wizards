using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DevExpress.DevAV {
    public abstract class DatabaseObject : IDataErrorInfo {
        /// <summary>
        ///_ Primary key property for all data model objects.
        /// See http://msdn.microsoft.com/en-us/data/jj679962.aspx for more details.
        /// </summary>
        [ScaffoldColumn(false)] //_Indicates that Scaffolding Wizards should not generate UI for editing and displaying this property
        public long Id { get; set; }
        #region IDataErrorInfo
        string IDataErrorInfo.Error { get { return null; } }
        string IDataErrorInfo.this[string columnName] {
            get {
                //_Obtains validation attributes applied to the corresponding property and combines errors provided by them into one. 
                //_Since code generated by Scaffolding Wizards supports the IDataErrorInfo interface out of the box, the error will be displayed in UI.
                //_The Save command will also be disabled until all errors are fixed.
                //_To learn more about IDataErrorInfo support in Scaffolding Wizards, refer to the https://documentation.devexpress.com/#WPF/CustomDocument17157 topic
                return IDataErrorInfoHelper.GetErrorText(this, columnName);
            }
        }
        #endregion
    }
}
