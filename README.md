- title : F# Type Provider
- description : Short Introduction to F# Type Provider
- author : Andreas Maier
- theme : night
- transition : default

***

### Lets type the world

- Type providers offer you a way of working with data in highly information rich, strong typed manner with a few lines of code
- ORM at its best ;-)

***

### One main task in our day job is to read and process data

***

### How is the general way to do that?

1. <b>Write code or use an existing lib to access the data source (e.g. a database (sql or nosql), file, web site, REST service, ...)</b>
- <b>Create data structures / a domain model, that will represent your world in your code</b>
- <b>Read the data from your source</b>
- <b>Transform the data into your typed data structures</b>
- Work with your data structures

***

### FSharp Type Provider

- A Design-time component that provides a computed space of types and methods... 
- An adaptor between the data / services and .NET languages...
- Is no static code generator, but targets in the same area
- Uses the <b>F# Compiler</b> to generate types, properties and methods based on meta information of the data sources 
- Lift type inference to a usage maximum => inferes types from the data sources
- A type provider can also ensure that groups of types are only expanded on demand. This allows on-demand integration of large-scale information spaces in a strongly typed way.

***

### FSharp Type Providers in action

#### CSV Type Provider

    Date,Open,High,Low,Close,Volume,Adj Close
    2012-01-27,29.45,29.53,29.17,29.23,44187700,29.23
    2012-01-26,29.61,29.70,29.40,29.50,49102800,29.50
    2012-01-25,29.07,29.65,29.07,29.56,59231700,29.56
    2012-01-24,29.47,29.57,29.18,29.34,51703300,29.34

---

#### CSV Type Provider

    type Stocks = CsvProvider<"../data/MSFT.csv">
    // Download the stock prices
    let msft = Stocks.Load("http://ichart.finance.yahoo.com/table.csv?s=MSFT")

    // Look at the most recent row. Note the 'Date' property
    // is of type 'DateTime' and 'Open' has a type 'decimal'
    let firstRow = msft.Rows |> Seq.head
    let lastDate = firstRow.Date
    let lastOpen = firstRow.Open

    // Print the prices in the HLOC format
    for row in msft.Rows do
    printfn "HLOC: (%A, %A, %A, %A)" row.High row.Low row.Open row.Close

***

#### Excel & SQL Type Provider

    type EngagementBookingFile = ExcelFile<"EngagementBookingFile.xlsx", "A6:N1000">
    
    [<Literal>]
    let connectionString = @"Data Source=...\...;Initial Catalog=...;Integrated Security=True"
    type EngagementTrackingDb = SqlProgrammabilityProvider<connectionString>
    
    // Booking DAL
    type Booking = EngagementTrackingDb.dbo.``User-Defined Table Types``.BookingTableType
    type UpdateBookings = EngagementTrackingDb.dbo.stUpdateBookings

    let readBookingExcel month (engagementCode: int) filePath =
        // Let the type provider do it's work
        let file = new EngagementBookingFile(filePath)
        let filteredData = file.Data |> Seq.filter(fun el -> el.``Entry date``.Month = month)
        let stUpdateBookings = new UpdateBookings()
        stUpdateBookings.Execute(
            [ for row in filteredData -> 
                new Booking(EmployeeGUI = row.``Employee GUI``,
                            EngagementCode = engagementCode,
                            ActivityId = (if row.Activity = "ERW1" then 17 else 1),
                            BookDate = row.``Entry date``.Date,
                            PersonHours = Convert.ToDecimal(row.``Hours Charged``))]) |> ignore


***

### Cool? Yes! But, it goes further 

- Take other programming languages as a data source and use it integrated and typed in F# with the power of FSharp Type Providers

***

### R Type Provider

