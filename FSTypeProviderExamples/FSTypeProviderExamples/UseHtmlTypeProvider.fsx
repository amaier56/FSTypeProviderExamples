#r @"..\packages\FSharp.Data.2.2.5\lib\net40\FSharp.Data.dll"
#r @"..\packages\Foogle.Charts.0.0.5\lib\net40\Foogle.Charts.dll"
#I "../../bin"
#load "Foogle.Charts.fsx"

open FSharp.Data
open System.Text.RegularExpressions
open Foogle
open System.Windows.Forms

type PanamaWikiPedia = HtmlProvider<"Panama_Papers_Wikipedia.html">

// Download the latest wikipedia site content
let panamaWik = PanamaWikiPedia.Load("https://en.wikipedia.org/wiki/Panama_Papers").Lists

// read relevant countries by content table of site
panamaWik.Contents.Values
|> Array.filter(fun content -> content.Contains "7.")
|> Array.map(fun countryWithNumber -> Regex.Replace(countryWithNumber, "\d.\d*", ""))
|> Array.iter(fun country -> printfn "%s" country)
//|> Array.map(fun country -> country, 0)
//|> Chart.GeoChart