(**
---
title: Axis styling
category: Chart Layout
categoryindex: 2
index: 1
---
*)

(*** hide ***)

(*** condition: prepare ***)
#r "nuget: Newtonsoft.JSON, 12.0.3"
#r "../bin/Plotly.NET/netstandard2.0/Plotly.NET.dll"

(*** condition: ipynb ***)
#if IPYNB
#r "nuget: Plotly.NET, {{fsdocs-package-version}}"
#r "nuget: Plotly.NET.Interactive, {{fsdocs-package-version}}"
#endif // IPYNB

(**
# Axis styling

[![Binder]({{root}}img/badge-binder.svg)](https://mybinder.org/v2/gh/plotly/Plotly.NET/gh-pages?filepath={{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}img/badge-script.svg)]({{root}}{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}img/badge-notebook.svg)]({{root}}{{fsdocs-source-basename}}.ipynb)

*Summary:* This example shows how to style chart axes in F#.

let's first create some data for the purpose of creating example charts:
*)

open Plotly.NET 
  
let x = [1.; 2.; 3.; 4.; 5.; 6.; 7.; 8.; 9.; 10.; ]
let y = [2.; 1.5; 5.; 1.5; 3.; 2.5; 2.5; 1.5; 3.5; 1.]
let y' = y |> List.map (fun y -> y * 2.) |> List.rev
(**
## Single axis styling

To style a specific axis of a plot, use the respective `Chart.with*_AxisStyle` function:

*)

let plot1 =
    Chart.Point(x,y)
    |> Chart.withXAxisStyle ("X axis title", MinMax = (-1.,10.))
    |> Chart.withYAxisStyle ("Y axis title", MinMax = (-1.,10.))

(*** condition: ipynb ***)
#if IPYNB
plot1
#endif // IPYNB

(***hide***)
plot1 |> GenericChart.toChartHTML
(***include-it-raw***)

(**
for even more fine-grained control, initialize a new axis and replace the old one of the plot with ````.
The following example creates two mirrored axes with inside ticks, one of them with a log scale:
*)

let mirroredXAxis =
    Axis.LinearAxis.init(
        Title = Title.init(Text="Mirrored axis"),
        ShowLine = true,
        Mirror = StyleParam.Mirror.AllTicks,
        ShowGrid = false,
        Ticks = StyleParam.TickOptions.Inside
    )

let mirroredLogYAxis = 
    Axis.LinearAxis.init(
        Title = Title.init(Text="Log axis"),
        AxisType = StyleParam.AxisType.Log,
        ShowLine = true,
        Mirror = StyleParam.Mirror.AllTicks,
        ShowGrid = false,
        Ticks = StyleParam.TickOptions.Inside
    )

let plot2 =
    Chart.Point(x,y)
    |> Chart.withXAxis mirroredXAxis
    |> Chart.withYAxis mirroredLogYAxis

(*** condition: ipynb ***)
#if IPYNB
plot2
#endif // IPYNB

(***hide***)
plot2 |> GenericChart.toChartHTML
(***include-it-raw***)


(**
## Multiple axes

Assign different axis anchors to subplots to map them to different axes.

### Multiple axes on different sides of the chart

The following example first creates a multichart containing two plots with different axis anchors.
Subsequently, multiple axes with the respective anchors are added to the plot. 
Note that the same can be done as above, defining axes beforehand.

*)

let anchoredAt1 =
    Chart.Line (x,y,Name="anchor 1")
        |> Chart.withAxisAnchor(Y=1)

let anchoredAt2 =
     Chart.Line (x,y',Name="anchor 2")
        |> Chart.withAxisAnchor(Y=2)

let twoXAxes1 = 
    [
       anchoredAt1
       anchoredAt2
    ]
    |> Chart.combine
    |> Chart.withYAxisStyle(
        "axis 1",
        Side=StyleParam.Side.Left,
        Id=1
    )
    |> Chart.withYAxisStyle(
        "axis2",
        Side=StyleParam.Side.Right,
        Id=2,
        Overlaying=StyleParam.AxisAnchorId.Y 1
    )
        
(*** condition: ipynb ***)
#if IPYNB
twoXAxes1
#endif // IPYNB

(***hide***)
twoXAxes1 |> GenericChart.toChartHTML
(***include-it-raw***)

(**
### Multiple axes on the same side of the chart

Analogous to above, but move the whole plot to the right by adjusting its domain, and then add a second axis to the left:

*)

let twoXAxes2 =
    [
        anchoredAt1
        anchoredAt2
    ]
    |> Chart.combine
    |> Chart.withYAxisStyle(
            "first y-axis",
            ShowLine=true
            )
    |> Chart.withXAxisStyle(
        "x-axis",
        Domain=(0.3, 1.0) // moves the first axis and the whole plot to the right
    ) 
    |> Chart.withYAxisStyle(
        "second y-axis",
        Side=StyleParam.Side.Left,
        Id=2,
        Overlaying=StyleParam.AxisAnchorId.Y 1,
        Position=0.10, // position the axis beteen the leftmost edge and the firt axis at 0.3
        Anchor=StyleParam.AxisAnchorId.Free,
        ShowLine=true
    )

(*** condition: ipynb ***)
#if IPYNB
twoXAxes2
#endif // IPYNB

(***hide***)
twoXAxes2 |> GenericChart.toChartHTML
(***include-it-raw***)