The [R Type Provider](http://bluemountaincapital.github.io/FSharpRProvider/index.html) makes it possible to use all of R capabilities, from the F# interactive environment. It enables on-the-fly charting and data analysis using R packages, with the added benefit of IntelliSense over R, and compile-time type-checking that the R functions you are using exist

***

### R Type Provider Sample (using ggplot)

See sample by [Evelina Gabasova](http://evelinag.com/blog/2015/12-03-using-ggplot2-from-f/#.VwwWfqSLQ2w)

    open RProvider
    open RProvider.ggplot2
    
    let x = [0.0 .. 0.1 .. 10.0]
    let y = x |> List.map (fun value -> sin(value))

    // create a data frame
    let dataframe = 
        namedParams ["X", x; "Value", y] 
        |> R.data_frame

    G.ggplot(dataframe, G.aes(x="X", y="Value"))
    ++ R.geom__line()

### Extract of further Type Providers

- [HTML Type Provider](http://fsharp.github.io/FSharp.Data/library/HtmlProvider.html)
- [JSON Type Provider](http://fsharp.github.io/FSharp.Data/library/JsonProvider.html)
- [Azure Storage Type Provider](https://github.com/fsprojects/AzureStorageTypeProvider)
- [WSDL Type Provider](https://msdn.microsoft.com/en-US/library/hh362328.aspx)
- [WorldBank Type Provider](http://fsharp.github.io/FSharp.Data/library/WorldBank.html)

***

### DEMO

***

### Program your own type providers

- You have the possibility to create your own type provider.
- Remember, before you start to create your own type provider: 
- Type providers are best suited to situations where the schema is stable at runtime and during the lifetime of compiled code.
- When you create your own type provider, basically you need to do the following steps:
- Create a namespace, where the provided type could live
- Create the types, with members and methods inferred by the schema and add it to the namespace and assembly 

***

### HelloWorldTypeProvider

    // See https://msdn.microsoft.com/de-de/library/hh361034.aspx
    // This type defines the type provider. When compiled to a DLL, it can be referenced
    [<TypeProvider>]
    type SampleTypeProvider(config: TypeProviderConfig) as this = 
        // Inheriting from this type provides implementations of ITypeProvider 
        inherit TypeProviderForNamespaces()

        let namespaceName = "Samples.HelloWorldTypeProvider"
        let thisAssembly = Assembly.GetExecutingAssembly()

        // Make one provided type, called TypeN.
        let makeOneProvidedType (n:int) = 
            // Implementation ...
        // Now generate 100 types
        let types = [ for i in 1 .. 100 -> makeOneProvidedType i ] 

        // And add them to the namespace
        do this.AddNamespace(namespaceName, types)

    [<assembly:TypeProviderAssembly>] 
    do()

***

### Generative VS Erasing Type Providers

Explanation by [Thomas Petricek in a stackoverflow post](http://stackoverflow.com/questions/12118866/how-do-i-create-an-f-type-provider-that-can-be-used-from-c)
<font size ="3" align="left"><br>
The reason why standard type providers (for OData, LINQ to SQL and WSDL) work with C# is that they generate real .NET types behind the cover. This is called generative type provider. In fact, they simply call the code generation tool that would be called if you were using these technologies from C# in a standard way. So, these type providers are just wrappers over some standard .NET tools.
Most of the providers that are newly written are written as erasing type providers. This means that they only generate "fake" types that tell the F# compiler what members can be called (etc.) but when the compiler compiles them, the "fake" types are replaced with some other code. This is the reason why you cannot see any types when you're using the library from C# - none of the types actually exist in the compiled code.
Unless you're wrapping existing code-generator, it is easier to write erased type provider and so most of the examples are written in this way. Erasing type providers have other beneftis - i.e. they can generate huge number of "fake" types without generating excessively big assemblies.
Anyway, there is a brief note "Providing Generated Types" in the MSDN tutorial, which has some hints on writing generative providers. However, I'd expect most of the new F# type providers to be written as erasing. It notes that you must have a real .NET assembly (with the generated types) and getting that is not simplified by the F# helpers for building type providers - so you'll need to emit the IL for the assembly or generate C#/F# code and compile that (i.e. using CodeDOM or Roslyn).  
</font>

***

### Wrap Up

- The great advantage over other codegen tools is that this "service" is provided at compiler level!
- This means, that we have <b>compile time safety</b> in the binding process to the data source
- If some meta / structure information is changed in your data source, you will get a compiler error, rather than a runtime error
- The type provider feature itself is limited to F#, but the generated types are accessible, if we have a generative type provider
- Very, very powerful feature in the data science area, where you have to deal with a lot of crapy ;-), semistructured data

***

### Further links

- Little intro [F# for Data](http://de.slideshare.net/sergeytihon/f-for-data) presentation from [Sergey Tihon](http://de.slideshare.net/sergeytihon)
- [Create your own Type Provider](https://www.google.de/url?sa=t&rct=j&q=&esrc=s&source=web&cd=2&ved=0ahUKEwjxwaGPmYrMAhVE7A4KHfxPBycQFggkMAE&url=https%3A%2F%2Fmsdn.microsoft.com%2Fde-de%2Flibrary%2Fhh361034.aspx&usg=AFQjCNG4a0CeiimQU9Wn_fUs7L9DwGiz6g&cad=rja)
- [F# Type Provider Starter Pack](https://github.com/fsprojects/FSharp.TypeProviders.StarterPack)