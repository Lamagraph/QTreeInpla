#!/usr/bin/env -S dotnet fsi

// mtx to Inpla experiment script assuming the matrix only contains lower half (or upper half) elements

#load "./common_get_experiment.fsx"

open Common_get_experiment

open System.IO
open System

let handleFile path algorithm =
    let lines = File.ReadLines(path) |> Seq.toArray
    let removedComments = lines |> Array.skipWhile (fun s -> s.[0] = '%')
    let linewords = removedComments |> Array.map (fun s -> s.Split " ")
    let first = linewords.[0]

    let nrows, ncols, nnz = int first.[0], int first.[1], int first.[2]

    let tl = Array.skip 1 linewords

    Directory.CreateDirectory(experimentsPath algorithm) |> ignore

    let newFilePath =
        Path.Combine(experimentsPath algorithm, Path.GetFileNameWithoutExtension path + inplaExtension)

    let experiment = getExperiment algorithm nrows (getBigCooDuplicate tl)
    File.WriteAllText(newFilePath, experiment)
    printfn "Written to %s %d elements" newFilePath (tl.Length * 2)
    ()


let main (args: string array) =
    if Array.length args <> 3 then
        eprintfn "%s" usage
        1
    else
        let path = args[1]
        let algorithm = getAlgorithm args[2]
        handleFile path algorithm
        0

exit <| main fsi.CommandLineArgs
