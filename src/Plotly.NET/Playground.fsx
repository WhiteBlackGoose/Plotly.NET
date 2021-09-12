﻿#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharpAux"
#r "nuget: DynamicObj"
#r "nuget: Newtonsoft.Json, 13.0.1"

#I "CommonAbstractions"

#load "StyleParams.fs"
#load "ColorKeyword.fs"
#load "Colors.fs"
#load "Frame.fs"
#load "Font.fs"
#load "Title.fs"
#load "Line.fs"

#I "Layout/ObjectAbstractions/Common"

#load "Button.fs"
#load "RangeSelector.fs"
#load "RangeSlider.fs"
#load "Transition.fs"
#load "ActiveShape.fs"
#load "ModeBar.fs"
#load "DefaultColorScales.fs"
#load "UniformText.fs"
#load "Margin.fs"
#load "Domain.fs"
#load "Shape.fs"
#load "Annotation.fs"
#load "LayoutGrid.fs"
#load "Legend.fs"
#load "Hoverlabel.fs"
#load "TickFormatStop.fs"
#load "ColorBar.fs"
#load "Rangebreak.fs"
#load "LinearAxis.fs"
#load "ColorAxis.fs"

#I "Layout/ObjectAbstractions/Map"

#load "GeoProjection.fs"
#load "Geo.fs"
#load "MapboxLayerSymbol.fs"
#load "MapboxLayer.fs"
#load "Mapbox.fs"

#I "Layout/ObjectAbstractions/3D"

#load "Camera.fs"
#load "AspectRatio.fs"
#load "Scene.fs"

#I "Layout/ObjectAbstractions/Polar"

#load "AngularAxis.fs"
#load "RadialAxis.fs"
#load "Polar.fs"

#load "Layout/Layout.fs"

#I "Traces/ObjectAbstractions"

#load "Marker.fs"
#load "Projection.fs"
#load "Surface.fs"
#load "SpaceFrame.fs"
#load "Slices.fs"
#load "Caps.fs"
#load "StreamTubeStarts.fs"
#load "Lighting.fs"
#load "Selection.fs"
#load "StockData.fs"
#load "Pathbar.fs"
#load "TreemapTiling.fs"
#load "Contours.fs"
#load "Dimensions.fs"
#load "WaterfallConnector.fs"
#load "FunnelConnector.fs"
#load "Box.fs"
#load "Meanline.fs"
#load "Bins.fs"
#load "Cumulative.fs"
#load "Error.fs"
#load "Table.fs"

#I "Traces"

#load "Trace.fs"
#load "Trace2D.fs"
#load "Trace3D.fs"
#load "TracePolar.fs"
#load "TraceGeo.fs"
#load "TraceMapbox.fs"
#load "TraceTernary.fs"
#load "TraceCarpet.fs"
#load "TraceDomain.fs"
#load "TraceID.fs"

#I "Template"

#load "Template.fs"

#I "Config/ObjectAbstractions"

#load "ToImageButtonOptions.fs"

#I "Config"

#load "Config.fs"

#I "DisplayOptions"

#load "DisplayOptions.fs"

#I "ChartAPI"

#load "GenericChart.fs"
#load "Chart.fs"
#load "Chart2D.fs"
#load "Chart3D.fs"
#load "ChartPolar.fs"
#load "ChartMap.fs"
#load "ChartDomain.fs"

#I "CSharpLayer"

#load "GenericChartExtensions.fs"

#I "Extensions"

#load "SankeyExtension.fs"

open DynamicObj
open DynamicObj.Operators

open Plotly.NET
open Plotly.NET.LayoutObjects
open Plotly.NET.TraceObjects
open Plotly.NET.ConfigObjects

open FSharp.Data
open Newtonsoft.Json
open System.Text
open System.IO
open Deedle
open FSharpAux

open System


let tableColorDependentChart =
    let header2 = ["Identifier";"T0";"T1";"T2";"T3"]
    let rowvalues = 
        [
         [10001.;0.2;2.0;4.0;5.0]
         [10002.;2.1;2.0;1.8;2.1]
         [10003.;4.5;3.0;2.0;2.5]
         [10004.;0.0;0.1;0.3;0.2]
         [10005.;1.0;1.6;1.8;2.2]
         [10006.;1.0;0.8;1.5;0.7]
         [10007.;2.0;2.0;2.1;1.9]
        ]
        |> Seq.sortBy (fun x -> x.[1])
    
    //map color from value to hex representation
    let mapColor min max value = 
        let proportion = 
            (255. * (value - min) / (max - min))
            |> int
        Color.fromRGB 255 (255 - proportion) proportion
        
    //Assign a color to every cell seperately. Matrix must be transposed for correct orientation.
    let cellcolor = 
         rowvalues
         |> Seq.map (fun row ->
            row 
            |> Seq.mapi (fun index value -> 
                if index = 0 then Color.fromString "white"
                else mapColor 0. 5. value
                )
            )
        |> Seq.transpose
        |> Seq.map Color.fromColors
        |> Color.fromColors

    Chart.Table(header2,rowvalues,ColorCells=cellcolor)

