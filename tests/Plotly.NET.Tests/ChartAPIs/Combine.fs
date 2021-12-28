﻿module Tests.ChartAPIs.Combine

open Expecto
open Plotly.NET
open Plotly.NET.LayoutObjects
open Plotly.NET.TraceObjects
open Plotly.NET.GenericChart
open Newtonsoft.Json

let testAnnotations1 = [Annotation.init(Text = "test")]
let testAnnotations2 = [Annotation.init(Text = "another one")]

let testShapes1 = [Shape.init(X0 = 42)]
let testShapes2 = [Shape.init(X0 = 69)]

let testImages1 = [LayoutImage.init(Name = "image1")]
let testImages2 = [LayoutImage.init(Name = "image2")]

let testSliders1 = [Slider.init(X = 1337)]
let testSliders2 = [Slider.init(X = 5002)]

let testHiddenLabels1 = ["first"]
let testHiddenLabels2 = ["second"]

let chart1 = 
    Chart.Invisible()
    |> Chart.withLayout(
        Layout.init(
            Annotations    = testAnnotations1,
            Shapes         = testShapes1,
            Images         = testImages1,
            Sliders        = testSliders1,
            HiddenLabels   = testHiddenLabels1
        )
    )

let chart2 = 
    Chart.Invisible()
    |> Chart.withLayout(
        Layout.init(
            Annotations    = testAnnotations2,
            Shapes         = testShapes2,
            Images         = testImages2,
            Sliders        = testSliders2,
            HiddenLabels   = testHiddenLabels2
        )
    )

let combined = Chart.combine [chart1; chart2]

[<Tests>]
let ``Chart.Combine layouts tests`` =
    testList "ChartAPIs.Combine" [
        testList "Combine Layouts" [
            testCase "should combine annotations" (fun _ ->
                let actual = combined |> GenericChart.getLayout |> fun l -> l["annotations"] |> unbox<seq<Annotation>>
                Expect.sequenceEqual actual (Seq.append testAnnotations1 testAnnotations2) "combined chart layout did not contain correct annotations"
            )          
            testCase "should combine shapes" (fun _ ->
                let actual = combined |> GenericChart.getLayout |> fun l -> l["shapes"] |> unbox<seq<Shape>>
                Expect.sequenceEqual actual (Seq.append testShapes1 testShapes2) "combined chart layout did not contain correct shapes"
            )         
            testCase "should combine images" (fun _ ->
                let actual = combined |> GenericChart.getLayout |> fun l -> l["images"] |> unbox<seq<LayoutImage>>
                Expect.sequenceEqual actual (Seq.append testImages1 testImages2) "combined chart layout did not contain correct images"
            )          
            testCase "should combine sliders" (fun _ ->
                let actual = combined |> GenericChart.getLayout |> fun l -> l["sliders"] |> unbox<seq<Slider>>
                Expect.sequenceEqual actual (Seq.append testSliders1 testSliders2) "combined chart layout did not contain correct sliders"
            )          
            testCase "should combine hidden labels" (fun _ ->
                let actual = combined |> GenericChart.getLayout |> fun l -> l["hiddenlabels"] |> unbox<seq<string>>
                Expect.sequenceEqual actual (Seq.append testHiddenLabels1 testHiddenLabels2) "combined chart layout did not contain correct hidden labels"
            )  
        ]
    ]