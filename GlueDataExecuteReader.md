# ExecuteReader #
`IDataReader ExecuteReader(IDbCommand command);`<br>
<code>IDataReader ExecuteReader(string commandText, params object[] paramNameValueList);</code>

The Glue ExecuteReader methods return an open IDataReader. Use it if you have created a custom SQL query or an IDbCommand for which the resulting data is not scalar and cannot be mapped to objects using Glue.Mapping.<br>
Readers should be closed after use. The easiest way is with a "using" statement.<br>
<br>
<pre><code>// Using an IDbCommand<br>
IDbCommand command = DataProvider.Current.CreateSelectCommand(<br>
        "Contacts",             // table <br>
        "Id,DisplayName",       // columns<br>
        null,                   // filter<br>
        "-DisplayName,+Id",     // order<br>
        Limit.Range(100,110)    // limit<br>
    );<br>
using (IDataReader reader = DataProvider.Current.ExecuteReader(command))<br>
    while (reader.Read())<br>
        Console.WriteLine(reader["Id"]);<br>
<br>
// Using a custom query<br>
using (IDataReader reader = DataProvider.Current.ExecuteReader("SELECT * FROM Contacts"))<br>
    while (reader.Read())<br>
        Console.WriteLine(reader[0]);<br>
<br>
</code></pre>