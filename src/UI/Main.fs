module Percadyn.App

open Feliz
open Browser.Dom

[<ReactComponent>]
let Counter() =
    let count, setCount = React.useState(0)
    Html.div [
        Html.h1 "Percadyn Scaffolding"
        Html.button [
            prop.onClick (fun _ -> setCount(count + 1))
            prop.textf "Count: %d" count
        ]
    ]

let root = ReactDOM.createRoot(document.getElementById "root")
root.render(Counter())