tableColorDependentChart |> Chart.show

Chart.Invisible() 
|> Chart.withLayout(
    Layout.init(
        DragMode = StyleParam.DragMode.DrawRect
    )
)
|> Chart.withConfig(
    Config.init(
        ModeBarButtonsToAdd = [
            StyleParam.ModeBarButton.DrawLine
            StyleParam.ModeBarButton.DrawOpenPath
            StyleParam.ModeBarButton.DrawClosedPath
            StyleParam.ModeBarButton.DrawCircle
            StyleParam.ModeBarButton.DrawRect
            StyleParam.ModeBarButton.EraseShape

        ]
    )
)
|> Chart.show

type TraceIDLocal =
    | Cartesian2D 
    | Cartesian3D 
    | Polar 
    | Geo 
    | Mapbox
    | Ternary 
    | Carpet 
    | Domain 
    | Multi

    static member ofTrace (t:Trace) : TraceIDLocal =
        match t with
        | :? Trace2D      -> TraceIDLocal.Cartesian2D
        | :? Trace3D      -> TraceIDLocal.Cartesian3D
        | :? TracePolar   -> TraceIDLocal.Polar      
        | :? TraceGeo     -> TraceIDLocal.Geo        
        | :? TraceMapbox  -> TraceIDLocal.Mapbox     
        | :? TraceTernary -> TraceIDLocal.Ternary    
        | :? TraceCarpet  -> TraceIDLocal.Carpet     
        | :? TraceDomain  -> TraceIDLocal.Domain     
        | _ as unknownTraceType -> failwith $"cannot get trace id for type {unknownTraceType.GetType()}"

    static member ofTraces (t:seq<Trace>) : TraceIDLocal =
        let traceIds = t |> Seq.map TraceIDLocal.ofTrace |> Seq.distinct |> Array.ofSeq
        match traceIds with
        | [|sameTraceID|]   -> sameTraceID
        | [||]              -> TraceIDLocal.Domain
        | _                 -> TraceIDLocal.Multi

