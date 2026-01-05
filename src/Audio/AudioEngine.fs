namespace Percadyn.Audio

open Fable.Core
open Fable.Core.JsInterop
open Browser

module AudioEngine =

    // We need to define some Web Audio types if they aren't in Fable.Browser.Dom
    // Or we can use dynamic typing for simplicity in this phase.
    
    // Simple Pentatonic Scale (C Minor Pentatonic: C, Eb, F, G, Bb)
    let scale = [| 130.81; 155.56; 174.61; 196.00; 233.08; 261.63; 311.13; 349.23; 392.00; 466.16 |]

    let private createAudioContext () =
        let win = window :?> obj
        if win?AudioContext then
            createNew win?AudioContext ()
        elif win?webkitAudioContext then
            createNew win?webkitAudioContext ()
        else
            failwith "Web Audio API not supported"

    // Lazy initialization to respect browser autoplay policies
    let mutable private context : obj option = None

    let getContext () =
        match context with
        | Some ctx -> ctx
        | None ->
            let ctx = createAudioContext()
            context <- Some ctx
            ctx

    let playNote (pitchIndex: int) (duration: float) =
        try
            let ctx = getContext()
            let t = ctx?currentTime
            
            // Oscillator
            let osc = ctx?createOscillator()
            
            // Simple mapping: wrap index around scale length
            let freq = scale.[pitchIndex % scale.Length]
            // Shift octaves if higher
            let octave = pitchIndex / scale.Length
            let finalFreq = freq * (2.0 ** float octave)

            osc?frequency?value <- finalFreq
            osc?``type`` <- "sine"

            // Gain (Envelope)
            let gain = ctx?createGain()
            gain?gain?setValueAtTime(0.0, t)
            gain?gain?linearRampToValueAtTime(0.1, t + 0.05) // Attack
            gain?gain?exponentialRampToValueAtTime(0.001, t + duration) // Release

            // Connect
            osc?connect(gain)
            gain?connect(ctx?destination)

            // Start/Stop
            osc?start(t)
            osc?stop(t + duration)
        with
        | ex -> console.error("Audio Error:", ex)

    let resumeContext () =
        match context with
        | Some ctx -> 
            if ctx?state = "suspended" then
                ctx?resume() |> ignore
        | None -> ignore (getContext())
