module Percadyn.App

open Feliz
open Fable.Core
open Percadyn.Domain
open Percadyn.UI.Components

open Percadyn.Midi

type State =
    { Grid: Grid
      IsRunning: bool
      Step: int
      MidiDevices: MidiEngine.MidiOutput list
      SelectedMidiOutput: string option }

type Msg =
    | Tick
    | ToggleRunning
    | Clear
    | Randomize
    | MidiAccessGranted of MidiEngine.MidiOutput list
    | MidiAccessFailed of string
    | SelectMidiOutput of string

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
      Step = 0 
      MidiDevices = []
      SelectedMidiOutput = None }

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
                    // Audio Engine (Web Audio)
                    // Map row to pitch (invert so 0 is high or low depending on preference)
                    Percadyn.Audio.AudioEngine.playNote r 0.1
                    
                    // MIDI Engine
                    // Simple Chromatic mapping for now: Middle C (60) + Row Index (reversed?)
                    // Let's assume Middle C is roughly row 16.
                    let midiNote = 60 + (16 - r)
                    // Channel 1 (0)
                    MidiEngine.sendNoteOn state.SelectedMidiOutput 0 midiNote 100
                    
                    // Note Off after 100ms (rough hack, ideally scheduling)
                    // In a real scheduler we'd schedule the NoteOff. 
                    // For setInterval MVP, we utilize the fire-and-forget.
                    // This creates short blips.
                    // Better: use setTimeout? No, blocking.
                    ignore (Fable.Core.JS.setTimeout (fun () -> 
                        MidiEngine.sendNoteOff state.SelectedMidiOutput 0 midiNote) 100)

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

    | MidiAccessGranted devices ->
        { state with MidiDevices = devices; SelectedMidiOutput = devices |> List.tryHead |> Option.map (fun d -> d.Id) }

    | MidiAccessFailed error ->
        JS.console.error("MIDI Init Failed: " + error)
        state

    | SelectMidiOutput id ->
        { state with SelectedMidiOutput = Some id }

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

    // MIDI Init Effect
    React.useEffect(fun () ->
        promise {
            let! result = MidiEngine.init()
            match result with
            | Ok devices -> dispatch (MidiAccessGranted devices)
            | Error err -> dispatch (MidiAccessFailed err)
        } |> ignore
    , [| |])

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
            
            // MIDI Settings
            if not state.MidiDevices.IsEmpty then
                Html.div [
                    prop.className "mt-4 flex flex-col items-center gap-2"
                    prop.children [
                        Html.label [
                            prop.className "text-gray-400 text-sm"
                            prop.text "MIDI Output"
                        ]
                        Html.select [
                            prop.className "bg-gray-800 text-white px-4 py-1 rounded border border-gray-600"
                            prop.value (defaultArg state.SelectedMidiOutput "")
                            prop.onChange (fun (value: string) -> dispatch (SelectMidiOutput value))
                            prop.children [
                                for device in state.MidiDevices do
                                    Html.option [
                                        prop.value device.Id
                                        prop.text device.Name
                                    ]
                            ]
                        ]
                    ]
                ]
        ]
    ]

open Browser.Dom

let root = ReactDOM.createRoot(document.getElementById "root")
root.render(App())