let gridNew (nRows: int) (nCols: int) = 

    fun (gCharts:#seq<GenericChart.GenericChart>) ->
        
        let pattern = StyleParam.LayoutGridPattern.Independent

        let hasSharedAxes = false

        // rows x cols coordinate grid
        let gridCoordinates = 
            Array.init nRows (fun rowIndex ->
                Array.init nCols (fun colIndex ->
                    rowIndex+1,colIndex+1
                )
            )
            |> Array.concat

        gCharts
        |> Seq.zip gridCoordinates
        |> Seq.mapi (fun i ((rowIndex, colIndex), gChart) ->

            let layout = gChart |> GenericChart.getLayout

            match TraceIDLocal.ofTraces (gChart |> GenericChart.getTraces) with
            | TraceIDLocal.Multi -> failwith $"the trace for ({rowIndex},{colIndex}) contains multiple different subplot types. this is not supported."
            | TraceIDLocal.Cartesian2D ->
                
                let xAxis = layout.TryGetTypedValue<LinearAxis> "xaxis" |> Option.defaultValue (LinearAxis.init())
                let yAxis = layout.TryGetTypedValue<LinearAxis> "yaxis" |> Option.defaultValue (LinearAxis.init())

                let xAnchor, yAnchor = 
                    if hasSharedAxes then 
                        colIndex, rowIndex //set axis anchors according to grid coordinates
                    else
                        i+1, i+1

                gChart
                |> Chart.withAxisAnchor(xAnchor,yAnchor) // set adapted axis anchors
                |> Chart.withXAxis(xAxis,(StyleParam.SubPlotId.XAxis (i+1))) // set previous axis with adapted id (one individual axis for each subplot, wether or not they will be used later)
                |> Chart.withYAxis(yAxis,(StyleParam.SubPlotId.YAxis (i+1))) // set previous axis with adapted id (one individual axis for each subplot, wether or not they will be used later)
                |> GenericChart.mapLayout (fun l ->
                    if i > 0 then 
                        // remove default axes from consecutive charts, otherwise they will override the first one
                        l.Remove("xaxis") |> ignore
                        l.Remove("yaxis") |> ignore
                    l
                )
            | TraceIDLocal.Cartesian3D ->
                
                let scene = 
                    layout.TryGetTypedValue<Scene> "scene" |> Option.defaultValue (Scene.init())
                    |> Scene.style(Domain = Domain.init(Row = rowIndex - 1, Column = colIndex - 1))

                let sceneAnchor = StyleParam.SubPlotId.Scene (i+1)

                gChart
                |> GenericChart.mapTrace(fun t ->
                    t 
                    :?> Trace3D
                    |> Trace3DStyle.SetScene sceneAnchor
                    :> Trace
                )
                |> Chart.withScene(scene,sceneAnchor)
            | TraceIDLocal.Polar ->

                let polar = 
                    layout.TryGetTypedValue<Polar> "polar" |> Option.defaultValue (Polar.init())
                    |> Polar.style(Domain = Domain.init(Row = rowIndex - 1, Column = colIndex - 1))

                let polarAnchor = StyleParam.SubPlotId.Polar (i+1)

                gChart
                |> GenericChart.mapTrace(fun t ->
                    t 
                    :?> TracePolar
                    |> TracePolarStyle.SetPolar polarAnchor
                    :> Trace
                )
                |> Chart.withPolar(polar,polarAnchor)
            | TraceIDLocal.Geo ->
                let geo = 
                    layout.TryGetTypedValue<Geo> "geo" |> Option.defaultValue (Geo.init())
                    |> Geo.style(Domain = Domain.init(Row = rowIndex - 1, Column = colIndex - 1))

                let geoAnchor = StyleParam.SubPlotId.Geo (i+1)

                gChart
                |> GenericChart.mapTrace(fun t ->
                    t 
                    :?> TraceGeo
                    |> TraceGeoStyle.SetGeo geoAnchor
                    :> Trace
                )
                |> Chart.withGeo(geo,geoAnchor)
            | TraceIDLocal.Mapbox ->
                let mapbox = 
                    layout.TryGetTypedValue<Mapbox> "mapbox" |> Option.defaultValue (Mapbox.init())
                    |> Mapbox.style(Domain = Domain.init(Row = rowIndex - 1, Column = colIndex - 1))

                let mapboxAnchor = StyleParam.SubPlotId.Mapbox (i+1)

                gChart
                |> GenericChart.mapTrace(fun t ->
                    t 
                    :?> TraceMapbox
                    |> TraceMapboxStyle.SetMapbox mapboxAnchor
                    :> Trace
                )
                |> Chart.withMapbox(mapbox,mapboxAnchor)
            | TraceIDLocal.Domain ->
                let newDomain = Domain.init(Row = rowIndex - 1, Column = colIndex - 1)

                gChart
                |> GenericChart.mapTrace(fun t ->
                    t 
                    :?> TraceDomain
                    |> TraceDomainStyle.SetDomain newDomain
                    :> Trace
                )
        )
        |> Chart.combine
        |> Chart.withLayoutGrid (
            LayoutGrid.init(
                Rows      = nRows,
                Columns   = nCols,
                Pattern   = pattern
            )
        )

gridNew 3 3 [

    let header = ["<b>RowIndex</b>";"A";"simple";"table"]
    let rows = 
        [
         ["0";"I"     ;"am"     ;"a"]        
         ["1";"little";"example";"!"]       
        ]

    
    // Generate linearly spaced vector
    let linspace (min,max,n) = 
        if n <= 2 then failwithf "n needs to be larger then 2"
        let bw = float (max - min) / (float n - 1.)
        [|min ..bw ..max|]
    
    // Create example data
    let size = 100
    let x = linspace(-2. * Math.PI, 2. * Math.PI, size)
    let y = linspace(-2. * Math.PI, 2. * Math.PI, size)
    
    let f x y = - (5. * x / (x**2. + y**2. + 1.) )
    
    let z = 
        Array.init size (fun i -> 
            Array.init size (fun j -> 
                f x.[j] y.[i] 
            )
        )
    
    

    Chart.Contour(z, Showscale = false)
    Chart.Table(header, rows)
    Chart.Point3d([1,3,2])
    [
        Chart.Line([1,2; 3,4; 5,6])
        Chart.Spline([1,2; 3,4; 5,7])
    ]
    |> Chart.combine
    Chart.PointPolar([1,2])
    Chart.PointGeo([1,2])
    Chart.PointMapbox([1,2]) |> Chart.withMapbox(Mapbox.init(Style = StyleParam.MapboxStyle.OpenStreetMap))
    Chart.Sunburst(
        ["A";"B";"C";"D";"E"],
        ["";"";"B";"B";""],
        Values=[5.;0.;3.;2.;3.],
        Text=["At";"Bt";"Ct";"Dt";"Et"]
    )
    let x = ["bin1";"bin2";"bin1";"bin2";"bin1";"bin2";"bin1";"bin1";"bin2";"bin1"]
    let y' =  [2.; 1.5; 5.; 1.5; 2.; 2.5; 2.1; 2.5; 1.5; 1.;2.; 1.5; 5.; 1.5; 3.; 2.5; 2.5; 1.5; 3.5; 1.]
    

    [
        Chart.BoxPlot("y" ,y,Name="bin1",Jitter=0.1,Boxpoints=StyleParam.Boxpoints.All);
        Chart.BoxPlot("y'",y',Name="bin2",Jitter=0.1,Boxpoints=StyleParam.Boxpoints.All);
    ]
    |> Chart.combine
]
|> Chart.withSize(1500,1500)
|> Chart.show
