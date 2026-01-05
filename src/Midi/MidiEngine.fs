namespace Percadyn.Midi

open Fable.Core
open Fable.Core.JsInterop
open Browser

module MidiEngine =
    
    // MIDI Message Types
    let [<Literal>] NoteOn = 0x90
    let [<Literal>] NoteOff = 0x80
    let [<Literal>] Clock = 0xF8
    let [<Literal>] Start = 0xFA
    let [<Literal>] Stop = 0xFC
    let [<Literal>] Continue = 0xFB

    type MidiOutput =
        { Id: string
          Name: string
          Port: obj } // Keep raw JS object internally

    type MidiState =
        { IsInitialized: bool
          Outputs: MidiOutput list
          SelectedOutputId: string option }

    let mutable private midiAccess : obj option = None

    // Request MIDI Access
    let init () =
        let nav : obj = (window :?> obj)?navigator
        if not (isNull nav?requestMIDIAccess) then
            promise {
                try
                    let! access = nav?requestMIDIAccess(createObj [ "sysex" ==> false ])
                    midiAccess <- Some access
                    
                    // Enumerate Outputs
                    let outputs = ResizeArray<MidiOutput>()
                    let iterator = access?outputs?values()
                    
                    let mutable item = iterator?next()
                    while not item?``done`` do
                        let port = item?value
                        outputs.Add({ Id = port?id; Name = port?name; Port = port })
                        item <- iterator?next()
                        
                    return Ok (outputs |> Seq.toList)
                with ex ->
                    return Error ex.Message
            }
        else
            Promise.lift (Error "Web MIDI API not supported in this browser.")

    // Send generic MIDI message
    let private send (outputId: string option) (bytes: int[]) =
        match midiAccess, outputId with
        | Some _, Some id ->
            // Re-find the port to ensure validity (or cache it in a real app)
            // For MVP, we'll scan our cached access object or just trust the ID passed if we had a map.
            // Better: Iterate cached access again.
            let access = midiAccess.Value
            let iterator = access?outputs?values()
            let mutable item = iterator?next()
            let mutable found = false
            
            while not item?``done`` && not found do
                let port = item?value
                if port?id = id then
                    // Send 3 bytes (or more)
                    // Fable needs us to pass a JS Uint8Array
                    let uint8Params = ResizeArray<obj>()
                    for b in bytes do uint8Params.Add(b)
                    
                    port?send(uint8Params.ToArray())
                    found <- true
                item <- iterator?next()
        | _ -> ()

    let sendNoteOn (outputId: string option) (channel: int) (note: int) (velocity: int) =
        // Channel is 0-15. Status byte = NoteOn | Channel
        let status = NoteOn ||| (channel &&& 0x0F)
        send outputId [| status; note; velocity |]

    let sendNoteOff (outputId: string option) (channel: int) (note: int) =
        let status = NoteOff ||| (channel &&& 0x0F)
        send outputId [| status; note; 0 |]

    let sendClock (outputId: string option) =
        send outputId [| Clock |]

    let sendStart (outputId: string option) =
        send outputId [| Start |]

    let sendStop (outputId: string option) =
        send outputId [| Stop |]
