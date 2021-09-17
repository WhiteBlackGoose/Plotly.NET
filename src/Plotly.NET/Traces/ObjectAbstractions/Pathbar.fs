﻿namespace Plotly.NET.TraceObjects

open Plotly.NET
open Plotly.NET.LayoutObjects
open DynamicObj
open DynamicObj.Operators
open System
open System.Runtime.InteropServices

type Pathbar () =
    inherit ImmutableDynamicObj ()

    ///Initializes pathbar object (used in Chart.Treemap)
    ///
    ///Parameters:
    ///
    ///Visible  : Determines if the path bar is drawn i.e. outside the trace `domain` and with one pixel gap.
    ///         
    ///Side     : Determines on which side of the the treemap the `pathbar` should be presented.
    ///         
    ///EdgeShape: Determines which shape is used for edges between `pathbar` labels.
    ///         
    ///Thickness: Sets the thickness of `pathbar` (in px). If not specified the `pathbar.textfont.size` is used with 3 pixles extra padding on each side.
    ///         
    ///Textfont : Sets the font used inside `pathbar`.
    static member init 
        (
            [<Optional;DefaultParameterValue(null)>] ?Visible    :bool,
            [<Optional;DefaultParameterValue(null)>] ?Side       :StyleParam.Side,
            [<Optional;DefaultParameterValue(null)>] ?EdgeShape  :StyleParam.TreemapEdgeShape,
            [<Optional;DefaultParameterValue(null)>] ?Thickness  :float,
            [<Optional;DefaultParameterValue(null)>] ?Textfont   :Font
        ) = 

        Pathbar() 
        |> Pathbar.style 
            (
                ?Visible    = Visible  ,
                ?Side       = Side     ,
                ?EdgeShape  = EdgeShape,
                ?Thickness  = Thickness,
                ?Textfont   = Textfont 
            )

    ///Applies the given styles to the given pathbar object 
    ///
    ///Parameters:
    ///
    ///Visible  : Determines if the path bar is drawn i.e. outside the trace `domain` and with one pixel gap.
    ///         
    ///Side     : Determines on which side of the the treemap the `pathbar` should be presented.
    ///         
    ///EdgeShape: Determines which shape is used for edges between `pathbar` labels.
    ///         
    ///Thickness: Sets the thickness of `pathbar` (in px). If not specified the `pathbar.textfont.size` is used with 3 pixles extra padding on each side.
    ///         
    ///Textfont : Sets the font used inside `pathbar`.
    static member style 
        (
            [<Optional;DefaultParameterValue(null)>] ?Visible    :bool,
            [<Optional;DefaultParameterValue(null)>] ?Side       :StyleParam.Side,
            [<Optional;DefaultParameterValue(null)>] ?EdgeShape  :StyleParam.TreemapEdgeShape,
            [<Optional;DefaultParameterValue(null)>] ?Thickness  :float,
            [<Optional;DefaultParameterValue(null)>] ?Textfont   :Font
        ) = 
            (fun (pathbar:Pathbar) -> 

                pathbar
                ++? ("visible", Visible   )
                ++?? ("side", Side      , StyleParam.Side.convert)
                ++?? ("edgeshape", EdgeShape , StyleParam.TreemapEdgeShape.convert)
                ++? ("thickness", Thickness )
                ++? ("textfont", Textfont)
            )