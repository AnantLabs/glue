using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data
{
    public interface IActiveRecord
    {
        /// <summary>
        /// Return the DataProvider for this class
        /// </summary>
        /// <remarks>
        /// The DataProvider can to be declared in the Table-attribute.
        ///
        /// If the DataProvider is not declared, the default DataProvider is initialized
        /// from the element "dataprovider" (which, obviously, has to be there...).
        /// </remarks>
        /// <example>
        /// [Table(DataProvider="dataprovider-account")]
        /// public class Account : ActiveRecord
        /// {
        /// [...]
        /// }
        /// </example>
        IDataProvider Provider { get; }


        /// <summary>
        /// Insert the current record
        /// </summary>
        void Insert();

        /// <summary>
        /// Update the current record
        /// </summary>
        void Update();

        /// <summary>
        /// Delete the current record
        /// </summary>
        void Delete();
    }
}
