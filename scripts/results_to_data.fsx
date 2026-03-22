#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#load "./common_get_experiment.fsx"

open Common_get_experiment

let resultsPath alg =
    match alg with
    | TC -> "experiments_tc/results/"
    | BFS -> "experiments_bfs/results/"
    | SSSP -> "experiments_sssp/results/"


let convertationPath alg =
    match alg with
    | TC -> "experiments_tc/data/convertation_data/"
    | BFS -> "experiments_bfs/data/convertation_data/"
    | SSSP -> "experiments_sssp/data/convertation_data/"

let algPath alg =
    match alg with
    | TC -> "./experiments_tc/data/algorithm_data/"
    | BFS -> "./experiments_bfs/data/algorithm_data/"
    | SSSP -> "./experiments_sssp/data/algorithm_data/"

let convertationLine alg =
    alg |> ignore
    2

let algLine alg =
    match alg with
    | TC
    | SSSP -> 3
    | BFS -> 5

let getTime (str: string) =
    // ... interactions (by n threads), t sec)
    let words = str.Split(' ')
    let timeword = words.Length - 2
    words.[timeword]

// We can get this data from the filename
// or from the file itself
let getThreads (str: string) =
    let words = str.Split(' ')
    let threadword = 3
    int <| words.[threadword]

// Gets number of threads and time
let handleFile alg filePath =
    let text = File.ReadAllLines filePath
    let threads = getThreads text.[algLine alg]
    let algTime = getTime text.[algLine alg]
    let convertationTime = getTime text.[convertationLine alg]
    threads, algTime, convertationTime

let handleDirectory alg dir =
    let files = Directory.GetFiles(dir, "*.output")
    let data = files |> Array.map (handleFile alg) |> Array.rev

    let convertationDataFile =
        data
        |> Array.map (fun (threads, _, convertTime) -> sprintf "%d %s" threads convertTime)
        |> String.concat "\n"

    let algDataFile =
        data
        |> Array.map (fun (threads, algTime, _) -> sprintf "%d %s" threads algTime)
        |> String.concat "\n"

    let name = DirectoryInfo(dir).Name
    name, algDataFile, convertationDataFile

let usage = $"Usage: {fsi.CommandLineArgs.[0]} ('bfs' | 'tc' | 'sssp')"

let main (args: string array) =
    if Array.length args <> 2 then
        eprintfn "%s" usage
        exit 1

    let alg = args.[1] |> getAlgorithm

    let dirs =
        Directory.GetDirectories(resultsPath alg)
        |> Array.except [ algPath alg; convertationPath alg ]

    let data = dirs |> Array.map (handleDirectory alg)
    Directory.CreateDirectory(convertationPath alg) |> ignore
    Directory.CreateDirectory(algPath alg) |> ignore

    for name, algData, convertationData in data do
        printfn "Successfully processed %s data" name
        let algData = sprintf "%s\n%s" name algData
        let convertationData = sprintf "%s\n%s" name convertationData
        let name = name + ".data"
        File.WriteAllText(algPath alg + name, algData)
        File.WriteAllText(convertationPath alg + name, convertationData)

    0

exit <| main fsi.CommandLineArgs
