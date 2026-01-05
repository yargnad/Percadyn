module Percadyn.App

open Feliz
open Fable.Core
open Percadyn.Domain
open Percadyn.UI.Components

type State =
    { Grid: Grid
      IsRunning: bool
      Step: int }

type Msg =
    | Tick
    | ToggleRunning
    | Clear
    | Randomize

let init () =
    // Initialize a 32x32 grid
    let rows, cols = 32, 32
    let grid = Array.init rows (fun _ -> Array.init cols (fun _ -> Dead))
    
    // Add some initial noise (Glider-ish)
    // Use simple assignments for initial state
    grid.[10].[10] <- Alive
    grid.[10].[11] <- Alive
    grid.[10].[12] <- Alive
    grid.[11].[12] <- Alive
    grid.[12].[11] <- Alive

    { Grid = grid
      IsRunning = false
      Step = 0 }

// Reducer returns just the new state (no Cmd)
let update (state: State) (msg: Msg) =
    match msg with
    | ToggleRunning ->
        if not state.IsRunning then
            Percadyn.Audio.AudioEngine.resumeContext()
        { state with IsRunning = not state.IsRunning }

    | Tick ->
        if not state.IsRunning then state
        else
            // 1. Partition
            let blocks = Rules.partition state.Grid state.Step
            
            // 2. Apply Rule (Single Rotation)
            let nextBlocks = 
                blocks 
                |> Array.map (Array.map Rules.singleRotation)

            // 3. Reassemble
            let nextGrid = Rules.reassemble nextBlocks state.Step state.Grid
            
            // 4. Audio Triggering
            // Determine active column based on step
            let cols = if nextGrid.Length > 0 then nextGrid.[0].Length else 0
            let activeCol = state.Step % cols
            
            // Scan column and play notes
            for r in 0 .. nextGrid.Length - 1 do
                if nextGrid.[r].[activeCol] = Alive then
                    // Map row to pitch (invert so 0 is high or low depending on preference)
                    // Here: 0 = Low pitch
                    Percadyn.Audio.AudioEngine.playNote r 0.1

            { state with 
                Grid = nextGrid
                Step = state.Step + 1 }

    | Clear ->
        let rows = state.Grid.Length
        let cols = if rows > 0 then state.Grid.[0].Length else 0
        let emptyGrid = Array.init rows (fun _ -> Array.init cols (fun _ -> Dead))
        { state with Grid = emptyGrid; IsRunning = false; Step = 0 }

    | Randomize ->
        let rows = state.Grid.Length
        let cols = if rows > 0 then state.Grid.[0].Length else 0
        let rnd = System.Random()
        let newGrid = Array.init rows (fun _ -> 
            Array.init cols (fun _ -> if rnd.NextDouble() > 0.7 then Alive else Dead))
        { state with Grid = newGrid; Step = 0 }

[<ReactComponent>]
let App() =
    let state, dispatch = React.useReducer(update, init())

    // Timer Effect
    React.useEffect(fun () ->
        if state.IsRunning then
            let timer = Fable.Core.JS.setInterval (fun () -> dispatch Tick) 100
            React.createDisposable(fun () -> Fable.Core.JS.clearInterval timer)
        else
            React.createDisposable(fun () -> ())
    , [| box state.IsRunning |])

    Html.div [
        prop.className "min-h-screen bg-black text-white p-8 flex flex-col items-center font-mono"
        prop.children [
            
            // Header
            Html.h1 [
                prop.className "text-4xl font-bold mb-2 tracking-widest text-transparent bg-clip-text bg-gradient-to-r from-cyan-400 to-purple-500"
                prop.text "PERCADYN"
            ]
            Html.p [
                prop.className "text-gray-400 mb-8"
                prop.textf "Step: %d" state.Step
            ]

            // Main Display
            GridDisplay { Grid = state.Grid; CellSize = 12 }

            // Controls
            Html.div [
                prop.className "flex gap-4 mt-8"
                prop.children [
                    Html.button [
                        prop.className "px-6 py-2 bg-gray-800 hover:bg-gray-700 rounded transition-colors border border-gray-600"
                        prop.text (if state.IsRunning then "STOP" else "START")
                        prop.onClick (fun _ -> dispatch ToggleRunning)
                    ]
                    
                    Html.button [
                        prop.className "px-6 py-2 bg-gray-800 hover:bg-gray-700 rounded transition-colors border border-gray-600"
                        prop.text "RANDOMIZE"
                        prop.onClick (fun _ -> dispatch Randomize)
                    ]

                    Html.button [
                        prop.className "px-6 py-2 bg-red-900/30 hover:bg-red-900/50 text-red-400 rounded transition-colors border border-red-900"
                        prop.text "CLEAR"
                        prop.onClick (fun _ -> dispatch Clear)
                    ]
                ]
            ]
        ]
    ]

open Browser.Dom

let root = ReactDOM.createRoot(document.getElementById "root")
root.render(App())
