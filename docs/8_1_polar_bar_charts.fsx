(**
---
title: Polar bar charts
category: Polar Charts
categoryindex: 9 
index: 2
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
# Polar bar charts

[![Binder]({{root}}img/badge-binder.svg)](https://mybinder.org/v2/gh/plotly/Plotly.NET/gh-pages?filepath={{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}img/badge-script.svg)]({{root}}{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}img/badge-notebook.svg)]({{root}}{{fsdocs-source-basename}}.ipynb)

*Summary:* This example shows how to create polar bar charts in F#.

let's first create some data for the purpose of creating example charts:

*)

open Plotly.NET 
  
let r   = [77.5; 72.5; 70.0; 45.0; 22.5; 42.5; 40.0; 62.5]
let r2  = [57.5; 50.0; 45.0; 35.0; 20.0; 22.5; 37.5; 55.0]
let r3  = [40.0; 30.0; 30.0; 35.0; 7.5; 7.5; 32.5; 40.0]
let r4  = [20.0; 7.5; 15.0; 22.5; 2.5; 2.5; 12.5; 22.5]

let t = ["North"; "N-E"; "East"; "S-E"; "South"; "S-W"; "West"; "N-W"]

(**
Polar bar charts plot data on a radial axis and a categorical angular axis. 

A common use case is the **windrose chart**.

A wind rose is a graphic tool used by meteorologists to give a succinct view 
of how wind speed and direction are typically distributed at a particular location.
*)
  
let windrose1 =
    [
        Chart.BarPolar (r , t, Name="11-14 m/s")
        Chart.BarPolar (r2, t, Name="8-11 m/s")
        Chart.BarPolar (r3, t, Name="5-8 m/s")
        Chart.BarPolar (r4, t, Name="< 5 m/s")
    ]
    |> Chart.combine
    |> Chart.withAngularAxis(
        Axis.AngularAxis.init(
            CategoryOrder = StyleParam.CategoryOrder.Array,
            CategoryArray = (["East"; "N-E"; "North"; "N-W"; "West"; "S-W"; "South"; "S-E";]) // set the order of the categorical axis
        )
    )

(*** condition: ipynb ***)
#if IPYNB
windrose1
#endif // IPYNB

(***hide***)
windrose1 |> GenericChart.toChartHTML
(***include-it-raw***)



