#!/usr/bin/env -S dotnet fsi

open System.IO

let usage = sprintf "Usage: %s <path_to_integer_mtx_matrix>" fsi.CommandLineArgs[0]

let toTriple (arr: 'a array) = arr.[0], arr.[1], arr.[2]

let processLines (lines: string array) =
    try

        let comments, data = lines |> Array.partition (fun s -> s.[0] = '%')
        let header, data = data[0], Array.skip 1 data
        let dataInt = data |> Array.map (fun s -> s.Split ' ' |> Array.map int |> toTriple)
        let _, _, min = dataInt |> Array.minBy (fun (_, _, z) -> z)
        printfn "Minimum is %d. Adding %d to all weights." min (abs min + 1)
        let added = dataInt |> Array.map (fun (x, y, z) -> x, y, z + abs min + 1)
        let added_lines = added |> Array.map (fun (x, y, z) -> sprintf "%d %d %d" x y z)

        Some
        <| String.concat "\n" (Array.toList comments @ [ header ] @ Array.toList added_lines)
           + "\n"
    with e ->
        printfn "%s" e.Message
        printfn "%s" usage
        None

let main (args: string array) =
    if args.Length < 2 then
        printfn "%s" usage
        1
    else
        let path = args[1]

        let new_path =
            Path.GetDirectoryName path
            + "/"
            + Path.GetFileNameWithoutExtension path
            + "_positive"
            + Path.GetExtension path

        let lines = File.ReadAllLines path
        let processed = processLines lines

        match processed with
        | None -> 1
        | Some processed ->
            printfn "Writing to %s" new_path
            File.WriteAllText(new_path, processed)
            0

exit <| main fsi.CommandLineArgs
