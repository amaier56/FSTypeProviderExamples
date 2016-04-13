﻿
#nowarn "211"
#I "../../bin"
#r @"..\packages\Foogle.Charts.0.0.5\lib\net40\Foogle.Charts.dll"
open Foogle
open Foogle.Formatting
open Foogle.SimpleHttp
open System.Windows.Forms
open System.IO

let server : HttpServer option ref = ref None
let tempDir = Path.GetTempFileName()
do File.Delete(tempDir)
do Directory.CreateDirectory(tempDir) |> ignore

fsi.AddPrinter(fun (chart:FoogleChart) ->
  match !server with 
  | None -> server := Some (HttpServer.Start("http://localhost:8081/", tempDir))
  | _ -> ()

  match chart.Options.Engine with
  | Some Highcharts ->
      let highChart = Highcharts.CreateHighchartsChart(chart)
      File.WriteAllText(Path.Combine(tempDir, "index.html"), Highcharts.HighchartsHtml highChart)
      printfn "HIGHCHARTS: %s\n%s" tempDir (Highcharts.HighchartsHtml highChart)

  | _ ->
      let googleChart = Google.CreateGoogleChart(chart)
      File.WriteAllText(Path.Combine(tempDir, "index.html"), Google.GoogleChartHtml googleChart)
      printfn "GOOGLE: %s\n%s" tempDir (Google.GoogleChartHtml googleChart)

  System.Diagnostics.Process.Start("http://localhost:8081/index.html") |> ignore
  "(Foogle Chart)" )