# shape2mssql

**shape2mssql** is a small command line utility that allows you to import shape files into MS SQL Server database (spatial data support needed) based on [Catfood.Shapefile project](https://shapefile.codeplex.com). You can run this program this way:

```dos
shape2mssql.exe [filename.shp] [srid] [keyColumnName] [encoding]
```
If no arguments are passed program scans current directory for shape files, processes them and inserts geodata and attribute information to a database with defualt srid, defualt key column name and default encoding. (How to tune these default values see info below)
* If first argument is specifed (file name or file path) shape2mssql processes the only file.
* If first and second arguments are specifed shape2mssql processes the file with spatial reference identifier taken from second param (int value).
* If first second and third arguments are specifed shape2mssql processes the file with supplied srid. Database key fileld name is taken from third param. Key is integer autoincrement value.
* If all parameter are passed program processes the file with supplied srid, creates key column with custom name. The last parameter allows you to specify dbf text data encoding (if there is some problem with it)

Connection string and default params are defined in config-file.

```xml
<connectionStrings>
  <add name="SpatialDb" connectionString="Server=.\SQLEXPRESS;Database=MySpatialDb;Integrated Security=SSPI;" />
</connectionStrings>
<appSettings>
<add key="Srid" value="4326" />
  <add key="KeyFieldName" value="Id" />
  <add key="SourceEncoding" value="cp866" />
  <!--<add key="DestEncoding" value="UTF-8" />-->
</appSettings>
```
SourceEncoding defines the encoding of string data taken from dbf provider.

More information about **shape2mssql** you can find in my blog article: http://vector-sol.ru/blog/3 (in Russian).
