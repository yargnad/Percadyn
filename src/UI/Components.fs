namespace Percadyn.UI

open Feliz
open Percadyn.Domain

module Components =

    type GridDisplayProps =
        { Grid: Grid
          CellSize: int }

    [<ReactComponent>]
    let GridDisplay (props: GridDisplayProps) =
        let rows = props.Grid.Length
        let cols = if rows > 0 then props.Grid.[0].Length else 0
        let width = cols * props.CellSize
        let height = rows * props.CellSize

        Html.svg [
            prop.width width
            prop.height height
            prop.className "border border-gray-400 bg-gray-900"
            prop.children [
                for r in 0 .. rows - 1 do
                    for c in 0 .. cols - 1 do
                        match props.Grid.[r].[c] with
                        | Alive ->
                            Html.rect [
                                prop.x (c * props.CellSize)
                                prop.y (r * props.CellSize)
                                prop.width props.CellSize
                                prop.height props.CellSize
                                prop.className "fill-cyan-400 hover:fill-cyan-300 transition-colors duration-200"
                            ]
                        | Dead -> Html.none
            ]
        ]
