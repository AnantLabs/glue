using System;

namespace Glue.Data.Schema
{
	/// <summary>
	/// Summary description for ImportMode.
	/// </summary>
	public enum ImportMode
	{
        Update,         // Import existing and new rows
        Freshen,        // Only import existing rows
        Incremental//,    // Only import new rows
        //Replace         // Delete old rows, import new ones
	}

    public enum ExportFormat
    {
        Xml,
        Sql
    }
}
