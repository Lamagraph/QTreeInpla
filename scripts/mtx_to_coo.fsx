#!/usr/bin/env -S dotnet fsi

// mtx to Inpla coo script
// The input matrix should not contain any other data(like comments)

open System.IO
open System

let cooExtension = ".coo"

let getBigCoo (linewords: string array array) =
    linewords
    |> Array.map (fun x -> sprintf "(%s, %s, 1)" x.[0] x.[1])
    |> String.concat ", "
    |> sprintf "[%s]"

let handleFile path =
    let lines = File.ReadLines(path) |> Seq.toArray
    let linewords = lines |> Array.map (fun s -> s.Split " ")
    let first = linewords.[0]

    let nrows, ncols, nnz =
        int first.[0], int first.[1], int first.[2]

    let tl = Array.skip 1 linewords
    let newFilePath = Path.ChangeExtension(path, cooExtension)
    File.WriteAllText(newFilePath, getBigCoo tl)
    printfn "Written to %s" newFilePath
    ()


let main (args: string array) =
    if Array.length args = 1 then
        eprintfn "Path to mtx matrix was not provided"
        1
    else
        let path = args[1]
        handleFile path
        0

exit <| main fsi.CommandLineArgs
