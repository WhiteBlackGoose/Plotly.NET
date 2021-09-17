﻿namespace Plotly.NET.LayoutObjects

open Plotly.NET
OHNONONO
open System
open System.Runtime.InteropServices

/// <summary>Determines the style of the map shown in mapbox traces</summary>
type Mapbox() = 

    inherit ImmutableDynamicObj ()

    /// <summary>Initialize a Mapbox object that determines the style of the map shown in geo mapbox</summary>

    static member init
        (   
            [<Optional;DefaultParameterValue(null)>] ?Domain: Domain,
            [<Optional;DefaultParameterValue(null)>] ?AccessToken: string,
            [<Optional;DefaultParameterValue(null)>] ?Style: StyleParam.MapboxStyle,
            [<Optional;DefaultParameterValue(null)>] ?Center: (float*float),
            [<Optional;DefaultParameterValue(null)>] ?Zoom: float,
            [<Optional;DefaultParameterValue(null)>] ?Bearing: float,
            [<Optional;DefaultParameterValue(null)>] ?Pitch: float,
            [<Optional;DefaultParameterValue(null)>] ?Layers: seq<MapboxLayer>
        ) =
            Mapbox()
            |> Mapbox.style
                (
                    ?Domain     = Domain,
                    ?AccessToken= AccessToken,
                    ?Style      = Style,
                    ?Center     = Center,
                    ?Zoom       = Zoom,
                    ?Bearing    = Bearing,
                    ?Pitch      = Pitch,
                    ?Layers     = Layers
                )

    /// <summary>Create a function that applies the given style parameters to a Mapbox object.</summary>

    static member style
        (   
            [<Optional;DefaultParameterValue(null)>] ?Domain: Domain,
            [<Optional;DefaultParameterValue(null)>] ?AccessToken: string,
            [<Optional;DefaultParameterValue(null)>] ?Style: StyleParam.MapboxStyle,
            [<Optional;DefaultParameterValue(null)>] ?Center: (float*float),
            [<Optional;DefaultParameterValue(null)>] ?Zoom: float,
            [<Optional;DefaultParameterValue(null)>] ?Bearing: float,
            [<Optional;DefaultParameterValue(null)>] ?Pitch: float,
            [<Optional;DefaultParameterValue(null)>] ?Layers: seq<MapboxLayer>

        ) =
            (fun (mapBox:Mapbox) -> 

                Center
                
                ++? ("domain", Domain          )
                ++? ("accesstoken", AccessToken     )
                ++?? ("style", Style           , StyleParam.MapboxStyle.convert)         
                |> Option.map (fun (lon,lat) -> 
                    let t = ImmutableDynamicObj()
                    t?lon <- lon
                    t?lat <- lat
                    t

                mapBox
                ++? ("center", ))

                ++? ("zoom", Zoom            )
                ++? ("bearing", Bearing         )
                ++? ("pitch", Pitch           )
                ++? ("layers", Layers          )
            ) 
